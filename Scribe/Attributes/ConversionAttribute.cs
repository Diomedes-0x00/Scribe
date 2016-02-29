using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Attributes
{
    /// <summary>
    /// Flags property for conversion at time of merge.  Example: String .Net CLR type to SqlDbType of XML.
    /// Improper conversions will cause exceptions at runtime on merge operations
    /// </summary>
    public class ConversionAttribute : Attribute
    {
        /// <summary>
        /// The destination column's data type to convert to.  Example: XML
        /// </summary>
        public SqlDbType Destination { get; private set; }

        /// <summary>
        /// Default attribute constructor.  The destination column's data type to convert to.  Example: XML
        /// </summary>
        /// <param name="destinationType">Required. Type to convert to on merge</param>
        public ConversionAttribute(SqlDbType destinationType)
        {
            if (!Config.Global.DefaultSqlProvider.CanConvert(destinationType))
                throw new NotSupportedException(String.Format("Conversion to type {0} is not supported with the provider", destinationType));

            Destination = destinationType;
        }
    }
}
