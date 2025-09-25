namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs.YesNoCancelDialog
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class YesNoCancelDialogView : ScriptDialog
	{
		public YesNoCancelDialogView(IEngine engine) : base(engine)
		{
		}

		public Label Message { get; } = new Label();

		public Button CancelButton { get; } = new Button("Cancel") { Width = 150, Height = 25 };

		public Button YesButton { get; } = new Button("Yes") { Width = 70, Height = 25 };

		public Button NoButton { get; } = new Button("No") { Width = 70, Height = 25, Margin = new Margin(75, 5, 5, 5) };

		public override void Build()
		{
			Clear();
			Layout.RowPosition = 0;
			AllowOverlappingWidgets = true;

			Title = "Confirmation required";

			AddWidget(Message, Layout.RowPosition, 0, 1, 2);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(YesButton, ++Layout.RowPosition, 0);
			AddWidget(NoButton, Layout.RowPosition, 0);
			AddWidget(CancelButton, ++Layout.RowPosition, 0);

			SetColumnWidth(0, 160);
		}
	}
}