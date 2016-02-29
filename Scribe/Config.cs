using Scribe.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scribe
{
    public class Config
    {
        public static Config Global { get; private set; }
        static Config()
        {
            Global = new Config();
        }
        private Config()
        {

        }

        private IScribeDatabaseConfigurationProvider _dbConfigProvider;
        private ISqlProvider _defaultSqlProvider;

        public ISqlProvider DefaultSqlProvider
        {
            get {
                if (_defaultSqlProvider == null)
                    throw new InvalidOperationException("Cannot return default sql provider because no provider has been set for the Config global singleton (Config.Global)");
                return _defaultSqlProvider; }
            set { _defaultSqlProvider = value; }
        }

        public IScribeDatabaseConfigurationProvider DbConfigProvider
        {
            get {
                if(_dbConfigProvider == null)
                    throw new InvalidOperationException("No IScribeDatabaseConfigurationProvider has been set for the Config global singleton (Config.Global).  Default constructors of scribe context require this property to be set. Set a database configuration provider or provide sqlconnection to scribe context");
                return _dbConfigProvider; }
            set { _dbConfigProvider = value; }
        }


    }
}
