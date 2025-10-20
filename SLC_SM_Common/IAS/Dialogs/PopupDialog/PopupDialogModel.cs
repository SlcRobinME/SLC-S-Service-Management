namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs.PopupDialog
{
	public class PopupDialogModel
	{
		public PopupDialogModel(string title, string message, string buttonText)
		{
			Title = title;
			Message = message;
			ButtonText = buttonText;
		}

		public string Title { get; }

		public string Message { get; }

		public string ButtonText { get; }
	}
}