using System;
using System.Net.Mail;
using System.Timers;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.BusinessLogic.Scheduler.Schedules;
using RemoteAnalyst.Scheduler.Schedules;

namespace RemoteAnalyst.Scheduler {
    internal class Scheduler {
        private Timer _timerCheckLicense;
        private Timer _timerCleanupArchive;
        private Timer _timerCleanupSPAM;
        private Timer _timerMonthlyDiskLoad;
        private Timer _timerMonthlyTrendLoad;
        private Timer _timerReportDispatcher;
        private Timer _timerNewReportDispatcher;
        private Timer _timerTrendDataCleaner;
        private Timer _timerDiskDataCleaner;
        private Timer _timerHourlyEmailNotification;
        private Timer _timerDailyEmailNotification;
        private Timer _timerReportChecker;
        private Timer _timerBatchSequenceMonitor;
		private Timer _timerBatchSequenceQueue;
		private Timer _timerLoadingInfoCleaner;
		private Timer _timerDailyStorageAnalysis;
        private Timer _timerXVEntityCleaner;
        private Timer _timerForecast;
        private Timer _timerMonitor;



        public void StartScheduleTimers() {

            if (!ConnectionString.IsLocalAnalyst) {
                StartHourlyEmailNotification();
                StartReportDispatcher();
                StartDailyEmailNotification();
                StartForecast();
                StartMonitor();
            }

            StartNewReportDispatcher();
			StartCheckLicense();
			//Comment out if this dll is going to be on the dev or stg site.
			if (!ConnectionString.IsLocalAnalyst) {
                StartReportChecker();
            }

            StartCleanupArchive();
            StartCleanupSPAM();

            StartMonthlyDiskLoad();
            StartMonthlyTrendLoad();
            StartTrendDataCleaner();
            StartDiskDataCleaner();
            StartLoadingInfoCleaner();
            StartBatchSequenceMonitor();
			StartBatchReadQueue();
		}

		private void StartMonitor() {
            var loadMonitor = new LoadMonitor();
            _timerMonitor = new Timer(30000);
            _timerMonitor.Elapsed += loadMonitor.Timer_Elapsed;
            _timerMonitor.AutoReset = true;
            _timerMonitor.Enabled = true;
        }
        private void StartForecast() {
            var forecastLoad = new ForecastLoad();
            _timerForecast = new Timer(3600000);
            _timerForecast.Elapsed += forecastLoad.Timer_Elapsed;
            _timerForecast.AutoReset = true;
            _timerForecast.Enabled = true;
        }

        private void StartCheckLicense() {
            var checkLicense = new CheckLicense();
            _timerCheckLicense = new Timer(3600000);
            _timerCheckLicense.Elapsed += checkLicense.Timer_Elapsed;
            _timerCheckLicense.AutoReset = true;
            _timerCheckLicense.Enabled = true;
        }

        private void StartCleanupArchive() {
            var cleanArchive = new CleanupArchive();
            _timerCleanupArchive = new Timer(3600000);
            _timerCleanupArchive.Elapsed += cleanArchive.Timer_Elapsed;
            _timerCleanupArchive.AutoReset = true;
            _timerCleanupArchive.Enabled = true;
        }

        private void StartCleanupSPAM() {
            var cleanSPAM = new CleanupSPAM();
            _timerCleanupSPAM = new Timer(3600000);
            _timerCleanupSPAM.Elapsed += cleanSPAM.Timer_Elapsed;
            _timerCleanupSPAM.AutoReset = true;
            _timerCleanupSPAM.Enabled = true;
        }

        private void StartMonthlyDiskLoad() {
            var monthlyDiskLoad = new MonthlyDiskLoad();
            _timerMonthlyDiskLoad = new Timer(3600000);
            _timerMonthlyDiskLoad.Elapsed += monthlyDiskLoad.Timer_Elapsed;
            _timerMonthlyDiskLoad.AutoReset = true;
            _timerMonthlyDiskLoad.Enabled = true;
        }

        private void StartMonthlyTrendLoad() {
            var monthlyLoad = new MonthlyTrendLoad();
            _timerMonthlyTrendLoad = new Timer(3600000);
            _timerMonthlyTrendLoad.Elapsed += monthlyLoad.Timer_Elapsed;
            _timerMonthlyTrendLoad.AutoReset = true;
            _timerMonthlyTrendLoad.Enabled = true;
        }

        private void StartReportDispatcher() {
            var dispatcher = new ReportDispatcher();
            _timerReportDispatcher = new Timer(3600000);
            _timerReportDispatcher.Elapsed += dispatcher.Timer_Elapsed;
            _timerReportDispatcher.AutoReset = true;
            _timerReportDispatcher.Enabled = true;
        }

        private void StartNewReportDispatcher() {
            var dispatcher = new RANewReportDispatcher();
            //RA-1512: Calculate how many minutes to wait for till the next hour.
            //+5 To start at 5 minutes past the hour
            var waitForInMinutes = (60 - DateTime.Now.Minute) + 5;
            _timerNewReportDispatcher = new Timer(waitForInMinutes * 60 * 1000);
            _timerNewReportDispatcher.Elapsed += StartNewReportDispatcher;
            _timerNewReportDispatcher.AutoReset = false;    //Just do it once
            _timerNewReportDispatcher.Enabled = true;
        }

        private void StartNewReportDispatcher(object source, System.Timers.ElapsedEventArgs e) {
            var dispatcher = new RANewReportDispatcher();
            //Do it once now, and timer will take care of subsequent ones
            dispatcher.DoRANewReportDispatch();
            _timerNewReportDispatcher = new Timer(1800000); //Reduce frequency to 30 minutes to support Walgreens requeset
            _timerNewReportDispatcher.Elapsed += dispatcher.Timer_Elapsed;
            _timerNewReportDispatcher.AutoReset = true;
            _timerNewReportDispatcher.Enabled = true;
        }

        private void StartTrendDataCleaner() {
            var dataCleaner = new TrendDataCleaner();
            _timerTrendDataCleaner = new Timer(3600000);
            _timerTrendDataCleaner.Elapsed += dataCleaner.Timer_Elapsed;
            _timerTrendDataCleaner.AutoReset = true;
            _timerTrendDataCleaner.Enabled = true;
        }

        private void StartDiskDataCleaner() {
            var diskCleaner = new DiskDataCleaner();
            _timerDiskDataCleaner = new Timer(3600000);
            _timerDiskDataCleaner.Elapsed += diskCleaner.Timer_Elapsed;
            _timerDiskDataCleaner.AutoReset = true;
            _timerDiskDataCleaner.Enabled = true;
        }

        private void StartHourlyEmailNotification() {
            var hourlyEmail = new EmailNotification();
            //RA-1512: Calculate how many minutes to wait for till the next hour.
            //+5 To start at 5 minutes past the hour
            var waitForInMinutes = (60 - DateTime.Now.Minute) + 5;
            _timerHourlyEmailNotification = new Timer(waitForInMinutes * 60 * 1000);
            _timerHourlyEmailNotification.Elapsed += StartHourlyEmailNotification;
            _timerHourlyEmailNotification.AutoReset = false;    //Just do it once
            _timerHourlyEmailNotification.Enabled = true;
        }

        private void StartHourlyEmailNotification(object source, System.Timers.ElapsedEventArgs e) {
            var hourlyEmail = new EmailNotification();
            //Do it once now, and timer will take care of subsequent ones
            hourlyEmail.CheckHourlyEmails();
            _timerHourlyEmailNotification = new Timer(3600000);
            _timerHourlyEmailNotification.Elapsed += hourlyEmail.Timer_ElapsedHourly;
            _timerHourlyEmailNotification.AutoReset = true;
            _timerHourlyEmailNotification.Enabled = true;
        }


        private void StartDailyEmailNotification() {
            var dailyEmail = new EmailNotification();
            //RA-1512: Calculate how many minutes to wait for till the next hour.
            //+5 To start at 5 minutes past the hour
            var waitForInMinutes = (60 - DateTime.Now.Minute) + 5;
            _timerDailyEmailNotification = new Timer(waitForInMinutes * 60 * 1000);
            _timerDailyEmailNotification.Elapsed += StartDailyEmailNotification;
            _timerDailyEmailNotification.AutoReset = false;    //Just do it once
            _timerDailyEmailNotification.Enabled = true;
        }

        private void StartDailyEmailNotification(object source, System.Timers.ElapsedEventArgs e) {
            var dailyEmail = new EmailNotification();
            //Do it once now, and timer will take care of subsequent ones
            dailyEmail.CheckWeeklyEmails();
            _timerDailyEmailNotification = new Timer(3600000);
            _timerDailyEmailNotification.Elapsed += dailyEmail.Timer_ElapsedDaily;
            _timerDailyEmailNotification.AutoReset = true;
            _timerDailyEmailNotification.Enabled = true;
        }

        private void StartReportChecker() {
            var reportWatch = new ReportWatcher();
            _timerReportChecker = new Timer(7200000);
            _timerReportChecker.Elapsed += reportWatch.Timer_Elapsed;
            _timerReportChecker.AutoReset = true;
            _timerReportChecker.Enabled = true;
        }

        private void StartBatchSequenceMonitor() {
            var batchSequenceMonitor = new Schedules.BatchSequenceMonitor();
            _timerBatchSequenceMonitor = new Timer(3600000);
            _timerBatchSequenceMonitor.Elapsed += batchSequenceMonitor.Timer_ElapsedHourly;
            _timerBatchSequenceMonitor.AutoReset = true;
            _timerBatchSequenceMonitor.Enabled = true;
        }

		public void StartBatchReadQueue() {
			var batchSequenceMonitor = new Schedules.BatchSequenceMonitor();
			_timerBatchSequenceQueue = new Timer(60000);
			_timerBatchSequenceQueue.Elapsed += batchSequenceMonitor.Timer_ElapsedMinutely;
			_timerBatchSequenceQueue.AutoReset = true;
			_timerBatchSequenceQueue.Enabled = true;

		}

        private void StartLoadingInfoCleaner() {
            var loadingInfoCleaner = new LoadingInfoCleaner();
            _timerLoadingInfoCleaner = new Timer(3600000);
            _timerLoadingInfoCleaner.Elapsed += loadingInfoCleaner.Timer_ElapsedHourly;
            _timerLoadingInfoCleaner.AutoReset = true;
            _timerLoadingInfoCleaner.Enabled = true;
        }

        private void StartXVEntityCleaner() {
            var xvDailyEntityCleaner = new XVDailyEntityCleaner();
            _timerXVEntityCleaner = new Timer(3600000);
            _timerXVEntityCleaner.Elapsed += xvDailyEntityCleaner.Timer_ElapsedHourly;
            _timerXVEntityCleaner.AutoReset = true;
            _timerXVEntityCleaner.Enabled = true;
        }
    }
}