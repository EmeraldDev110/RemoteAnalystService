using System;
using System.IO;
using System.Timers;
using RemoteAnalyst.ReportGenerator.BLL.Schedule;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.ReportGenerator.BLL {
    class JobScheduleLocalAnalyst {
        private Timer _timerTrigger;
        private Timer _timerDelete;

        public void StartTriggerTimers() {
            StartTriggerQueue();
            TriggerDeleteReports();
        }

        public void StopTriggerTimers() {
        }

        private void StartTriggerQueue() {
           var checkTriggerQueue = new CheckTriggerQueue();
            _timerTrigger = new Timer(60000); //Once an 60 sec.
            _timerTrigger.Elapsed += checkTriggerQueue.TimerTriggerQueue_Elapsed;
            _timerTrigger.AutoReset = true;
            _timerTrigger.Enabled = true;
        }

        //Delete Reports Functions
        private void TriggerDeleteReports()
        {
            _timerDelete = new Timer(86400000);
            _timerDelete.Elapsed += DeleteReports;
            _timerDelete.AutoReset = true;
            _timerDelete.Enabled = true;
        }

        public void DeleteReports(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(ConnectionString.SystemLocation);
                DateTime _three_days_ago = DateTime.Now.AddDays(-3);
                foreach (DirectoryInfo system in di.EnumerateDirectories())
                {
                    foreach (FileInfo file in system.EnumerateFiles())
                        if (File.GetLastWriteTime(file.FullName) < _three_days_ago)
                        {
                            file.Delete();
                        }
                    foreach (DirectoryInfo dir in system.EnumerateDirectories())
                        if (File.GetLastWriteTime(dir.FullName) < _three_days_ago)
                        {
                            dir.Delete(true);
                        }
                }
            }
            /*
            catch (ArgumentNullException ANEx)
            {
                Console.WriteLine(ANEx.Message);
            }
            catch (ArgumentException AEx)
            {
                Console.WriteLine(AEx.Message);
            }
            */
            catch (FileNotFoundException FNFEx)
            {
                Console.WriteLine(FNFEx.Message);
            }
            catch (DirectoryNotFoundException DNFEx)
            {
                Console.WriteLine(DNFEx.Message);
            }
            catch (UnauthorizedAccessException UAEx)
            {
                Console.WriteLine(UAEx.Message);
            }
            catch (System.Security.SecurityException SSEx)
            {
                Console.WriteLine(SSEx.Message);
            }
            catch (PathTooLongException PTLEx)
            {
                Console.WriteLine(PTLEx.Message);
            }
            catch (IOException IOEx)
            {
                Console.WriteLine(IOEx.Message);
            }
        }
    }
}
