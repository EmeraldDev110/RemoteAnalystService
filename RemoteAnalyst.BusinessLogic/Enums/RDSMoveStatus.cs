using System;
using System.ComponentModel;
using System.Reflection;


namespace RemoteAnalyst.BusinessLogic.Enums {
	public static class RDSMoveStatus {
		public enum StatusMessage {
			[Description("RDSMove Requested")]
			RDSMoveRequested,
			[Description("RDSMove Completed")]
			RDSMoveCompleted
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
