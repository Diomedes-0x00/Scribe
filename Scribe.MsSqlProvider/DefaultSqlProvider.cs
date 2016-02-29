using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scribe.Attributes;
using Dapper;

namespace Scribe.MsSqlProvider
{
    public class DefaultSqlProvider : ISqlProvider
    {
        public static readonly Dictionary<SqlDbType, String> DefaultMap =
            new Dictionary<SqlDbType, String>()
            {
                { SqlDbType.TinyInt, "TINYINT" },
                {SqlDbType.SmallInt, "SMALLINT" },
                {  SqlDbType.Int , "INT"},
                {  SqlDbType.BigInt, "BIGINT" },
                {  SqlDbType.Decimal, "DECIMAL(29,4)" },
                { SqlDbType.Real, "REAL" },
                {SqlDbType.Float, "FLOAT(53)" },
                { SqlDbType.Bit, "BIT" },
                {SqlDbType.VarChar, "VARCHAR(MAX)" },
                { SqlDbType.NVarChar, "NVARCHAR(MAX)" },
                { SqlDbType.UniqueIdentifier, "UNIQUEIDENTIFIER" },
                {SqlDbType.DateTime2, "DATETIME2" },
                { SqlDbType.Time , "TIME" },
                { SqlDbType.DateTimeOffset , "DATETIMEOFFSET" },
                { SqlDbType.VarBinary, "VARBINARY(MAX)" },
                {SqlDbType.Xml, "XML" }
            };

        public bool CanConvert(SqlDbType dataType)
        {
            return DefaultMap.ContainsKey(dataType);
        }

        public string FormulateConversionExpression(SqlDbType targetType, string formatedColumnName)
        {
            return String.Format("CONVERT({0}, {1})", GetSqlDataTypeExpression(targetType), formatedColumnName);
        }

        public string FormulateCreateTableStatement<T>(TableAttribute destinationTable, List<ColumnMetadata> columns)
        {
            String TableCreateBlueprint = @"
CREATE TABLE [{0}].[{1}]  (
{2}
)
";
            var columnExpressions = columns.Select(
                x =>
                String.Format("[{0}] {1} {2}", x.AssociatedProperty.Name, GetSqlDataTypeExpression(x.SqlDataType),
                x.IsNullable ? "NULL" : "NOT NULL"
                )).ToArray();

            return String.Format(TableCreateBlueprint, destinationTable.SchemaName, destinationTable.TableName,
                String.Join(", ", columnExpressions));
        }

        public string FormulateDropTableStatement(TableAttribute targetTable)
        {
            return String.Format("DROP TABLE [{0}].[{1}]", targetTable.SchemaName, targetTable.TableName);
        }

        public string GetMergeStatementForDelete<T>(List<ColumnMetadata> allColumns, TableAttribute source, TableAttribute destination, TableAttribute outputTable)
        {
            String template = @"
MERGE INTO [{0}].[{1}] AS T
USING (
	SELECT * FROM [{2}].[{3}]
) AS S
ON {4}
WHEN MATCHED 
THEN DELETE

OUTPUT {5} INTO [{6}].[{7}];";

            var matches = allColumns.Where(x => x.IsPrimaryKey).Select(x =>
            String.Format("T.{0} = S.{0}", x.GetColumnExpression(z => String.Format("[{0}]", z), ConversionDirection.None), x.GetColumnExpression(z => String.Format("[{0}]", z), ConversionDirection.None))).ToArray();

            var outputColumns = allColumns.Select(x => x.GetColumnExpression(z => String.Format("DELETED.[{0}]", z), ConversionDirection.Original)
         ).ToArray();

            return String.Format(template,

                destination.SchemaName, destination.TableName,
                 source.SchemaName, source.TableName,
                String.Join(" AND ", matches),
                               String.Join(", ", outputColumns),
                outputTable.SchemaName, outputTable.TableName
                );
        }

        public string GetMergeStatementForInsert<T>(List<ColumnMetadata> allColumns, TableAttribute source, TableAttribute destination, TableAttribute outputTable)
        {
            String template = @"
MERGE INTO [{0}].[{1}] AS T
USING (
	SELECT * FROM [{2}].[{3}]
) AS S
ON {4}
WHEN NOT MATCHED 
THEN INSERT ({5})
VALUES ({6})
OUTPUT {7} INTO [{8}].[{9}];";

            Func<String, String> defaultFormat = z => String.Format("[{0}]", z);

            var matches = allColumns.Where(x => x.IsPrimaryKey).Select(x =>
            String.Format("T.{0} = S.{0}",
                    x.GetColumnExpression(defaultFormat, ConversionDirection.None),
                    x.GetColumnExpression(defaultFormat, ConversionDirection.None))).ToArray();


            var columnSources = allColumns.Where(x => !x.IsPrimaryKey).Select(x => x.GetColumnExpression(defaultFormat, ConversionDirection.None)
            ).ToArray();

            var columnValues = allColumns.Where(x => !x.IsPrimaryKey).Select(x => x.GetColumnExpression(
                z => String.Format("S.[{0}]", z), ConversionDirection.Destination)
           ).ToArray();

            var outputColumns = allColumns.Select(x => x.GetColumnExpression(
                 z => String.Format("INSERTED.[{0}]", z), ConversionDirection.Original)
                 ).ToArray();

            return String.Format(template,
                destination.SchemaName, destination.TableName,
                source.SchemaName, source.TableName,
                String.Join(" AND ", matches),
                String.Join(", ", columnSources),
                String.Join(", ", columnValues),
                String.Join(", ", outputColumns),
                outputTable.SchemaName, outputTable.TableName
                );
        }

        public string GetMergeStatementForUpdate<T>(List<ColumnMetadata> allColumns, TableAttribute source, TableAttribute destination, TableAttribute outputTable)
        {
            String template = @"
MERGE INTO [{0}].[{1}] AS T
USING (
	SELECT * FROM [{2}].[{3}]
) AS S
ON {4}
WHEN MATCHED 
THEN UPDATE 
SET {5}
OUTPUT {6} INTO [{7}].[{8}];";

            Func<string, string> defaultFormat = z => String.Format("[{0}]", z);

            var matches = allColumns.Where(x => x.IsPrimaryKey).Select(x =>
            String.Format("T.{0} = S.{0}",
                    x.GetColumnExpression(defaultFormat, ConversionDirection.None),
                    x.GetColumnExpression(defaultFormat, ConversionDirection.None))).ToArray();

            var columnUpdates = allColumns.Where(x => !x.IsPrimaryKey).Select(x =>
            String.Format("T.{0} = {1}",
                    x.GetColumnExpression(defaultFormat, ConversionDirection.None),
                    x.GetColumnExpression(z => String.Format("S.[{0}]", z), ConversionDirection.Destination))).ToArray();

            var outputColumns = allColumns.Select(x => x.GetColumnExpression(z => String.Format("INSERTED.[{0}]", z), ConversionDirection.Original)
                 ).ToArray();

            return String.Format(template,
                destination.SchemaName, destination.TableName,
                source.SchemaName, source.TableName,
                
                String.Join(" AND ", matches),
                String.Join(", ", columnUpdates),
                String.Join(", ", outputColumns),
                outputTable.SchemaName, outputTable.TableName
                );


        }

        public List<T> GetOutputResults<T>(TableAttribute outputTable, SqlConnection conn)
        {
            return conn.Query<T>(String.Format("SELECT * FROM [{0}].[{1}]", outputTable.SchemaName, outputTable.TableName), new { }).ToList();
        }

        public string GetSqlDataTypeExpression(SqlDbType dataType)
        {
            if (!DefaultMap.ContainsKey(dataType))
            {
                throw new NotSupportedException(String.Format("SqlDataType is not yet supported {0}", dataType.ToString()));
            }
            return DefaultMap[dataType];
        }
    }
}
