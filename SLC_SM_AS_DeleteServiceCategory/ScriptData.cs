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

		public Guid DomId { get; set; }

		private void LoadParameters()
		{
			var domIdRaw = _engine.GetScriptParam("DomId")?.Value;
			DomId = JsonConvert.DeserializeObject<List<Guid>>(domIdRaw ?? "[]").FirstOrDefault();
			if (DomId == Guid.Empty)
			{
				throw new InvalidOperationException("No valid DOM ID provided as input to the script");
			}
		}
	}
}
