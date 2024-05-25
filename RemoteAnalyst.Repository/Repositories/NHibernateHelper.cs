using NHibernate;
using NHibernate.Cfg;
using System;
using NHibernate.XFactories;
using System.Reflection;
using System.Linq;
using NHibernate.Util;
using RemoteAnalyst.Repository.Interceptors;
using System.Configuration;
using NHibernate.Tool.hbm2ddl;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;

namespace RemoteAnalyst.Repository.Repositories
{
    public class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;
        private static NHibernate.Cfg.Configuration configuration;
        private static string _className;
        private static string _connectionString;
        private static string _mainConnectionString;
        private static string _dialect = "MySql";
        private const string CurrentSessionKey = "nhibernate.current_session";
        private static Dictionary<string, Type> typeMapping = new Dictionary<string, Type>()
        {
            {"CPUEntity", typeof(CPUEntity) },
            {"ProcessEntity", typeof(ProcessEntity) },
            {"FileEntity", typeof(FileEntity) },
            {"DISCEntity", typeof(DISCEntity) },
            { "TrendApplicationInterval", typeof(TrendApplicationInterval) },
            { "TrendApplicationHourly", typeof(TrendApplicationHourly) },
            { "TrendCpuInterval", typeof(TrendCpuInterval) },
            { "TrendCpuHourly", typeof(TrendCpuHourly) },
            { "TrendDiskInterval", typeof(TrendDiskInterval) },
            { "TrendDiskHourly", typeof(TrendDiskHourly) },
            { "TrendExpandInterval", typeof(TrendExpandInterval) },
            { "TrendExpandHourly", typeof(TrendExpandHourly) },
            { "TrendHiLo", typeof(TrendHiLo) },
            { "TrendIpuInterval", typeof(TrendIpuInterval) },
            { "TrendIpuHourly", typeof(TrendIpuHourly) },
            { "TrendPathwayHourly", typeof(TrendPathwayHourly) },
            { "TrendProcessInterval", typeof(TrendProcessInterval) },
            { "TrendProcessHourly", typeof(TrendProcessHourly) },
            { "TrendProgramInterval", typeof(TrendProgramInterval) },
            { "TrendProgramHourly", typeof(TrendProgramHourly) },
            { "TrendTCPProcessInterval", typeof(TrendTCPProcessInterval) },
            { "TrendTCPProcessHourly", typeof(TrendTCPProcessHourly) },
            { "TrendTCPSubnetInterval", typeof(TrendTCPSubnetInterval) },
            { "TrendTCPSubnetHourly", typeof(TrendTCPSubnetHourly) },
            { "TrendTmfInterval", typeof(TrendTmfInterval) },
            { "TrendTmfHourly", typeof(TrendTmfHourly) },
            { "TrendWalkthrough", typeof(TrendWalkthrough) }
        };
        public static string ClassName
        {
            get { return _className; }

        }
        /*public static ISessionFactory CreateSessionFactoryForDate(DateTime date)
        {
            var configuration = new Configuration();
            configuration.Configure(); // Assuming your NHibernate.cfg.xml is set up

            // Dynamically set the table mapping
            configuration.ClassMappings.FirstOrDefault(m => m.MappedClass == typeof(TcpPacketDetail)).Table.Name = $"dbo.qnm_tcppacketsdetail_{date:yyyy_M_dd}";

            return configuration.BuildSessionFactory();
        }*/
        private static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    configuration = new NHibernate.Cfg.Configuration();
#if (!DEBUG)
                    configuration.Configure("../../Hibernate.cfg/nhibernate.cfg.xml", "MySql");
#else
                    //configuration.Configure("../../../RemoteAnalyst.Repository/Hibernate.cfg/nhibernate.cfg.xml", _dialect);
                    configuration.Configure("RemoteAnalyst.Repository/Hibernate.cfg/nhibernate.cfg.xml", _dialect);
#endif
                    // if(_className!="")
                    // configuration.AddAssembly(Type.GetType(_className).Assembly);
                    configuration.SetInterceptor(new DynamicTableInterceptor());
                    var temp = Assembly.GetExecutingAssembly();
                    configuration.AddAssembly(temp);

                    _mainConnectionString = configuration.Properties["connection.connection_string"];
                    if (_connectionString == null)
                    {
                        _connectionString = _mainConnectionString;
                    }
                    else
                    {
                        configuration.SetProperty("connection.connection_string", _connectionString); // Alter the property
                    }

                    _sessionFactory = configuration.BuildSessionFactory();
                    //new SchemaExport(configuration).Execute(false, true, false);
                }
                return _sessionFactory;
            }
        }
        public static ISession GetCurrentSession()
        {
            if (SessionFactory == null)
                NHibernateHelper.OpenSession(_connectionString);

            return SessionFactory.OpenSession();
        }
        public static void BuildSessionFactoryForPartioned(string className, string tableName, DateTime? dateTime = null, string systemSerial = null)
        {
            // Dynamically set the table mapping
            var session = SessionFactory;
            if (dateTime == null)
            {
                configuration.ClassMappings.FirstOrDefault(m => m.MappedClass.Name.Equals(className, StringComparison.OrdinalIgnoreCase)).Table.Name = tableName;
            }
            else
            {
                if (systemSerial == null)
                {
                    configuration.ClassMappings.FirstOrDefault(m => m.MappedClass.Name.Equals(className, StringComparison.OrdinalIgnoreCase)).Table.Name = $"{tableName}_{dateTime?.Year}_{dateTime?.Month}_{dateTime?.Day}";
                }
                else
                {
                    configuration.ClassMappings.FirstOrDefault(m => m.MappedClass.Name.Equals(className, StringComparison.OrdinalIgnoreCase)).Table.Name = $"{systemSerial}_{tableName}_{dateTime?.Year}_{dateTime?.Month}_{dateTime?.Day}";
                }
            }

            _sessionFactory = configuration.BuildSessionFactory();
        }

        public static ISession OpenSession(string className = "")
        {
            if (_mainConnectionString != null)
            {
                _connectionString = _mainConnectionString;
                if (configuration != null)
                {
                    configuration.SetProperty("connection.connection_string", _mainConnectionString); // Alter the property
                    _sessionFactory = configuration.BuildSessionFactory(); // Get a new ISessionFactory
                }
            }
            _className = className ?? string.Empty;
            return SessionFactory.OpenSession();
        }
        public static ISession OpenSessionCustom(string connectionString, string className = "")
        {
            _connectionString = connectionString;
            if (configuration != null)
            {
                configuration.SetProperty("connection.connection_string", connectionString); // Alter the property
                _sessionFactory = configuration.BuildSessionFactory(); // Get a new ISessionFactory
            }
            _className = className ?? string.Empty;
            return SessionFactory.OpenSession();
        }
        public static ISession OpenSessionForPartioned(string className, string tableName, string connectionString = null, DateTime? dateTime = null, string systemSerial = null)
        {
            BuildSessionFactoryForPartioned(className, tableName, dateTime, systemSerial);
            if (connectionString == null)
                return OpenSession(connectionString);
            else
                return OpenSessionCustom(connectionString);
        }

        /*public static ISession GetCurrentSession()
        {
            var context = HttpContext.Current;
            var currentSession = context.Items[CurrentSessionKey] as ISession;

            if (currentSession == null)
            {
                currentSession = _sessionFactory.OpenSession();
                context.Items[CurrentSessionKey] = currentSession;
            }

            return currentSession;
        }*/
        /*public static void CloseSession()
        {
            var context = HttpContext.Current;
            var currentSession = context.Items[CurrentSessionKey] as ISession;

            if (currentSession == null)
            {
                 No current session
                return;
            }

            currentSession.Close();
            context.Items.Remove(CurrentSessionKey);
        }*/
        public static void CloseSessionFactory()
        {
            if (_sessionFactory != null)
            {
                _sessionFactory.Close();
            }
        }
        public static string GetConnectionString()
        {
            return _connectionString;
        }
        public static string GetDBDialect()
        {
            return _dialect;
        }
        public static Type GetType(string name)
        {
            Type type;
            typeMapping.TryGetValue(name, out type);
            return type;
        }

        //public static bool CheckTableExists(string tableName, string connectionString = null)
        //{
        //    using (ISession session = (connectionString == null ? OpenSession() : OpenSessionCustom(connectionString)))
        //    {
        //        DatabaseMetadata meta = new DatabaseMetadata(session.Connection, new NHibernate.Dialect.MySQLDialect());
        //        return meta.IsTable(tableName);
        //    }
        //}

        //        public static void CreateTables(string connectionString, Assembly assembly)
        //        {
        //            var cfg = new NHibernate.Cfg.Configuration();
        //#if (!DEBUG)
        //            cfg.Configure("../../Hibernate.cfg/nhibernate.cfg.xml", "MySql");
        //#else
        //            cfg.Configure("../../../RemoteAnalyst.Repository/Hibernate.cfg/nhibernate.cfg.xml", _dialect);
        //            //cfg.Configure("RemoteAnalyst.Repository/Hibernate.cfg/nhibernate.cfg.xml", _dialect);
        //#endif
        //            cfg.SetProperty("connection.connection_string", connectionString); // Alter the property
        //            cfg.AddAssembly(typeof(BatchSequenceProfile).Assembly);
        //            new SchemaExport(cfg).Execute(false, true, false);
        //        }
    }
}
