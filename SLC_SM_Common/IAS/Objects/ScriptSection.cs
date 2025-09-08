namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS
{
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class ScriptSection : Section
	{
		protected ScriptSection()
		{
			Init();
		}

		#region Properties
		protected ScriptLayout Layout { get; set; }
		#endregion

		#region Methods
		public abstract void Build();

		private void Init()
		{
			Layout = new ScriptLayout
			{
				RowPosition = 0,
			};
		}
		#endregion

		#region Classes
		protected class ScriptLayout
		{
			public int RowPosition { get; set; }
		}
		#endregion
	}
}
