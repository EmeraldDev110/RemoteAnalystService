using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class SpecialDayService {

        private readonly string _connectionString;

        public SpecialDayService(string connectionString) {
            _connectionString = connectionString;
        }

        public List<DateTime> GetSpecialDayFor(string systemSerial, DateTime startTime) {
            var specialDayList = new List<DateTime>();
            var specialDays = new SpecialDays(_connectionString);
            var dataTable = specialDays.GetSpecialDays(systemSerial);

            if (dataTable.AsEnumerable().Any(x => x.Field<DateTime?>("SpecialDate").HasValue && x.Field<DateTime?>("SpecialDate") == startTime.Date)) {
                specialDayList = dataTable.AsEnumerable().Where(x => x.Field<DateTime>("SpecialDate") <= startTime.Date).Select(x => x.Field<DateTime>("SpecialDate")).ToList();
            }
            else if (startTime.AddDays(1).Day == 1 && (startTime.Month == 3 || startTime.Month == 6 || startTime.Month == 9 || startTime.Month == 12)) {
                if (dataTable.AsEnumerable().Any(x => x.Field<string>("SpecialDayType") == "EndOfQuarter")) {
                    for (var x = 0; x < 12; x++) {
                        startTime = startTime.AddMonths(-3);
                        if (!specialDayList.Contains(startTime))
                            specialDayList.Add(startTime);
                    }
                }
            }
            else if (startTime.AddDays(1).Day == 1) {
                if (dataTable.AsEnumerable().Any(x => x.Field<string>("SpecialDayType") == "EndOfMonth")) {
                    for (var x = 0; x < 12; x++) {
                        startTime = startTime.AddMonths(-1);
                        var daysInMonth = DateTime.DaysInMonth(startTime.Year, startTime.Month);
                        var newStartTime = new DateTime(startTime.Year, startTime.Month, daysInMonth);
                        if (!specialDayList.Contains(newStartTime))
                            specialDayList.Add(newStartTime);
                    }
                }
            }

            return specialDayList;
        }
    }
}
