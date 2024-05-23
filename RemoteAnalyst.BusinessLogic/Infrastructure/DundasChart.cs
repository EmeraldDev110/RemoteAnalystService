using System;
using System.Data;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;

namespace RemoteAnalyst.BusinessLogic.Infrastructure
{
    internal class DundasChart
    {
        private readonly string ServerPath;

        public DundasChart(string serverPath)
        {
            ServerPath = serverPath;
        }

        public string WhatIfDateTimeFormat(string time)
        {
            string fullTime = string.Empty;
            switch (time)
            {
                case "00":
                    fullTime = "12:00:00 AM";
                    break;
                case "01":
                    fullTime = "01:00:00 AM";
                    break;
                case "02":
                    fullTime = "02:00:00 AM";
                    break;
                case "03":
                    fullTime = "03:00:00 AM";
                    break;
                case "04":
                    fullTime = "04:00:00 AM";
                    break;
                case "05":
                    fullTime = "05:00:00 AM";
                    break;
                case "06":
                    fullTime = "06:00:00 AM";
                    break;
                case "07":
                    fullTime = "07:00:00 AM";
                    break;
                case "08":
                    fullTime = "08:00:00 AM";
                    break;
                case "09":
                    fullTime = "09:00:00 AM";
                    break;
                case "10":
                    fullTime = "10:00:00 AM";
                    break;
                case "11":
                    fullTime = "11:00:00 AM";
                    break;
                case "12":
                    fullTime = "12:00:00 PM";
                    break;
                case "13":
                    fullTime = "01:00:00 PM";
                    break;
                case "14":
                    fullTime = "02:00:00 PM";
                    break;
                case "15":
                    fullTime = "03:00:00 PM";
                    break;
                case "16":
                    fullTime = "04:00:00 PM";
                    break;
                case "17":
                    fullTime = "05:00:00 PM";
                    break;
                case "18":
                    fullTime = "06:00:00 PM";
                    break;
                case "19":
                    fullTime = "07:00:00 PM";
                    break;
                case "20":
                    fullTime = "08:00:00 PM";
                    break;
                case "21":
                    fullTime = "09:00:00 PM";
                    break;
                case "22":
                    fullTime = "10:00:00 PM";
                    break;
                case "23":
                    fullTime = "11:00:00 PM";
                    break;
            }

            return fullTime;
        }

    }
}