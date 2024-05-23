using System;
using System.IO;
using log4net;
using RemoteAnalyst.BusinessLogic.Email;

namespace RemoteAnalyst.BusinessLogic.BLL {
    public static class AmazonError {
        private static readonly ILog Log = LogManager.GetLogger("AWSError");

        public static void WriteLog(Exception ex, string source, 
                                                    string advisorEmail, 
                                                    string supportEmail,
                                                    string webSite,
                                                    string emailServer, 
                                                    int emailPort, 
                                                    string emailUser,
                                                    string emailPassword, 
                                                    bool emailAuthentication,
                                                    string systemLocation,
                                                    string serverPath,
                                                    bool isSSL,
                                                    bool isLocalAnalyst,
                                                    string mailGunSendAPIKey, 
                                                    string mailGunSendDomain) {
            
            Log.ErrorFormat("Source {0}, Error {1}", source, ex);

            if (!isLocalAnalyst) {
                //Send Email to Support.
                Log.ErrorFormat(
                    "Emailing exception: {0} {1} {2} {3} {4} {5} {6} {7} {8}",
                    advisorEmail, supportEmail, webSite, 
                    emailServer, emailPort, emailUser, serverPath,
                    isLocalAnalyst);
                
                var emailToSupport = new EmailToSupport(advisorEmail,
                    supportEmail,
                    webSite,
                    emailServer,
                    emailPort,
                    emailUser,
                    emailPassword,
                    emailAuthentication,
                    systemLocation,
                    serverPath,
                    isSSL,
                    isLocalAnalyst,
                    mailGunSendAPIKey, mailGunSendDomain);
                emailToSupport.SendAWSErrorEmail(ex.Message, source);
            }
        }

        public static void WriteLog(string message, string source, 
                                                    string advisorEmail, 
                                                    string supportEmail,
                                                    string webSite,
                                                    string emailServer, 
                                                    int emailPort, 
                                                    string emailUser,
                                                    string emailPassword, 
                                                    bool emailAuthentication,
                                                    string systemLocation,
                                                    string serverPath,
                                                    bool isSSL,
                                                    bool isLocalAnalyst,
                                                    string mailGunSendAPIKey, string mailGunSendDomain) {
            
            Log.ErrorFormat("Source {0}, Error Message {1}", source, message);

            if (!isLocalAnalyst) {
                Log.ErrorFormat(
                    "Emailing exception: {0} {1} {2} {3} {4} {5} {6} {7} {8}",
                    advisorEmail, supportEmail, webSite,
                    emailServer, emailPort, emailUser, serverPath,
                    isLocalAnalyst);                
                //Send Email to Support.
                var emailToSupport = new EmailToSupport(advisorEmail,
                    supportEmail,
                    webSite,
                    emailServer,
                    emailPort,
                    emailUser,
                    emailPassword,
                    emailAuthentication,
                    systemLocation,
                    serverPath,
                    isSSL,
                    isLocalAnalyst,
                    mailGunSendAPIKey, mailGunSendDomain);
                emailToSupport.SendAWSErrorEmail(message, source);
            }
        }

        public static int GetTimeoutDuration(int retry) {
            int timeoutDuration;
            switch (retry) {
                case 0:
                    timeoutDuration = 1;    //One mins.
                    break;
                case 1:
                    timeoutDuration = 60000;    //One mins.
                    break;
                case 2:
                    timeoutDuration = 120000;    //Two mins.
                    break;
                case 3:
                    timeoutDuration = 240000;    //Four mins.
                    break;
                default:
                    timeoutDuration = 480000;    //Eight mins.
                    break;
            }

            return timeoutDuration;
        }
    }
}
