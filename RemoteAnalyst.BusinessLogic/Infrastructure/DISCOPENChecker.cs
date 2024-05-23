using System;
using System.Collections.Generic;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;

namespace RemoteAnalyst.BusinessLogic.Infrastructure
{
    public class DISCOPENChecker
    {
        private readonly string ConnectionString;
        private string SystemLocation;

        public DISCOPENChecker(string systemLocation, string connectionString)
        {
            SystemLocation = systemLocation;
            ConnectionString = connectionString;
        }

        public List<string> CheckDISCOPEN(string systemSerial, DateTime startTime, DateTime stopTime)
        {
            var uwsFiles = new List<string>();
            var uws = new UWSDirectoryService(ConnectionString);
            uwsFiles = uws.CheckDataFor(systemSerial, startTime, stopTime);
            return uwsFiles;
        }

        public List<string> CheckFiles(string systemSerial, DateTime startTime, DateTime stopTime, bool qt) {
            var uws = new UWSDirectoryService(ConnectionString);
            var uwsFiles = uws.CheckAllDataFor(systemSerial, startTime, stopTime, qt);

            return uwsFiles;
        } 
    }
}