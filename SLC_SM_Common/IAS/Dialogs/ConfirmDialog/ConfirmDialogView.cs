namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs.ConfirmDialog
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class ConfirmDialogView : ScriptDialog
	{
		public ConfirmDialogView(IEngine engine) : base(engine)
		{
		}

		public Label ConfirmationMessage { get; } = new Label();

		public Button CancelButton { get; } = new Button("Cancel") { Width = 150, Height = 25 };

		public Button ConfirmButton { get; } = new Button("Confirm") { Width = 150, Height = 25, Style = ButtonStyle.CallToAction };

		public override void Build()
		{
			Clear();
			Layout.RowPosition = 0;

			Title = "Confirmation required";

			AddWidget(ConfirmationMessage, Layout.RowPosition, 0, 1, 2);

			AddWidget(new WhiteSpace { Height = 25 }, ++Layout.RowPosition, 0);

			AddWidget(ConfirmButton, ++Layout.RowPosition, 0);
			AddWidget(CancelButton, Layout.RowPosition, 1);

			SetColumnWidth(0, 160);
		}
	}
}