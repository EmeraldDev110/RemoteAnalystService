using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;

namespace RemoteAnalyst.BusinessLogic.Email {
    public class ProcessWatchEmail {
        private readonly string _emailServer = "";
        private readonly int _emailPort;
        private readonly string _emailUser = "";
        private readonly string _emailPassword = "";
        private readonly bool _emailAuthentication;
        private readonly string _advisorEmail = "";
        private readonly bool _isSSL;
        private readonly SmtpClient _emailClient;
        private MailMessage _emailMessage;
        private readonly NetworkCredential credential;

        public ProcessWatchEmail(string emailServer
                        , int emailPort
                        , string emailUser
                        , string emailPassword
                        , bool emailAuthentication
                        , string advisorEmail
                        , bool isSSL) {
            _emailServer = emailServer;
            _emailPort = emailPort;
            _emailUser = emailUser;
            _emailPassword = emailPassword;
            _emailAuthentication = emailAuthentication;
            _advisorEmail = advisorEmail;
            _isSSL = isSSL;

            //Setup Email Server
            System.Net.ServicePointManager.SecurityProtocol = (SecurityProtocolType)(0xc0 | 0x300 | 0xc00);
            _emailClient = new SmtpClient();
            _emailClient.EnableSsl = isSSL;
            _emailClient.Host = _emailServer;
            _emailClient.Port = _emailPort;
            _emailClient.UseDefaultCredentials = false;
            //Set up credentials
            credential = new NetworkCredential();
            credential.UserName = _emailUser;
            credential.Password = _emailPassword;
            _emailClient.Credentials = credential;
        }

        public void SendProcessWatchEmail(DataRow row, DataTable startedBy, int stoppedByCount, Dictionary<string, int> maxCount, Dictionary<string,int> minCount,
            Dictionary<string, ProcessWatchInfo> outOfBalance, Dictionary<string, ProcessWatchInfo> abourtThres, DateTime fromTime, DateTime toTime, List<string> emails) {
            var startedByEmailText = "";
            var stoppedByEmailText = "";
            var maxProcessEmailText = "";
            var minProcessEmailText = "";
            var outOfBalanceEmailText = "";
            var abortTransText = "";
            var style = "<style>" +
                        "   table { " +
                        "       border-collapse: collapse; " +
                        "   }" +
                        "" +
                        "   table, td, th {" +
                        "       border: 1px solid black; " +
                        "   }" +
                        "</style>";
            var bodyStyle = "style='font-family:verdana;font-size:12;'";
            var tableStyle = "style='background-color:black;font-family:verdana;font-size:12;";

            if (startedBy != null && startedBy.Rows.Count == 0) {
                startedByEmailText = "There were no Process running this Program by " + Convert.ToDateTime(row["MustStartBy"]).ToString("HH:mm") + ".<br>";
            }
            if (stoppedByCount > 0) {
                //There are still “n” Processes running.  Expected finish time was HH:MM.
                stoppedByEmailText = "There are still " + stoppedByCount;
                if (stoppedByCount > 1) stoppedByEmailText += " processes ";
                else stoppedByEmailText += " process ";
                stoppedByEmailText += "running. Expected finish time was " + Convert.ToDateTime(row["MustStoppBy"]).ToString("HH:mm") + ".<br>";
            }
            if (maxCount.Count > 0) {
                #region maxCount
                maxProcessEmailText += "<br>";
                maxProcessEmailText += "There are more processes running than expected. Max expected was " + row["MaxProcess"] + ".";
                maxProcessEmailText += "<br>";
                maxProcessEmailText += "<table " + tableStyle + ">";
                maxProcessEmailText += "<tr bgcolor='#FFFFFF'><td align=center>Interval</td><td align=center># of Proc Running</td></tr>";
                var color = "";
                var count = 0;
                foreach (var maxCnt in maxCount) {
                    if (count % 2 == 0) {
                        color = "#CCCCCC";
                    }
                    else {
                        color = "#FFFFFF";
                    }
                    maxProcessEmailText += "<tr bgcolor=" + color + "><td>" + maxCnt.Key + "</td><td align=right>" + maxCnt.Value + "</td></tr>";
                    count++;
                }
                maxProcessEmailText += "</table><br>";
                #endregion
            }
            if (minCount.Count > 0) {
                #region minCount
                minProcessEmailText += "<br>";
                minProcessEmailText += "There are less processes running than expected. Min expected was " + row["MinProcess"] + ".";
                minProcessEmailText += "<br>";
                minProcessEmailText += "<table " + tableStyle + ">";
                minProcessEmailText += "<tr bgcolor='#FFFFFF'><td align=center>Interval</td><td align=center># of Proc Running</td></tr>";
                var color = "";
                var count = 0;
                foreach (var minCnt in minCount) {
                    if (count % 2 == 0) {
                        color = "#CCCCCC";
                    } else {
                        color = "#FFFFFF";
                    }
                    minProcessEmailText += "<tr bgcolor=" + color + "><td>" + minCnt.Key + "</td><td align=right>" + minCnt.Value + "</td></tr>";
                    count++;
                }
                minProcessEmailText += "</table><br>";
                #endregion
            }

            if (outOfBalance.Count > 0) {
                #region outOfBalance
                outOfBalanceEmailText += "<br>";
                outOfBalanceEmailText += "There are processes exceeding the max variance of " + row["OutOfBalanceLimit"] + "%.";
                outOfBalanceEmailText += "<br>";
                outOfBalanceEmailText += "<table " + tableStyle + ">";
                outOfBalanceEmailText += "<tr bgcolor='#FFFFFF'><td align=center>Interval</td><td align=center>Average Busy</td><td align=center>Process Name</td><td align=center>Process Busy</td></tr>";
                var color = "";
                var count = 0;
                foreach (var bal in outOfBalance) {
                    if (count % 2 == 0) {
                        color = "#CCCCCC";
                    }
                    else {
                        color = "#FFFFFF";
                    }

                    var rowspan = bal.Value.processInfo.Count;
                    var intervalWritten = false;
                    foreach (var prcInfo in bal.Value.processInfo) {
                        if (intervalWritten == false) {
                            outOfBalanceEmailText += "<tr bgcolor=" + color + ">" +
                                                        "<td valign=top rowspan=" + rowspan + ">" + bal.Key + "</td>" +
                                                        "<td valign=top align=right rowspan=" + rowspan + ">" + Math.Round(bal.Value.AverageBusy, 2) + "</td>" +
                                                        "<td>" + prcInfo.ProcessName + "</td>" +
                                                        "<td align=right>" + Math.Round(prcInfo.Busy, 2) + "</td>" +
                                                        "</tr>";
                            intervalWritten = true;
                        }
                        else {
                            outOfBalanceEmailText += "<tr bgcolor=" + color + "><td>" + prcInfo.ProcessName + "</td><td align=right>" + Math.Round(prcInfo.Busy, 2) + "</td></tr>";
                        }
                    }

                    count++;
                }
                outOfBalanceEmailText += "</table><br>";
                #endregion
            }

            if (abourtThres.Count > 0) {
                #region abourtThres
                abortTransText += "<br>";
                abortTransText += "There are processes that exceed the Abort % threshold of " + row["AbortThres"] + "%.";
                abortTransText += "<br>";
                abortTransText += "<table " + tableStyle + ">";
                abortTransText += "<tr bgcolor='#FFFFFF'><td align=center>Interval</td><td align=center>Process Name</td><td align=center>Process Busy</td></tr>";
                var color = "";
                var count = 0;
                foreach (var abourt in abourtThres) {
                    if (count % 2 == 0) {
                        color = "#CCCCCC";
                    }
                    else {
                        color = "#FFFFFF";
                    }

                    var rowspan = abourt.Value.processInfo.Count;
                    var intervalWritten = false;
                    foreach (var procInfo in abourt.Value.processInfo) {
                        if (intervalWritten == false) {
                            abortTransText += "<tr bgcolor=" + color + ">" +
                                                        "<td valign=top rowspan=" + rowspan + ">" + abourt.Key + "</td>" +
                                //'<td valign=top align=right rowspan=' + rowspan + '>' + oobData.averageBusy.toFixed(2) + '</td>' + 
                                                        "<td>" + procInfo.ProcessName + "</td>" +
                                                        "<td align=right>" + Math.Round(procInfo.Busy, 2) + "</td>" +
                                                     "</tr>";
                            intervalWritten = true;
                        }
                        else {
                            abortTransText += "<tr bgcolor=" + color + "><td>" + procInfo.ProcessName + "</td><td align=right>" + Math.Round(procInfo.Busy, 2) + "</td></tr>";
                        }
                    }
                    count++;
                }
                abortTransText += "</table><br>";
                #endregion
            }

            if (startedByEmailText.Length > 0 || stoppedByEmailText.Length > 0 || maxProcessEmailText.Length > 0 || minProcessEmailText.Length > 0 || outOfBalanceEmailText.Length > 0 || abortTransText.Length > 0) {
                var emailContent =  "<html><head>" + 
                                    "</head><body " + bodyStyle + ">" +
                                    "Alert: " + row["AlertName"] + "<br>" +
                                                    "Data duration: From: " + fromTime.ToString("yyyy-MM-dd HH:mm") +
                                                    " through " + toTime.ToString("yyyy-MM-dd HH:mm") + "<br>" +
                                                    "Program: " + row["ProgramName"] + "<br><br>" +
                                                    startedByEmailText + stoppedByEmailText + maxProcessEmailText + minProcessEmailText + outOfBalanceEmailText + abortTransText +
                                    "</body></html>";

                MailAddress fromEmail = new MailAddress(_advisorEmail);
                foreach (var email in emails) {
                    MailAddress toEmail = new MailAddress(email);
                    _emailMessage = new MailMessage(fromEmail, toEmail);
                    _emailMessage.IsBodyHtml = true;
                    _emailMessage.Subject = "Process Watch Alert " + row["AlertName"] + " on " + row["SystemName"];
                    _emailMessage.Body = emailContent;

                    _emailClient.Send(_emailMessage);
                }
            }
        }
    }
}
