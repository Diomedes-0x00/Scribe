using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Conversion
{
    /// <summary>
    /// Helper class used to map .Net CLR types to SqlDbTypes
    /// </summary>
    internal class CLRConversion
    {
        /// <summary>
        /// Default map as a dictionary to lookup supported SqlDbTypes
        /// </summary>
        private static readonly Dictionary<Type, SqlDbType> DefaultMap =
            new Dictionary<Type, SqlDbType>()
            {
                { typeof(Int16), SqlDbType.SmallInt },
                { typeof(Int32), SqlDbType.Int },
                { typeof(Int64), SqlDbType.BigInt },
                { typeof(Decimal), SqlDbType.Decimal },
                {typeof(Single), SqlDbType.Real },
                {typeof(double),SqlDbType.Float },
                {typeof(Boolean), SqlDbType.Bit },
                {typeof(String), SqlDbType.VarChar },
                {typeof(char), SqlDbType.NVarChar },
                {typeof(Guid), SqlDbType.UniqueIdentifier },
                {typeof(DateTime), SqlDbType.DateTime2 },
                {typeof(TimeSpan), SqlDbType.Time },
                {typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
                {typeof(byte), SqlDbType.TinyInt },
                {typeof(byte[]), SqlDbType.VarBinary }
            };


       
        public static SqlDbType GetTypeFor(Type target)
        {
            if (IsNullable(target))
                return GetTypeFor(target.GetGenericArguments().First());

            if (DefaultMap.ContainsKey(target))
                return DefaultMap[target];
            throw new NotSupportedException(
                String.Format("Target type of {0} is not currently supported for conversion to DbType", target.FullName));

        }

        public static bool IsNullable(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition()
                == typeof(Nullable<>);
        }

        //public static String GetSqlDataExpressionFor(Type t)
        //{
        //    if (IsNullable(t))
        //        return GetSqlDataExpressionFor(t.GetGenericArguments().First());

        //    if (DefaultMap.ContainsKey(t))
        //        return SqlDataExpressionMap[t];
        //    throw new NotSupportedException(
        //        String.Format("Target type of {0} is not currently supported for conversion to SqlDataExpression", t.FullName));

        //}

    }
}
