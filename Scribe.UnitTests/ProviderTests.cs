using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Attributes;
using Scribe.MsSqlProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.UnitTests
{
    [TestClass]
    public class ProviderTests
    {
        ISqlProvider _provider;

        [TestInitialize]
        public void LoadProvider()
        {
            Config.Global.DefaultSqlProvider = new DefaultSqlProvider();
            _provider = Config.Global.DefaultSqlProvider;
        }
        [TestMethod]
        public void TestCreateTable()
        {
            TableAttribute ta = new TableAttribute("Employee", "dbo");
             
         
            var meta = ColumnMetadata.GetColumnMetadata<Employee>().ToList();
            var testTable = _provider.FormulateCreateTableStatement<Employee>(ta, meta);

            Trace.WriteLine(testTable);

        }
        [TestMethod]
        public void TestUpdateMergeStatement()
        {
            TableAttribute ta = new TableAttribute("Employee", "dbo");

          


            var meta = ColumnMetadata.GetColumnMetadata<Employee>().ToList();


            var testTable = _provider.GetMergeStatementForUpdate<Employee>(meta,
                ta, ta, ta);

            Trace.WriteLine(testTable);

        }
        [TestMethod]
        public void TestInsertMergeStatement()
        {
            TableAttribute ta = new TableAttribute("Employee", "dbo");

           


            var meta = ColumnMetadata.GetColumnMetadata<Employee>().ToList();


            var testTable = _provider.GetMergeStatementForInsert<Employee>(meta,
                ta, ta, ta);

            Trace.WriteLine(testTable);

        }
        [TestMethod]
        public void TestDeleteMergeStatement()
        {
            TableAttribute input = new TableAttribute("EmployeeIn", "dbo");
            TableAttribute destination = new TableAttribute("Employee", "dbo");



            var meta = ColumnMetadata.GetColumnMetadata<Employee>().ToList();


            var testTable = _provider.GetMergeStatementForDelete<Employee>(meta,
                input, destination, input);

            Trace.WriteLine(testTable);

        }

    }
}
