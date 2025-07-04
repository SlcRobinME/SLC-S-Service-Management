namespace SLC_SM_IAS_Configurations.Views
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using SLC_SM_Common.API.ConfigurationsApi;

	public class DiscreteValuesView : Dialog
	{
		public DiscreteValuesView(IEngine engine) : base(engine)
		{
			Title = "Select Eligible Discrete Options";

			AddWidget(Options, 0, 0, 1, 2);
			AddWidget(BtnReturn, 1, 0);
			AddWidget(BtnApply, 1, 1);
		}

		public CheckBoxList<Models.DiscreteValue> Options { get; } = new CheckBoxList<Models.DiscreteValue>();

		public Button BtnApply { get; } = new Button("Apply Selection");

		public Button BtnReturn { get; } = new Button("Return");
	}
}