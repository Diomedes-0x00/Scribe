using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Interop
{
    public interface IScribeDatabaseConfigurationProvider
    {
        string ConnectionString { get; }
        TimeSpan TransactionScopeTimeout { get; }
        int SqlBulkCopyTimeoutInSeconds { get; }
        int CommandTimeout { get; }
    }
}
