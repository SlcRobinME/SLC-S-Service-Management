namespace SLC_SM_IAS_Manage_Discretes.Views
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using SLC_SM_Common.API.ConfigurationsApi;

	public class DiscreteMgmtDeleteView : Dialog
	{
		public DiscreteMgmtDeleteView(IEngine engine) : base(engine)
		{
			Title = "Remove Discrete Option";

			int row = 0;
			AddWidget(LblValue, row, 0);
			AddWidget(Value, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(Info, ++row, 0, 1, 2);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(BtnClose, ++row, 0);
			AddWidget(BtnRemove, row, 1);
		}

		public Label LblValue { get; } = new Label("Discrete Value");

		public DropDown<Models.DiscreteValue> Value { get; } = new DropDown<Models.DiscreteValue> { IsDisplayFilterShown = true };

		public Label Info { get; } = new Label(String.Empty);

		public Button BtnClose { get; } = new Button("Cancel");

		public Button BtnRemove { get; } = new Button("Remove Discrete Option");
	}
}