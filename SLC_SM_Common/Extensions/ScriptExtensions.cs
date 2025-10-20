namespace Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;

	public static class ScriptExtensions
	{
		public static T ReadScriptParamFromApp<T>(this IEngine engine, string name)
		{
			return ReadScriptParamsFromApp<T>(engine, name).FirstOrDefault();
		}

		public static string ReadScriptParamFromApp(this IEngine engine, string name)
		{
			return ReadScriptParamsFromApp<string>(engine, name).FirstOrDefault();
		}

		public static ICollection<T> ReadScriptParamsFromApp<T>(this IEngine engine, string name)
		{
			string param = engine.GetScriptParam(name)?.Value;
			if (param == null)
			{
				throw new ArgumentException($"No script input parameter provided with name '{name}'");
			}

			try
			{
				return JsonConvert.DeserializeObject<ICollection<T>>(param);
			}
			catch
			{
				return new List<T> { (T)Convert.ChangeType(param, typeof(T)) };
			}
		}

		public static ICollection<string> ReadScriptParamsFromApp(this IEngine engine, string name)
		{
			return ReadScriptParamsFromApp<string>(engine, name);
		}
	}
}