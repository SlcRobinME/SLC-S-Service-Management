namespace SLC_SM_IAS_Add_Service_Configuration_1.Presenters
{
	using System;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Automation;
	using SLC_SM_IAS_Add_Service_Configuration_1.Views;

	public class ServiceConfigurationPresenter
	{
		private readonly IEngine engine;
		private readonly string[] existingNames;
		private readonly ServiceConfigurationView view;

		public ServiceConfigurationPresenter(IEngine engine, ServiceConfigurationView view, string[] existingNames)
		{
			this.engine = engine;
			this.view = view;
			this.existingNames = existingNames;

			view.Label.Changed += (sender, args) => ValidateLabel(args.Value);
			view.Mandatory.Changed += (sender, args) => OnMandatoryValue(args.IsChecked);
			view.TBoxValue.Changed += (sender, args) => ValidateValue(args.Value);
			view.ValueType.Changed += (sender, args) => OnUpdateServiceProperty(args.Selected);
		}

		public ServiceConfigurationParametersValuesSection Section => new ServiceConfigurationParametersValuesSection
		{
			Label = view.Label.Text,
			Mandatory = view.Mandatory.IsChecked,
			StringValue = view.ValueType.Selected == ServiceConfigurationView.ValueTypeEnum.String ? view.TBoxValue.Text : null,
			DoubleValue = view.ValueType.Selected == ServiceConfigurationView.ValueTypeEnum.String ? default(double?) : view.NumValue.Value,
			ProfileParameterID = String.Empty,
			ServiceParameterID = String.Empty,
		};

		public void LoadFromModel()
		{
			OnUpdateServiceProperty(view.ValueType.Selected);
			OnMandatoryValue(view.Mandatory.IsChecked);
		}

		public void LoadFromModel(ServiceConfigurationParametersValuesSection section)
		{
			// Load correct types
			LoadFromModel();

			view.BtnAdd.Text = "Edit";
			view.ValueType.Selected = String.IsNullOrEmpty(section.StringValue) ? ServiceConfigurationView.ValueTypeEnum.Double : ServiceConfigurationView.ValueTypeEnum.String;
			OnUpdateServiceProperty(view.ValueType.Selected);

			view.Mandatory.IsChecked = section.Mandatory == true;
			OnMandatoryValue(view.Mandatory.IsChecked);

			view.Label.Text = section.Label;
			view.TBoxValue.Text = section.StringValue ?? String.Empty;
			view.NumValue.Value = section.DoubleValue ?? 0;
		}

		public bool Validate()
		{
			bool ok = true;

			ok &= ValidateLabel(view.Label.Text);

			////if (view.ValueType.Selected == ServiceConfigurationView.ValueTypeEnum.String)
			////{
			////	ok &= ValidateValue(view.TBoxValue.Text);
			////}

			return ok;
		}

		private void OnMandatoryValue(bool isChecked)
		{
			view.Mandatory.Text = isChecked ? "Yes" : "No";
		}

		private void OnUpdateServiceProperty(ServiceConfigurationView.ValueTypeEnum selectedType)
		{
			if (selectedType == ServiceConfigurationView.ValueTypeEnum.String)
			{
				view.NumValue.IsEnabled = false;
				view.TBoxValue.Text = String.Empty;
				view.TBoxValue.IsEnabled = true;
			}
			else
			{
				view.NumValue.IsEnabled = true;
				view.TBoxValue.Text = String.Empty;
				view.TBoxValue.IsEnabled = false;
			}
		}

		private bool ValidateLabel(string newValue)
		{
			if (String.IsNullOrWhiteSpace(newValue))
			{
				view.ErrorLabel.Text = "Please enter a value!";
				return false;
			}

			if (existingNames.Contains(newValue))
			{
				view.ErrorLabel.Text = "Label already exists!";
				return false;
			}

			view.ErrorLabel.Text = String.Empty;
			return true;
		}

		private bool ValidateValue(string newValue)
		{
			if (String.IsNullOrWhiteSpace(newValue))
			{
				view.ErrorValue.Text = "Please enter a value!";
				return false;
			}

			view.ErrorValue.Text = String.Empty;
			return true;
		}
	}
}