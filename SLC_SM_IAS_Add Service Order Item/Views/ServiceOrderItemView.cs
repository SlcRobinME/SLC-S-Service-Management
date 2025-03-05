namespace SLC_SM_IAS_Add_Service_Order_Item_1.Views
{
	using System;
	using DomHelpers.SlcServicemanagement;
	using Library;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ServiceOrderItemView : Dialog
	{
		public ServiceOrderItemView(IEngine engine) : base(engine)
		{
			Title = "Manage Service Order Item";
			MinWidth = Defaults.DialogMinWidth;

			int row = 0;
			AddWidget(new Label("Service Order Item Details") { Style = TextStyle.Heading }, row, 0);
			AddWidget(LblName, ++row, 0);
			AddWidget(TboxName, row, 1);
			AddWidget(ErrorName, row, 2);

			AddWidget(LblAction, ++row, 0);
			AddWidget(ActionType, row, 1);

			AddWidget(new Label("Service Order Configuration Details") { Style = TextStyle.Heading }, ++row, 0);
			AddWidget(LblCategory, ++row, 0);
			AddWidget(Category, row, 1);
			AddWidget(LblSpecification, ++row, 0);
			AddWidget(Specification, row, 1);
			AddWidget(LblService, ++row, 0);
			AddWidget(Service, row, 1);
			AddWidget(BtnCreateNewService, row, 2);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(BtnCancel, ++row, 0);
			AddWidget(BtnAdd, row, 1);
		}

		public enum ActionTypeEnum
		{
			Add,
			Modify,
			Delete,
			NoChange,
			Undefined,
		}

		public Label LblName { get; } = new Label("Label");

		public TextBox TboxName { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Label ErrorName { get; } = new Label(String.Empty);

		public Label LblAction { get; } = new Label("Action");

		public DropDown<ActionTypeEnum> ActionType { get; } = new DropDown<ActionTypeEnum>
		{
			Width = Defaults.WidgetWidth,
			Options = new[]
			{
				new Option<ActionTypeEnum>("Add", ActionTypeEnum.Add),
				new Option<ActionTypeEnum>("Delete", ActionTypeEnum.Delete),
				new Option<ActionTypeEnum>("Modify", ActionTypeEnum.Modify),
				new Option<ActionTypeEnum>("No Change", ActionTypeEnum.NoChange),
				new Option<ActionTypeEnum>("Undefined", ActionTypeEnum.Undefined),
			},
		};

		public Label LblCategory { get; } = new Label("Category");

		public DropDown<ServiceCategoryInstance> Category { get; } = new DropDown<ServiceCategoryInstance> { Width = Defaults.WidgetWidth };

		public Label LblSpecification { get; } = new Label("Service Specification");

		public DropDown<ServiceSpecificationsInstance> Specification { get; } = new DropDown<ServiceSpecificationsInstance> { Width = Defaults.WidgetWidth };

		public Label LblService { get; } = new Label("Service Reference");

		public DropDown<ServicesInstance> Service { get; } = new DropDown<ServicesInstance> { Width = Defaults.WidgetWidth };

		public Button BtnAdd { get; } = new Button("Add");

		public Button BtnCancel { get; } = new Button("Cancel");

		public Button BtnCreateNewService { get; } = new Button("Create New Service");
	}
}