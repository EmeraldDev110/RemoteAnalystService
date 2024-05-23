using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Infrastructure;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class UWSDirectoryService {
        private readonly string _connectionString = "";

        public UWSDirectoryService(string connectionString) {
            _connectionString = connectionString;
        }

        public List<string> CheckDataFor(string systemSerial, DateTime startTime, DateTime stopTime) {
            var directories = new UWSDirectories(_connectionString);

            List<UWSDirectoryInfo> uwsFiles = directories.CheckData(systemSerial, startTime, stopTime);

            var files = new List<string>();
            //Check if we have entry on UWSLoadInfos.
            var uwsLoadingInfos = new UWSLoadInfoService(_connectionString);
            foreach (UWSDirectoryInfo file in uwsFiles) {
                bool exits = uwsLoadingInfos.CheckLoadedTimeFor(systemSerial, file.StartTime, file.StopTime);
                if (!exits) {
                    if (!files.Contains(file.UWSLocation)) {
                        files.Add(file.UWSLocation);
                    }
                }
            }
            return files;
        }

        public List<string> CheckAllDataFor(string systemSerial, DateTime startTime, DateTime stopTime, bool qt) {
            var directories = new UWSDirectories(_connectionString);
            List<UWSDirectoryInfo> uwsFiles = directories.CheckData(systemSerial, startTime, stopTime);

            var files = new List<string>();
            foreach (UWSDirectoryInfo file in uwsFiles.Where(file => !files.Contains(file.UWSLocation))) {
                if (qt) {
                    if (!file.UWSLocation.Contains("_DO_")) files.Add(file.UWSLocation);
                }
                else
                    files.Add(file.UWSLocation);
            }
            return files;
        }

        public void UpdateLoadingFor(string systemSerial, DateTime startTime, DateTime stopTime, int isLoading) {
            var directories = new UWSDirectories(_connectionString);
            directories.UpdateLoading(systemSerial, startTime, stopTime, isLoading);
        }

        public void UpdateLoadingFor(string systemSerial, string uwsLocation, int isLoading) {
            var directories = new UWSDirectories(_connectionString);
            directories.UpdateLoading(systemSerial, uwsLocation, isLoading);
        }

        public void InsertUWSDirectoryFor(string systemSerial, DateTime startTime, DateTime stopTime, string location) {
            var directories = new UWSDirectories(_connectionString);
            directories.InsertUWSDirectory(systemSerial, startTime, stopTime, location);
        }

        public bool CheckDuplicateTimeFor(string systemSerial, DateTime startTime, DateTime stopTime, string location) {
            var directories = new UWSDirectories(_connectionString);
            bool retVal = directories.CheckDuplicateTime(systemSerial, startTime, stopTime, location);
            return retVal;
        }

    }
}