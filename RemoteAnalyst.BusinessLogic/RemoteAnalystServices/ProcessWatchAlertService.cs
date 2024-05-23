using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class ProcessWatchAlertService {
        private static readonly ILog Log = LogManager.GetLogger("ProcessLoad");
        private readonly string _connectionString;
        private readonly string _connectionStringSystem;
        private readonly string _systemSerial;
        private readonly string _processTableName;
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;
        private readonly long _interval;
        private readonly string _emailServer = "";
        private readonly int _emailPort;
        private readonly string _emailUser = "";
        private readonly string _emailPassword = "";
        private readonly bool _emailAuthentication;
        private readonly string _advisorEmail = "";
        private readonly bool _isSSL;

        public ProcessWatchAlertService(string connectionString, string connectionStringSystem, string systemSerial, string processTableName, DateTime startTime, DateTime endTime, long interval,
                        string emailServer
                        , int emailPort
                        , string emailUser
                        , string emailPassword
                        , bool emailAuthentication
                        , string advisorEmail
                        , bool isSSL) {
            _connectionString = connectionString;
            _connectionStringSystem = connectionStringSystem;
            _systemSerial = systemSerial;
            _processTableName = processTableName;
            _startTime = startTime;
            _endTime = endTime;
            _interval = interval;
            _emailServer = emailServer;
            _emailPort = emailPort;
            _emailUser = emailUser;
            _emailPassword = emailPassword;
            _emailAuthentication = emailAuthentication;
            _advisorEmail = advisorEmail;
            _isSSL = isSSL;
        }

        public void GetProcessWatchFor() {
            try {
                var processWatchAlerts = new ProcessWatchAlerts(_connectionString, _connectionStringSystem);
                var alerts = processWatchAlerts.GetProcessWatch(_systemSerial);
                var currentDay = (int)DateTime.Now.DayOfWeek;
                foreach (DataRow alert in alerts.Rows) {
                    if (alert["RunsOn"].ToString()[currentDay] != '1') {
                        return;
                    }
                    var programName = alert["ProgramName"].ToString().Split('.');
                    var volume = programName[0];
                    var subVol = programName[1];
                    var fileName = programName[2];

                    //fromDateTime.AddSeconds(_interval * 0.1), toDateTime.AddSeconds(-_interval * 0.1)
                    var allowanceStartTime = _startTime.AddSeconds(-_interval * 0.1);
                    var allowanceEndTime = _endTime.AddSeconds(_interval * 0.1);

                    //checkStartedBy
                    var startedBy = new DataTable();

                    #region Started By

                    if (Convert.ToBoolean(alert["EnableStart"]) &&
                        (_startTime.Hour == Convert.ToDateTime(alert["MustStartBy"]).Hour) &&
                        (Convert.ToDateTime(alert["MustStartBy"]).Hour < _endTime.Hour)) {
                        var mustStartBy = Convert.ToDateTime(alert["MustStartBy"]);
                        mustStartBy = mustStartBy.AddSeconds(_interval * 0.1);

                        startedBy = processWatchAlerts.GetStartedBy(mustStartBy, _processTableName, allowanceStartTime, allowanceEndTime, volume, subVol, fileName);
                    }
                    else {
                        startedBy = null;
                    }

                    #endregion

                    //checkStoppedBy
                    var stoppedBy = new DataTable();

                    #region Stopped By

                    if (Convert.ToBoolean(alert["EnableStop"]) &&
                        (_startTime.Hour == Convert.ToDateTime(alert["MustStopBy"]).Hour) &&
                        (Convert.ToDateTime(alert["MustStopBy"]).Hour == _endTime.Hour + 1)) {
                        stoppedBy = processWatchAlerts.GetStoppedBy(Convert.ToDateTime(alert["MustStopBy"]), _processTableName, allowanceStartTime, allowanceEndTime, volume, subVol, fileName);
                    }

                    #endregion

                    //checkMaxCount 
                    var maxCountMap = new Dictionary<string, int>();

                    #region Max Count

                    if (Convert.ToBoolean(alert["EnableMax"])) {
                        for (var currentTime = _startTime; currentTime < _endTime; currentTime = currentTime.AddSeconds(_interval)) {
                            var allowanceCurrentTime = currentTime.AddSeconds(-_interval * 0.1);
                            var allowanceCurrentTimeEnd = currentTime.AddSeconds(_interval).AddSeconds(_interval * 0.1);

                            var maxCount = processWatchAlerts.GetProcessCount(_processTableName, allowanceCurrentTime, allowanceCurrentTimeEnd, volume, subVol, fileName);
                            if (maxCount.Rows.Count > 0) {
                                foreach (DataRow row in maxCount.Rows) {
                                    var key = row["FromIntv"].ToString() + '-' + row["ToIntv"];
                                    var processCount = Convert.ToInt32(row["Total"]);
                                    if (processCount > Convert.ToInt32(alert["MaxProcess"])) {
                                        if (!maxCountMap.ContainsKey(key))
                                            maxCountMap.Add(key, processCount);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    //checkMinCount 
                    var minCountMap = new Dictionary<string, int>();

                    #region Min Count

                    if (Convert.ToBoolean(alert["EnableMin"])) {
                        for (var currentTime = _startTime; currentTime < _endTime; currentTime = currentTime.AddSeconds(_interval)) {
                            var allowanceCurrentTime = currentTime.AddSeconds(-_interval * 0.1);
                            var allowanceCurrentTimeEnd = currentTime.AddSeconds(_interval).AddSeconds(_interval * 0.1);

                            var minCount = processWatchAlerts.GetProcessCount(_processTableName, allowanceCurrentTime, allowanceCurrentTimeEnd, volume, subVol, fileName);
                            if (minCount.Rows.Count > 0) {
                                foreach (DataRow row in minCount.Rows) {
                                    var key = row["FromIntv"].ToString() + '-' + row["ToIntv"];
                                    var processCount = Convert.ToInt32(row["Total"]);
                                    if (processCount < Convert.ToInt32(alert["MinProcess"])) {
                                        if (!minCountMap.ContainsKey(key))
                                            minCountMap.Add(key, processCount);
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    //checkOutOfBalance
                    var outOfBalanceMap = new Dictionary<string, ProcessWatchInfo>();

                    #region Out Fo Balance

                    if (Convert.ToBoolean(alert["EnableThres"])) {
                        for (var currentTime = _startTime; currentTime < _endTime; currentTime = currentTime.AddSeconds(_interval)) {
                            var allowanceCurrentTime = currentTime.AddSeconds(-_interval * 0.1);
                            var allowanceCurrentTimeEnd = currentTime.AddSeconds(_interval).AddSeconds(_interval * 0.1);

                            var outOfBalance = processWatchAlerts.GetProcessBusy(_processTableName, allowanceCurrentTime, allowanceCurrentTimeEnd, volume, subVol, fileName);
                            if (outOfBalance.Rows.Count > 0) {
                                var sumBusy = outOfBalance.AsEnumerable().Sum(x => x.Field<double>("Busy"));
                                var averageBusy = sumBusy / outOfBalance.Rows.Count;

                                foreach (DataRow row in outOfBalance.Rows) {
                                    if ((Convert.ToDouble(row["Busy"]) < (averageBusy - Convert.ToDouble(alert["OutOfBalanceLimit"])) ||
                                         (Convert.ToDouble(row["Busy"]) > (averageBusy + Convert.ToDouble(alert["OutOfBalanceLimit"]))))) {
                                        var key = row["FromIntv"].ToString() + '-' + row["ToIntv"];
                                        var processName = "";
                                        if (row["ProcessName"].ToString().Length == 0)
                                            processName = row["CPU"] + "," + row["PIN"];
                                        else
                                            processName = row["ProcessName"].ToString();

                                        if (!outOfBalanceMap.ContainsKey(key)) {
                                            var newData = new ProcessWatchInfo {
                                                AverageBusy = averageBusy,
                                                processInfo = new List<ProcessInfo> {
                                                    new ProcessInfo {
                                                        ProcessName = processName,
                                                        Busy = Convert.ToDouble(row["Busy"])
                                                    }
                                                }
                                            };
                                            outOfBalanceMap.Add(key, newData);
                                        }
                                        else {
                                            var currentData = outOfBalanceMap[key];
                                            currentData.processInfo.Add(new ProcessInfo {
                                                ProcessName = processName,
                                                Busy = Convert.ToDouble(row["Busy"])
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    //checkAbortThres
                    var abourtThresMap = new Dictionary<string, ProcessWatchInfo>();

                    #region Abort Thres

                    if (alert["EnableTMF"] != DBNull.Value && Convert.ToBoolean(alert["EnableTMF"])) {
                        for (var currentTime = _startTime; currentTime < _endTime; currentTime = currentTime.AddSeconds(_interval)) {
                            var allowanceCurrentTime = currentTime.AddSeconds(-_interval * 0.1);
                            var allowanceCurrentTimeEnd = currentTime.AddSeconds(_interval).AddSeconds(_interval * 0.1);

                            var aboutThres = processWatchAlerts.GetAbortTrans(_processTableName, allowanceCurrentTime, allowanceCurrentTimeEnd, volume, subVol, fileName);
                            if (aboutThres.Rows.Count > 0) {
                                foreach (DataRow row in aboutThres.Rows) {
                                    if (row["AbortTMF"] != DBNull.Value && Convert.ToDouble(row["AbortTMF"]) > Convert.ToDouble(alert["AbortThres"])) {
                                        var key = row["FromIntv"].ToString() + '-' + row["ToIntv"];
                                        var processName = "";
                                        if (row["ProcessName"].ToString().Length == 0)
                                            processName = row["CPU"] + "," + row["PIN"];
                                        else
                                            processName = row["ProcessName"].ToString();

                                        if (!abourtThresMap.ContainsKey(key)) {
                                            var newData = new ProcessWatchInfo {
                                                processInfo = new List<ProcessInfo> {
                                                    new ProcessInfo {
                                                        ProcessName = processName,
                                                        Busy = Convert.ToDouble(row["AbortTMF"])
                                                    }
                                                }
                                            };
                                            abourtThresMap.Add(key, newData);
                                        }
                                        else {
                                            var currentData = abourtThresMap[key];
                                            currentData.processInfo.Add(new ProcessInfo {
                                                ProcessName = processName,
                                                Busy = Convert.ToDouble(row["AbortTMF"])
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    //Get emails.
                    var alertRecipients = new AlertRecipientRepository();
                    var emails = alertRecipients.GetEmails(Convert.ToInt32(alert["idProcessWatchAlerts"]));

                    //Generate the Email.
                    var processWatchEmail = new ProcessWatchEmail(_emailServer, _emailPort, _emailUser, _emailPassword, _emailAuthentication, _advisorEmail, _isSSL);
                    processWatchEmail.SendProcessWatchEmail(alert, startedBy, stoppedBy.Rows.Count, maxCountMap, minCountMap, outOfBalanceMap, abourtThresMap, _startTime, _endTime, emails);
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("systemSerial: {0}", _systemSerial);
                Log.ErrorFormat("processTableName: {0}", _processTableName);
                Log.ErrorFormat("_startTime: {0}", _startTime);
                Log.ErrorFormat("_endTime: {0}", _endTime);
                Log.ErrorFormat("_interval: {0}", _interval);
                Log.ErrorFormat("Error {0}", ex);
            }
        }
    }

    public class ProcessWatchInfo {
        public double AverageBusy { get; set; }
        public List<ProcessInfo> processInfo = new List<ProcessInfo>();
    }

    public class ProcessInfo {
        public string ProcessName { get; set; }
        public double Busy { get; set; }
    }
}
