namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs.PopupDialog
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class PopupDialogView : Dialog
	{
		public PopupDialogView(IEngine engine) : base(engine)
		{
		}

		public Label Lbl { get; } = new Label();

		public Button ButtonOk { get; } = new Button { Style = ButtonStyle.CallToAction };

		public void Build()
		{
			Clear();

			int row = 0;

			Lbl.SetWidthAuto();
			AddWidget(Lbl, row++, 0);

			AddWidget(new WhiteSpace { Height = 25 }, row++, 0);
			AddWidget(ButtonOk, row++, 1, HorizontalAlignment.Right);

			SetColumnWidth(0, 300);
		}
	}
}