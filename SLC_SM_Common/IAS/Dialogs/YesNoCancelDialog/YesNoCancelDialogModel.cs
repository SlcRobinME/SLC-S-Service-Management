namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs.YesNoCancelDialog
{
	internal class YesNoCancelDialogModel
	{
		public YesNoCancelDialogModel(string message)
		{
			Message = message;
		}

		public string Message { get; }
	}
}