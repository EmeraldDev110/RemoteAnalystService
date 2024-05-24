using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.Scheduler.Schedules
{
    /// <summary>
    /// This class checks current R.A. license for every systems and sends notice email when the license is about to expire with 29 days.
    /// </summary>
    internal class CheckLicense
    {
        private static readonly ILog Log = LogManager.GetLogger("ScheduledChecks");
        /// <summary>
        /// Timer_Elapsed is a event that gets call by Scheduler to start the schedule task.
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="e">Timer ElapsedEventArgs</param>
        public void Timer_Elapsed(object source, System.Timers.ElapsedEventArgs e) {
            int currHour = DateTime.Now.Hour;

            if (currHour.Equals(6)) {
                CheckExpire();
				//CheckPMCExpire();
            }
        }

        /// <summary>
        /// CheckExpire checks current R.A. license for every systems and sends notice email when the license is about to expire with 29 days.
        /// </summary>
        public void CheckExpire()
        {
            try
            {
                Log.Info("Calling CheckExpire");
                    
                var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);

                //Force all datetime to be in US format.
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                Dictionary<string, string> dicLicense = systemTable.GetLicenseDateFor();

				//Check Date.
				// TODO: check expire logic
				DateTime emailDate = DateTime.Today;
				//DateTime emailDate = DateTime.Parse("07/01/2019");
				var dec = new Decrypt();

                Log.InfoFormat("dicLicense Count: {0}", dicLicense.Count);

				//when system going to expire on next 30/15/10/5/1/0/-7 day(s) send email
				HashSet<int> daysConcerned = new HashSet<int>();
				Dictionary<string, int> dicLicenseExp = new Dictionary<string, int>();
				daysConcerned.Add(30);
				daysConcerned.Add(15);
				daysConcerned.Add(10);
				daysConcerned.Add(5);
				daysConcerned.Add(1);
				daysConcerned.Add(0);
				if (!ConnectionString.IsLocalAnalyst) {
					daysConcerned.Add(-7); //system out of expired 7 days grace period only for RA
				}
				DateTime currentDate = DateTime.Now;
				foreach (KeyValuePair<string, string> systemSerialAndEncrypedEndDate in dicLicense) {
                    DateTime licenseDate = Convert.ToDateTime(dec.strDESDecrypt(systemSerialAndEncrypedEndDate.Value).Split(' ')[1]);
					int daysLeft = (licenseDate.Date - currentDate.Date).Days;
					if (daysConcerned.Contains(daysLeft)) {
						dicLicenseExp.Add(systemSerialAndEncrypedEndDate.Key, daysLeft);
					}
				}
				//PMC(LocalAnalyst will send license notice after a year from license expiration date)
				if (ConnectionString.IsLocalAnalyst) {
					foreach (KeyValuePair<string, string> systemSerialAndEncrypedEndDate in dicLicense) {
						DateTime licenseDate = Convert.ToDateTime(dec.strDESDecrypt(systemSerialAndEncrypedEndDate.Value).Split(' ')[1]);
						int month = 1;
						while (month <= 12) {
							if (licenseDate.Date.AddMonths(month) == currentDate.Date) {
								int daysLeft = (licenseDate.Date - currentDate.Date).Days;
								dicLicenseExp.Add(systemSerialAndEncrypedEndDate.Key, daysLeft);
							}
							month++;
						}
					}
				}
				System_tblService system_TblService = new System_tblService(ConnectionString.ConnectionStringDB);
				var systemSerialANDComapnyName = system_TblService.GetSystemSerialAndCompanyName();
				Log.InfoFormat("dicLicenseExp Count: {0}", dicLicenseExp.Count);
                    
                //Send Email to admins.
                if (dicLicenseExp.Count > 0)
                {
                    var emailToAdvisor = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
						ConnectionString.SystemLocation, ConnectionString.ServerPath, 
						ConnectionString.EmailIsSSL,
                        ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    emailToAdvisor.SendLicenseNotice(dicLicenseExp, systemSerialANDComapnyName);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("CheckLicense Error: {0}", ex);
                    
                if (!ConnectionString.IsLocalAnalyst) {
                    var amazon = new AmazonOperations();
                    amazon.WriteErrorQueue("CheckLicense Error: " + ex.Message);
                }
                else {
                    var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
						ConnectionString.SystemLocation, ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL,
                        ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    email.SendLocalAnalystErrorMessageEmail("Scheduler - CheckLicense.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                }
            }
            finally
            {
                ConnectionString.TaskCounter--;
            }
        }
    }
}