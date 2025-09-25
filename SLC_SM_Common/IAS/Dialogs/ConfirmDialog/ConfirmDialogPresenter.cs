namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs.ConfirmDialog
{
	using System;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions;

	internal class ConfirmDialogPresenter
	{
		private readonly ConfirmDialogModel model;
		private readonly ConfirmDialogView view;

		public ConfirmDialogPresenter(ConfirmDialogView view, ConfirmDialogModel model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		public event EventHandler<EventArgs> Cancel;

		public event EventHandler<EventArgs> Confirm;

		public void BuildView()
		{
			view.Build();
		}

		public void LoadFromModel()
		{
			view.ConfirmationMessage.Text = StringExtensions.Wrap(model.ConfirmationMessage, 100);
		}

		private void Init()
		{
			view.CancelButton.Pressed += OnCancelButtonPressed;
			view.ConfirmButton.Pressed += OnConfirmButtonPressed;
		}

		private void OnCancelButtonPressed(object sender, EventArgs e)
		{
			Cancel?.Invoke(this, EventArgs.Empty);
		}

		private void OnConfirmButtonPressed(object sender, EventArgs e)
		{
			Confirm?.Invoke(this, EventArgs.Empty);
		}
	}
}