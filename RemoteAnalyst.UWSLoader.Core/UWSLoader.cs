using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using RemoteAnalyst.UWSLoader.Core.Enums;
using RemoteAnalyst.UWSLoader.Core.ModelView;

namespace RemoteAnalyst.UWSLoader.Core {
    public class UWSLoader {
        private readonly bool _remoteAnalyst;
        private readonly bool _websiteLoad;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteAnalyst">True when it's calling from Remote Analyst</param>
        /// <param name="websiteLoad">True when it's calling from Remote Analyst</param>
        public UWSLoader(bool remoteAnalyst, bool websiteLoad) {
            _remoteAnalyst = remoteAnalyst;
            _websiteLoad = websiteLoad;
        }

        /// <summary>
        /// This function will load UWS File to SQL Server and MySQL. On MySQL we currently load CPU, DISC, DISKFIL, FILE, PROCESS, USERDEF, FILETREND, and DISKBROWSER.
        /// *Please note that this function does not have function to create a Database on SQL Server and MySQL. All the checks and Database Create needs to be done before.
        /// *Not for Remote Analyst: This function do not call Trend, SCM Load, and Process Watch.
        /// </summary>
        /// <param name="uwsPath">Full Path of UWS file including File Name</param>
        /// <param name="newFileLog">Stream Writer - Log</param>
        /// <param name="uwsID">Only for Remote Analyst pass 0 from PMC</param>
        /// <param name="connectionString">Main Database ConnectionString</param>
        /// <param name="newConnectionString">Detail (System) Database Connection</param>
        /// <param name="systemFolder">This Folder is where all the log files will get created.</param>
        /// <param name="uwsVersion">Option Param. This is for Remote Analyst only.</param>
        /// <returns>
        /// UWSInfo: 
        /// DateTime StartDateTime, 
        /// DateTime StopDateTime, 
        /// long Interval, 
        /// List<int> EntityIds, 
        /// bool Success, 
        /// string ErrorMessage,
        /// Dictionary<int, int> DuplicatedEntityIds
        ///     Key: EntityID
        ///     Values:
        ///     - OverLap = 1,
        ///     - IntervalMismatch = 2,
        ///     - SameStartAndStopTime = 3
        /// </returns>
        public UWSInfo StartLoad(string uwsPath, ILog log, int uwsID, string connectionString, string newConnectionString, string systemFolder, UWS.Types uwsVersion = UWS.Types.Version2013) {
            var loadUWS = new LoadUWS(_remoteAnalyst, _websiteLoad);
            var returnValue = loadUWS.CreateMultiDayDataSet(uwsPath, log, uwsID, connectionString, newConnectionString, systemFolder);
            return returnValue;
        }
    }
}
