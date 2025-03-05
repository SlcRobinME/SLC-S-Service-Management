namespace SLC_SM_IAS_Add_Service_Property_1.Presenters
{
	using System;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using SLC_SM_IAS_Add_Service_Property_1.Views;

	public class ServicePropertyPresenter
	{
		private readonly IEngine engine;
		private readonly ServicePropertyView view;
		private readonly ServicePropertiesInstance[] servicePropertiesInstances;

		public ServicePropertyPresenter(IEngine engine, ServicePropertyView view, ServicePropertiesInstance[] servicePropertiesInstances)
		{
			this.engine = engine;
			this.view = view;
			this.servicePropertiesInstances = servicePropertiesInstances;

			view.TBoxValue.Changed += (sender, args) => ValidateValue(args.Value);
			view.ServiceProperty.Changed += (sender, args) => OnUpdateServiceProperty(args.Selected);
		}

		public ServicePropertyValueSection Section => new ServicePropertyValueSection
		{
			Property = view.ServiceProperty.Selected.ID.Id,
			PropertyName = view.ServiceProperty.Selected.Name,
			Value = !String.IsNullOrEmpty(view.TBoxValue.Text) ? view.TBoxValue.Text : view.DdValue.Selected,
		};

		public void LoadFromModel()
		{
			// Load correct types
			view.ServiceProperty.SetOptions(servicePropertiesInstances.Select(x => new Option<ServicePropertiesInstance>(x.Name, x)));

			OnUpdateServiceProperty(view.ServiceProperty.Selected);
		}

		private void OnUpdateServiceProperty(ServicePropertiesInstance servicePropertySelected)
		{
			view.DdValue.SetOptions(servicePropertySelected.DiscreteServicePropertyValueOptions.Select(x => x.DiscreteValue));
			view.TBoxValue.Text = String.Empty;
			view.TBoxValue.IsEnabled = false;
		}

		public void LoadFromModel(ServicePropertyValueSection section)
		{
			// Load correct types
			LoadFromModel();

			view.BtnAdd.Text = "Edit";
			view.ServiceProperty.Selected = servicePropertiesInstances.FirstOrDefault(x => x.ID.Id == section.Property);
			OnUpdateServiceProperty(view.ServiceProperty.Selected);

			if (view.ServiceProperty.Selected.ServicePropertyInfo.Type == SlcServicemanagementIds.Enums.TypeEnum.Discrete)
			{
				view.TBoxValue.Text = String.Empty;
				view.TBoxValue.IsEnabled = false;
				view.DdValue.Selected = section.Value;
				view.DdValue.IsEnabled = true;
			}
			else
			{
				view.TBoxValue.Text = section.Value;
				view.TBoxValue.IsEnabled = true;
				view.DdValue.Selected = String.Empty;
				view.DdValue.IsEnabled = false;
			}
		}

		public bool Validate()
		{
			bool ok = true;

			if (view.ServiceProperty.Selected.ServicePropertyInfo.Type == SlcServicemanagementIds.Enums.TypeEnum.String)
			{
				ok &= ValidateValue(view.TBoxValue.Text);
			}

			return ok;
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