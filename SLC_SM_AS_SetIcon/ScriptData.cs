namespace SLC_SM_AS_SetIcon
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;

	internal class ScriptData
	{
		private IEngine _engine;

		public ScriptData(IEngine engine)
		{
			_engine = engine;
			LoadParameters();
		}

		internal enum ObjectType
		{
			ServiceCategory,
			Service,
			ServiceItem,
		}

		public ObjectType Type { get; set; }

		public string Name { get; set; }

		public Guid DomId { get; set; }

		private void LoadParameters()
		{
			var paramType = _engine.GetScriptParam("Type")?.Value;
			if (!Enum.TryParse(paramType, out ObjectType type))
			{
				throw new InvalidOperationException($"Invalid object type: {paramType}");
			}

			var domIdRaw = _engine.GetScriptParam("DomId")?.Value;
			DomId = JsonConvert.DeserializeObject<List<Guid>>(domIdRaw ?? "[]").FirstOrDefault();
			if (DomId == Guid.Empty)
			{
				throw new InvalidOperationException("No valid DOM ID provided as input to the script");
			}

			Type = type;

			var nameRaw = _engine.GetScriptParam("Path")?.Value;
			Name = JsonConvert.DeserializeObject<List<string>>(nameRaw ?? "[]").FirstOrDefault();
		}
	}
}
