namespace SLC_SM_IAS_Add_Service_Item_1.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using SLC_SM_IAS_Add_Service_Item_1.Views;

	public class ServiceItemPresenter
	{
		private readonly IEngine engine;
		private readonly ServiceItemView view;
		private readonly string[] getServiceItemLabels;

		public ServiceItemPresenter(IEngine engine, ServiceItemView view, string[] getServiceItemLabels)
		{
			this.engine = engine;
			this.view = view;
			this.getServiceItemLabels = getServiceItemLabels;

			view.TboxLabel.Changed += (sender, args) => ValidateLabel(args.Value);
			view.ServiceItemType.Changed += (sender, args) => OnUpdateServiceItemType(args.Selected);
			view.DefinitionReferences.Changed += (sender, args) => OnUpdateDefinitionReference(args.Selected);
		}

		public ServiceItemsSection Section => new ServiceItemsSection
		{
			Label = view.TboxLabel.Text,
			ServiceItemType = view.ServiceItemType.Selected,
			DefinitionReference = view.DefinitionReferences.Selected ?? String.Empty,
			ServiceItemScript = view.ScriptSelection.Selected ?? String.Empty,
			ImplementationReference = String.Empty,
		};

		public void LoadFromModel()
		{
			// Load correct types
			view.ServiceItemType.SetOptions(
				new List<Option<SlcServicemanagementIds.Enums.ServiceitemtypesEnum>>
				{
					new Option<SlcServicemanagementIds.Enums.ServiceitemtypesEnum>(
						SlcServicemanagementIds.Enums.Serviceitemtypes.SRMBooking,
						SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking),
					new Option<SlcServicemanagementIds.Enums.ServiceitemtypesEnum>(
						SlcServicemanagementIds.Enums.Serviceitemtypes.Workflow,
						SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow),
				});
			OnUpdateServiceItemType(view.ServiceItemType.Selected);
		}

		public void LoadFromModel(ServiceItemsSection section)
		{
			// Load correct types
			LoadFromModel();

			view.BtnAdd.Text = "Edit";
			view.TboxLabel.Text = section.Label;

			if (section.ServiceItemType.HasValue)
			{
				view.ServiceItemType.Selected = section.ServiceItemType.Value;
			}

			if (!String.IsNullOrEmpty(section.DefinitionReference))
			{
				view.DefinitionReferences.Selected = section.DefinitionReference;
			}

			if (!String.IsNullOrEmpty(section.ServiceItemScript))
			{
				view.ScriptSelection.SetOptions(new[] { section.ServiceItemScript });
			}
		}

		public bool Validate()
		{
			bool ok = true;

			ok &= ValidateLabel(view.TboxLabel.Text);

			return ok;
		}

		private bool ValidateLabel(string newValue)
		{
			if (String.IsNullOrWhiteSpace(newValue))
			{
				view.ErrorLabel.Text = "Please enter a value!";
				return false;
			}

			if (getServiceItemLabels.Contains(newValue, StringComparer.InvariantCultureIgnoreCase))
			{
				view.ErrorLabel.Text = "Label already exists!";
				return false;
			}

			view.ErrorLabel.Text = String.Empty;
			return true;
		}

		private void OnUpdateDefinitionReference(string selected)
		{
			if (String.IsNullOrEmpty(selected))
			{
				view.ScriptSelection.SetOptions(new List<string>());
				return;
			}

			var el = engine.FindElement(selected);
			if (el == null)
			{
				view.ScriptSelection.SetOptions(new List<string>());
				return;
			}

			var scriptName = Convert.ToString(el.GetParameter(195));
			view.ScriptSelection.SetOptions(new[] { scriptName });
		}

		private void OnUpdateServiceItemType(SlcServicemanagementIds.Enums.ServiceitemtypesEnum serviceItemType)
		{
			if (serviceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow)
			{
				view.DefinitionReferences.SetOptions(new List<string>());
				view.ScriptSelection.SetOptions(new List<string>());
				view.ScriptSelection.IsEnabled = true;
			}
			else
			{
				var bookingManagers = engine.FindElementsByProtocol("Skyline Booking Manager").Where(x => x.IsActive).Select(x => x.ElementName).ToArray();
				view.DefinitionReferences.SetOptions(bookingManagers);
				OnUpdateDefinitionReference(view.DefinitionReferences.Selected);
				view.ScriptSelection.IsEnabled = false;
			}
		}
	}
}