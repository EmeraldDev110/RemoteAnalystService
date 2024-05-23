using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    class ExceptionService {
        private readonly string _connectionString;

        public ExceptionService(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetExceptionFor(DateTime startTime, DateTime stopTime, string entity, string counter) {
            var exceptions = new Exceptions(_connectionString);
            var exceptionData = exceptions.GetException(startTime, stopTime, entity, counter);
                        
            //Build table.
            var dailyEmailUtil = new DailyEmailUtil();
            var dataTable = dailyEmailUtil.GenerateGridDataTable(startTime, stopTime);

            DataRow yellowCountRow = dataTable.NewRow();
            DataRow redCountRow = dataTable.NewRow();
            yellowCountRow["Entity"] = entity;
            var counterName = counter;
            if (counter == "Used%")
                counterName = "Used %";
            else if (counter == "DP2")
                counterName = "DP2 Busy";
            else
                counterName = counter;

            yellowCountRow["Counter"] = counterName;
            redCountRow["Entity"] = entity;
            redCountRow["Counter"] = counterName;

            if (entity != "Storage")
            {
                var timestampService = new TableTimeStampService(_connectionString);
                var entityTS = (entity == "IPU") ? "CPU" : (entity == "Disk") ? "DISC" : entity;
                var timestamps = timestampService.GetTimestampsFor(entityTS, startTime, stopTime);
                for (var ts = startTime; ts < stopTime; ts = ts.AddHours(1))
                {
                    if (timestamps.AsEnumerable().Any(x => x.Field<DateTime>("Start").ToString("yyyy-MM-dd HH") == ts.ToString("yyyy-MM-dd HH")))
                    {
                        yellowCountRow[ts.ToString("HH")] = 0;
                        redCountRow[ts.ToString("HH")] = 0;
                    }
                    else
                    {
                        yellowCountRow[ts.ToString("HH")] = -1;
                        redCountRow[ts.ToString("HH")] = -1;
                    }
                }
            }
            if (exceptionData.Rows.Count > 0) {
                if (entity != "Storage") {
                    for (var start = startTime; start < stopTime; start = start.AddHours(1)) {
                        if (exceptionData.AsEnumerable().Any(x => x.Field<DateTime>("FromTimestamp").ToString("yyyy-MM-dd HH") == start.ToString("yyyy-MM-dd HH"))) {
                            var yellowCount = exceptionData.AsEnumerable().Count(x => x.Field<DateTime>("FromTimestamp").ToString("yyyy-MM-dd HH") == start.ToString("yyyy-MM-dd HH") && x.Field<SByte>("DisplayRed") == 0);
                            var redCount = exceptionData.AsEnumerable().Count(x => x.Field<DateTime>("FromTimestamp").ToString("yyyy-MM-dd HH") == start.ToString("yyyy-MM-dd HH") && x.Field<SByte>("DisplayRed") == 1);

                            yellowCountRow[start.ToString("HH")] = yellowCount;
                            redCountRow[start.ToString("HH")] = redCount;
                        }
                    }
                }
                else {
                    if (exceptionData.AsEnumerable().Any(x => x.Field<DateTime>("FromTimestamp").ToString("HH") == "00")) {
                        var yellowCount = exceptionData.AsEnumerable().Count(x => x.Field<DateTime>("FromTimestamp").ToString("HH") == "00" && x.Field<SByte>("DisplayRed") == 0);
                        var redCount = exceptionData.AsEnumerable().Count(x => x.Field<DateTime>("FromTimestamp").ToString("HH") == "00" && x.Field<SByte>("DisplayRed") == 1);
                        yellowCountRow[2] = yellowCount;
                        redCountRow[2] = redCount;
                    }
                    else
                    {
                        yellowCountRow[2] = 0;
                        redCountRow[2] = 0;
                    }
                }
            }

            dataTable.Rows.Add(yellowCountRow);
            dataTable.Rows.Add(redCountRow);
            return dataTable;
        }
    }
}
