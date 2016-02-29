using Scribe.Attributes;
using Scribe.Conversion;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scribe
{
    public enum ConversionDirection
    {
        None = 0,
        Destination = 1,
        Original = 2
    }
    public class ColumnMetadata
    {

        private bool _isIdentity;
        private bool _isPrimaryKey;
        private bool _isForeignKey;
        private bool _hasConversion;
        private bool _isNotMapped;
        private bool _isNullable;

        private SqlDbType _sqlDataType;
        private SqlDbType _targetSqlDataType;

        public ColumnMetadata(PropertyInfo pi)
        {
            if (pi == null) throw new ArgumentNullException("pi");
            _isIdentity = HasAttribute<IdentityAttribute>(pi);
            _isPrimaryKey = HasAttribute<PrimaryKeyAttribute>(pi);
            _isForeignKey = HasAttribute<ForeignKeyAttribute>(pi);
            _hasConversion = HasAttribute<ConversionAttribute>(pi);
            if (_hasConversion)
                _targetSqlDataType = pi.GetCustomAttribute<ConversionAttribute>().Destination;
            _isNotMapped = HasAttribute<NotMappedAttribute>(pi);
            _sqlDataType = CLRConversion.GetTypeFor(pi.PropertyType);
            _isNullable = CLRConversion.IsNullable(pi.PropertyType);

            AssociatedProperty = pi;
        }



        public bool IsIdentity { get { return _isIdentity; } }
        public bool IsPrimaryKey { get { return _isPrimaryKey; } }
        public bool IsForeignKey { get { return _isForeignKey; } }
        public bool HasConversion { get { return _hasConversion; } }
        public bool IsNotMapped { get { return _isNotMapped; } }
        public bool IsNullable { get { return _isNullable; } }
        public SqlDbType SqlDataType { get { return _sqlDataType; } }
        public SqlDbType TargetDataType { get { return _targetSqlDataType; } }

        public PropertyInfo AssociatedProperty { get; private set; }

        public string GetColumnExpression(Func<string, string> outsideFormat, 
            ConversionDirection direction)
        {
            if (_hasConversion && direction != ConversionDirection.None)
            {
                SqlDbType target = TargetDataType;
                if (direction == ConversionDirection.Original)
                    target = SqlDataType;

                return Config.Global.DefaultSqlProvider.FormulateConversionExpression(
                    target,
                    outsideFormat(AssociatedProperty.Name));
            }
            return outsideFormat(AssociatedProperty.Name);
        }

        public bool HasAttribute<T>(PropertyInfo pi) where T : Attribute
        {
            return pi.GetCustomAttribute<T>() != null;
        }

       

        public static IEnumerable<ColumnMetadata> GetColumnMetadata<T>()
        {
            foreach (var pi in typeof(T).GetProperties())
            {
                yield return new ColumnMetadata(pi);
            }
        }
    }
}
