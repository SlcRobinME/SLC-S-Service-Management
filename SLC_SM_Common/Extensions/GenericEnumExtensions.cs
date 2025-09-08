namespace Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.GenericEnums;

	public static class GenericEnumExtensions
	{
		public static string FindDisplayName<T>(this GenericEnum<T> genericEnum, T value)
		{
			return genericEnum.Entries
				.First(entry => EqualityComparer<T>.Default.Equals(entry.Value, value))
				.DisplayName;
		}

		public static string FindDisplayName(this IGenericEnum genericEnum, object value)
		{
			return genericEnum.Entries
				.First(entry => Equals(entry.Value, value))
				.DisplayName;
		}

		public static T FindValue<T>(this GenericEnum<T> genericEnum, string displayName)
		{
			return genericEnum.Entries
				.First(entry => StringComparer.OrdinalIgnoreCase.Equals(entry.DisplayName, displayName))
				.Value;
		}

		public static object FindValue(this IGenericEnum genericEnum, string displayName)
		{
			return genericEnum.Entries
				.First(entry => StringComparer.OrdinalIgnoreCase.Equals(entry.DisplayName, displayName))
				.Value;
		}
	}
}