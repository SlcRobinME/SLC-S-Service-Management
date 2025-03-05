namespace SLC_SM_Create_Service_Inventory_Item_1.Views
{
	using System;
	using DomHelpers.SlcServicemanagement;
	using Library;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ServiceView : Dialog
	{
		public ServiceView(IEngine engine) : base(engine)
		{
			Title = "Manage Service";
			MinWidth = Defaults.DialogMinWidth;

			int row = 0;
			AddWidget(new Label("Service Details") { Style = TextStyle.Heading }, row, 0);
			AddWidget(LblName, ++row, 0);
			AddWidget(TboxName, row, 1);
			AddWidget(ErrorName, row, 2);

			AddWidget(LblServiceCategory, ++row, 0);
			AddWidget(ServiceCategory, row, 1);

			AddWidget(LblSpecification, ++row, 0);
			AddWidget(Specs, row, 1);

			AddWidget(LblStart, ++row, 0);
			AddWidget(Start, row, 1);
			AddWidget(ErrorStart, row, 2);
			AddWidget(LblEnd, ++row, 0);
			AddWidget(End, row, 1);
			AddWidget(ErrorEnd, row, 2);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(BtnCancel, ++row, 0);
			AddWidget(BtnAdd, row, 1);
		}

		public Label LblName { get; } = new Label("Name");

		public TextBox TboxName { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Label ErrorName { get; } = new Label(String.Empty);

		public Button BtnCancel { get; } = new Button("Cancel");

		public Label LblServiceCategory { get; } = new Label("Category");

		public DropDown<ServiceCategoryInstance> ServiceCategory { get; } = new DropDown<ServiceCategoryInstance> { Width = Defaults.WidgetWidth };

		public Label LblSpecification { get; } = new Label("Service Specification");

		public DropDown<ServiceSpecificationsInstance> Specs { get; } = new DropDown<ServiceSpecificationsInstance> { Width = Defaults.WidgetWidth };

		public Label LblStart { get; } = new Label("Start Time");

		public DateTimePicker Start { get; } = new DateTimePicker
		{
			Height = 25,
			Width = Defaults.WidgetWidth,
			IsTimePickerVisible = true,
			Kind = DateTimeKind.Local,
			HasSpinnerButton = true,
			AutoCloseCalendar = true,
		};

		public Label ErrorStart { get; } = new Label(String.Empty);

		public Label LblEnd { get; } = new Label("End Time");

		public DateTimePicker End { get; } = new DateTimePicker
		{
			Height = 25,
			Width = Defaults.WidgetWidth,
			IsTimePickerVisible = true,
			Kind = DateTimeKind.Local,
			HasSpinnerButton = true,
			AutoCloseCalendar = true,
		};

		public Label ErrorEnd { get; } = new Label(String.Empty);

		public Button BtnAdd { get; } = new Button("Add");
	}
}