namespace SLC_SM_IAS_Add_Service_Order_1.Views
{
	using System;
	using DomHelpers.SlcPeople_Organizations;
	using DomHelpers.SlcServicemanagement;
	using Library;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ServiceOrderView : Dialog
	{
		public ServiceOrderView(IEngine engine) : base(engine)
		{
			Title = "Manage Service Order";
			MinWidth = Defaults.DialogMinWidth;

			int row = 0;
			AddWidget(new Label("Service Order Details") { Style = TextStyle.Heading }, row, 0);
			AddWidget(LblName, ++row, 0);
			AddWidget(TboxName, row, 1);
			AddWidget(ErrorName, row, 2);

			AddWidget(LblDescription, ++row, 0);
			AddWidget(Description, row, 1);

			AddWidget(LblExternalId, ++row, 0);
			AddWidget(ExternalId, row, 1);

			AddWidget(LblPriority, ++row, 0);
			AddWidget(Priority, row, 1);

			AddWidget(LblOrg, ++row, 0);
			AddWidget(Org, row, 1);

			AddWidget(LblContact, ++row, 0);
			AddWidget(Contact, row, 1);

			AddWidget(new WhiteSpace(), ++row, 0);
			AddWidget(BtnCancel, ++row, 0);
			AddWidget(BtnAdd, row, 1);
		}

		public Label LblName { get; } = new Label("Label");

		public TextBox TboxName { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Label ErrorName { get; } = new Label(String.Empty);

		public Label LblExternalId { get; } = new Label("External ID");

		public TextBox ExternalId { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Label LblPriority { get; } = new Label("Priority");

		public DropDown<SlcServicemanagementIds.Enums.ServiceorderpriorityEnum> Priority { get; } = new DropDown<SlcServicemanagementIds.Enums.ServiceorderpriorityEnum> { Width = Defaults.WidgetWidth };

		public Label LblDescription { get; } = new Label("Description");

		public TextBox Description { get; } = new TextBox { Width = Defaults.WidgetWidth };

		public Label LblOrg { get; } = new Label("Organization");

		public DropDown<OrganizationsInstance> Org { get; } = new DropDown<OrganizationsInstance> { Width = Defaults.WidgetWidth };

		public Label LblContact { get; } = new Label("Order Contact");

		public CheckBoxList<PeopleInstance> Contact { get; } = new CheckBoxList<PeopleInstance> { Width = Defaults.WidgetWidth };

		public Button BtnAdd { get; } = new Button("Add");

		public Button BtnCancel { get; } = new Button("Cancel");
	}
}