namespace SLC_SM_AS_DynamicDelete
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Skyline.DataMiner.Automation;

	internal class ScriptData
	{
		private readonly IEngine _engine;

		public ScriptData(IEngine engine)
		{
			_engine = engine;
			LoadParameters();
		}

		public Guid DomId { get; set; }

		public HashSet<string> NodeIds { get; set; }

		public HashSet<Guid> ConnectionIds { get; set; }

		private void LoadParameters()
		{
			string domIdRaw = _engine.GetScriptParam("DomId").Value;
			DomId = JsonConvert.DeserializeObject<List<Guid>>(domIdRaw).FirstOrDefault();
			if (DomId == Guid.Empty)
				throw new InvalidOperationException("No DOM ID provided as input to the script");

			string nodeIdsRaw = _engine.GetScriptParam("NodeIds").Value;
			var nodeIdsString = JsonConvert.DeserializeObject<List<string>>(nodeIdsRaw);
			NodeIds = (nodeIdsString.FirstOrDefault() ?? string.Empty)
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.ToHashSet();

			string connectionIdsRaw = _engine.GetScriptParam("ConnectionIds").Value;
			var connectionIdsString = JsonConvert.DeserializeObject<List<string>>(connectionIdsRaw);
			ConnectionIds = (connectionIdsString.FirstOrDefault() ?? string.Empty)
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Where(s => Guid.TryParse(s.Trim(), out _))
				.Select(Guid.Parse)
				.ToHashSet();
		}
	}
}
