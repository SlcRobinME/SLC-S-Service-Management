namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs.YesNoCancelDialog
{
	using System;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions;

	internal class YesNoCancelDialogPresenter
	{
		private readonly YesNoCancelDialogModel model;
		private readonly YesNoCancelDialogView view;

		public YesNoCancelDialogPresenter(YesNoCancelDialogView view, YesNoCancelDialogModel model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		public event EventHandler<EventArgs> Cancel;

		public event EventHandler<EventArgs> No;

		public event EventHandler<EventArgs> Yes;

		public void BuildView()
		{
			view.Build();
		}

		public void LoadFromModel()
		{
			view.Message.Text = StringExtensions.Wrap(model.Message, 100);
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.NoButton.Pressed += OnNoButtonPressed;
			view.YesButton.Pressed += OnYesButtonPressed;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Cancel?.Invoke(this, EventArgs.Empty);
		}

		private void OnNoButtonPressed(object sender, EventArgs e)
		{
			No?.Invoke(this, EventArgs.Empty);
		}

		private void OnYesButtonPressed(object sender, EventArgs e)
		{
			Yes?.Invoke(this, EventArgs.Empty);
		}
	}
}