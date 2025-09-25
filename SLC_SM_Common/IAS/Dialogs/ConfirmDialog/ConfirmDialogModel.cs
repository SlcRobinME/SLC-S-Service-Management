namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs.ConfirmDialog
{
	internal class ConfirmDialogModel
	{
		public ConfirmDialogModel(string confirmationMessage)
		{
			ConfirmationMessage = confirmationMessage;
		}

		public string ConfirmationMessage { get; }
	}
}