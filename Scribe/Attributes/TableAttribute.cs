using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Attributes
{
    /// <summary>
    /// Specifies the destination table and schema for the object.  Required to formulate merge logic
    /// </summary>
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// Target table name without any formatting applied.
        /// </summary>
        public String TableName { get; private set; }
        /// <summary>
        /// Target Schema name without any formatting applied.
        /// </summary>
        public String SchemaName { get; private set; }

        /// <summary>
        /// Default constructor.  Table name and Schema name are required
        /// </summary>
        /// <param name="tableName">Table name without formatting</param>
        /// <param name="schemaName">Table name without formatting</param>
        public TableAttribute(string tableName, string schemaName)
        {
            TableName = tableName;
            SchemaName = schemaName;
        }
        
        /// <summary>
        /// Gets the table attribute for the given type
        /// </summary>
        /// <typeparam name="T">Type of object to retrieve attribute from</typeparam>
        /// <returns>The attribute instance</returns>
        public static TableAttribute GetFor<T>()
        {
            return typeof(T).GetCustomAttributes(false).OfType<TableAttribute>().FirstOrDefault();
        }
    }
}
