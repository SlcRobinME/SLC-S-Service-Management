namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Components
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	// using Skyline.DataMiner.Utils.MediaOps.Common.Tools;

	public class EnumDropDown<T> : DropDown<T>
		where T : struct, Enum
	{
		//public EnumDropDown(ICollection<T> exclude = null)
		//{
		//	var options = new List<Choice<T>>();

		//	var type = typeof(T);
		//	var values = Enum.GetValues(type).Cast<T>();

		//	foreach (var value in values)
		//	{
		//		if (exclude != null && exclude.Contains(value))
		//		{
		//			continue;
		//		}

		//		options.Add(Choice.Create(value, value.GetDescription()));
		//	}

		//	Options = options;
		//}

		public EnumDropDown(Func<T, string> convertValueToString, ICollection<T> exclude = null)
		{
			if (convertValueToString == null)
			{
				throw new ArgumentNullException(nameof(convertValueToString));
			}

			var options = new List<Choice<T>>();

			var type = typeof(T);
			var values = Enum.GetValues(type).Cast<T>();

			foreach (var value in values)
			{
				if (exclude != null && exclude.Contains(value))
				{
					continue;
				}

				options.Add(Choice.Create(value, convertValueToString(value)));
			}

			Options = options;
		}
	}
}