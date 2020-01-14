using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace QM.Service
{
    public static class ConnectionFactory
    {

        private static ConnectionConfig _ConnectionConfig;

        public static void Init(Func<string, string> func)
        {
            _ConnectionConfig = new ConnectionConfig()
            {
                DbType = (DbType)Enum.Parse(typeof(DbType), func.Invoke("MyConfig:ConnectionStrings:DbType")),
                ConnectionString = func.Invoke("MyConfig:ConnectionStrings:DbConnectionString")
            };

        }
    
        public static OrmLiteConnectionFactory BuildDbConn()
        {
            #region  8.0的写法
            IOrmLiteDialectProvider ormLiteDialectProvider = _ConnectionConfig.DbType switch
            {
                DbType.MySQL => MySqlDialect.Provider,
                DbType.SqlServer => SqlServerDialect.Provider,
                _ => throw new NotImplementedException("not support dbtype")
            };
         

            #endregion
            //switch (_ConnectionConfig.DbType)
            //{
            //    case DbType.MySQL:
            //        ormLiteDialectProvider = MySqlDialect.Provider;
            //        break;
            //    case DbType.SqlServer:
            //        ormLiteDialectProvider = SqlServerDialect.Provider;
            //        break;
            //    default:
            //        throw new NotImplementedException("not support dbtype");
            //}
            return new OrmLiteConnectionFactory(_ConnectionConfig.ConnectionString, ormLiteDialectProvider)
            {
                AutoDisposeConnection = true
            };
        }
    }

    public class ConnectionConfig
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DbType DbType { get; set; }
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
    }

    public enum DbType
    {
        MySQL,
        SqlServer
    }
}
