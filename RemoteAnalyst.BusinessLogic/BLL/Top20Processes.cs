using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.BLL {
    public class Top20Processes {

        public void PopulateTop20Processes(string connectionStringSystem, string systemSerial, DateTime startTime, DateTime stopTime, long interval, string databasePrefix, string tempSaveLocation, ILog log) {
            try {
                var processEntityTableService = new ProcessEntityTableService(connectionStringSystem);
                var processEntityTable = new ProcessEntityTable(connectionStringSystem);

                var processTalbeNames = processEntityTableService.GetTableNames(systemSerial, startTime, stopTime);

                //Check Table Exists.
                var databaseName = Helper.FindKeyName(connectionStringSystem, "DATABASE");
                var dailyCpuDatas = new DailyCPUDataRepository(connectionStringSystem);
                var tableExists = dailyCpuDatas.CheckDailiesTopProcessesTableName(databaseName);
                if (!tableExists)
                    dailyCpuDatas.CreateDailiesTopProcessesTable();

                if (processTalbeNames.Count > 0) {
                    var cpuEntityTableService = new CPUEntityTableService(connectionStringSystem);
                    var pageSizeBytes = cpuEntityTableService.GetPageSizeBytesFor(processTalbeNames.FirstOrDefault().Replace("PROCESS", "CPU"));
                    var isIPU = processEntityTableService.CheckIPUFor(systemSerial, startTime, stopTime, databasePrefix);
                    var dailiesTopProcesses = new DailiesTopProcessRepository(connectionStringSystem);

                    for (var start = Convert.ToDateTime(startTime.ToString("yyyy-MM-dd HH:00:00")); start < stopTime; start = start.AddHours(1)) {
                        //Check if the data exists for same hour.

                        var processBusy = dailiesTopProcesses.GetProcessBusyData(start, start.AddHours(1));
                        var processQueue = dailiesTopProcesses.GetProcessQueueData(start, start.AddHours(1));
                        
                        var processBusyStatic = processEntityTable.GetTop20ProcessByBusyStatic(processTalbeNames, start, start.AddHours(1), pageSizeBytes, interval, isIPU);
                        var processQueueStatic = processEntityTable.GetTop20ProcessByQueueStatic(processTalbeNames, start, start.AddHours(1), pageSizeBytes, interval, isIPU);

                        if (processBusyStatic.Rows.Count > 0) {
                            System.Data.DataColumn newColumn = new System.Data.DataColumn("DataType", typeof(System.Int32)) {
                                DefaultValue = "1"
                            };
                            processBusyStatic.Columns.Add(newColumn);
                            //Reorder the columns.
                            processBusyStatic.Columns["DataType"].SetOrdinal(0);
                            processBusyStatic.Columns["FromTimestamp"].SetOrdinal(1);
                            processBusyStatic.Columns["CpuNum"].SetOrdinal(2);
                            processBusyStatic.Columns["PIN"].SetOrdinal(3);
                            processBusyStatic.Columns["IPUNum"].SetOrdinal(4);
                            processBusyStatic.Columns["ProcessName"].SetOrdinal(5);
                            processBusyStatic.Columns["Priority"].SetOrdinal(6);
                            processBusyStatic.Columns["Busy %"].SetOrdinal(7);
                            processBusyStatic.Columns["Program"].SetOrdinal(8);
                            processBusyStatic.Columns["ReceiveQueue"].SetOrdinal(9);
                            processBusyStatic.Columns["MemUsed"].SetOrdinal(10);
                            processBusyStatic.Columns["AncestorProcessName"].SetOrdinal(11);
                            processBusyStatic.Columns["User"].SetOrdinal(12);
                            processBusyStatic.Columns["Group"].SetOrdinal(13);
                        }

                        if (processQueueStatic.Rows.Count > 0) {
                            System.Data.DataColumn newColumn = new System.Data.DataColumn("DataType", typeof(System.Int32)) {
                                DefaultValue = "2"
                            };
                            processQueueStatic.Columns.Add(newColumn);
                            //Reorder the columns.
                            processQueueStatic.Columns["DataType"].SetOrdinal(0);
                            processQueueStatic.Columns["FromTimestamp"].SetOrdinal(1);
                            processQueueStatic.Columns["CpuNum"].SetOrdinal(2);
                            processQueueStatic.Columns["PIN"].SetOrdinal(3);
                            processQueueStatic.Columns["IPUNum"].SetOrdinal(4);
                            processQueueStatic.Columns["ProcessName"].SetOrdinal(5);
                            processQueueStatic.Columns["Priority"].SetOrdinal(6);
                            processQueueStatic.Columns["Busy %"].SetOrdinal(7);
                            processQueueStatic.Columns["Program"].SetOrdinal(8);
                            processQueueStatic.Columns["ReceiveQueue"].SetOrdinal(9);
                            processQueueStatic.Columns["MemUsed"].SetOrdinal(10);
                            processQueueStatic.Columns["AncestorProcessName"].SetOrdinal(11);
                            processQueueStatic.Columns["User"].SetOrdinal(12);
                            processQueueStatic.Columns["Group"].SetOrdinal(13);
                        }

                        //Merge tables.
                        processBusy.Merge(processBusyStatic);
                        processQueue.Merge(processQueueStatic);

                        //Group the values.
                        var groupBy = processBusy.AsEnumerable().GroupBy(r => new {
                                DataType = r["DataType"],
                                FromTimestamp = r["FromTimestamp"],
                                CpuNum = r["CpuNum"],
                                PIN = r["PIN"],
                                IPUNum = r["IPUNum"],
                                ProcessName = r["ProcessName"],
                                Priority = r["Priority"],
                                Program = r["Program"],
                                AncestorProcessName = r["AncestorProcessName"],
                                User = r["User"],
                                Group = r["Group"],
                            }
                        ).Select(g => new DailesProcessView {
                            DataType = Convert.ToInt32(g.Key.DataType),
                            FromTimestamp = Convert.ToDateTime(g.Key.FromTimestamp),
                            CpuNum = Convert.ToInt32(g.Key.CpuNum),
                            PIN = Convert.ToInt32(g.Key.PIN),
                            IPUNum = Convert.ToInt32(g.Key.IPUNum),
                            ProcessName = g.Key.ProcessName.ToString(),
                            Priority = Convert.ToInt32(g.Key.Priority),
                            Busy = g.Average(x => x.Field<double>("Busy %")),
                            Program = g.Key.Program.ToString(),
                            ReceiveQueue = g.Average(x => x.Field<double>("ReceiveQueue")),
                            MemUsed = g.Average(x => x.Field<double>("MemUsed")),
                            AncestorProcessName = g.Key.AncestorProcessName.ToString(),
                            User = Convert.ToInt32(g.Key.User),
                            Group = Convert.ToInt32(g.Key.Group)
                        }).OrderByDescending(x => x.Busy).Take(20).ToList();

                        string pathToCsv = tempSaveLocation + @"\BulkInsert_" + DateTime.Now.Ticks + ".csv";
                        var sb = new StringBuilder();
                        foreach (var g in groupBy) {
                            sb.Append(g.DataType + "|");
                            sb.Append(g.FromTimestamp.ToString("yyyy-MM-dd HH:mm:ss") + "|");
                            sb.Append(g.CpuNum + "|");
                            sb.Append(g.PIN + "|");
                            sb.Append(g.IPUNum + "|");
                            sb.Append(g.ProcessName + "|");
                            sb.Append(g.Priority + "|");
                            sb.Append(g.Busy + "|");
                            sb.Append(g.Program + "|");
                            sb.Append(g.ReceiveQueue + "|");
                            sb.Append(g.MemUsed + "|");
                            sb.Append(g.AncestorProcessName + "|");
                            sb.Append(g.User + "|");
                            sb.Append(g.Group + Environment.NewLine);
                        }
                        File.AppendAllText(pathToCsv, sb.ToString());
#if DEBUG
#else
                        var dataTables = new DataTables(connectionStringSystem);
                        dataTables.InsertDailieData("DailiesTopProcesses", pathToCsv);
#endif

                        var groupByQueue = processQueue.AsEnumerable().GroupBy(r => new {
                                DataType = r["DataType"],
                                FromTimestamp = r["FromTimestamp"],
                                CpuNum = r["CpuNum"],
                                PIN = r["PIN"],
                                IPUNum = r["IPUNum"],
                                ProcessName = r["ProcessName"],
                                Priority = r["Priority"],
                                Program = r["Program"],
                                AncestorProcessName = r["AncestorProcessName"],
                                User = r["User"],
                                Group = r["Group"],
                            }
                        ).Select(g => new DailesProcessView {
                            DataType = Convert.ToInt32(g.Key.DataType),
                            FromTimestamp = Convert.ToDateTime(g.Key.FromTimestamp),
                            CpuNum = Convert.ToInt32(g.Key.CpuNum),
                            PIN = Convert.ToInt32(g.Key.PIN),
                            IPUNum = Convert.ToInt32(g.Key.IPUNum),
                            ProcessName = g.Key.ProcessName.ToString(),
                            Priority = Convert.ToInt32(g.Key.Priority),
                            Busy = g.Average(x => x.Field<double>("Busy %")),
                            Program = g.Key.Program.ToString(),
                            ReceiveQueue = g.Average(x => x.Field<double>("ReceiveQueue")),
                            MemUsed = g.Average(x => x.Field<double>("MemUsed")),
                            AncestorProcessName = g.Key.AncestorProcessName.ToString(),
                            User = Convert.ToInt32(g.Key.User),
                            Group = Convert.ToInt32(g.Key.Group)
                        }).OrderByDescending(x => x.ReceiveQueue).Take(20).ToList();

                        string pathToCsvQueue = tempSaveLocation + @"\BulkInsert_" + DateTime.Now.Ticks + ".csv";
                        var sbQueue = new StringBuilder();
                        foreach (var g in groupByQueue) {
                            sbQueue.Append(g.DataType + "|");
                            sbQueue.Append(g.FromTimestamp.ToString("yyyy-MM-dd HH:mm:ss") + "|");
                            sbQueue.Append(g.CpuNum + "|");
                            sbQueue.Append(g.PIN + "|");
                            sbQueue.Append(g.IPUNum + "|");
                            sbQueue.Append(g.ProcessName + "|");
                            sbQueue.Append(g.Priority + "|");
                            sbQueue.Append(g.Busy + "|");
                            sbQueue.Append(g.Program + "|");
                            sbQueue.Append(g.ReceiveQueue + "|");
                            sbQueue.Append(g.MemUsed + "|");
                            sbQueue.Append(g.AncestorProcessName + "|");
                            sbQueue.Append(g.User + "|");
                            sbQueue.Append(g.Group + Environment.NewLine);
                        }
                        File.AppendAllText(pathToCsvQueue, sbQueue.ToString());
#if DEBUG
#else
                        dataTables.InsertDailieData("DailiesTopProcesses", pathToCsvQueue);
#endif
                    }
                }
            }
            catch (Exception ex) {
				log.InfoFormat("Error: {0}", ex);
            }
        }
    }
    public class DailesProcessView {
        public int DataType { get; set; }
        public DateTime FromTimestamp { get; set; }
        public int CpuNum { get; set; }
        public int PIN { get; set; }
        public int IPUNum { get; set; }
        public string ProcessName { get; set; }
        public int Priority { get; set; }
        public double Busy { get; set; }
        public string Program { get; set; }
        public double ReceiveQueue { get; set; }
        public double MemUsed { get; set; }
        public string AncestorProcessName { get; set; }
        public int User { get; set; }
        public int Group { get; set; }
    }
}
