using System;
using System.ComponentModel;
using System.Reflection;

namespace RemoteAnalyst.BusinessLogic.Enums {
    public static class ReportGeneratorStatus {
        public enum StatusMessage {
            [Description("QT Order")]
            QTOrder,
            [Description("QT Order Completed")]
            QTCompleted,
            [Description("QT Order Exception Occurred")]
            QTException,
            [Description("QT Order No Data Avaiable")]
            QTNoDataAvailable,
            [Description("DPA Order")]
            DPAOrder,
            [Description("DPA Order Completed")]
            DPACompleted,
            [Description("DPA Order Exception Occurred")]
            DPAException,
            [Description("DPA Order No Data Avaiable")]
            DPANoDataAvailable,
            [Description("Terminate EC2 (No Reports)")]
            TerminateEC2,
            [Description("Forecast")]
            Forecast,
            [Description("Loader")]
            Loader

        }

        public static string GetEnumDescription(Enum enumValue) {
            string enumValueAsString = enumValue.ToString();
            var type = enumValue.GetType();
            FieldInfo fieldInfo = type.GetField(enumValueAsString);
            object[] attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0) {
                var attribute = (DescriptionAttribute)attributes[0];
                return attribute.Description;
            }

            return enumValueAsString;
        }
    }
}