using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DataBrowser.Entities;
using MySQLDataBrowser.Model;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass;
using RemoteAnalyst.UWSLoader.Email;
using RemoteAnalyst.UWSLoader.BLL;
using RemoteAnalyst.BusinessLogic.Util;
using log4net;
using DataBrowser.Context;

namespace RemoteAnalyst.UWSLoader.DiskBrowser {
    internal class DiskBrowserLoader {
        private static readonly ILog Log = LogManager.GetLogger("JobLoader");

        internal void LoadDiskBrowserData(string systemSerial, string newConnectionString, string mySqlConnectionString, string cpuTableName, string discTableName, long sampleInterval) {
            try {
                Log.InfoFormat("systemSerial: {0}", systemSerial);
                Log.InfoFormat("newConnectionString: {0}", DiskLoader.RemovePassword(newConnectionString));
                Log.InfoFormat("mySqlConnectionString: {0}", DiskLoader.RemovePassword(mySqlConnectionString));
                Log.InfoFormat("cpuTableName: {0}", cpuTableName);
                Log.InfoFormat("discTableName: {0}", discTableName);
                Log.InfoFormat("sampleInterval: {0}", sampleInterval);
                


                Log.InfoFormat("Create DISC Browser table");
                
                //Use cpu intervals to loop and populate the from & to timestamps
                var cpuEntityTableService = new CPUEntityTableService(newConnectionString);
                var discEntityTableService = new DISCEntityTableService(mySqlConnectionString);

                List<MultiDays> cpuIntervalList = cpuEntityTableService.GetCPUEntityTableIntervalListFor(cpuTableName, mySqlConnectionString);
                string discBrowserTableName = discTableName.ToUpper().Replace("DISC", "DISKBROWSER");

                List<MultiDays> discBrowserIntervalList = discEntityTableService.GetDISCEntityTableIntervalListFor(discBrowserTableName);
                List<MultiDays> intervalList = cpuIntervalList.Except(discBrowserIntervalList).ToList();

                Log.InfoFormat("intervalList: {0}", intervalList.Count);
                
                Log.Info("*****Start Time");
                

                var discBrowserData = new List<DISCBrowserView>();
                var discService = new DISKService(new DataContext(newConnectionString), cpuTableName);

                foreach (MultiDays interval in intervalList) {
                    try {
                        Log.InfoFormat("cpuTableName: {0}", cpuTableName);
                        Log.InfoFormat("StartDate: {0}", interval.StartDate);
                        Log.InfoFormat("EndDate: {0}", interval.EndDate);
                        Log.InfoFormat("sampleInterval: {0}", sampleInterval);
                        Log.InfoFormat("mySqlConnectionString: {0}", DiskLoader.RemovePassword(mySqlConnectionString));
                        Log.InfoFormat("discBrowserTableName: {0}", discBrowserTableName);
                        Log.Info("Start Time");
                        
                        List<DISCBrowserView> data = discService.GetDISCBrowserData(cpuTableName, interval.StartDate, interval.EndDate, sampleInterval, mySqlConnectionString, Log);
                        
                        Log.Info("Check data integrity");
						bool doubleCheck = false;
						foreach (DISCBrowserView view in data) {
							if(view.BusiestFileLogicalName == null || view.BusiestFileLogicalName.Length == 0 || view.BusiestFilePhysicalName == null || view.BusiestFilePhysicalName.Length == 0) {
								Log.Info("********File Name Empty!!!************");
								Log.InfoFormat("Device: {0}", view.DeviceName);
								Log.InfoFormat("From: {0}", view.FromTimestamp);
								Log.InfoFormat("To: {0}", view.ToTimestamp);
								Log.InfoFormat("QueueLength: {0}", view.QueueLength);
								if(discService.CheckDeviceNameInDISKFIL(view.DeviceName, cpuTableName, interval.StartDate, interval.EndDate, sampleInterval, mySqlConnectionString)) {
									Thread.Sleep(60 * 1000);
									data = discService.GetDISCBrowserData(cpuTableName, interval.StartDate, interval.EndDate, sampleInterval, mySqlConnectionString, Log);
									doubleCheck = true;
									break;
								}
							}
						}

						if (doubleCheck) {
							foreach (DISCBrowserView view in data) {
								if (view.BusiestFileLogicalName == null || view.BusiestFileLogicalName.Length == 0 || view.BusiestFilePhysicalName == null || view.BusiestFilePhysicalName.Length == 0) {
                                    var email = new EmailHelper();
									email.SendErrorEmail("During second DiskBrowser load, the system " + 
                                        systemSerial + "with device name" + view.DeviceName + 
                                        " has empty BusiestFileLogicalName and BusiestFilePhysicalName");
									doubleCheck = false;
									break;
								}
							}
						}
                        Log.InfoFormat("End Time");
                        Log.InfoFormat("data: {0}", data.Count);
                        if (data.Count > 0) {
                            Log.InfoFormat("Before PopulateDISCBrowserTable");
                            
                            var discBrowser = new DISCBrowser(new DataContext(mySqlConnectionString), discBrowserTableName);
                            discBrowser.PopulateDISCBrowserTable(data, ConnectionString.SystemLocation + systemSerial + @"\");
                            Log.InfoFormat("After PopulateDISCBrowserTable");
                        }
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("Error on GetDISCBrowserData: {0}", ex);
                    }
                }
                Log.Info("*****End Time");
                
            }
            catch (Exception ex) {
                Log.ErrorFormat("Error on GetDISCBrowserData: {0}", ex);
            }
        }
    }
}