namespace SLC_SM_IAS_Profiles.Data
{
	using System;
	using Skyline.DataMiner.Automation;

	public enum Mode
	{
		Configuration,
		Profile,
	}

	public class ScriptData
	{
		private IEngine _engine;

		public ScriptData(IEngine engine)
		{
			_engine = engine;

			Init();
		}

		public Mode Mode { get; set; }

		private void Init()
		{
			var modeRaw = _engine.GetScriptParam("Mode").Value;
			if (!Enum.TryParse(modeRaw, out Mode mode))
			{
				throw new Exception($"Could not parse script mode {modeRaw}");
			}

			Mode = mode;
		}
	}
}
