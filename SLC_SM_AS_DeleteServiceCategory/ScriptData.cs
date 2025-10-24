namespace SLC_SM_AS_SetIcon
{
	using System;
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

		private void LoadParameters()
		{
			DomId = _engine.ReadScriptParamFromApp<Guid>("DomId");
		}
	}
}
