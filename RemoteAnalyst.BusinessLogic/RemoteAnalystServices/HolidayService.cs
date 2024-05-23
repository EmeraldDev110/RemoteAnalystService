using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class HolidayService {
        private readonly string _connectionString;

        public HolidayService(string connectionString) {
            _connectionString = connectionString;
        }

        public HolidayData GetWorkDayFactorFor(string systemSerial, DateTime workdayDate) {
            var holidays = new Holidays(_connectionString);
            var holidayFactor = holidays.GetWorkDayFactor(systemSerial, workdayDate);

            var holidayData = new HolidayData();
            if (holidayFactor.Rows.Count > 0) {
                holidayData.Increase = Convert.ToInt32(holidayFactor.Rows[0]["Increase"]);
                holidayData.Percentage = Convert.ToDouble(holidayFactor.Rows[0]["Percentage"]);
                holidayData.FromHour = Convert.ToInt32(holidayFactor.Rows[0]["FromHour"]);
                holidayData.ToHour = Convert.ToInt32(holidayFactor.Rows[0]["ToHour"]);
                holidayData.HasData = true;
            }
            else {
                holidayData.HasData = false;
            }

            return holidayData;
        }

    }

    public class HolidayData {
        public bool HasData { get; set; }
        public DateTime HolidayDateTime { get; set; }
        public int Increase { get; set; }
        public double Percentage { get; set; }
        public int FromHour { get; set; }
        public int ToHour { get; set; }
    }
}
