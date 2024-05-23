using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.BLL {
    public class ForecastService {
        private readonly string _systemSerial;
        private readonly string _systemConnectionString;
        private readonly string _mainConnectionString;
        public ForecastService(string systemSerial, string mainConnectionString, string systemConnectionString) {
            _systemSerial = systemSerial;
            _mainConnectionString = mainConnectionString;
            _systemConnectionString = systemConnectionString;
        }

        public List<ForecastData> GetForecastCpu(DateTime startTime, DateTime stopTime, List<DateTime> specialDayList) {
            var forcastData = new List<ForecastData>();
            var newStartTime = startTime;
            var newStopTime = stopTime;
            var cpuEntityTable = new CPUEntityTable(_systemConnectionString);
            var databaseCheck = new Database(_systemConnectionString);
            var databaseName = Helper.FindKeyName(_systemConnectionString, "DATABASE");
            var currentTables = new CurrentTableRepository(_systemConnectionString);

            var cpuTableNames = new List<string>();
			var dateList = new List<DateTime>();

            if (specialDayList.Count == 0) {
                //Goes back 12 weeks.
                for (var x = 0; x < 12; x++) {
                    newStartTime = newStartTime.AddDays(-7);
                    newStopTime = newStopTime.AddDays(-7);

                    for (var i = newStartTime; i.Date < newStopTime.Date; i = i.AddDays(1)) {
						dateList.Add(newStartTime);
                        var cpuTableName = _systemSerial + "_CPU_" + i.Year + "_" + i.Month + "_" + i.Day;
                        var exists = databaseCheck.CheckTableExists(cpuTableName, databaseName);

                        if (exists) {
                            cpuTableNames.Add(cpuTableName);
                        }
                    }
                }
            }
            else {
                newStartTime = specialDayList.Min(x => x.Date);
                foreach (var specialDay in specialDayList) {
                    var cpuTableName = _systemSerial + "_CPU_" + specialDay.Year + "_" + specialDay.Month + "_" + specialDay.Day;
                    var exists = databaseCheck.CheckTableExists(cpuTableName, databaseName);

                    if (exists) {
                        cpuTableNames.Add(cpuTableName);
                    }
                }
            }

            #region CPU
            var cpuData = new DataTable();
            var holidayServices = new HolidayService(_mainConnectionString);
            var holidayInfo = holidayServices.GetWorkDayFactorFor(_systemSerial, startTime);

            if (cpuTableNames.Count > 0) {
                cpuData = cpuEntityTable.GetAllCPUBusyAndMemory(dateList, newStartTime, stopTime.AddDays(1));
            }

            var cpuNumbers = cpuData.AsEnumerable().Select(x => x.Field<string>("CPUNumber")).Distinct().ToList();

            var hours = cpuData.AsEnumerable().Select(x => x.Field<DateTime>("Date & Time").ToString("HH:mm")).Distinct().OrderBy(x => x).ToList();

            foreach (var cpuNumber in cpuNumbers) {
                foreach (var hour in hours) {
                    var subDataPer = cpuData.AsEnumerable().Where(x => x.Field<string>("CPUNumber").Equals(cpuNumber) && x.Field<DateTime>("Date & Time").ToString("HH:mm").Equals(hour))
                                                .Select(x => new {
                                                    DateTime = x.Field<DateTime>("Date & Time"),
                                                    Busy = x.Field<double>("Busy"),
                                                    MemoryUsed = x.Field<long>("MemoryUsed"),
                                                    Queue = x.Field<double>("Queue")
                                                });

                    var avgCPUDataPerCpu = subDataPer.Average(x => x.Busy);
                    var avgQueueDataPerCpu = subDataPer.Average(x => x.Queue);
                    var avgMemoryDataPerCpu = subDataPer.Average(x => x.MemoryUsed);

                    var output = 0d;
                    var outputQueue = 0d;
                    var outputMemory = 0d;
                    foreach (var subData in subDataPer) {
                        output += Math.Pow(subData.Busy - avgCPUDataPerCpu, 2);
                        outputMemory += Math.Pow(subData.MemoryUsed - avgMemoryDataPerCpu, 2);
                        outputQueue += Math.Pow(subData.Queue - avgQueueDataPerCpu, 2);
                    }

                    var stdDevCPU = Math.Sqrt(output / subDataPer.Count());
                    var stdDevMemory = Math.Sqrt(outputMemory / subDataPer.Count());
                    var stdDevQueue = Math.Sqrt(outputQueue / subDataPer.Count());

                    var highRangeCPU = avgCPUDataPerCpu + stdDevCPU;
                    var lowRangeCPU = avgCPUDataPerCpu - stdDevCPU;
                    var highRangeMemory = avgMemoryDataPerCpu + stdDevMemory;
                    var lowRangeMemory = avgMemoryDataPerCpu - stdDevMemory;
                    var highRangeQueue = avgQueueDataPerCpu + stdDevQueue;
                    var lowRangeQueue = avgQueueDataPerCpu - stdDevQueue;

                    var newOutputCPU = 0d;
                    var newOutputMemory = 0d;
                    var newOutputQueue = 0d;

                    var counterCPU = 0;
                    var counterMemory = 0;
                    var counterQueue = 0;

                    foreach (var subData in subDataPer) {
                        if (subData.Busy <= highRangeCPU && subData.Busy >= lowRangeCPU) {
                            newOutputCPU += subData.Busy;
                            counterCPU++;
                        }
                        if (subData.MemoryUsed <= highRangeMemory && subData.MemoryUsed >= lowRangeMemory) {
                            newOutputMemory += subData.MemoryUsed;
                            counterMemory++;
                        }
                        if (subData.Queue <= highRangeQueue && subData.Queue >= lowRangeQueue) {
                            newOutputQueue += subData.Queue;
                            counterQueue++;
                        }
                    }
                   //parse hour.
                    if (holidayInfo.HasData) {
                        var newHour = Convert.ToInt32(hour.Split(':')[0]);
                        var cpuBusy = 0d;
                        var memoryUsed = 0d;
                        var queue = 0d;

                        if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                            if (holidayInfo.Increase == 1) {
                                cpuBusy = (newOutputCPU / counterCPU) * (1 + (holidayInfo.Percentage/100));
                                memoryUsed = (newOutputMemory / counterMemory) * (1 + (holidayInfo.Percentage / 100));
                                queue = (newOutputQueue / counterQueue) * (1 + (holidayInfo.Percentage / 100));
                            }
                            else {
                                cpuBusy = (newOutputCPU / counterCPU) * (1 - (holidayInfo.Percentage / 100));
                                memoryUsed = (newOutputMemory / counterMemory) * (1 - (holidayInfo.Percentage / 100));
                                queue = (newOutputQueue / counterQueue) * (1 - (holidayInfo.Percentage / 100));
                            }
                        }
                        else {
                            cpuBusy = newOutputCPU / counterCPU;
                            memoryUsed = Convert.ToInt64(newOutputMemory / counterMemory);
                            queue = newOutputQueue / counterQueue;
                        }
                        forcastData.Add(new ForecastData {
                            Hour = hour,
                            CpuNumber = int.Parse(cpuNumber),
                            CpuBusy = cpuBusy,
                            MemoryUsed = memoryUsed,
                            Queue = queue,
                            StdDevCpuBusy = stdDevCPU,
                            StdDevMemoryUsed = stdDevMemory,
                            StdDevQueue = stdDevQueue
                        });
                    }
                    else {
                        forcastData.Add(new ForecastData {
                            Hour = hour,
                            CpuNumber = int.Parse(cpuNumber),
                            CpuBusy = newOutputCPU / counterCPU,
                            MemoryUsed = Convert.ToInt64(newOutputMemory / counterMemory),
                            Queue = newOutputQueue / counterQueue,
                            StdDevCpuBusy = stdDevCPU,
                            StdDevMemoryUsed = stdDevMemory,
                            StdDevQueue = stdDevQueue
                        });
                    }
                }
            }
            #endregion
            
            return forcastData;
        }

        public List<ForecastData> GetForecastIpu(DateTime startTime, DateTime stopTime, List<DateTime> specialDayList) {
            var forcastData = new List<ForecastData>();
            var newStartTime = startTime;
            var newStopTime = stopTime;
            var cpuEntityTable = new CPUEntityTable(_systemConnectionString);
            var databaseCheck = new Database(_systemConnectionString);
            var databaseName = Helper.FindKeyName(_systemConnectionString, "DATABASE");
            var currentTables = new CurrentTableRepository(_systemConnectionString);

            var cpuTableNames = new List<string>();

            if (specialDayList.Count == 0) {
                //Goes back 12 weeks.
                for (var x = 0; x < 12; x++) {
                    newStartTime = newStartTime.AddDays(-7);
                    newStopTime = newStopTime.AddDays(-7);

                    for (var i = newStartTime; i.Date < newStopTime.Date; i = i.AddDays(1)) {
                        var cpuTableName = _systemSerial + "_CPU_" + i.Year + "_" + i.Month + "_" + i.Day;
                        var exists = databaseCheck.CheckTableExists(cpuTableName, databaseName);

                        if (exists) {
                            cpuTableNames.Add(cpuTableName);
                        }
                    }
                }
            }
            else {
                newStartTime = specialDayList.Min(x => x.Date);
                foreach (var specialDay in specialDayList) {
                    var cpuTableName = _systemSerial + "_CPU_" + specialDay.Year + "_" + specialDay.Month + "_" + specialDay.Day;
                    var exists = databaseCheck.CheckTableExists(cpuTableName, databaseName);

                    if (exists) {
                        cpuTableNames.Add(cpuTableName);
                    }
                }
            }

            #region IPU
            var ipuData = new DataTable();

            if (cpuTableNames.Count > 0) {
                var ipus = cpuEntityTable.CheckIPU(cpuTableNames.First());
                var internals = currentTables.GetInterval(cpuTableNames.First());
                var holidayServices = new HolidayService(_mainConnectionString);
                var holidayInfo = holidayServices.GetWorkDayFactorFor(_systemSerial, startTime);

                if (ipus) {
                    var ipusNumber = cpuEntityTable.GetNumOfIPUs(cpuTableNames.First());
                    ipuData = cpuEntityTable.GetAllIPUBusyAndQueue(cpuTableNames, ipusNumber, internals, newStartTime, stopTime.AddDays(1));


                    var cpuNumbers = ipuData.AsEnumerable().Select(x => x.Field<int>("CPUNumber")).Distinct().ToList();
                    var hours = ipuData.AsEnumerable().Select(x => x.Field<DateTime>("Date & Time").ToString("HH:mm")).Distinct().OrderBy(x => x).ToList();

                    foreach (var cpuNumber in cpuNumbers) {
                        foreach (var hour in hours) {
                            for (var m = 1; m <= ipusNumber; m++) {
                                var subDataPer = ipuData.AsEnumerable().Where(x => x.Field<int>("CPUNumber").Equals(cpuNumber) && x.Field<DateTime>("Date & Time").ToString("HH:mm").Equals(hour))
                                                .Select(x => new {
                                                    DateTime = x.Field<DateTime>("Date & Time"),
                                                    IpuBusy = x.Field<double>("IPUBusy" + m),
                                                    IpuQueue = x.Field<double>("IPUQLength" + m)
                                                });

                                var avgCPUDataPerCpu = subDataPer.Average(x => x.IpuBusy);
                                var avgQueueDataPerCpu = subDataPer.Average(x => x.IpuQueue);

                                var output = 0d;
                                var outputQueue = 0d;

                                foreach (var subData in subDataPer) {
                                    output += Math.Pow(subData.IpuBusy - avgCPUDataPerCpu, 2);
                                    outputQueue += Math.Pow(subData.IpuQueue - avgQueueDataPerCpu, 2);
                                }

                                var stdDevIPUBusy = Math.Sqrt(output / subDataPer.Count());
                                var stdDevIPUQueue = Math.Sqrt(outputQueue / subDataPer.Count());

                                var highRangeCPU = avgCPUDataPerCpu + stdDevIPUBusy;
                                var lowRangeCPU = avgCPUDataPerCpu - stdDevIPUBusy;

                                var highRangeQueue = avgQueueDataPerCpu + stdDevIPUQueue;
                                var lowRangeQueue = avgQueueDataPerCpu - stdDevIPUQueue;

                                var newOutputCPU = 0d;
                                var newOutputQueue = 0d;

                                var counterCPU = 0;
                                var counterQueue = 0;

                                foreach (var subData in subDataPer) {
                                    if (subData.IpuBusy <= highRangeCPU && subData.IpuBusy >= lowRangeCPU) {
                                        newOutputCPU += subData.IpuBusy;
                                        counterCPU++;
                                    }
                                    if (subData.IpuQueue <= highRangeQueue && subData.IpuQueue >= lowRangeQueue) {
                                        newOutputQueue += subData.IpuQueue;
                                        counterQueue++;
                                    }
                                }
                                //parse hour.

                                if (holidayInfo.HasData) {
                                    var newHour = Convert.ToInt32(hour.Split(':')[0]);
                                    var cpuBusy = 0d;
                                    var queue = 0d;

                                    if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                                        if (holidayInfo.Increase == 1) {
                                            cpuBusy = (newOutputCPU / counterCPU) * (1 + (holidayInfo.Percentage / 100));
                                            queue = (newOutputQueue / counterQueue) * (1 + (holidayInfo.Percentage / 100));
                                        }
                                        else {
                                            cpuBusy = (newOutputCPU / counterCPU) * (1 - (holidayInfo.Percentage / 100));
                                            queue = (newOutputQueue / counterQueue) * (1 - (holidayInfo.Percentage / 100));
                                        }
                                    }
                                    else {
                                        cpuBusy = newOutputCPU / counterCPU;
                                        queue = newOutputQueue / counterQueue;
                                    }

                                    forcastData.Add(new ForecastData {
                                        Hour = hour,
                                        CpuNumber = cpuNumber,
                                        IpuNumber = m,
                                        CpuBusy = cpuBusy,
                                        Queue = queue,
                                        StdDevIpuBusy = stdDevIPUBusy,
                                        StdDevIpuQueue = stdDevIPUQueue
                                    });
                                }
                                else {
                                    forcastData.Add(new ForecastData {
                                        Hour = hour,
                                        CpuNumber = cpuNumber,
                                        IpuNumber = m,
                                        CpuBusy = newOutputCPU / counterCPU,
                                        Queue = newOutputQueue / counterQueue,
                                        StdDevIpuBusy = stdDevIPUBusy,
                                        StdDevIpuQueue = stdDevIPUQueue
                                    });
                                }
                            }
                        }
                    }
                }

            }
            #endregion

            return forcastData;
        }

        public List<ForecastDiskData> GetForecastDisk(DateTime startTime, DateTime stopTime, List<DateTime> specialDayList) {
            var forcastData = new List<ForecastDiskData>();
            var newStartTime = startTime;
            var detailDiskForForecast = new DetailDiskForForecast(_systemConnectionString);
            var diskData = new DataTable();

            if (specialDayList.Count == 0) {
                //Goes back 12 weeks.
                for (var x = 0; x < 12; x++) {
                    newStartTime = newStartTime.AddDays(-7);
                }
                diskData = detailDiskForForecast.GetQueueLength(newStartTime, stopTime.AddDays(1));
            }
            else {
                var tempDiskTable = new DataTable();
                foreach (var specialDay in specialDayList) {
                    var diskForecast = detailDiskForForecast.GetQueueLength(specialDay, specialDay.AddDays(1));
                    if(diskForecast.Rows.Count > 0)
                        tempDiskTable.Merge(diskForecast);
                }

                diskData = tempDiskTable.AsEnumerable().OrderBy(x => x.Field<string>("DeviceName")).ThenBy(x => x.Field<string>("Hour")).CopyToDataTable();
            }

            #region Disk
            var holidayServices = new HolidayService(_mainConnectionString);
            var holidayInfo = holidayServices.GetWorkDayFactorFor(_systemSerial, startTime);
            

            //var hourIndex = 0;
            var uniqueDeviceName = "";
            var uniqueHour = "";
            var storedQueue = new List<double>();
            var storedDP2 = new List<double>();

            uniqueDeviceName = diskData.Rows[0]["DeviceName"].ToString();
            uniqueHour = diskData.Rows[0]["Hour"].ToString();

            foreach (DataRow row in diskData.Rows) {
                var deviceName = row["DeviceName"].ToString();
                var hour = row["Hour"].ToString();

                if (uniqueDeviceName == deviceName && hour == uniqueHour) {
                    storedQueue.Add(Convert.ToDouble(row["QueueLength"]));
                    storedDP2.Add(row.IsNull("DP2Busy") ? 0 : Convert.ToDouble(row["DP2Busy"]));
                }
                else {
                    #region Get the STV Value.
                    
                    var avgQueueDataPerCpu = storedQueue.Average();
                    var avgDp2DataPerCpu = storedDP2.Average();

                    var outputQueue = 0d;
                    var outputDp2 = 0d;

                    foreach (var subData in storedQueue) {
                        outputQueue += Math.Pow(subData - avgQueueDataPerCpu, 2);
                    }
                    foreach (var subData in storedDP2) {
                        outputDp2 += Math.Pow(subData - avgDp2DataPerCpu, 2);
                    }

                    var stdDevQueue = Math.Sqrt(outputQueue / storedQueue.Count());
                    var stdDevDp2 = Math.Sqrt(outputDp2 / storedDP2.Count());

                    var highRangeQueue = avgQueueDataPerCpu + stdDevQueue;
                    var lowRangeQueue = avgQueueDataPerCpu - stdDevQueue;
                    var highRangeDp2 = avgDp2DataPerCpu + stdDevDp2;
                    var lowRangeDp2 = avgDp2DataPerCpu - stdDevDp2;

                    var newOutputQueue = 0d;
                    var counterQueue = 0;
                    var newOutputDp2 = 0d;
                    var counterDp2 = 0;

                    foreach (var subData in storedQueue) {
                        if (subData <= highRangeQueue && subData >= lowRangeQueue) {
                            newOutputQueue += subData;
                            counterQueue++;
                        }
                    }
                    foreach (var subData in storedDP2) {
                        if (subData <= highRangeDp2 && subData >= lowRangeDp2) {
                            newOutputDp2 += subData;
                            counterDp2++;
                        }
                    }

                    //parse hour.
                    if (holidayInfo.HasData) {
                        var newHour = Convert.ToInt32(uniqueHour.Split(':')[0]);
                        var dp2Busy = 0d;
                        var queue = 0d;

                        if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                            if (holidayInfo.Increase == 1) {
                                queue = (newOutputQueue / counterQueue) * (1 + (holidayInfo.Percentage / 100));
                                dp2Busy = (newOutputDp2 / counterDp2) * (1 + (holidayInfo.Percentage / 100));
                            }
                            else {
                                queue = (newOutputQueue / counterQueue) * (1 - (holidayInfo.Percentage / 100));
                                dp2Busy = (newOutputDp2 / counterDp2) * (1 - (holidayInfo.Percentage / 100));
                            }
                        }
                        else {
                            queue = newOutputQueue / counterQueue;
                            dp2Busy = newOutputDp2 / counterDp2;
                        }
                        forcastData.Add(new ForecastDiskData {
                            Hour = uniqueHour,
                            DeviceName = uniqueDeviceName,
                            QueueLength = queue,
                            StdDevQueueLength = stdDevQueue,
                            DP2Busy = dp2Busy,
                            StdDevDP2Busy = stdDevDp2
                        });
                    }
                    else {

                        forcastData.Add(new ForecastDiskData {
                            Hour = uniqueHour,
                            DeviceName = uniqueDeviceName,
                            QueueLength = newOutputQueue / counterQueue,
                            StdDevQueueLength = stdDevQueue,
                            DP2Busy = newOutputDp2 / counterDp2,
                            StdDevDP2Busy = stdDevDp2
                        });
                    }
                    #endregion

                    uniqueHour = hour;
                    uniqueDeviceName = deviceName;
                    storedQueue = new List<double>();
                    storedDP2 = new List<double>();

                    storedQueue.Add(Convert.ToDouble(row["QueueLength"]));
                    storedDP2.Add(row.IsNull("DP2Busy") ? 0 : Convert.ToDouble(row["DP2Busy"]));
                }
            }
            //Load the last entry.
            #region Get the STV Value.
            
            var avgQueueDataPerCpuE = storedQueue.Average();
            var avgDp2DataPerCpuE = storedDP2.Average();

            var outputQueueE = 0d;
            var outputDp2E = 0d;

            foreach (var subData in storedQueue) {
                outputQueueE += Math.Pow(subData - avgQueueDataPerCpuE, 2);
            }
            foreach (var subData in storedDP2) {
                outputDp2E += Math.Pow(subData - avgDp2DataPerCpuE, 2);
            }

            var stdDevQueueE = Math.Sqrt(outputQueueE / storedQueue.Count());
            var stdDevDp2E = Math.Sqrt(outputDp2E / storedDP2.Count());

            var highRangeQueueE = avgQueueDataPerCpuE + stdDevQueueE;
            var lowRangeQueueE = avgQueueDataPerCpuE - stdDevQueueE;
            var highRangeDp2E = avgDp2DataPerCpuE + stdDevDp2E;
            var lowRangeDp2E = avgDp2DataPerCpuE - stdDevDp2E;

            var newOutputQueueE = 0d;
            var counterQueueE = 0;
            var newOutputDp2E = 0d;
            var counterDp2E = 0;

            foreach (var subData in storedQueue) {
                if (subData <= highRangeQueueE && subData >= lowRangeQueueE) {
                    newOutputQueueE += subData;
                    counterQueueE++;
                }
            }
            foreach (var subData in storedDP2) {
                if (subData <= highRangeDp2E && subData >= lowRangeDp2E) {
                    newOutputDp2E += subData;
                    counterDp2E++;
                }
            }

            //parse hour.
            if (holidayInfo.HasData) {
                var newHour = Convert.ToInt32(uniqueHour.Split(':')[0]);
                var queue = 0d;
                var dp2 = 0d;

                if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                    if (holidayInfo.Increase == 1) {
                        queue = (newOutputQueueE / counterQueueE) * (1 + (holidayInfo.Percentage / 100));
                        dp2 = (newOutputDp2E / counterDp2E) * (1 + (holidayInfo.Percentage / 100));
                    }
                    else {
                        queue = (newOutputQueueE / counterQueueE) * (1 - (holidayInfo.Percentage / 100));
                        dp2 = (newOutputDp2E / counterDp2E) * (1 - (holidayInfo.Percentage / 100));
                    }
                }
                else {
                    queue = newOutputQueueE / counterQueueE;
                    dp2 = newOutputDp2E / counterDp2E;
                }
                forcastData.Add(new ForecastDiskData {
                    Hour = uniqueHour,
                    DeviceName = uniqueDeviceName,
                    QueueLength = queue,
                    StdDevQueueLength = stdDevQueueE,
                    DP2Busy = dp2,
                    StdDevDP2Busy = stdDevDp2E
                });
            }
            else {
                forcastData.Add(new ForecastDiskData {
                    Hour = uniqueHour,
                    DeviceName = uniqueDeviceName,
                    QueueLength = newOutputQueueE / counterQueueE,
                    StdDevQueueLength = stdDevQueueE,
                    DP2Busy = newOutputDp2E / counterDp2E,
                    StdDevDP2Busy = stdDevDp2E
                });
            }
            #endregion
            #region Old Code
            /*
            var deviceNames = diskData.AsEnumerable().Select(x => x.Field<string>("DeviceName")).Distinct().ToList();
            var hours = diskData.AsEnumerable().Select(x => x.Field<DateTime>("FromTimestamp").ToString("HH:mm")).Distinct().OrderBy(x => x).ToList();

            foreach (var deviceName in deviceNames) {
                foreach (var hour in hours) {
                    var subDataPer = diskData.AsEnumerable().Where(x => x.Field<string>("DeviceName").Equals(deviceName) && x.Field<DateTime>("FromTimestamp").ToString("HH:mm").Equals(hour))
                        .Select(x => new {
                            DateTime = x.Field<DateTime>("FromTimestamp"),
                            QueueLength = x.Field<double>("QueueLength")
                        });

                    var avgQueueDataPerDevice = subDataPer.Average(x => x.QueueLength);

                    var outputQueue = 0d;
                    foreach (var subData in subDataPer) {
                        outputQueue += Math.Pow(subData.QueueLength - avgQueueDataPerDevice, 2);
                    }

                    var stdDevQueue = Math.Sqrt(outputQueue / subDataPer.Count());

                    var highRangeQueue = avgQueueDataPerDevice + stdDevQueue;
                    var lowRangeQueue = avgQueueDataPerDevice - stdDevQueue;

                    var newOutputQueue = 0d;

                    var counterQueue = 0;

                    foreach (var subData in subDataPer) {
                        if (subData.QueueLength <= highRangeQueue && subData.QueueLength >= lowRangeQueue) {
                            newOutputQueue += subData.QueueLength;
                            counterQueue++;
                        }
                    }

                    //parse hour.
                    if (holidayInfo.HasData) {
                        var newHour = Convert.ToInt32(hour.Split(':')[0]);
                        var queue = 0d;

                        if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                            if (holidayInfo.Increase == 1) {
                                queue = (newOutputQueue / counterQueue) * (1 + (holidayInfo.Percentage / 100));
                            }
                            else {
                                queue = (newOutputQueue / counterQueue) * (1 - (holidayInfo.Percentage / 100));
                            }
                        }
                        else {
                            queue = newOutputQueue / counterQueue;
                        }
                        forcastData.Add(new ForecastDiskData {
                            Hour = hour,
                            DeviceName = deviceName,
                            QueueLength = queue,
                            StdDevQueueLength = stdDevQueue
                        });
                    }
                    else {
                        forcastData.Add(new ForecastDiskData {
                            Hour = hour,
                            DeviceName = deviceName,
                            QueueLength = newOutputQueue / counterQueue,
                            StdDevQueueLength = stdDevQueue
                        });
                    }
                }
            }
            */
            #endregion
            #endregion

            return forcastData;
        }

        public List<ForecastDiskData> GetForecastDiskDP2(DateTime startTime, DateTime stopTime, List<DateTime> specialDayList) {
            var forcastData = new List<ForecastDiskData>();
            var newStartTime = startTime;
            var detailDiskForForecast = new DetailDiskForForecast(_systemConnectionString);
            var diskData = new DataTable();

            if (specialDayList.Count == 0) {
                //Goes back 12 weeks.
                for (var x = 0; x < 12; x++) {
                    newStartTime = newStartTime.AddDays(-7);
                }
                diskData = detailDiskForForecast.GetQueueLength(newStartTime, stopTime.AddDays(1));
            }
            else {
                var tempDiskTable = new DataTable();
                foreach (var specialDay in specialDayList) {
                    var diskForecast = detailDiskForForecast.GetQueueLength(specialDay, specialDay.AddDays(1));
                    if (diskForecast.Rows.Count > 0)
                        tempDiskTable.Merge(diskForecast);
                }

                diskData = tempDiskTable.AsEnumerable().OrderBy(x => x.Field<string>("DeviceName")).ThenBy(x => x.Field<string>("Hour")).CopyToDataTable();
            }

            #region Disk
            var holidayServices = new HolidayService(_mainConnectionString);
            var holidayInfo = holidayServices.GetWorkDayFactorFor(_systemSerial, startTime);


            //var hourIndex = 0;
            var uniqueDeviceName = "";
            var uniqueHour = "";
            var storedQueue = new List<double>();

            uniqueDeviceName = diskData.Rows[0]["DeviceName"].ToString();
            uniqueHour = diskData.Rows[0]["Hour"].ToString();

            foreach (DataRow row in diskData.Rows) {
                var deviceName = row["DeviceName"].ToString();
                var hour = row["Hour"].ToString();

                if (uniqueDeviceName == deviceName && hour == uniqueHour) {
                    storedQueue.Add(Convert.ToDouble(row["QueueLength"]));
                }
                else {
                    #region Get the STV Value.

                    var avgQueueDataPerCpu = storedQueue.Average();

                    var outputQueue = 0d;

                    foreach (var subData in storedQueue) {
                        outputQueue += Math.Pow(subData - avgQueueDataPerCpu, 2);
                    }

                    var stdDevQueue = Math.Sqrt(outputQueue / storedQueue.Count());

                    var highRangeQueue = avgQueueDataPerCpu + stdDevQueue;
                    var lowRangeQueue = avgQueueDataPerCpu - stdDevQueue;

                    var newOutputQueue = 0d;
                    var counterQueue = 0;

                    foreach (var subData in storedQueue) {
                        if (subData <= highRangeQueue && subData >= lowRangeQueue) {
                            newOutputQueue += subData;
                            counterQueue++;
                        }
                    }

                    //parse hour.
                    if (holidayInfo.HasData) {
                        var newHour = Convert.ToInt32(uniqueHour.Split(':')[0]);
                        var cpuBusy = 0d;
                        var queue = 0d;

                        if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                            if (holidayInfo.Increase == 1) {
                                queue = (newOutputQueue / counterQueue) * (1 + (holidayInfo.Percentage / 100));
                            }
                            else {
                                queue = (newOutputQueue / counterQueue) * (1 - (holidayInfo.Percentage / 100));
                            }
                        }
                        else {
                            queue = newOutputQueue / counterQueue;
                        }
                        forcastData.Add(new ForecastDiskData {
                            Hour = uniqueHour,
                            DeviceName = uniqueDeviceName,
                            QueueLength = queue,
                            StdDevQueueLength = stdDevQueue
                        });
                    }
                    else {

                        forcastData.Add(new ForecastDiskData {
                            Hour = uniqueHour,
                            DeviceName = uniqueDeviceName,
                            QueueLength = newOutputQueue / counterQueue,
                            StdDevQueueLength = stdDevQueue
                        });
                    }
                    #endregion

                    uniqueHour = hour;
                    uniqueDeviceName = deviceName;
                    storedQueue = new List<double>();
                    storedQueue.Add(Convert.ToDouble(row["QueueLength"]));
                }
            }
            //Load the last entry.
            #region Get the STV Value.

            var avgQueueDataPerCpuE = storedQueue.Average();

            var outputQueueE = 0d;

            foreach (var subData in storedQueue) {
                outputQueueE += Math.Pow(subData - avgQueueDataPerCpuE, 2);
            }

            var stdDevQueueE = Math.Sqrt(outputQueueE / storedQueue.Count());

            var highRangeQueueE = avgQueueDataPerCpuE + stdDevQueueE;
            var lowRangeQueueE = avgQueueDataPerCpuE - stdDevQueueE;

            var newOutputQueueE = 0d;

            var counterQueueE = 0;

            foreach (var subData in storedQueue) {
                if (subData <= highRangeQueueE && subData >= lowRangeQueueE) {
                    newOutputQueueE += subData;
                    counterQueueE++;
                }
            }

            //parse hour.
            if (holidayInfo.HasData) {
                var newHour = Convert.ToInt32(uniqueHour.Split(':')[0]);
                var queue = 0d;

                if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                    if (holidayInfo.Increase == 1) {
                        queue = (newOutputQueueE / counterQueueE) * (1 + (holidayInfo.Percentage / 100));
                    }
                    else {
                        queue = (newOutputQueueE / counterQueueE) * (1 - (holidayInfo.Percentage / 100));
                    }
                }
                else {
                    queue = newOutputQueueE / counterQueueE;
                }
                forcastData.Add(new ForecastDiskData {
                    Hour = uniqueHour,
                    DeviceName = uniqueDeviceName,
                    QueueLength = queue,
                    StdDevQueueLength = stdDevQueueE
                });
            }
            else {
                forcastData.Add(new ForecastDiskData {
                    Hour = uniqueHour,
                    DeviceName = uniqueDeviceName,
                    QueueLength = newOutputQueueE / counterQueueE,
                    StdDevQueueLength = stdDevQueueE
                });
            }
            #endregion
            #region Old Code
            /*
            var deviceNames = diskData.AsEnumerable().Select(x => x.Field<string>("DeviceName")).Distinct().ToList();
            var hours = diskData.AsEnumerable().Select(x => x.Field<DateTime>("FromTimestamp").ToString("HH:mm")).Distinct().OrderBy(x => x).ToList();

            foreach (var deviceName in deviceNames) {
                foreach (var hour in hours) {
                    var subDataPer = diskData.AsEnumerable().Where(x => x.Field<string>("DeviceName").Equals(deviceName) && x.Field<DateTime>("FromTimestamp").ToString("HH:mm").Equals(hour))
                        .Select(x => new {
                            DateTime = x.Field<DateTime>("FromTimestamp"),
                            QueueLength = x.Field<double>("QueueLength")
                        });

                    var avgQueueDataPerDevice = subDataPer.Average(x => x.QueueLength);

                    var outputQueue = 0d;
                    foreach (var subData in subDataPer) {
                        outputQueue += Math.Pow(subData.QueueLength - avgQueueDataPerDevice, 2);
                    }

                    var stdDevQueue = Math.Sqrt(outputQueue / subDataPer.Count());

                    var highRangeQueue = avgQueueDataPerDevice + stdDevQueue;
                    var lowRangeQueue = avgQueueDataPerDevice - stdDevQueue;

                    var newOutputQueue = 0d;

                    var counterQueue = 0;

                    foreach (var subData in subDataPer) {
                        if (subData.QueueLength <= highRangeQueue && subData.QueueLength >= lowRangeQueue) {
                            newOutputQueue += subData.QueueLength;
                            counterQueue++;
                        }
                    }

                    //parse hour.
                    if (holidayInfo.HasData) {
                        var newHour = Convert.ToInt32(hour.Split(':')[0]);
                        var queue = 0d;

                        if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                            if (holidayInfo.Increase == 1) {
                                queue = (newOutputQueue / counterQueue) * (1 + (holidayInfo.Percentage / 100));
                            }
                            else {
                                queue = (newOutputQueue / counterQueue) * (1 - (holidayInfo.Percentage / 100));
                            }
                        }
                        else {
                            queue = newOutputQueue / counterQueue;
                        }
                        forcastData.Add(new ForecastDiskData {
                            Hour = hour,
                            DeviceName = deviceName,
                            QueueLength = queue,
                            StdDevQueueLength = stdDevQueue
                        });
                    }
                    else {
                        forcastData.Add(new ForecastDiskData {
                            Hour = hour,
                            DeviceName = deviceName,
                            QueueLength = newOutputQueue / counterQueue,
                            StdDevQueueLength = stdDevQueue
                        });
                    }
                }
            }
            */
            #endregion
            #endregion

            return forcastData;
        }


        public List<ForecastStorageData> GetForecastStorage(DateTime startTime, DateTime stopTime, List<DateTime> specialDayList) {
            var forcastData = new List<ForecastStorageData>();
            var newStartTime = startTime;
            var dailyDisk = new DailyDisk(_systemConnectionString);
            var diskData = new DataTable();

            if (specialDayList.Count == 0) {
                //Goes back 12 weeks.
                for (var x = 0; x < 12; x++) {
                    newStartTime = newStartTime.AddDays(-7);
                }
                diskData = dailyDisk.GetDailyDiskInfo(newStartTime, stopTime.AddDays(1));
            }
            else {
                foreach (var specialDay in specialDayList) {
                    var diskInfo = dailyDisk.GetDailyDiskInfo(specialDay, specialDay.AddDays(1));

                    if (diskInfo.Rows.Count > 0) {
                        diskData.Merge(diskInfo);
                    }
                }
            }

            #region Disk
            //var holidayServices = new HolidayService(_mainConnectionString);
            //var holidayInfo = holidayServices.GetWorkDayFactorFor(_systemSerial, startTime);


            var deviceNames = diskData.AsEnumerable().Select(x => x.Field<string>("DeviceName")).Distinct().ToList();

            //var days = diskData.AsEnumerable().Select(x => x.Field<DateTime>("FromTimestamp")).Distinct().OrderBy(x => x).ToList();

            foreach (var deviceName in deviceNames) {
                //foreach (var day in days) {
                    var subDataPer = diskData.AsEnumerable().Where(x => x.Field<string>("DeviceName").Equals(deviceName) /*&& x.Field<DateTime>("FromTimestamp").Equals(day)*/)
                        .Select(x => new {
                            DateTime = x.Field<DateTime>("FromTimestamp"),
                            UsedPercent = x.Field<double>("UsedPercent")
                        });

                    var avgQueueDataPerDevice = subDataPer.Average(x => x.UsedPercent);

                    var outputQueue = 0d;
                    foreach (var subData in subDataPer) {
                        outputQueue += Math.Pow(subData.UsedPercent - avgQueueDataPerDevice, 2);
                    }

                    var stdDevQueue = Math.Sqrt(outputQueue / subDataPer.Count());

                    var highRangeQueue = avgQueueDataPerDevice + stdDevQueue;
                    var lowRangeQueue = avgQueueDataPerDevice - stdDevQueue;

                    var newOutputQueue = 0d;

                    var counterQueue = 0;

                    foreach (var subData in subDataPer) {
                        if (subData.UsedPercent <= highRangeQueue && subData.UsedPercent >= lowRangeQueue) {
                            newOutputQueue += subData.UsedPercent;
                            counterQueue++;
                        }
                    }

                    //parse hour.
                    /*if (holidayInfo.HasData) {
                        var newHour = Convert.ToInt32(hour.Split(':')[0]);
                        var queue = 0d;

                        if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                            if (holidayInfo.Increase == 1) {
                                queue = (newOutputQueue / counterQueue) * (1 + (holidayInfo.Percentage / 100));
                            }
                            else {
                                queue = (newOutputQueue / counterQueue) * (1 - (holidayInfo.Percentage / 100));
                            }
                        }
                        else {
                            queue = newOutputQueue / counterQueue;
                        }
                        forcastData.Add(new ForecastStorageData {
                            DeviceName = deviceName,
                            UsedPercent = queue,
                            StdDevUsedPercent = stdDevQueue
                        });
                    }
                    else {*/

                    forcastData.Add(new ForecastStorageData {
                        ForecastDateTime = startTime,
                        DeviceName = deviceName,
                        UsedPercent = newOutputQueue / counterQueue,
                        StdDevUsedPercent = stdDevQueue
                    });
                    //}
                //}
            }
            #endregion

            return forcastData;
        }
        

        public List<ForecastProcessData> GetForecastProcess(DateTime startTime, DateTime stopTime) {
            var forcastData = new List<ForecastProcessData>();
            var newStartTime = startTime;
            var detailProcessForForecast = new DetailProcessForForecast(_systemConnectionString);

            //Goes back 12 weeks.
            for (var x = 0; x < 12; x++) {
                newStartTime = newStartTime.AddDays(-7);
            }

            #region CPU
            var processData = new DataTable();
            var holidayServices = new HolidayService(_mainConnectionString);
            var holidayInfo = holidayServices.GetWorkDayFactorFor(_systemSerial, startTime);

            var orderedTable = detailProcessForForecast.GetProcessData(newStartTime, stopTime.AddDays(1));
            

            //var cpuNumbers = processData.AsEnumerable().Select(x => x.Field<int>("CPUNumber")).Distinct().ToList();
            /*var orderedTable = processData.AsEnumerable().OrderBy(x => x.Field<string>("ProcessName"))
                                                        .ThenBy(x => x.Field<string>("CpuNumber"))
                                                        .ThenBy(x => x.Field<int>("Pin"))
                                                        .ThenBy(x => x.Field<string>("Volume"))
                                                        .ThenBy(x => x.Field<string>("SubVol"))
                                                        .ThenBy(x => x.Field<string>("FileName"))
                                                        .ThenBy(x => x.Field<long>("Hour")).CopyToDataTable();*/

            //int hourIndex = 0;
            var uniqueProcessName = "";
            var uniqueCpuNum = "";
            var uniquePin = "";
            var uniqueVolume = "";
            var uniqueSubVol = "";
            var uniqueFileName = "";
            var uniqueHour = "";

            var storedBusy = new List<double>();
            var storedQueue = new List<double>();

            uniqueProcessName = orderedTable.Rows[0]["ProcessName"].ToString();
            uniqueCpuNum = orderedTable.Rows[0]["CpuNumber"].ToString();
            uniquePin = orderedTable.Rows[0]["Pin"].ToString();
            uniqueVolume = orderedTable.Rows[0]["Volume"].ToString();
            uniqueSubVol = orderedTable.Rows[0]["SubVol"].ToString();
            uniqueFileName = orderedTable.Rows[0]["FileName"].ToString();
            uniqueHour = orderedTable.Rows[0]["Hour"].ToString();

            foreach (DataRow row in orderedTable.Rows) {
                var processName = row["ProcessName"].ToString();
                var cpuNum = row["CpuNumber"].ToString();
                var pin = row["Pin"].ToString();
                var volume = row["Volume"].ToString();
                var subVol = row["SubVol"].ToString();
                var fileName = row["FileName"].ToString();
                var hour = row["Hour"].ToString();
                
                if ((uniqueProcessName == processName && 
                    uniqueCpuNum == cpuNum &&
                    uniquePin == pin &&
                    uniqueVolume == volume &&
                    uniqueSubVol == subVol &&
                    uniqueFileName == fileName) && uniqueHour == hour) {
                    storedBusy.Add(Convert.ToDouble(row["ProcessBusy"]));
                    storedQueue.Add(Convert.ToDouble(row["RecvQueueLength"]));
                }
                else {

                    #region Get the STV Value.

                    var avgCPUDataPerCpu = storedBusy.Average();
                    var avgQueueDataPerCpu = storedQueue.Average();

                    var output = 0d;
                    var outputQueue = 0d;
                    foreach (var subData in storedBusy) {
                        output += Math.Pow(subData - avgCPUDataPerCpu, 2);
                    }

                    foreach (var subData in storedQueue) {
                        outputQueue += Math.Pow(subData - avgQueueDataPerCpu, 2);
                    }

                    var stdDevCPU = Math.Sqrt(output / storedBusy.Count());
                    var stdDevQueue = Math.Sqrt(outputQueue / storedQueue.Count());

                    var highRangeCPU = avgCPUDataPerCpu + stdDevCPU;
                    var lowRangeCPU = avgCPUDataPerCpu - stdDevCPU;
                    var highRangeQueue = avgQueueDataPerCpu + stdDevQueue;
                    var lowRangeQueue = avgQueueDataPerCpu - stdDevQueue;

                    var newOutputCPU = 0d;
                    var newOutputQueue = 0d;

                    var counterCPU = 0;
                    var counterQueue = 0;

                    foreach (var subData in storedBusy) {
                        if (subData <= highRangeCPU && subData >= lowRangeCPU) {
                            newOutputCPU += subData;
                            counterCPU++;
                        }
                    }

                    foreach (var subData in storedQueue) {
                        if (subData <= highRangeQueue && subData >= lowRangeQueue) {
                            newOutputQueue += subData;
                            counterQueue++;
                        }
                    }

                    //parse hour.
                    if (holidayInfo.HasData) {
                        var newHour = Convert.ToInt32(uniqueHour.Split(':')[0]);
                        var cpuBusy = 0d;
                        var queue = 0d;

                        if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                            if (holidayInfo.Increase == 1) {
                                cpuBusy = (newOutputCPU / counterCPU) * (1 + (holidayInfo.Percentage / 100));
                                queue = (newOutputQueue / counterQueue) * (1 + (holidayInfo.Percentage / 100));
                            }
                            else {
                                cpuBusy = (newOutputCPU / counterCPU) * (1 - (holidayInfo.Percentage / 100));
                                queue = (newOutputQueue / counterQueue) * (1 - (holidayInfo.Percentage / 100));
                            }
                        }
                        else {
                            cpuBusy = newOutputCPU / counterCPU;
                            queue = newOutputQueue / counterQueue;
                        }
                        forcastData.Add(new ForecastProcessData {
                            Hour = uniqueHour,
                            ProcessName = uniqueProcessName,
                            CpuNumber = Convert.ToInt32(uniqueCpuNum),
                            Pin = Convert.ToInt32(uniquePin),
                            Volume = uniqueVolume,
                            SubVol = uniqueSubVol,
                            FileName = uniqueFileName,
                            ProcessBusy = cpuBusy,
                            StdDevProcessBusy = stdDevCPU,
                            RecvQueueLength = queue,
                            StdDevRecvQueueLength = stdDevQueue
                        });
                    }
                    else {
                        forcastData.Add(new ForecastProcessData {
                            Hour = uniqueHour,
                            ProcessName = uniqueProcessName,
                            CpuNumber = Convert.ToInt32(uniqueCpuNum),
                            Pin = Convert.ToInt32(uniquePin),
                            Volume = uniqueVolume,
                            SubVol = uniqueSubVol,
                            FileName = uniqueFileName,
                            ProcessBusy = newOutputCPU / counterCPU,
                            StdDevProcessBusy = stdDevCPU,
                            RecvQueueLength = newOutputQueue / counterQueue,
                            StdDevRecvQueueLength = stdDevQueue
                        });
                    }
                    #endregion
                    
                    uniqueHour = hour;
                    uniqueProcessName = processName;
                    uniqueCpuNum = cpuNum;
                    uniquePin = pin;
                    uniqueVolume = volume;
                    uniqueSubVol = subVol;
                    uniqueFileName = fileName;
                    
                    storedBusy = new List<double>();
                    storedQueue = new List<double>();
                    storedBusy.Add(Convert.ToDouble(row["ProcessBusy"]));
                    storedQueue.Add(Convert.ToDouble(row["RecvQueueLength"]));
                }
            }

            //Load the last entry.
            #region Get the STV Value.

            var avgCPUDataPerCpuE = storedBusy.Average();
            var avgQueueDataPerCpuE = storedQueue.Average();

            var outputE = 0d;
            var outputQueueE = 0d;
            foreach (var subData in storedBusy) {
                outputE += Math.Pow(subData - avgCPUDataPerCpuE, 2);
            }

            foreach (var subData in storedQueue) {
                outputQueueE += Math.Pow(subData - avgQueueDataPerCpuE, 2);
            }

            var stdDevCPUE = Math.Sqrt(outputE / storedBusy.Count());
            var stdDevQueueE = Math.Sqrt(outputQueueE / storedQueue.Count());

            var highRangeCPUE = avgCPUDataPerCpuE + stdDevCPUE;
            var lowRangeCPUE = avgCPUDataPerCpuE - stdDevCPUE;
            var highRangeQueueE = avgQueueDataPerCpuE + stdDevQueueE;
            var lowRangeQueueE = avgQueueDataPerCpuE - stdDevQueueE;

            var newOutputCPUE = 0d;
            var newOutputQueueE = 0d;

            var counterCPUE = 0;
            var counterQueueE = 0;

            foreach (var subData in storedBusy) {
                if (subData <= highRangeCPUE && subData >= lowRangeCPUE) {
                    newOutputCPUE += subData;
                    counterCPUE++;
                }
            }

            foreach (var subData in storedQueue) {
                if (subData <= highRangeQueueE && subData >= lowRangeQueueE) {
                    newOutputQueueE += subData;
                    counterQueueE++;
                }
            }

            //parse hour.
            if (holidayInfo.HasData) {
                var newHour = Convert.ToInt32(uniqueHour.Split(':')[0]);
                var cpuBusy = 0d;
                var queue = 0d;

                if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                    if (holidayInfo.Increase == 1) {
                        cpuBusy = (newOutputCPUE / counterCPUE) * (1 + (holidayInfo.Percentage / 100));
                        queue = (newOutputQueueE / counterQueueE) * (1 + (holidayInfo.Percentage / 100));
                    }
                    else {
                        cpuBusy = (newOutputCPUE / counterCPUE) * (1 - (holidayInfo.Percentage / 100));
                        queue = (newOutputQueueE / counterQueueE) * (1 - (holidayInfo.Percentage / 100));
                    }
                }
                else {
                    cpuBusy = newOutputCPUE / counterCPUE;
                    queue = newOutputQueueE / counterQueueE;
                }
                forcastData.Add(new ForecastProcessData {
                    Hour = uniqueHour,
                    ProcessName = uniqueProcessName,
                    CpuNumber = Convert.ToInt32(uniqueCpuNum),
                    Pin = Convert.ToInt32(uniquePin),
                    Volume = uniqueVolume,
                    SubVol = uniqueSubVol,
                    FileName = uniqueFileName,
                    ProcessBusy = cpuBusy,
                    StdDevProcessBusy = stdDevCPUE,
                    RecvQueueLength = queue,
                    StdDevRecvQueueLength = stdDevQueueE
                });
            }
            else {
                forcastData.Add(new ForecastProcessData {
                    Hour = uniqueHour,
                    ProcessName = uniqueProcessName,
                    CpuNumber = Convert.ToInt32(uniqueCpuNum),
                    Pin = Convert.ToInt32(uniquePin),
                    Volume = uniqueVolume,
                    SubVol = uniqueSubVol,
                    FileName = uniqueFileName,
                    ProcessBusy = newOutputCPUE / counterCPUE,
                    StdDevProcessBusy = stdDevCPUE,
                    RecvQueueLength = newOutputQueueE / counterQueueE,
                    StdDevRecvQueueLength = stdDevQueueE
                });
            }
            #endregion
            /*
            var hours = processData.AsEnumerable().Select(x => x.Field<DateTime>("FromTimestamp").ToString("HH:mm")).Distinct().OrderBy(x => x).ToList();

            foreach (var row in uniqueRows) {
                foreach (var hour in hours) {
                    var subDataPer = processData.AsEnumerable().Where(x => x.Field<string>("ProcessName").Equals(row.ProcessName) &&
                                                                           x.Field<string>("CpuNumber").Equals(row.CpuNum) &&
                                                                           x.Field<int>("Pin").Equals(row.Pin) &&
                                                                           x.Field<string>("Volume").Equals(row.Volume) &&
                                                                           x.Field<string>("SubVol").Equals(row.SubVol) &&
                                                                           x.Field<string>("FileName").Equals(row.FileName) &&
                                                                        x.Field<DateTime>("FromTimestamp").ToString("HH:mm").Equals(hour))
                                                                .Select(x => new {
                                                                    DateTime = x.Field<DateTime>("FromTimestamp"),
                                                                    Busy = x.Field<double>("ProcessBusy"),
                                                                    Queue = x.Field<double>("RecvQueueLength")
                                                                });

                    var avgCPUDataPerCpu = subDataPer.Average(x => x.Busy);
                    var avgQueueDataPerCpu = subDataPer.Average(x => x.Queue);

                    var output = 0d;
                    var outputQueue = 0d;
                    foreach (var subData in subDataPer) {
                        output += Math.Pow(subData.Busy - avgCPUDataPerCpu, 2);
                        outputQueue += Math.Pow(subData.Queue - avgQueueDataPerCpu, 2);
                    }

                    var stdDevCPU = Math.Sqrt(output / subDataPer.Count());
                    var stdDevQueue = Math.Sqrt(outputQueue / subDataPer.Count());

                    var highRangeCPU = avgCPUDataPerCpu + stdDevCPU;
                    var lowRangeCPU = avgCPUDataPerCpu - stdDevCPU;
                    var highRangeQueue = avgQueueDataPerCpu + stdDevQueue;
                    var lowRangeQueue = avgQueueDataPerCpu - stdDevQueue;

                    var newOutputCPU = 0d;
                    var newOutputQueue = 0d;

                    var counterCPU = 0;
                    var counterQueue = 0;

                    foreach (var subData in subDataPer) {
                        if (subData.Busy <= highRangeCPU && subData.Busy >= lowRangeCPU) {
                            newOutputCPU += subData.Busy;
                            counterCPU++;
                        }
                        if (subData.Queue <= highRangeQueue && subData.Queue >= lowRangeQueue) {
                            newOutputQueue += subData.Queue;
                            counterQueue++;
                        }
                    }
                    //parse hour.
                    if (holidayInfo.HasData) {
                        var newHour = Convert.ToInt32(hour.Split(':')[0]);
                        var cpuBusy = 0d;
                        var queue = 0d;

                        if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                            if (holidayInfo.Increase == 1) {
                                cpuBusy = (newOutputCPU / counterCPU) * (1 + (holidayInfo.Percentage / 100));
                                queue = (newOutputQueue / counterQueue) * (1 + (holidayInfo.Percentage / 100));
                            }
                            else {
                                cpuBusy = (newOutputCPU / counterCPU) * (1 - (holidayInfo.Percentage / 100));
                                queue = (newOutputQueue / counterQueue) * (1 - (holidayInfo.Percentage / 100));
                            }
                        }
                        else {
                            cpuBusy = newOutputCPU / counterCPU;
                            queue = newOutputQueue / counterQueue;
                        }
                        forcastData.Add(new ForecastProcessData() {
                            Hour = hour,
                            ProcessName = row.ProcessName,
                            CpuNumber = Convert.ToInt32(row.CpuNum),
                            Pin = row.Pin,
                            Volume = row.Volume,
                            SubVol = row.SubVol,
                            FileName = row.FileName,
                            ProcessBusy = cpuBusy,
                            StdDevProcessBusy = stdDevCPU,
                            RecvQueueLength = queue,
                            StdDevRecvQueueLength = stdDevQueue
                        });
                    }
                    else {
                        forcastData.Add(new ForecastProcessData {
                            Hour = hour,
                            ProcessName = row.ProcessName,
                            CpuNumber = Convert.ToInt32(row.CpuNum),
                            Pin = row.Pin,
                            Volume = row.Volume,
                            SubVol = row.SubVol,
                            FileName = row.FileName,
                            ProcessBusy = newOutputCPU / counterCPU,
                            StdDevProcessBusy = stdDevCPU,
                            RecvQueueLength = newOutputQueue / counterQueue,
                            StdDevRecvQueueLength = stdDevQueue
                        });
                    }
                }
            }
            */
            #endregion

            return forcastData;
        }
        public List<ForecastTMFData> GetForecastTmf(DateTime startTime, DateTime stopTime) {
            var forcastData = new List<ForecastTMFData>();
            var newStartTime = startTime;
            var detailTmfForForecast = new DetailTmfForForecast(_systemConnectionString);

            //Goes back 12 weeks.
            for (var x = 0; x < 12; x++) {
                newStartTime = newStartTime.AddDays(-7);
            }

            #region CPU
            var processData = new DataTable();
            var holidayServices = new HolidayService(_mainConnectionString);
            var holidayInfo = holidayServices.GetWorkDayFactorFor(_systemSerial, startTime);

            processData = detailTmfForForecast.GetTmfData(newStartTime, stopTime.AddDays(1));
            
            //var cpuNumbers = processData.AsEnumerable().Select(x => x.Field<int>("CPUNumber")).Distinct().ToList();
            var uniqueRows = processData.AsEnumerable().Select(x => new {
                                                        ProcessName = x.Field<string>("ProcessName"),
                                                        CpuNum = x.Field<string>("CpuNumber"),
                                                        Pin = x.Field<int>("Pin"),
                                                        Volume = x.Field<string>("Volume"),
                                                        SubVol = x.Field<string>("SubVol"),
                                                        FileName = x.Field<string>("FileName")
                                                    }).Distinct().ToList();

            var hours = processData.AsEnumerable().Select(x => x.Field<DateTime>("FromTimestamp").ToString("HH:mm")).Distinct().OrderBy(x => x).ToList();

            foreach (var row in uniqueRows) {
                foreach (var hour in hours) {
                    var subDataPer = processData.AsEnumerable().Where(x => x.Field<string>("ProcessName").Equals(row.ProcessName) &&
                                                                           x.Field<string>("CpuNumber").Equals(row.CpuNum) &&
                                                                           x.Field<int>("Pin").Equals(row.Pin) &&
                                                                           x.Field<string>("Volume").Equals(row.Volume) &&
                                                                           x.Field<string>("SubVol").Equals(row.SubVol) &&
                                                                           x.Field<string>("FileName").Equals(row.FileName) &&
                                                                           x.Field<DateTime>("FromTimestamp").ToString("HH:mm").Equals(hour))
                        .Select(x => new {
                            DateTime = x.Field<DateTime>("FromTimestamp"),
                            Busy = x.Field<double>("AbortPercent")
                        });

                    if (subDataPer.Count() > 0) {
                        var avgCPUDataPerCpu = subDataPer.Average(x => x.Busy);

                        var output = 0d;
                        foreach (var subData in subDataPer) {
                            output += Math.Pow(subData.Busy - avgCPUDataPerCpu, 2);
                        }

                        var stdDevCPU = Math.Sqrt(output / subDataPer.Count());

                        var highRangeCPU = avgCPUDataPerCpu + stdDevCPU;
                        var lowRangeCPU = avgCPUDataPerCpu - stdDevCPU;

                        var newOutputCPU = 0d;

                        var counterCPU = 0;

                        foreach (var subData in subDataPer) {
                            if (subData.Busy <= highRangeCPU && subData.Busy >= lowRangeCPU) {
                                newOutputCPU += subData.Busy;
                                counterCPU++;
                            }
                        }
                        //parse hour.
                        if (holidayInfo.HasData) {
                            var newHour = Convert.ToInt32(hour.Split(':')[0]);
                            var cpuBusy = 0d;

                            if (newHour >= holidayInfo.FromHour && newHour < holidayInfo.ToHour) {
                                if (holidayInfo.Increase == 1) {
                                    cpuBusy = (newOutputCPU / counterCPU) * (1 + (holidayInfo.Percentage / 100));
                                }
                                else {
                                    cpuBusy = (newOutputCPU / counterCPU) * (1 - (holidayInfo.Percentage / 100));
                                }
                            }
                            else {
                                cpuBusy = newOutputCPU / counterCPU;
                            }
                            forcastData.Add(new ForecastTMFData() {
                                Hour = hour,
                                ProcessName = row.ProcessName,
                                CpuNumber = Convert.ToInt32(row.CpuNum),
                                Pin = row.Pin,
                                Volume = row.Volume,
                                SubVol = row.SubVol,
                                FileName = row.FileName,
                                AbortPercent = cpuBusy,
                                StdDevAbortPercent = stdDevCPU
                            });
                        }
                        else {
                            forcastData.Add(new ForecastTMFData {
                                Hour = hour,
                                ProcessName = row.ProcessName,
                                CpuNumber = Convert.ToInt32(row.CpuNum),
                                Pin = row.Pin,
                                Volume = row.Volume,
                                SubVol = row.SubVol,
                                FileName = row.FileName,
                                AbortPercent = newOutputCPU / counterCPU,
                                StdDevAbortPercent = stdDevCPU
                            });
                        }
                    }
                }
            }
            #endregion

            return forcastData;
        }


        public List<ForecastData> GetForecastCpuDataFor(DateTime startTime, DateTime stopTime) {
            var forecastData = new List<ForecastData>();

            try {
                var forecast = new ForecastRepository(_systemConnectionString);
                var forecastInfo = forecast.GetForecastData(startTime, stopTime);

                if (forecastInfo.Rows.Count > 0) {
                    foreach (DataRow row in forecastInfo.Rows) {
                        forecastData.Add(new ForecastData {
                            ForecastDateTime = Convert.ToDateTime(row["FromTimestamp"]),
                            CpuNumber = Convert.ToInt32(row["CpuNumber"]),
                            CpuBusy = Convert.ToDouble(row["CPUBusy"]),
                            MemoryUsed = Convert.ToDouble(row["MemoryUsed"]),
                            Queue = Convert.ToDouble(row["CPUQueue"]),
                            StdDevCpuBusy = Convert.ToDouble(row["StdDevCPUBusy"]),
                            StdDevQueue = Convert.ToDouble(row["StdDevCPUQueue"]),
                            StdDevMemoryUsed = Convert.ToDouble(row["StdDevMemoryUsed"])
                        });
                    }
                }
            }
            catch { }

            return forecastData;
        }

        public List<ForecastData> GetForecastIpuDataFor(DateTime startTime, DateTime stopTime) {
            var forecastData = new List<ForecastData>();

            try {
                var forecast = new ForecastRepository(_systemConnectionString);
                var forecastInfo = forecast.GetForecastIpuData(startTime, stopTime);

                if (forecastInfo.Rows.Count > 0) {
                    foreach (DataRow row in forecastInfo.Rows) {
                        forecastData.Add(new ForecastData {
                            ForecastDateTime = Convert.ToDateTime(row["FromTimestamp"]),
                            CpuNumber = Convert.ToInt32(row["CpuNumber"]),
                            IpuNumber = Convert.ToInt32(row["IpuNumber"]),
                            IpuBusy = Convert.ToDouble(row["IpuBusy"]),
                            IpuQueue = Convert.ToDouble(row["IpuQueue"]),
                            StdDevIpuBusy = Convert.ToDouble(row["StdDevIpuBusy"]),
                            StdDevIpuQueue = Convert.ToDouble(row["StdDevIpuQueue"])
                        });
                    }
                }
            }
            catch { }

            return forecastData;
        }

        public void StoreForecastData(List<ForecastData> forecastData, DateTime startTime, DateTime endTime, long interval, string tempSaveLocation) {
            if (interval == 0) return; //Defensive code. Since if interval is zero, then the loop below will never exit
            for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                if (forecastData.Any(x => x.Hour.Equals(start.ToString("HH:mm")))) {
                    var currentVales = forecastData.Where(x => x.Hour.Equals(start.ToString("HH:mm")));
                    foreach (var val in currentVales) {
                        val.ForecastDateTime = start;
                    }
                }
            }

            var ordedrdForecastData = forecastData.OrderBy(x => x.ForecastDateTime);

            if (ordedrdForecastData.Any()) {
                var tableName = "Forecasts";
                var databaseName = Helper.FindKeyName(_systemConnectionString, "DATABASE");
                //Insert into the database.
                var databaseCheck = new Database(_systemConnectionString);
                var exists = databaseCheck.CheckTableExists(tableName, databaseName);

                if (!exists) {
                    databaseCheck.CreateForecastsTable();
                }

                var pathToCsv = tempSaveLocation + "BulkInsert_" + DateTime.Now.Ticks + ".csv";
                var sb = new StringBuilder();

                foreach (var forecast in ordedrdForecastData) {
                    sb.Append(forecast.ForecastDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "|" +
                              forecast.CpuNumber + "|" +
                              forecast.CpuBusy + "|" +
                              forecast.MemoryUsed + "|" +
                              forecast.Queue + "|" +
                              forecast.StdDevCpuBusy + "|" +
                              forecast.StdDevMemoryUsed + "|" +
                              forecast.StdDevQueue + Environment.NewLine);
                }
                File.AppendAllText(pathToCsv, sb.ToString());

                var dataTables = new DataTables(_systemConnectionString);
                dataTables.InsertForecastData(tableName, pathToCsv);
            }
        }

        public void StoreForecastIpuData(List<ForecastData> forecastData, DateTime startTime, DateTime endTime, long interval, string tempSaveLocation) {
            if (interval == 0) return; //Defensive code. Since if interval is zero, then the loop below will never exit
            for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                if (forecastData.Any(x => x.Hour.Equals(start.ToString("HH:mm")))) {
                    var currentVales = forecastData.Where(x => x.Hour.Equals(start.ToString("HH:mm")));
                    foreach (var val in currentVales) {
                        val.ForecastDateTime = start;
                    }
                }
            }

            var ordedrdForecastData =
                forecastData.OrderBy(x => x.ForecastDateTime).ThenBy(x => x.CpuNumber).ThenBy(x => x.IpuNumber);

            if (ordedrdForecastData.Any()) {
                var tableName = "ForecastIpus";
                var databaseName = Helper.FindKeyName(_systemConnectionString, "DATABASE");
                //Insert into the database.
                var databaseCheck = new Database(_systemConnectionString);
                var exists = databaseCheck.CheckTableExists(tableName, databaseName);

                if (!exists) {
                    databaseCheck.CreateForecastIpusTable();
                }

                var pathToCsv = tempSaveLocation + "BulkInsert_" + DateTime.Now.Ticks + ".csv";
                var sb = new StringBuilder();

                foreach (var forecast in ordedrdForecastData) {
                    sb.Append(forecast.ForecastDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "|" +
                              forecast.CpuNumber + "|" +
                              forecast.IpuNumber + "|" +
                              forecast.CpuBusy + "|" +
                              forecast.Queue + "|" +
                              forecast.StdDevIpuBusy + "|" +
                              forecast.StdDevIpuQueue + Environment.NewLine);
                }
                File.AppendAllText(pathToCsv, sb.ToString());

                var dataTables = new DataTables(_systemConnectionString);
                dataTables.InsertForecastData(tableName, pathToCsv);
            }
        }

        public void StoreForecastDiskData(List<ForecastDiskData> forecastDiskData, DateTime startTime, DateTime endTime, long interval, string tempSaveLocation) {
            if (interval == 0) return; //Defensive code. Since if interval is zero, then the loop below will never exit
            for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                if (forecastDiskData.Any(x => x.Hour.Equals(start.ToString("HH:mm")))) {
                    var currentVales = forecastDiskData.Where(x => x.Hour.Equals(start.ToString("HH:mm")));
                    foreach (var val in currentVales) {
                        val.ForecastDateTime = start;
                    }
                }
            }

            var ordedrdForecastDiskData = forecastDiskData.OrderBy(x => x.ForecastDateTime).ThenBy(x => x.DeviceName);

            if (ordedrdForecastDiskData.Any()) {
                var tableName = "ForecastDisks";
                var databaseName = Helper.FindKeyName(_systemConnectionString, "DATABASE");
                //Insert into the database.
                var databaseCheck = new Database(_systemConnectionString);
                var exists = databaseCheck.CheckTableExists(tableName, databaseName);

                if (!exists) {
                    databaseCheck.CreateForecastDiskTable();
                }
                else {
                    var columnName = "DP2Busy";
                    //Check for new column.
                    var columnExist = databaseCheck.CheckColumn(databaseName, tableName, columnName);
                    if (!columnExist)
                        databaseCheck.AlterForecastDisk();
                }

                var pathToCsv = tempSaveLocation + "BulkInsert_" + DateTime.Now.Ticks + ".csv";
                var sb = new StringBuilder();

                foreach (var forecast in ordedrdForecastDiskData) {
                    sb.Append(forecast.ForecastDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "|" +
                              forecast.DeviceName + "|" +
                              forecast.QueueLength + "|" +
                              forecast.StdDevQueueLength + "|" +
                              forecast.DP2Busy + "|" +
                              forecast.StdDevDP2Busy + Environment.NewLine);
                }
                File.AppendAllText(pathToCsv, sb.ToString());

                var dataTables = new DataTables(_systemConnectionString);
                dataTables.InsertForecastData(tableName, pathToCsv);
            }
        }

        public void StoreForecastStorageData(List<ForecastStorageData> forecastStorageData, DateTime startTime, DateTime endTime, long interval, string tempSaveLocation) {
            var ordedrdForecastStorageData = forecastStorageData.OrderBy(x => x.ForecastDateTime).ThenBy(x => x.DeviceName);

            if (ordedrdForecastStorageData.Any()) {
                var tableName = "ForecastStorages";
                var databaseName = Helper.FindKeyName(_systemConnectionString, "DATABASE");
                //Insert into the database.
                var databaseCheck = new Database(_systemConnectionString);
                var exists = databaseCheck.CheckTableExists(tableName, databaseName);

                if (!exists) {
                    databaseCheck.CreateForecastStorageTable();
                }

                var pathToCsv = tempSaveLocation + "BulkInsert_" + DateTime.Now.Ticks + ".csv";
                var sb = new StringBuilder();

                foreach (var forecast in ordedrdForecastStorageData) {
                    sb.Append(forecast.ForecastDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "|" +
                              forecast.DeviceName + "|" +
                              forecast.UsedPercent + "|" +
                              forecast.StdDevUsedPercent + Environment.NewLine);
                }
                File.AppendAllText(pathToCsv, sb.ToString());

                var dataTables = new DataTables(_systemConnectionString);
                dataTables.InsertForecastData(tableName, pathToCsv);
            }
        }

        public void StoreForecastProcessData(List<ForecastProcessData> forecastProcessData, DateTime startTime, DateTime endTime, long interval, string tempSaveLocation) {
            if (interval == 0) return; //Defensive code. Since if interval is zero, then the loop below will never exit
            for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                if (forecastProcessData.Any(x => x.Hour.Equals(start.ToString("HH:mm")))) {
                    var currentVales = forecastProcessData.Where(x => x.Hour.Equals(start.ToString("HH:mm")));
                    foreach (var val in currentVales) {
                        val.ForecastDateTime = start;
                    }
                }
            }

            var ordedrdForecastDiskData = forecastProcessData.OrderBy(x => x.ForecastDateTime);

            if (ordedrdForecastDiskData.Any()) {
                var tableName = "ForecastProcesses";
                var databaseName = Helper.FindKeyName(_systemConnectionString, "DATABASE");
                //Insert into the database.
                var databaseCheck = new Database(_systemConnectionString);
                var exists = databaseCheck.CheckTableExists(tableName, databaseName);

                if (!exists) {
                    databaseCheck.CreateForecastProcess();
                }

                var pathToCsv = tempSaveLocation + "BulkInsert_" + DateTime.Now.Ticks + ".csv";
                var sb = new StringBuilder();

                foreach (var forecast in ordedrdForecastDiskData) {
                    sb.Append(forecast.ForecastDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "|" +
                              forecast.ProcessName + "|" +
                              forecast.CpuNumber + "|" +
                              forecast.Pin + "|" +
                              forecast.Volume + "|" +
                              forecast.SubVol + "|" +
                              forecast.FileName + "|" +
                              forecast.ProcessBusy + "|" +
                              forecast.StdDevProcessBusy + "|" +
                              forecast.RecvQueueLength + "|" +
                              forecast.StdDevRecvQueueLength + "|" +
                              Environment.NewLine);
                }
                File.AppendAllText(pathToCsv, sb.ToString());

                var dataTables = new DataTables(_systemConnectionString);
                dataTables.InsertForecastData(tableName, pathToCsv);
            }
        }

        public void StoreForecastTmfData(List<ForecastTMFData> forecastDiskData, DateTime startTime, DateTime endTime, long interval, string tempSaveLocation) {
            if (interval == 0) return; //Defensive code. Since if interval is zero, then the loop below will never exit
            for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                if (forecastDiskData.Any(x => x.Hour.Equals(start.ToString("HH:mm")))) {
                    var currentVales = forecastDiskData.Where(x => x.Hour.Equals(start.ToString("HH:mm")));
                    foreach (var val in currentVales) {
                        val.ForecastDateTime = start;
                    }
                }
            }

            var ordedrdForecastDiskData = forecastDiskData.OrderBy(x => x.ForecastDateTime);

            if (ordedrdForecastDiskData.Any()) {
                var tableName = "ForecastTmfs";
                var databaseName = Helper.FindKeyName(_systemConnectionString, "DATABASE");
                //Insert into the database.
                var databaseCheck = new Database(_systemConnectionString);
                var exists = databaseCheck.CheckTableExists(tableName, databaseName);

                if (!exists) {
                    databaseCheck.CreateForecastTmf();
                }

                var pathToCsv = tempSaveLocation + "BulkInsert_" + DateTime.Now.Ticks + ".csv";
                var sb = new StringBuilder();

                foreach (var forecast in ordedrdForecastDiskData) {
                    sb.Append(forecast.ForecastDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "|" +
                              forecast.ProcessName + "|" +
                              forecast.CpuNumber + "|" +
                              forecast.Pin + "|" +
                              forecast.Volume + "|" +
                              forecast.SubVol + "|" +
                              forecast.FileName + "|" +
                              forecast.AbortPercent + "|" +
                              forecast.StdDevAbortPercent + "|" +
                              Environment.NewLine);
                }
                File.AppendAllText(pathToCsv, sb.ToString());

                var dataTables = new DataTables(_systemConnectionString);
                dataTables.InsertForecastData(tableName, pathToCsv);
            }
        }

        public List<ForecastDiskData> GetForecastDiskDataFor(DateTime startTime, DateTime stopTime) {
            var forecastData = new List<ForecastDiskData>();

            try {
                var tableName = "ForecastDisks";
                var databaseName = Helper.FindKeyName(_systemConnectionString, "DATABASE");
                //Insert into the database.
                var databaseCheck = new Database(_systemConnectionString);
                var exists = databaseCheck.CheckTableExists(tableName, databaseName);

                if (!exists) {
                    databaseCheck.CreateForecastDiskTable();
                }
                else {
                    var columnName = "DP2Busy";
                    //Check for new column.
                    var columnExist = databaseCheck.CheckColumn(databaseName, tableName, columnName);
                    if (!columnExist)
                        databaseCheck.AlterForecastDisk();
                }
                
                var forecast = new ForecastRepository(_systemConnectionString);
                var forecastInfo = forecast.GetForecastDiskData(startTime, stopTime);

                if (forecastInfo.Rows.Count > 0) {
                    foreach (DataRow row in forecastInfo.Rows) {
                        forecastData.Add(new ForecastDiskData {
                            ForecastDateTime = Convert.ToDateTime(row["FromTimestamp"]),
                            DeviceName = row["DeviceName"].ToString(),
                            QueueLength = Convert.ToDouble(row["QueueLength"]),
                            StdDevQueueLength = Convert.ToDouble(row["StdDevQueueLength"]),
                            DP2Busy = row.IsNull("DP2Busy") ? 0 : Convert.ToDouble(row["DP2Busy"]),
                            StdDevDP2Busy = row.IsNull("StdDevDP2Busy") ? 0 : Convert.ToDouble(row["StdDevDP2Busy"]),
                        });
                    }
                }
            }
            catch (Exception ex){ }

            return forecastData;
        }

        public List<ForecastStorageData> GetForecastStorageDataFor(DateTime startTime, DateTime stopTime) {
            var forecastData = new List<ForecastStorageData>();

            try {
                var forecast = new ForecastRepository(_systemConnectionString);
                var forecastInfo = forecast.GetForecastStorageData(startTime, stopTime);

                if (forecastInfo.Rows.Count > 0) {
                    foreach (DataRow row in forecastInfo.Rows) {
                        forecastData.Add(new ForecastStorageData {
                            ForecastDateTime = Convert.ToDateTime(row["FromTimestamp"]),
                            DeviceName = row["DeviceName"].ToString(),
                            UsedPercent = Convert.ToDouble(row["UsedPercent"]),
                            StdDevUsedPercent = Convert.ToDouble(row["StdDevUsedPercent"])
                        });
                    }
                }
            }
            catch { }

            return forecastData;
        }

        public List<ForecastProcessData> GetForecastProcessDataFor(DateTime startTime, DateTime stopTime) {
            var forecastData = new List<ForecastProcessData>();

            try {
                var forecast = new ForecastRepository(_systemConnectionString);
                var forecastInfo = forecast.GetForecastProcessData(startTime, stopTime);

                if (forecastInfo.Rows.Count > 0) {
                    foreach (DataRow row in forecastInfo.Rows) {
                        forecastData.Add(new ForecastProcessData {
                            ForecastDateTime = Convert.ToDateTime(row["FromTimestamp"]),
                            ProcessName = row["ProcessName"].ToString(),
                            CpuNumber = Convert.ToInt32(row["CpuNumber"]),
                            Pin = Convert.ToInt32(row["Pin"]),
                            Volume = row["Volume"].ToString(),
                            SubVol = row["SubVol"].ToString(),
                            FileName = row["FileName"].ToString(),
                            ProcessBusy = Convert.ToDouble(row["ProcessBusy"]),
                            StdDevProcessBusy = Convert.ToDouble(row["StdDevProcessBusy"]),
                            RecvQueueLength = Convert.ToDouble(row["RecvQueueLength"]),
                            StdDevRecvQueueLength = Convert.ToDouble(row["StdDevRecvQueueLength"])
                        });
                    }
                }
            }
            catch { }

            return forecastData;
        }

        public List<ForecastTMFData> GetForecastTmfDataFor(DateTime startTime, DateTime stopTime) {
            var forecastData = new List<ForecastTMFData>();

            try {
                var forecast = new ForecastRepository(_systemConnectionString);
                var forecastInfo = forecast.GetForecastTmfData(startTime, stopTime);

                if (forecastInfo.Rows.Count > 0) {
                    foreach (DataRow row in forecastInfo.Rows) {
                        forecastData.Add(new ForecastTMFData {
                            ForecastDateTime = Convert.ToDateTime(row["FromTimestamp"]),
                            ProcessName = row["ProcessName"].ToString(),
                            CpuNumber = Convert.ToInt32(row["CpuNumber"]),
                            Pin = Convert.ToInt32(row["Pin"]),
                            Volume = row["Volume"].ToString(),
                            SubVol = row["SubVol"].ToString(),
                            FileName = row["FileName"].ToString(),
                            AbortPercent = Convert.ToDouble(row["AbortPercent"]),
                            StdDevAbortPercent = Convert.ToDouble(row["StdDevAbortPercent"])
                        });
                    }
                }
            }
            catch { }

            return forecastData;
        }
    }
}
