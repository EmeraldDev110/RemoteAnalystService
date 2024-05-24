using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using RemoteAnalyst.AWS.SNS;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Trigger.JobPool;

namespace RemoteAnalyst.Scheduler.Schedules {
    public class ForecastLoad {
        private static readonly ILog Log = LogManager.GetLogger("ForecastLoad");
        public void Timer_Elapsed(object source, System.Timers.ElapsedEventArgs e) {
            var currentDay = DateTime.Now;
            int currHour = currentDay.Hour;
            var currentWeekday = currentDay.DayOfWeek;

            if (currentWeekday.Equals(DayOfWeek.Sunday) && currHour.Equals(4)) {
                GenerateForecast();
            }
        }

        public void GenerateForecast() {
            var systemService = new System_tblService(ConnectionString.ConnectionStringDB);
            var activeSystemList = systemService.GetActiveSystemFor(ConnectionString.IsLocalAnalyst);

            //Get time range.
            var nextWeekStart = DateTime.Now.Date;              //Sunday
#if DEBUG
            nextWeekStart = Convert.ToDateTime("2023-07-25");
#endif
            var nextWeekStop = nextWeekStart.AddDays(7).Date;   //Saturday

            Log.InfoFormat("nextWeekStart: {0}",nextWeekStart);
            Log.InfoFormat("nextWeekStop: {0}",nextWeekStop);
            

            var mapping = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
            foreach (var activeSystem in activeSystemList) {
#if DEBUG
                //if (!activeSystem.Equals("080984"))continue;
#endif
                var isProcessDirectlySystem = systemTblService.isProcessDirectlySystemFor(activeSystem);
                if (isProcessDirectlySystem) continue;

                var systemConnectionString = mapping.GetConnectionStringFor(activeSystem);
                if (systemConnectionString.Length > 0) {
                    var currentTables = new CurrentTableService(systemConnectionString);
                    var interval = currentTables.GetLatestIntervalFor();
                    if(interval == 0)
                    {
                        Log.InfoFormat("activeSystem: {0} at {1} skipping since interval is {2}",
                            activeSystem, DateTime.Now, interval);
                        continue;
                    }
                    var forecast = new ForecastService(activeSystem, ConnectionString.ConnectionStringDB, systemConnectionString);
                    Log.InfoFormat("Forecasting for activeSystem: {0} at {1} skipping since interval is {2}",
                            activeSystem, DateTime.Now, interval);

                    var specialDayService = new SpecialDayService(ConnectionString.ConnectionStringDB);
                    for (var x = nextWeekStart; x.Date < nextWeekStop.Date; x = x.AddDays(1)) {

                        //Check if current date is Special Date.
                        var specialDayList = specialDayService.GetSpecialDayFor(activeSystem, x);

                        try {
                            var forecastData = forecast.GetForecastCpu(x, x.AddDays(1), specialDayList);
                            if (forecastData.Count > 0) {
								forecast.StoreForecastData(forecastData, x, x.AddDays(1), interval, ConnectionString.SystemLocation);

								//Insert into SystemWeekException.
								//ForecastForSystemWeekException(activeSystem, forecastData, x, 1);
							}
                        }
                        catch (Exception ex) {
                            Log.ErrorFormat("Error ForecastService: {0}",ex.Message);
                            Log.ErrorFormat("Start: {0}",x);
                            
                        }
						try {
							var forecastIpuData = forecast.GetForecastIpu(x, x.AddDays(1), specialDayList);
							if (forecastIpuData.Count > 0) {
								forecast.StoreForecastIpuData(forecastIpuData, x, x.AddDays(1), interval, ConnectionString.SystemLocation);

								//Insert into SystemWeekException.
								//ForecastForSystemWeekException(activeSystem, forecastIpuData, x, 2);
							}
						}
						catch (Exception ex) {
							Log.ErrorFormat("Error ForecastService IPU: {0}",ex.Message);
							Log.ErrorFormat("Start: {0}",x);
							
						}

						try {
							var forecastDiskData = forecast.GetForecastDisk(x, x.AddDays(1), specialDayList);
							if (forecastDiskData.Count > 0) {
								forecast.StoreForecastDiskData(forecastDiskData, x, x.AddDays(1), interval, ConnectionString.SystemLocation);

								//Insert into SystemWeekException.
								//ForecastForSystemWeekExceptionDisk(activeSystem, forecastDiskData, x, 3);
							}
						}
						catch (Exception ex) {
							Log.ErrorFormat("Error ForecastService Disk: {0}",ex.Message);
							Log.ErrorFormat("Start: {0}",x);
							
						}

						try {
							var forecastStorageData = forecast.GetForecastStorage(x, x.AddDays(1), specialDayList);
							if (forecastStorageData.Count > 0) {
								forecast.StoreForecastStorageData(forecastStorageData, x, x.AddDays(1), interval, ConnectionString.SystemLocation);
							}
						}
						catch (Exception ex) {
							Log.ErrorFormat("Error ForecastService Storage: {0}",ex.Message);
							Log.ErrorFormat("Start: {0}",x);							
						}
					}
                }
            }
        }

        internal void SubmitForecastCall() {
            var systemService = new System_tblService(ConnectionString.ConnectionStringDB);
            var activeSystemList = systemService.GetActiveSystemFor();
            var systeList = string.Join(",", activeSystemList);

            //Get time range.
            var nextWeekStart = DateTime.Now.Date;

            //Write to Qeueue.
            string buildMessage = "Forecast|" + nextWeekStart + "|" + systeList;

            if (ConnectionString.IsLocalAnalyst) {
                var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                triggerInsert.InsertFor("", (int)TriggerType.Type.Forecast, buildMessage);
            }
            else {
                #region New scheduler for report generation

                IAmazonSNS amazonSns = new AmazonSNS();
                string subjectMessage = ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.Forecast);
                amazonSns.SendToTopic(subjectMessage, buildMessage, ConnectionString.SNSProdTriggerReportARN);
                //Make sure lambda won't timed out
                Thread.Sleep(60000);

                #endregion
            }
        }

        /*
        internal void ForecastForSystemWeekException(string systemSerial, List<ForecastData> forecastdata, DateTime currentDate, int entityId) {
            var cpuExceptionView = new List<SystemWeekExceptionView>();
            var queueExceptionView = new List<SystemWeekExceptionView>();
            var dayOfWeek = (int)currentDate.DayOfWeek;
            var systemWeekException = new SystemWeekExceptionService(ConnectionString.ConnectionStringDB);

            //Cpu Data
            for (var x = 0; x < 24; x++) {
                var hour = x.ToString("D2");

                var isChanged = systemWeekException.CheckIsChangedFor(systemSerial, entityId, 1, dayOfWeek, hour);
                if (!isChanged) {
                    var value = forecastdata.Where(i => i.Hour.StartsWith(hour)).Average(i => i.CpuBusy);
                    cpuExceptionView.Add(new SystemWeekExceptionView {
                        SystemSerial = systemSerial,
                        EntityId = entityId,
                        CounterId = 1,
                        DayOfWeek = dayOfWeek,
                        Hour = hour,
                        Value = Math.Round(value, 2),
                        IsChanged = 0
                    });
                }
            }

            if (cpuExceptionView.Count > 0) {
                systemWeekException.InsertData(ConnectionString.SystemLocation + systemSerial, cpuExceptionView);
            }

            //Queue Data
            for (var x = 0; x < 24; x++) {
                var hour = x.ToString("D2");

                var isChanged = systemWeekException.CheckIsChangedFor(systemSerial, entityId, 2, dayOfWeek, hour);
                if (!isChanged) {
                    var value = forecastdata.Where(i => i.Hour.StartsWith(hour)).Average(i => i.Queue);
                    queueExceptionView.Add(new SystemWeekExceptionView {
                        SystemSerial = systemSerial,
                        EntityId = entityId,
                        CounterId = 2,
                        DayOfWeek = dayOfWeek,
                        Hour = hour,
                        Value = Math.Round(value, 2),
                        IsChanged = 0
                    });
                }
            }

            if (queueExceptionView.Count > 0) {
                systemWeekException.InsertData(ConnectionString.SystemLocation + systemSerial, queueExceptionView);
            }

        }

        internal void ForecastForSystemWeekExceptionDisk(string systemSerial, List<ForecastDiskData> forecastDiskData, DateTime currentDate, int entityId) {
            var exceptionView = new List<SystemWeekExceptionView>();
            var dayOfWeek = (int)currentDate.DayOfWeek;
            var systemWeekException = new SystemWeekExceptionService(ConnectionString.ConnectionStringDB);

            //Cpu Data
            for (var x = 0; x < 24; x++) {
                var hour = x.ToString("D2");

                var isChanged = systemWeekException.CheckIsChangedFor(systemSerial, entityId, 1, dayOfWeek, hour);
                if (!isChanged) {
                    var value = forecastDiskData.Where(i => i.Hour.StartsWith(hour)).Average(i => i.QueueLength);
                    exceptionView.Add(new SystemWeekExceptionView {
                        SystemSerial = systemSerial,
                        EntityId = entityId,
                        CounterId = 1,
                        DayOfWeek = dayOfWeek,
                        Hour = hour,
                        Value = Math.Round(value, 2),
                        IsChanged = 0
                    });
                }
            }

            if (exceptionView.Count > 0) {
                systemWeekException.InsertData(ConnectionString.SystemLocation + systemSerial, exceptionView);
            }
        }
        */
    }
}
