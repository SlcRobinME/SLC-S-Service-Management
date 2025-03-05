namespace SLC_SM_IAS_Add_Service_Configuration_1.Views
{
	using System;
	using Library;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ServiceConfigurationView : Dialog
	{
		public ServiceConfigurationView(IEngine engine) : base(engine)
		{
			Title = "Manage Service Configuration";
			MinWidth = Defaults.DialogMinWidth;

			int row = 0;
			AddWidget(new Label("Service Configuration Details") { Style = TextStyle.Heading }, row, 0);
			AddWidget(LblLabel, ++row, 0);
			AddWidget(Label, row, 1);
			AddWidget(ErrorLabel, row, 2);

			AddWidget(LblMandatory, ++row, 0);
			AddWidget(Mandatory, row, 1);

			AddWidget(LblValueType, ++row, 0);
			AddWidget(ValueType, row, 1);

			AddWidget(LblValue, ++row, 0);
			AddWidget(TBoxValue, row, 1);
			AddWidget(ErrorValue, row, 2);
			AddWidget(NumValue, ++row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(BtnCancel, ++row, 0);
			AddWidget(BtnAdd, row, 1);
		}

		public enum ValueTypeEnum
		{
			String,
			Double,
		}

		public Label LblLabel { get; } = new Label("Label");

		public TextBox Label { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Label ErrorLabel { get; } = new Label(String.Empty);

		public Label LblMandatory { get; } = new Label("Mandatory");

		public CheckBox Mandatory { get; } = new CheckBox("Yes") { IsChecked = true };

		public Label LblValueType { get; } = new Label("Value Type");

		public DropDown<ValueTypeEnum> ValueType { get; } = new DropDown<ValueTypeEnum>
			{ Width = Defaults.WidgetWidth, Options = new[] { new Option<ValueTypeEnum>(ValueTypeEnum.String), new Option<ValueTypeEnum>(ValueTypeEnum.Double) } };

		public Label LblValue { get; } = new Label("Value");

		public TextBox TBoxValue { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Numeric NumValue { get; } = new Numeric { Width = Defaults.WidgetWidth, Decimals = 3, Value = 0.0, StepSize = 0.001 };

		public Label ErrorValue { get; } = new Label(String.Empty);

		public Button BtnAdd { get; } = new Button("Add");

		public Button BtnCancel { get; } = new Button("Cancel");
	}
}