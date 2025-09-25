﻿namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS
{
	using System;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs;

	public static class EngineExtensions
	{
		public static bool ShowConfirmDialog(this IEngine engine, string message)
		{
			var model = new Dialogs.ConfirmDialog.ConfirmDialogModel(message);
			var view = new Dialogs.ConfirmDialog.ConfirmDialogView(engine);
			var presenter = new Dialogs.ConfirmDialog.ConfirmDialogPresenter(view, model);

			var confirmed = false;
			presenter.Cancel += (sender, arg) => { confirmed = false; };
			presenter.Confirm += (sender, arg) => { confirmed = true; };

			presenter.LoadFromModel();
			presenter.BuildView();

			view.Show();

			return confirmed;
		}

		public static void ShowErrorDialog(this IEngine engine, Exception ex)
		{
			var model = new ErrorDialogModel("Error", ex.Message, ex.ToString());
			var view = new ErrorDialogView(engine);
			var presenter = new ErrorDialogPresenter(view, model);

			presenter.LoadFromModel();

			new InteractiveController(engine).ShowDialog(view);
		}

		public static YesNoCancelOption ShowYesNoCancelDialog(this IEngine engine, string message)
		{
			var model = new Dialogs.YesNoCancelDialog.YesNoCancelDialogModel(message);
			var view = new Dialogs.YesNoCancelDialog.YesNoCancelDialogView(engine);
			var presenter = new Dialogs.YesNoCancelDialog.YesNoCancelDialogPresenter(view, model);

			YesNoCancelOption result = YesNoCancelOption.Cancel;
			presenter.Cancel += (sender, arg) => { result = YesNoCancelOption.Cancel; };
			presenter.Yes += (sender, arg) => { result = YesNoCancelOption.Yes; };
			presenter.No += (sender, arg) => { result = YesNoCancelOption.No; };

			presenter.LoadFromModel();
			presenter.BuildView();

			view.Show();

			return result;
		}
	}
}