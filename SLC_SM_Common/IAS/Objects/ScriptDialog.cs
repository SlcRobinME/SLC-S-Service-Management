namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class ScriptDialog : Dialog
	{
		protected ScriptDialog(IEngine engine) : base(engine)
		{
			ShowScriptAbortPopup = false;
		}

		protected ScriptLayout Layout { get; } = new ScriptLayout { RowPosition = 0 };

		public abstract void Build();

		protected class ScriptLayout
		{
			public int RowPosition { get; set; }
		}
	}
}