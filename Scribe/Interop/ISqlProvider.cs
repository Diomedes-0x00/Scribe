using Scribe.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe
{
    public interface ISqlProvider
    {
        bool CanConvert(SqlDbType dataType);
        string GetSqlDataTypeExpression(SqlDbType dataType);

        string GetMergeStatementForInsert<T>(List<ColumnMetadata> allColumns, TableAttribute source, TableAttribute destination, TableAttribute outputTable );
        string GetMergeStatementForDelete<T>(List<ColumnMetadata> allColumns, TableAttribute source, TableAttribute destination, TableAttribute outputTable);
        string GetMergeStatementForUpdate<T>(List<ColumnMetadata> allColumns, TableAttribute source, TableAttribute destination, TableAttribute outputTable);

        List<T> GetOutputResults<T>(TableAttribute outputTable, SqlConnection conn);

        string FormulateCreateTableStatement<T>(TableAttribute destinationTable, List<ColumnMetadata> columns);
        string FormulateDropTableStatement(TableAttribute targetTable);

        string FormulateConversionExpression(SqlDbType targetType, string formatedColumnName);

    }
}
