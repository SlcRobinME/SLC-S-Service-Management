namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs.PopupDialog
{
	using System;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions;

	internal class PopupDialogPresenter
	{
		private readonly PopupDialogModel model;
		private readonly PopupDialogView view;

		public PopupDialogPresenter(PopupDialogView view, PopupDialogModel model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model ?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		public event EventHandler<EventArgs> Confirm;

		public void BuildView()
		{
			view.Build();
		}

		public void LoadFromModel()
		{
			view.Title = model.Title;
			view.Lbl.Text = StringExtensions.Wrap(model.Message, 300);
			view.ButtonOk.Text = model.ButtonText;
		}

		private void Init()
		{
			view.ButtonOk.Pressed += OnConfirmButtonPressed;
		}

		private void OnConfirmButtonPressed(object sender, EventArgs e)
		{
			Confirm?.Invoke(this, EventArgs.Empty);
		}
	}
}