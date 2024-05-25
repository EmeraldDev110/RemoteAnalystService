using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;
using System.Linq;
using NHibernate.Linq;
using System.Configuration;
using RemoteAnalyst.Repository.Resources;

namespace RemoteAnalyst.Repository.Repositories
{
    public class DatabaseMappingRepository
    {
        private readonly bool _isLocalAnalyst = false;

        public DatabaseMappingRepository()
        {
            RAInfoRepository raInfo = new RAInfoRepository();
            string productName = raInfo.GetValue("ProductName");
            if (productName == "PMC")
            {
                _isLocalAnalyst = true;
            }
        }
        public string encryptPassword(string connectionString, string isLocalAnalyst = "false")
        {
            isLocalAnalyst = isLocalAnalyst.ToLower();
            if (isLocalAnalyst == "false")
                isLocalAnalyst = ConfigurationManager.AppSettings["isLocalAnalyst"];
            if (isLocalAnalyst == "true")
            {
                var encrypt = new Decrypt();
                var encryptedString = encrypt.strDESEncrypt(connectionString);
                return encryptedString;
            }
            else
            {
                return connectionString;
            }
        }

        public string decryptPassword(string connectionString)
        {
            if (_isLocalAnalyst)
            {
                var decrypt = new Decrypt();
                var decryptedString = decrypt.strDESDecrypt(connectionString);
                return decryptedString;
            }
            else
            {
                return connectionString;
            }
        }

        public string GetConnectionString(string systemSerial)
        {
            string connectionString = "";

            try
            {
                using (ISession session = NHibernateHelper.OpenSession("DatabaseMapping"))
                {
                    DatabaseMapping res = session
                        .CreateCriteria(typeof(DatabaseMapping))
                        .Add(Restrictions.Eq("SystemSerial", systemSerial))
                        .UniqueResult<DatabaseMapping>();
                    connectionString = res.ConnectionString;
                    if (!string.IsNullOrEmpty(connectionString) && _isLocalAnalyst)
                    {
                        connectionString = decryptPassword(connectionString);
                    }
                }
            }
            catch
            {
                connectionString = "";
            }
            return connectionString;
        }

        public ICollection<DatabaseMapping> GetAllConnectionStrings()
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSession("DatabaseMapping"))
                {

                    ICollection<DatabaseMapping> res = session
                        .CreateCriteria(typeof(DatabaseMapping))
                        .List<DatabaseMapping>();
                    foreach (DatabaseMapping row in res)
                    {
                        string connectionString = row.ConnectionString;
                        if (!string.IsNullOrEmpty(connectionString))
                        {
                            row.ConnectionString = decryptPassword(connectionString);
                        }
                        else
                        {
                            row.ConnectionString = connectionString;
                        }
                    }
                    //return CollectionHelper.ToDataTable(res, new List<string> { "SystemSerial", "ConnectionString" });
                    return res;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<DatabaseMapping>();
            }
        }

        public bool CheckDatabase(string connectionString)
        {
            string cmdText = "SELECT * FROM ZmsBladeDataDictionary LIMIT 1";
            bool exists = false;

            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(connectionString, "ZmsBladeDataDictionary"))
                {
                    var res = session
                        .CreateCriteria(typeof(ZmsBladeDataDictionary))
                        .List<ZmsBladeDataDictionary>();
                    if (res != null)
                    {
                        exists = true;
                    }
                }
            }
            catch (Exception ex)
            {
                exists = false;
            }

            return exists;
        }

        public string CheckConnection(string connectionString)
        {
            var isConnection = "";

            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(connectionString, "DatabaseMapping"))
                {
                    var res = session.Connection;
                    if (res.State == ConnectionState.Open) return "";
                }
            }
            catch (Exception ex)
            {
                isConnection = ex.Message;
            }

            return isConnection;
        }

        public void InsertNewEntry(string systemSerial, string perSystemConnectionString)
        {
            string cmdText = "INSERT INTO DatabaseMappings (SystemSerial, ConnectionString) VALUES " +
                             "(@SystemSerial, @ConnectionString)";
            DatabaseMapping databaseMapping = new DatabaseMapping();
            databaseMapping.SystemSerial = systemSerial;
            databaseMapping.ConnectionString = perSystemConnectionString;
        }

        public void UpdateConnectionString(string systemSerial, string connectionString, string isLocalAnalyst = "false")
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSession("DatabaseMapping"))
                using (ITransaction transaction = session.BeginTransaction())
                {
                    DatabaseMapping databaseMapping = new DatabaseMapping();
                    databaseMapping.SystemSerial = systemSerial;
                    connectionString = encryptPassword(connectionString, isLocalAnalyst);
                    databaseMapping.ConnectionString = connectionString;
                    session.Update(databaseMapping);
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetAllDatabaseConnection()
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                Models.System system = null;
                Company company = null;
                DatabaseMapping databaseMapping = null;
                ICollection<object[]> res = session.QueryOver(() => databaseMapping)
                    .Left.JoinQueryOver(() => databaseMapping.System, () => system)
                    .Left.JoinQueryOver(() => system.Company, () => company)
                    .Select(
                        _ => system.SystemName,
                        _ => system.SystemSerial,
                        _ => databaseMapping.ConnectionString,
                        _ => company.CompanyName
                    )
                    .List<object[]>();
                foreach (object[] row in res)
                {
                    string connectionString = row[2] as string;
                    if (!string.IsNullOrEmpty(connectionString))
                        row[2] = decryptPassword(connectionString);
                }
                return CollectionHelper.ListToDataTable(res, new List<string>() { "SystemName", "SystemSerial", "ConnectionString", "CompanyName" });
            }
        }

        public string GetRdsConnectionString(string rdsname)
        {
            //string connectionstring = config.connectionstring;
            // added collate latin1_general_cs to do case sensitive search
            string cmdtext = "select connectionstring from databasemappings where connectionstring collate latin1_general_cs like '%" + rdsname + "%' limit 1";
            string connectionstring = "";

            //try
            //{
            //    using (var connection = new mysqlconnection(_connectionstring))
            //    {
            //        var command = new mysqlcommand(cmdtext + helper.commandparameter, connection);
            //        command.commandtimeout = 0;
            //        connection.open();
            //        var reader = command.executereader();

            //        if (reader.read())
            //        {
            //            connectionstring = decryptpassword(convert.tostring(reader["connectionstring"]));
            //        }
            //    }
            //catch
            //{
            //    connectionstring = "";
            //}

            return connectionstring;
        }
    }
}
