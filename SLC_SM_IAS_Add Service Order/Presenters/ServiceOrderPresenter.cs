namespace SLC_SM_IAS_Add_Service_Order_1.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcPeople_Organizations;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using SLC_SM_IAS_Add_Service_Order_1.Views;

	public class ServiceOrderPresenter
	{
		private readonly IEngine engine;
		private readonly ServiceOrderView view;
		private readonly List<string> getServiceOrderItemLabels;
		private ServiceOrdersInstance instanceToReturn;

		public ServiceOrderPresenter(IEngine engine, ServiceOrderView view, string[] getServiceOrderItemLabels)
		{
			this.engine = engine;
			this.view = view;
			this.getServiceOrderItemLabels = getServiceOrderItemLabels.ToList();
			instanceToReturn = new ServiceOrdersInstance();

			view.TboxName.Changed += (sender, args) => ValidateLabel(args.Value);
		}

		public ServiceOrdersInstance GetData
		{
			get
			{
				instanceToReturn.ServiceOrderInfo.Name = view.TboxName.Text;
				instanceToReturn.ServiceOrderInfo.ExternalID = view.ExternalId.Text;
				instanceToReturn.ServiceOrderInfo.Priority = view.Priority.Selected;
				instanceToReturn.ServiceOrderInfo.Description = view.Description.Text;
				instanceToReturn.ServiceOrderInfo.RelatedOrganization = view.Org.Selected?.ID.Id;

				instanceToReturn.ServiceOrderInfo.OrderContact.Clear();
				view.Contact.CheckedOptions.Select(x => x.Value.ID.Id).ForEach(f => instanceToReturn.ServiceOrderInfo.OrderContact.Add(f));

				if (!instanceToReturn.ServiceOrderItems.Any())
				{
					instanceToReturn.ServiceOrderItems.Add(new ServiceOrderItemsSection());
				}

				return instanceToReturn;
			}
		}

		public void LoadFromModel()
		{
			// Load correct types
			view.Priority.SetOptions(new List<Option<SlcServicemanagementIds.Enums.ServiceorderpriorityEnum>>
			{
				new Option<SlcServicemanagementIds.Enums.ServiceorderpriorityEnum>(SlcServicemanagementIds.Enums.Serviceorderpriority.High, SlcServicemanagementIds.Enums.ServiceorderpriorityEnum.High),
				new Option<SlcServicemanagementIds.Enums.ServiceorderpriorityEnum>(SlcServicemanagementIds.Enums.Serviceorderpriority.Medium, SlcServicemanagementIds.Enums.ServiceorderpriorityEnum.Medium),
				new Option<SlcServicemanagementIds.Enums.ServiceorderpriorityEnum>(SlcServicemanagementIds.Enums.Serviceorderpriority.Low, SlcServicemanagementIds.Enums.ServiceorderpriorityEnum.Low),
			});

			var orgDomHelper = new DomHelper(engine.SendSLNetMessages, SlcPeople_OrganizationsIds.ModuleId);
			var orgInstances = orgDomHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.Organizations.Id))
				.Select(x => new OrganizationsInstance(x))
				.ToArray();

			var orgOptions = orgInstances.Select(x => new Option<OrganizationsInstance>(x.Name, x)).ToList();
			orgOptions.Insert(0, new Option<OrganizationsInstance>("-None-", null));
			view.Org.SetOptions(orgOptions);

			var peopleInstances = orgDomHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcPeople_OrganizationsIds.Definitions.People.Id))
				.Select(x => new PeopleInstance(x))
				.ToArray();

			view.Contact.SetOptions(peopleInstances.Select(x => new Option<PeopleInstance>(x.Name, x)));
		}

		public void LoadFromModel(ServiceOrdersInstance instance)
		{
			instanceToReturn = instance;
			getServiceOrderItemLabels.RemoveAll(x => x == instance.ServiceOrderInfo.Name);

			// Load correct types
			LoadFromModel();

			view.BtnAdd.Text = "Edit";
			view.TboxName.Text = instance.ServiceOrderInfo.Name;

			if (instance.ServiceOrderInfo.Priority.HasValue)
			{
				view.Priority.Selected = instance.ServiceOrderInfo.Priority.Value;
			}

			if (instance.ServiceOrderInfo.RelatedOrganization.HasValue && view.Org.Options.Any(x => x.Value?.ID.Id == instance.ServiceOrderInfo.RelatedOrganization.Value))
			{
				view.Org.Selected = view.Org.Options.First(x => x.Value?.ID.Id == instance.ServiceOrderInfo.RelatedOrganization.Value).Value;
			}

			if (instance.ServiceOrderInfo.OrderContact.Any() && view.Contact.Options.Any(x => instance.ServiceOrderInfo.OrderContact.Contains(x.Value.ID.Id)))
			{
				var checkedOptions = view.Contact.Options.Where(x => instance.ServiceOrderInfo.OrderContact.Contains(x.Value.ID.Id)).ToList();
				foreach (Option<PeopleInstance> option in checkedOptions)
				{
					view.Contact.Check(option);
				}
			}
		}

		public bool Validate()
		{
			bool ok = true;

			ok &= ValidateLabel(view.TboxName.Text);

			return ok;
		}

		private bool ValidateLabel(string newValue)
		{
			if (String.IsNullOrWhiteSpace(newValue))
			{
				view.ErrorName.Text = "Please enter a value!";
				return false;
			}

			if (getServiceOrderItemLabels.Contains(newValue, StringComparer.InvariantCultureIgnoreCase))
			{
				view.ErrorName.Text = "Name already exists!";
				return false;
			}

			view.ErrorName.Text = String.Empty;
			return true;
		}
	}
}