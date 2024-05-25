using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.BLL
{
    class LicenseChecker
    {

        /// <summary>
        /// Check if the system has the license for RemoteAnalyst.
        /// </summary>
        /// <param name="systemSerial"> Serial number of the system who license is to be checked.</param>
        /// <param name="invalidLicenseReason"> [Output] Specifies why the license was considered invalid.</param>
        public static bool ValidLicenseToLoad(string systemSerial, ref string invalidLicenseReason)
        {
            string connectionStr = ConnectionString.ConnectionStringDB;

            //Get End Date.
            try
            {
                var systemTbl = new System_tblService(connectionStr);
                //System_tblServices systemTblServices = systemTbl.GetEndDate(tempSystemSerial);
                IDictionary<string, string> endDate = systemTbl.GetEndDateFor(systemSerial);
                foreach (KeyValuePair<string, string> kv in endDate)
                {
                    if (kv.Value.Length == 0)
                    {
                        invalidLicenseReason = "License for specified system serial not found.";
                        return false;
                    }
                    if (kv.Value != "")
                    {
                        //Decrypt the End date.
                        var decrypt = new Decrypt();

                        string decryptInfo = decrypt.strDESDecrypt(kv.Value);
                        string decryptSystemSerial = decryptInfo.Split(' ')[0].Trim();
                        string decryptDate = decryptInfo.Split(' ')[1].Trim(); //get the date [0] is systemName.

                        if (decryptSystemSerial == systemSerial)
                        {
                            //Get End Date.
                            DateTime planEndDate = Convert.ToDateTime(decryptDate);

#if EVALUATION_COPY
                            //For Eval, license should not be extended.
#else
                            //Check end date. Set 7 days grace days
                            planEndDate = planEndDate.AddDays(7);
                            if (ConnectionString.IsLocalAnalyst)
                            {
                                //Extend the license date for one more year.
                                planEndDate = planEndDate.AddYears(1);
                            }
#endif

                            int timeZoneIndex = systemTbl.GetTimeZoneFor(systemSerial);
                            DateTime systemLocalTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

                            if (planEndDate.Date < systemLocalTime.Date)
                            {
#if EVALUATION_COPY
                                invalidLicenseReason = "License Expired";
#else
                                invalidLicenseReason = "License Expired and out of 7 days grace period.";
#endif
                                return false;
                            }
                        }
                        else
                        {
                            invalidLicenseReason = "Invalid system serial.";
                            return false;
                        }
                    }
                    else
                    {
                        invalidLicenseReason = "Unknown system serial.";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {

                if (ConnectionString.IsLocalAnalyst)
                {
                    var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                        ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    email.SendLocalAnalystErrorMessageEmail("UWSLoader::LicenseChecker",
                        ex.Message, LicenseService.
                        GetProductName(ConnectionString.ConnectionStringDB));
                }
                else
                {
                    var amazonOperations = new AmazonOperations();
                    StringBuilder errorMessage = new StringBuilder();
                    errorMessage.Append("Source: UWSLoader:LicenseChecker Error \r\n");
                    errorMessage.Append("systemSerial: " + systemSerial + "\r\n");
                    errorMessage.Append("invalidLicenseReason: " + invalidLicenseReason + "\r\n");
                    errorMessage.Append("Message: " + ex.Message + "\r\n");
                    errorMessage.Append("StackTrace: " + ex.StackTrace + "\r\n");
                    amazonOperations.WriteErrorQueue(errorMessage.ToString());
                }

                //If fail to get a respone from the server exit the loading.
                invalidLicenseReason = "Failed to get a respone from the server.";
                return false;
            }

            invalidLicenseReason = "";
            return true;
        }
    }
}
