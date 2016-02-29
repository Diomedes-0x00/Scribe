using Scribe.Attributes;
using Scribe.Conversion;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Scribe
{
    public class ScribeContext : IDisposable
    {
        TransactionScope _scope;
        SqlConnection _conn;

        public ScribeContext() : this(
            new SqlConnection(Config.Global.DbConfigProvider.ConnectionString),
            new TransactionScope(TransactionScopeOption.Required, Config.Global.DbConfigProvider.TransactionScopeTimeout))
        {

        }
        public ScribeContext(SqlConnection connection) : this(connection,
            new TransactionScope(TransactionScopeOption.Required, Config.Global.DbConfigProvider.TransactionScopeTimeout))
        {

        }
        public ScribeContext(TransactionScope scope) : this(
            new SqlConnection(Config.Global.DbConfigProvider.ConnectionString),
            scope)
        {

        }
        public ScribeContext(SqlConnection connection, TransactionScope scope)
        {
            _scope = scope;
            _conn = connection;
            if (_conn.State != System.Data.ConnectionState.Open)
            {
                _conn.Open();
            }
        }

        private SqlCommand NewCommand(string commandText, SqlTransaction t)
        {
            var cmd = new SqlCommand(commandText, _conn, t);
            cmd.CommandTimeout = Config.Global.DbConfigProvider.CommandTimeout;
            return cmd;
        }

        public SqlConnection Connection { get { return _conn; } }

        public virtual T Insert<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            List<T> l = new List<T>();
            l.Add(obj);
            return BulkInsert<T>(l).FirstOrDefault();
        }

        public virtual T Update<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            List<T> l = new List<T>();
            l.Add(obj);
            return BulkUpdate<T>(l).FirstOrDefault();
        }
        public virtual T Delete<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            List<T> l = new List<T>();
            l.Add(obj);
            return BulkDelete<T>(l).FirstOrDefault();
        }

        public virtual List<T> BulkUpdate<T>(List<T> dataSource)
        {
            FormulateMerge<T> merger = Config.Global.DefaultSqlProvider.GetMergeStatementForUpdate<T>;

            return BulkExecuteMerge<T>(dataSource, merger);
        }
        public virtual List<T> BulkDelete<T>(List<T> dataSource)
        {
            FormulateMerge<T> merger = Config.Global.DefaultSqlProvider.GetMergeStatementForDelete<T>;

            return BulkExecuteMerge<T>(dataSource, merger);
        }
        public virtual List<T> BulkInsert<T>(List<T> dataSource)
        {
            FormulateMerge<T> merger = Config.Global.DefaultSqlProvider.GetMergeStatementForInsert<T>;

            return BulkExecuteMerge<T>(dataSource, merger);
        }
        protected delegate String FormulateMerge<T>(List<ColumnMetadata> allColumns, TableAttribute source, TableAttribute destination, TableAttribute outputTable);

        protected virtual List<T> BulkExecuteMerge<T>(List<T> dataSource,
            FormulateMerge<T> mergeStatementEngine)
        {
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if (mergeStatementEngine == null) throw new ArgumentNullException("mergeStatementEngine");
            if (dataSource.Count == 0) throw new ArgumentException("dataSource cannot be empty");

            var columns = ColumnMetadata.GetColumnMetadata<T>().Where(x => !x.IsNotMapped).ToList();

            using (SqlTransaction t = _conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                Guid session = Guid.NewGuid();

                TableAttribute tempIn = new TableAttribute("temp" + session.ToString(), "dbo");
                TableAttribute tempOut = new TableAttribute("tempOut" + session.ToString(), "dbo");
                TableAttribute destination = TableAttribute.GetFor<T>();

                String tempInTableCreate = Config.Global.DefaultSqlProvider.FormulateCreateTableStatement<T>(tempIn, columns);
                String tempOutTableCreate = Config.Global.DefaultSqlProvider.FormulateCreateTableStatement<T>(tempOut, columns);

                var cmdTempInCreate = NewCommand(tempInTableCreate, t); //new SqlCommand(tempInTableCreate, _conn, t);
                var cmdTempOutCreate = NewCommand(tempOutTableCreate, t);//new SqlCommand(tempOutTableCreate, _conn, t);

                cmdTempInCreate.ExecuteNonQuery();
                cmdTempOutCreate.ExecuteNonQuery();

                SqlBulkCopy sbk = new SqlBulkCopy(_conn, SqlBulkCopyOptions.Default, t);
                sbk.BulkCopyTimeout = Config.Global.DbConfigProvider.SqlBulkCopyTimeoutInSeconds;
                sbk.DestinationTableName = String.Format("[{0}].[{1}]", tempIn.SchemaName, tempIn.TableName);
                foreach(var c in columns)
                {
                    sbk.ColumnMappings.Add(c.AssociatedProperty.Name, c.AssociatedProperty.Name);
                }


                sbk.WriteToServer(DataTableFactory.CreateTableFromList<T>(dataSource, columns));

                String mergeStatement = mergeStatementEngine(columns, tempIn, destination, tempOut);

                var cmdMerge = NewCommand(mergeStatement, t); //new SqlCommand(mergeStatement, _conn, t);
                cmdMerge.ExecuteNonQuery();

                var outputResults = Config.Global.DefaultSqlProvider.GetOutputResults<T>(tempOut, _conn);

                if (outputResults == null || outputResults.Count != dataSource.Count)
                {
                    throw new RecordCountMismatchException(@"Merge completed suucessfully, but output record counts do not match input.  
This could indicate an issue with the primary keys or records in database not matching on update. 
Check keys and input data for operation.", dataSource.Count, outputResults != null ? outputResults.Count : 0);
                }


                var cmdDropInput = new SqlCommand(Config.Global.DefaultSqlProvider.FormulateDropTableStatement(tempIn),
                    _conn, t);

                var cmdDropOutput = new SqlCommand(Config.Global.DefaultSqlProvider.FormulateDropTableStatement(tempOut),
                    _conn, t);

                cmdDropInput.ExecuteNonQuery();
                cmdDropOutput.ExecuteNonQuery();

                t.Commit();

                return outputResults;
            }


        }

        public void Commit()
        {
            _scope.Complete();
        }
        public void Dispose()
        {
            if (_conn.State == System.Data.ConnectionState.Open)
                _conn.Close();

            _conn.Dispose();
            _scope.Dispose();
        }
    }
}
