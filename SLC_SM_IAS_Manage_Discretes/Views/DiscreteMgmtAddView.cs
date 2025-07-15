namespace SLC_SM_IAS_Manage_Discretes.Views
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DiscreteMgmtAddView : Dialog
	{
		public DiscreteMgmtAddView(IEngine engine) : base(engine)
		{
			Title = "Add Discrete Option";

			int row = 0;
			AddWidget(LblValue, row, 0);
			AddWidget(Value, row, 1);
			AddWidget(ErrorValue, row, 2);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(BtnClose, ++row, 0);
			AddWidget(BtnAdd, row, 1);
		}

		public Label LblValue { get; } = new Label("Discrete Value");

		public TextBox Value { get; } = new TextBox();

		public Label ErrorValue { get; } = new Label(String.Empty);

		public Button BtnClose { get; } = new Button("Cancel");

		public Button BtnAdd { get; } = new Button("Add Discrete Option");
	}
}