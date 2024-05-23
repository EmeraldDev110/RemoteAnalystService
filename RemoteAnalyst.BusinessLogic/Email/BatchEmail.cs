using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.ModelView;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Net.Mime;
using RemoteAnalyst.Repository.Models;

namespace RemoteAnalyst.BusinessLogic.Email {
    public class BatchEmail {
        private int AlertNumber = 0;
        private readonly EmailManager _emailManager;
        private readonly string _advisorEmail;
        private readonly string _serverPath;
        private readonly string _supportEmail;
        private readonly string _website;
        private readonly bool _isLocalAnalyst;
        

        public BatchEmail(string advisorEmail, string supportEmail, string website, string emailServer,
            int emailPort, string emailUser, string emailPassword, bool emailAuth,
            string serverPath, string systemLocation,
            bool isSSL, bool isLocalAnalyst,
            string mailGunSendAPIKey, string mailGunSendDomain) {
			_emailManager = new EmailManager(emailServer
								, serverPath
								, emailPort
								, emailUser
								, emailPassword
								, emailAuth
								, systemLocation
								, advisorEmail
								, supportEmail
								, website
								, isSSL
								, isLocalAnalyst
								, mailGunSendAPIKey
								, mailGunSendDomain);
        }

        public void SendBatchAlertEmailIfMeetsCriteria(BatchView batch, List<BatchTrendView> trendList) {

            DateTime dateTwoDaysAgo = DateTime.Now.AddDays(-2);
            if (trendList.Count <= 0 || !DayIsSelected(batch.StartWindowDoW, dateTwoDaysAgo.DayOfWeek.ToString())) return;
            AlertNumber = 0;
            try {
                //Force all datetime to be in US format.
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                
                var email = new EmailHeaderFooter();
                string email_subject = $"RA Alert: Job Sequence: {batch.BatchName}";
                
                //Email Body
                string email_body = email.EmailHeader(_isLocalAnalyst);
                email_body += $"Job Sequence: {batch.BatchName}<br>";
                email_body += $"Analyses of activity on {trendList[0].StartTime.ToString("ddd yyyy/MM/dd")}";
                email_body += "<br><br>";

                DateTime expectedStartTimeStart = new DateTime(dateTwoDaysAgo.Year, dateTwoDaysAgo.Month, dateTwoDaysAgo.Day, batch.StartWindowStart.Hour, batch.StartWindowStart.Minute, batch.StartWindowStart.Second);
                DateTime expectedStartTimeEnd = new DateTime(dateTwoDaysAgo.Year, dateTwoDaysAgo.Month, dateTwoDaysAgo.Day, batch.StartWindowEnd.Hour, batch.StartWindowEnd.Minute, batch.StartWindowEnd.Second);
                DateTime expectedFinishTime = new DateTime(dateTwoDaysAgo.Year, dateTwoDaysAgo.Month, dateTwoDaysAgo.Day, batch.ExpectedFinishBy.Hour, batch.ExpectedFinishBy.Minute, batch.ExpectedFinishBy.Second);
                
                if (batch.ExpectedFinishBy.TimeOfDay <= batch.StartWindowStart.TimeOfDay)
                    expectedFinishTime = expectedFinishTime.AddDays(1);

                if (batch.StartWindowEnd.TimeOfDay <= batch.StartWindowStart.TimeOfDay)
                    expectedStartTimeEnd = expectedStartTimeEnd.AddDays(1);

                if (batch.AlertIfDoesNotStartOnTime) {                 
                    if (DateTime.Compare(trendList[0].StartTime,expectedStartTimeEnd) > 0 || DateTime.Compare(trendList[0].StartTime, expectedStartTimeStart) < 0) {
                        email_body += $"<strong>Alert[{++AlertNumber}]: Start time </strong>";
                        email_body += "<br>";
                        email_body += $"Expected: {expectedStartTimeStart.ToString("yyyy/MM/dd hh:mm:ss tt")} - {expectedStartTimeEnd.ToString("yyyy/MM/dd hh:mm:ss tt")}<br>";
                        email_body += $"Actual: {trendList[0].StartTime.ToString("yyyy/MM/dd hh:mm:ss tt")}";
                        email_body += "<br><br>";
                    }
                }   
                
                if (batch.AlertIfDoesNotFinishOnTime) {
                    if (DateTime.Compare(trendList[trendList.Count - 1].EndTime, expectedFinishTime) > 0) {
                        email_body += $"<strong>Alert[{++AlertNumber}]: End time</strong>";
                        email_body += "<br>";
                        email_body += $"Expected: {expectedFinishTime.ToString("yyyy/MM/dd hh:mm:ss tt")} <br>";
                        email_body += $"Actual: {trendList[trendList.Count - 1].EndTime.ToString("yyyy/MM/dd hh:mm:ss tt")}";
                        email_body += "<br><br>";
                    }
                }

                if (batch.AlertIfOrderNotFollowed) {
                    string actualOrder = CreateOrderOfProgramsBasedOnTrendStartTime(trendList);
                    if (!OrderFollowedByProcesses(batch.ProgramFiles, actualOrder)) {
                        email_body += $"<strong>Alert[{++AlertNumber}]: Out of order</strong>";
                        email_body += "<br>";
                        //string actualOrder = CreateOrderOfProgramsBasedOnTrendStartTime(trendList);
                        string[] actualProgramNameInOrderList = actualOrder.Split(',');
                        string[] expectedProgramNameInOrderList = batch.ProgramFiles.Split(',');
                        var programNumber = 0;

                        email_body += "Expected:<br>";
                        foreach(string programName in expectedProgramNameInOrderList) {
                            email_body += $"{++programNumber}. {programName} <br>";
                        }
                        programNumber = 0;
                        email_body += "Actual: <br>";
                        foreach (string programName in actualProgramNameInOrderList) {
                            email_body += $"{++programNumber}. {programName} <br>";
                        }
                    }
                }
                email_body += email.EmailFooter(_supportEmail, _website);

                //Send Email
                if (AlertNumber > 0)
                {
                    _emailManager.SendEmail(batch.EmailList.Split(','), email_subject, email_body, _supportEmail);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        private string CreateOrderOfProgramsBasedOnTrendStartTime(List<BatchTrendView> trendList) {
            var actualProgramListOrder = new StringBuilder();
            trendList.Sort((p1, p2) => DateTime.Compare(p1.StartTime, p2.StartTime));

            foreach(var trend in trendList) {
                actualProgramListOrder.Append($"{trend.FullFileName},");
            }
            if (actualProgramListOrder.Length > 0) actualProgramListOrder.Length--;
            return actualProgramListOrder.ToString();
        }

        private bool OrderFollowedByProcesses(string programNameList, string trendInOrderList) {
            string[] expectedProgramNameInOrderList = programNameList.Split(',');
            string[] actualProgramNameInOrderList = trendInOrderList.Split(',');
            if (expectedProgramNameInOrderList.Length != actualProgramNameInOrderList.Length) return false;

            for (int currentProgramIndex = 0; currentProgramIndex < expectedProgramNameInOrderList.Length; currentProgramIndex++) {
                if (expectedProgramNameInOrderList[currentProgramIndex] != actualProgramNameInOrderList[currentProgramIndex]) return false;
            }
            return true;
        }

        private bool DayIsSelected(char[] daysSelected, string currentDay) {
            if (GetWeekDayCode(currentDay) == -1) return false;
            else if (daysSelected[GetWeekDayCode(currentDay)] == '1') return true;
            return false;
        }

        private int GetWeekDayCode(string currentDay) {
            switch (currentDay) {
                case "Sunday":
                    return 0;    
                case "Monday":
                    return 1;
                case "Tuesday":
                    return 2;
                case "Wednesday":
                    return 3;
                case "Thursday":
                    return 4;
                case "Friday":
                    return 5;
                case "Saturday":
                    return 6;
                default:
                    return -1;
            }
        }


        //private bool ProgramOrderFollowed(BatchView batch, List<BatchSequenceTrend> trendList) {

        //    //foreach(var trend in trendList) {
        //    //    if (batch.)
        //    //}
        //    //return true;
        //}
    }
}
