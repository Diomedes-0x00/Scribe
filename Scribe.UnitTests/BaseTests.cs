using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Diagnostics;
using Scribe.MsSqlProvider;

namespace Scribe.UnitTests
{
    [TestClass]
    public class BaseTests
    {
        [TestInitialize]
        public void LoadDefaultProvider()
        {
            Config.Global.DefaultSqlProvider = new DefaultSqlProvider();
        }

        [TestMethod]
        public void TestDataTableConversion()
        {

            Employee test = new Employee()
            {
                Id = 5,
                DateHired = DateTime.Now,
                EmployeeCode = 'D',
                GlobalId = Guid.NewGuid(),
                IsEnabled = false,
                MaxTimeOffAllowed = new TimeSpan(7, 0, 0, 0),
                MilesPerHourWalkingSpeed = 3.4F,
                Name = "Brad",
                NumberOfCupcakesEaten = 662553223,
                OrgCode = "ASDF",
                Picture = new byte[] { 0x00000 },
                RecordLoadedTime = DateTime.Now,
                RoomNumber = 30000,
                SalaryPerHour = 7.25M,
                Weight = 230.345676

            };

            List<Employee> testList = new List<Employee>();
            testList.Add(test);

            List<ColumnMetadata> metadata = ColumnMetadata.GetColumnMetadata<Employee>().ToList();

            DataTable dt = Scribe.Conversion.DataTableFactory.CreateTableFromList(testList, metadata);

            Assert.IsTrue(dt.Rows.Count == testList.Count);
            Assert.IsTrue(dt.Columns.Count == metadata.Count);

        }

        [TestMethod]
        public void TestColumnMetadata()
        {
            List<ColumnMetadata> metadata = ColumnMetadata.GetColumnMetadata<Employee>().ToList();

            Assert.IsTrue(typeof(Employee).GetProperties().Count() == metadata.Count);
            Assert.IsTrue(metadata.Where(x => x.IsNotMapped).Count() == 1);
            Assert.IsTrue(metadata.Where(x => x.IsPrimaryKey && x.IsIdentity).Count() == 1);
            Assert.IsTrue(metadata.Where(x => x.HasConversion).Count() == 1);

        }

        [TestMethod]
        public void TestBasicSqlExpressionDataTypes()
        {
           
            List<ColumnMetadata> metadata = ColumnMetadata.GetColumnMetadata<Employee>().ToList();

            metadata.ForEach(x => Trace.WriteLine(String.Format("{0}\t{1}\tIs nullable:{2}", 
                x.AssociatedProperty.Name,x.SqlDataType, x.IsNullable)));
            


        }

    }
}