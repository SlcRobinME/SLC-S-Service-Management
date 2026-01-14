namespace SLC_SM_IAS_Profiles.Views
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class TextOptionsView : Dialog
	{
		public TextOptionsView(IEngine engine) : base(engine)
		{
			Title = "Update Text Options";

			AddWidget(LblRegex, 0, 0);
			AddWidget(Regex, 0, 1, 1, 3);
			AddWidget(LblUserMessage, 1, 0);
			AddWidget(UserMessage, 1, 1, 1, 3);
			AddWidget(new WhiteSpace(), 2, 0);
			AddWidget(BtnReturn, 3, 0);
			AddWidget(BtnApply, 3, 1);
			AddWidget(new WhiteSpace() { Width = 150}, 3, 3);
			AddWidget(new WhiteSpace() { Width = 150 }, 3, 4);
			AddWidget(new WhiteSpace() { Width = 150 }, 3, 5);
			AddWidget(new WhiteSpace() { Width = 150 }, 3, 6);
			AddWidget(new WhiteSpace() { Width = 150 }, 3, 7);
			AddWidget(new WhiteSpace() { Width = 150 }, 3, 8);
			AddWidget(new WhiteSpace() { Width = 150 }, 3, 9);
			AddWidget(new WhiteSpace() { Width = 150 }, 3, 10);
		}

		public Label LblRegex { get; } = new Label("Regex");

		public TextBox Regex { get; } = new TextBox() { IsMultiline = true };

		public Label LblUserMessage { get; } = new Label("User Message");

		public TextBox UserMessage { get; } = new TextBox();

		public Button BtnApply { get; } = new Button("Apply") { Width = 100, Style = ButtonStyle.CallToAction };

		public Button BtnReturn { get; } = new Button("Return") { Width = 100 };
	}
}