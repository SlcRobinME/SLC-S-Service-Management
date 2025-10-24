namespace SLC_SM_AS_DynamicDelete
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions;

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
			DomId = _engine.ReadScriptParamFromApp<Guid>("DomId");

			string nodeIdsRaw = _engine.ReadScriptParamFromApp("NodeIds");
			NodeIds = (nodeIdsRaw ?? String.Empty)
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.ToHashSet();

			string connectionIdsRaw = _engine.ReadScriptParamFromApp("ConnectionIds");
			ConnectionIds = (connectionIdsRaw ?? String.Empty)
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Where(s => Guid.TryParse(s.Trim(), out _))
				.Select(Guid.Parse)
				.ToHashSet();
		}
	}
}
