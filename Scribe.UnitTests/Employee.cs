using Scribe.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.UnitTests
{
    [Table("Employee", "dbo")]
    public class Employee
    {
        [PrimaryKey]
        [Identity]
        public int Id { get; set; }

        public String Name { get; set; }
        public double Weight { get; set; }
        public char EmployeeCode { get; set; }
        public Int16 RoomNumber { get; set; }
        public Int64 NumberOfCupcakesEaten { get; set; }
        public Decimal SalaryPerHour { get; set; }
        public Single MilesPerHourWalkingSpeed { get; set; }
        public bool IsEnabled { get; set; }
        public string OrgCode { get; set; }
        public Guid GlobalId { get; set; }
        public DateTime DateHired { get; set; }
        public TimeSpan MaxTimeOffAllowed { get; set; }
        public byte[] Picture { get; set; }

        public int? PhoneNumberId { get; set; }

        [NotMapped]
        public DateTime RecordLoadedTime { get; set; }
        [Conversion(SqlDbType.Xml)]
        public String XmlData { get; set; }


    }
}
