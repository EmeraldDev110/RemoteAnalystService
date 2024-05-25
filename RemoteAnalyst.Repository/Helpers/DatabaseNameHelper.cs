using NHibernate;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Linq;

namespace RemoteAnalyst.Repository.Helpers
{
    public static class DatabaseNameHelper
    {
       public static string FindKeyName(string connStr)
        {
            string databaseName = "";
            string[] tempNames = connStr.Split(';');
            foreach (string s in tempNames)
            {
                if (s.ToUpper().Contains("DATABASE"))
                {
                    databaseName = s.Split('=')[1];
                }
            }
            return databaseName;
        }
        public static bool CheckTable(string databaseName, string tableName)
        {
            try
            {
                bool dataExists = false;
                string cmdText = @"SELECT COUNT(*) AS CNT
                                    FROM information_schema.tables 
                                    WHERE table_schema = :DatabaseName 
                                    AND table_name = :TableName";
                using (ISession session = NHibernateHelper.OpenSession())
                {
                    var qNMAbout = session
                        .CreateQuery(cmdText)
                        .SetParameter("DatabaseName", databaseName)
                        .SetParameter("table_name", tableName)
                         .List<object[]>().Select(porperties => new
                         {
                             CNT = porperties[0]
                         })
                          .First();
                    if (qNMAbout != null)
                    {
                        dataExists = Convert.ToBoolean(qNMAbout.CNT);
                    }
                }
                return dataExists;

            }
            catch
            {
                return false;
            }
        }
    }
}
