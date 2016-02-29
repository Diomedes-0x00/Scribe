using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Scribe
{
    public class RecordCountMismatchException : Exception, ISerializable
    {

        public int ExpectedRecordCount { get; private set; }
        public int ActualRecordCount { get; private set; }
        public RecordCountMismatchException()
        {
        }

        public RecordCountMismatchException(string message, int expectedCount, int actualCount) : base(message)
        {
            ExpectedRecordCount = expectedCount;
            ActualRecordCount = actualCount;
        }

        public RecordCountMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RecordCountMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
