using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Attributes
{
    /// <summary>
    /// Marks property as means to determine row uniqueness in destination table.  Used to formulate merge logic.
    /// </summary>
    public class PrimaryKeyAttribute : Attribute
    {
    }
}
