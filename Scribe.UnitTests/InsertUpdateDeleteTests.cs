using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scribe.Interop;
using Scribe.MsSqlProvider;
using Scribe.Attributes;
using System.Linq;
using System.Data.SqlClient;
using Dapper;
using System.Collections.Generic;

namespace Scribe.UnitTests
{
    [TestClass]
    public class InsertUpdateDeleteTests
    {
        ISqlProvider provider;

        [TestInitialize]
        public void BootstrapDb()
        {
            Config.Global.DbConfigProvider = new TestDbConfigProvider();
            Config.Global.DefaultSqlProvider = new DefaultSqlProvider();

            provider = Config.Global.DefaultSqlProvider;

            using (ScribeContext con = new ScribeContext())
            {
                var tableAttribute = TableAttribute.GetFor<Employee>();
                var meta = ColumnMetadata.GetColumnMetadata<Employee>().Where(x => !x.IsNotMapped).ToList();

                bool employeeTableExists =
                    con.Connection.Query<int>("SELECT COUNT(*) FROM sys.tables where name like 'Employee'").First() == 1;

                if (!employeeTableExists)
                {
                    var employeeTable = provider.FormulateCreateTableStatement<Employee>(tableAttribute,
                    meta
                    );

                    con.Connection.Execute(employeeTable);
                    string alterStatement = @"
ALTER TABLE [dbo].[Employee]
DROP COLUMN XmlData;

ALTER TABLE [dbo].[Employee]
	ADD [XmlData] XML NOT NULL;

ALTER TABLE [dbo].[Employee]
	DROP COLUMN [Id];

ALTER TABLE [dbo].[Employee]
	ADD [Id] INT NOT NULL IDENTITY(1,1);

ALTER TABLE [dbo].[Employee]
	ADD CONSTRAINT PK_Employee PRIMARY KEY ([Id]);";

                    con.Connection.Execute(alterStatement);

                }
                string truncateStatement = @"
TRUNCATE TABLE [dbo].[Employee]
";

                con.Connection.Execute(truncateStatement);



                con.Commit();
            }


        }
        [TestMethod]
        public void TestMassiveInsert()
        {
            using (ScribeContext con = new ScribeContext())
            {
                List<Employee> employees = new List<Employee>();
                for(int x = 0; x < 100000; x++)
                {
                    Employee e = new Employee()
                    {
                        DateHired = DateTime.Now,
                        EmployeeCode = 'D',
                        GlobalId = Guid.NewGuid(),
                        IsEnabled = true,
                        MaxTimeOffAllowed = new TimeSpan(12, 0, 0),
                        MilesPerHourWalkingSpeed = 3.4F,
                        Name = "BRADLY",
                        NumberOfCupcakesEaten = 566554,
                        OrgCode = "ABCD",
                        Picture = new byte[] { 0x0000 },
                        RoomNumber = 305,
                        SalaryPerHour = 30.6M,
                        Weight = 225.5565,
                        XmlData = "<r></r>"

                    };
                    employees.Add(e);
                }

                var eOut = con.BulkInsert(employees);
                Assert.IsTrue(!eOut.Any(x => x.Id == 0));
                con.Commit();
            }

        }

        [TestMethod]
        public void TestInsert()
        {
            using (ScribeContext con = new ScribeContext())
            {
                Employee e = new Employee()
                {
                    DateHired = DateTime.Now,
                    EmployeeCode = 'D',
                    GlobalId = Guid.NewGuid(),
                    IsEnabled = true,
                    MaxTimeOffAllowed = new TimeSpan( 12, 0, 0),
                    MilesPerHourWalkingSpeed = 3.4F,
                    Name = "BRADLY",
                    NumberOfCupcakesEaten = 566554,
                    OrgCode = "ABCD",
                    Picture = new byte[] { 0x0000 },
                    RoomNumber = 305,
                    SalaryPerHour = 30.6M,
                    Weight = 225.5565,
                    XmlData = "<r></r>"

                };
                var eOut = con.Insert(e);
                Assert.IsTrue(eOut.Id != e.Id);
                con.Commit();
            }
        }

        [TestMethod]
        public void TestUpdate()
        {
            TestInsert();
            using (ScribeContext con = new ScribeContext())
            {
                var e = con.Connection.Query<Employee>("SELECT * FROM [dbo].[Employee]", new { }).First();
                e.MilesPerHourWalkingSpeed = 5.6F;
                var eOut = con.Update(e);

                Assert.IsTrue(eOut.MilesPerHourWalkingSpeed == e.MilesPerHourWalkingSpeed);
                con.Commit();
            }
        }

        [TestMethod]
        public void TestDelete()
        {
            TestInsert();
            using (ScribeContext con = new ScribeContext())
            {
                var e = con.Connection.Query<Employee>("SELECT * FROM [dbo].[Employee]", new { }).First();
               
                var eOut = con.Delete(e);

                var records = con.Connection.Query<int>("SELECT COUNT(*) FROM [dbo].[Employee] where Id = @Id",
                    new {Id = e.Id }).First();

                Assert.IsTrue(records == 0);
                con.Commit();
            }
        }




        private class TestDbConfigProvider : IScribeDatabaseConfigurationProvider
        {
            public int CommandTimeout
            {
                get
                {
                    return 0;
                }
            }

            public string ConnectionString
            {
                get
                {
                    return "Data Source=localhost;Initial Catalog=ScribeTest;Integrated Security=True;";
                }
            }

            public int SqlBulkCopyTimeoutInSeconds
            {
                get
                {
                    return 0;
                }
            }

            public TimeSpan TransactionScopeTimeout
            {
                get
                {
                    return new TimeSpan(1, 0, 0);
                }
            }
        }
    }




}
