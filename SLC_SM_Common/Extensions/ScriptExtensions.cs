namespace Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;

	public static class ScriptExtensions
	{
		public static ICollection<T> ReadScriptParamListFromApp<T>(this IEngine engine, string name)
		{
			var param = engine.GetScriptParam(name);

			if (param == null)
			{
				throw new ArgumentException($"Couldn't find script parameter with name '{name}'");
			}

			try
			{
				return JsonConvert.DeserializeObject<ICollection<T>>(param.Value);
			}
			catch
			{
				throw new InvalidOperationException($"Unable to convert script parameter '{name}' to {typeof(ICollection<T>).Name}.");
			}
		}

		public static ICollection<string> ReadScriptParamListFromApp(this IEngine engine, string name)
		{
			return ReadScriptParamListFromApp<string>(engine, name);
		}

		public static T ReadScriptParamSingleFromApp<T>(this IEngine engine, string name)
		{
			var param = engine.GetScriptParam(name);

			if (param == null)
			{
				throw new ArgumentException($"Couldn't find script parameter with name '{name}'");
			}

			try
			{
				return JsonConvert.DeserializeObject<IEnumerable<T>>(param.Value).Single();
			}
			catch
			{
				return (T)Convert.ChangeType(param.Value, typeof(T));
			}
		}

		public static string ReadScriptParamSingleFromApp(this IEngine engine, string name)
		{
			return ReadScriptParamSingleFromApp<string>(engine, name);
		}
	}
}