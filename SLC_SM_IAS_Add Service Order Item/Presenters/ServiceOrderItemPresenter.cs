namespace SLC_SM_IAS_Add_Service_Order_Item_1.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Library;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using SLC_SM_IAS_Add_Service_Order_Item_1.Views;

	public class ServiceOrderItemPresenter
	{
		private readonly IEngine engine;
		private readonly DomHelper domHelper;
		private readonly ServiceOrderItemView view;
		private readonly Repo repo;
		private readonly string[] getServiceOrderItemLabels;
		private ServiceOrderItemsInstance instanceToReturn;
		private bool isEdit;

		public ServiceOrderItemPresenter(IEngine engine, DomHelper domHelper, ServiceOrderItemView view, Repo repo, string[] getServiceOrderItemLabels)
		{
			this.engine = engine;
			this.domHelper = domHelper;
			this.view = view;
			this.repo = repo;
			this.getServiceOrderItemLabels = getServiceOrderItemLabels;
			instanceToReturn = new ServiceOrderItemsInstance();

			view.TboxName.Changed += (sender, args) => ValidateLabel(args.Value);
		}

		public ServiceOrderItemsInstance GetData
		{
			get
			{
				instanceToReturn.ServiceOrderItemInfo.Name = view.TboxName.Text;
				instanceToReturn.ServiceOrderItemInfo.Action = view.ActionType.Selected.ToString();
				instanceToReturn.ServiceOrderItemServiceInfo.ServiceCategory = view.Category.Selected?.ID.Id;
				instanceToReturn.ServiceOrderItemServiceInfo.ServiceSpecification = view.Specification.Selected?.ID.Id;
				instanceToReturn.ServiceOrderItemServiceInfo.Service = view.Service.Selected?.ID.Id;

				if (!isEdit && view.Specification.Selected != null)
				{
					if (view.Specification.Selected.ServiceSpecificationInfo.ServiceConfiguration.HasValue && view.Specification.Selected.ServiceSpecificationInfo.ServiceConfiguration != Guid.Empty)
					{
						instanceToReturn.ServiceOrderItemServiceInfo.Configuration = DuplicateInstance(view.Specification.Selected.ServiceSpecificationInfo.ServiceConfiguration.Value);
					}

					if (view.Specification.Selected.ServiceSpecificationInfo.ServiceProperties.HasValue && view.Specification.Selected.ServiceSpecificationInfo.ServiceProperties != Guid.Empty)
					{
						instanceToReturn.ServiceOrderItemServiceInfo.Properties = DuplicateInstance(view.Specification.Selected.ServiceSpecificationInfo.ServiceProperties.Value);
					}
				}

				return instanceToReturn;
			}
		}

		private Guid? DuplicateInstance(Guid id)
		{
			var instance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(id)).FirstOrDefault();
			if (instance == null)
			{
				return default(Guid?);
			}

			instance.ID = new DomInstanceId(Guid.NewGuid());
			domHelper.DomInstances.CreateOrUpdate(new List<DomInstance> { instance });
			return instance.ID.Id;
		}

		public void LoadFromModel()
		{
			// Load correct types
			view.Category.SetOptions(repo.AllCategories.Select(x => new Option<ServiceCategoryInstance>(x.Name, x)));
			view.Specification.SetOptions(repo.AllSpecs.Select(x => new Option<ServiceSpecificationsInstance>(x.Name, x)));

			var serviceOptions = repo.AllServices.Select(x => new Option<ServicesInstance>(x.Name, x)).ToList();
			serviceOptions.Insert(0, new Option<ServicesInstance>("-None-", null));
			view.Service.SetOptions(serviceOptions);
		}

		public void LoadFromModel(ServiceOrderItemsInstance instance)
		{
			instanceToReturn = instance;
			isEdit = true;

			// Load correct types
			LoadFromModel();

			view.BtnAdd.Text = "Edit";
			view.TboxName.Text = instance.Name;
			view.ActionType.Selected = Enum.TryParse(instance.ServiceOrderItemInfo.Action, true, out ServiceOrderItemView.ActionTypeEnum action) ? action : ServiceOrderItemView.ActionTypeEnum.NoChange;

			ServiceCategoryInstance serviceCategoryInstance = repo.AllCategories.FirstOrDefault(x => x.ID.Id == instance.ServiceOrderItemServiceInfo.ServiceCategory);
			if (serviceCategoryInstance != null && view.Category.Values.Contains(serviceCategoryInstance))
			{
				view.Category.Selected = serviceCategoryInstance;
			}

			ServiceSpecificationsInstance serviceSpecificationsInstance = repo.AllSpecs.FirstOrDefault(x => x.ID.Id == instance.ServiceOrderItemServiceInfo.ServiceSpecification);
			if (serviceSpecificationsInstance != null && view.Specification.Values.Contains(serviceSpecificationsInstance))
			{
				view.Specification.Selected = serviceSpecificationsInstance;
			}

			ServicesInstance serviceInstance = repo.AllServices.FirstOrDefault(x => x.ID.Id == instance.ServiceOrderItemServiceInfo.Service);
			if (serviceInstance != null && view.Service.Values.Contains(serviceInstance))
			{
				view.Service.Selected = serviceInstance;
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
				view.ErrorName.Text = "Label already exists!";
				return false;
			}

			view.ErrorName.Text = String.Empty;
			return true;
		}
	}
}