namespace SLC_SM_IAS_Profiles.Views
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DiscreteValuesView : Dialog
	{
		public DiscreteValuesView(IEngine engine) : base(engine)
		{
			Title = "Select Eligible Discrete Options";

			AddWidget(Options, 0, 0, 1, 2);
			AddWidget(BtnReturn, 1, 0);
			AddWidget(BtnApply, 1, 1);
			AddWidget(new WhiteSpace() { Width = 150 }, 1, 2);
			AddWidget(new WhiteSpace() { Width = 150 }, 1, 3);
			AddWidget(new WhiteSpace() { Width = 150 }, 1, 4);
			AddWidget(new WhiteSpace() { Width = 150 }, 1, 5);
			AddWidget(new WhiteSpace() { Width = 150 }, 1, 6);
			AddWidget(new WhiteSpace() { Width = 150 }, 1, 7);
			AddWidget(new WhiteSpace() { Width = 150 }, 1, 8);
			AddWidget(new WhiteSpace() { Width = 150 }, 1, 9);
			AddWidget(new WhiteSpace() { Width = 150 }, 1, 10);
		}

		public CheckBoxList<Models.DiscreteValue> Options { get; } = new CheckBoxList<Models.DiscreteValue>();

		public Button BtnApply { get; } = new Button("Apply") { Width = 100, Style = ButtonStyle.CallToAction };

		public Button BtnReturn { get; } = new Button("Return") { Width = 100 };
	}
}