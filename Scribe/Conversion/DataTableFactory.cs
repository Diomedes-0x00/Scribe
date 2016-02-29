using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Conversion
{
   public static class DataTableFactory
    {
        public static DataTable CreateTableFromList<T>(List<T> dataSource, List<ColumnMetadata> columnMetaData)
        {
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if(columnMetaData == null) throw new ArgumentNullException("columnMetaData");

            if (columnMetaData.Count == 0) throw new ArgumentException("columnMetaData must contain at least one entry");
            if (dataSource.Count == 0) throw new ArgumentException("dataSource must contain at least 1 element");

            DataTable dt = new DataTable();
            
            foreach(var cm in columnMetaData)
            {
                dt.Columns.Add(new DataColumn(cm.AssociatedProperty.Name, 
                    cm.IsNullable ? Nullable.GetUnderlyingType(cm.AssociatedProperty.PropertyType) :
                    cm.AssociatedProperty.PropertyType));
            }

            foreach(var r in dataSource)
            {
                var row = dt.NewRow();
                foreach(var cm in columnMetaData)
                {
                    row[cm.AssociatedProperty.Name] = cm.AssociatedProperty.GetValue(r) ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }
            return dt;

        }
    }
}
