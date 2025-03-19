namespace SLC_SM_IAS_Add_Service_Order_Item_1.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Library;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using SLC_SM_IAS_Add_Service_Order_Item_1.Views;

	public class ServiceOrderItemPresenter
	{
		private readonly DomHelper domHelper;
		private readonly string[] getServiceOrderItemLabels;
		private readonly Repo repo;
		private readonly ServiceOrderItemView view;
		private ServiceOrderItemsInstance instanceToReturn;
		private bool isEdit;

		public ServiceOrderItemPresenter(DomHelper domHelper, ServiceOrderItemView view, Repo repo, string[] getServiceOrderItemLabels)
		{
			this.domHelper = domHelper;
			this.view = view;
			this.repo = repo;
			this.getServiceOrderItemLabels = getServiceOrderItemLabels;
			instanceToReturn = new ServiceOrderItemsInstance();

			view.IndefiniteTime.Changed += (sender, args) => view.End.IsEnabled = !args.IsChecked;
			view.TboxName.Changed += (sender, args) => ValidateLabel(args.Value);
			view.ActionType.Changed += (sender, args) =>
			{
				UpdateUiOnActionTypeChange(args.Selected);
				Validate();
			};
		}

		public ServiceOrderItemsInstance GetData
		{
			get
			{
				instanceToReturn.ServiceOrderItemInfo.Name = Name;
				instanceToReturn.ServiceOrderItemInfo.Action = view.ActionType.Selected.ToString();
				instanceToReturn.ServiceOrderItemInfo.ServiceStartTime = view.Start.DateTime;
				instanceToReturn.ServiceOrderItemInfo.ServiceEndTime = view.IndefiniteTime.IsChecked ? default(DateTime?) : view.End.DateTime;
				instanceToReturn.ServiceOrderItemInfo.ServiceIndefiniteRuntime = view.IndefiniteTime.IsChecked;
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

		public string Name => String.IsNullOrWhiteSpace(view.TboxName.Text) ? view.TboxName.PlaceHolder : view.TboxName.Text;

		public void LoadFromModel(int nr)
		{
			view.TboxName.PlaceHolder = $"Service Order Item #{nr + 1:000}";

			// Load correct types
			var categories = repo.AllCategories.OrderBy(x => x.Name).Select(x => new Option<ServiceCategoryInstance>(x.Name, x)).ToList();
			categories.Insert(0, new Option<ServiceCategoryInstance>("-None-", null));
			view.Category.SetOptions(categories);

			var specs = repo.AllSpecs.OrderBy(x => x.Name).Select(x => new Option<ServiceSpecificationsInstance>(x.Name, x)).ToList();
			specs.Insert(0, new Option<ServiceSpecificationsInstance>("-None-", null));
			view.Specification.SetOptions(specs);

			var serviceOptions = repo.AllServices.OrderBy(x => x.ServiceInfo.ServiceName).Select(x => new Option<ServicesInstance>(x.ServiceInfo.ServiceName, x)).ToList();
			serviceOptions.Insert(0, new Option<ServicesInstance>("-None-", null));
			view.Service.SetOptions(serviceOptions);

			view.Start.DateTime = DateTime.Now + TimeSpan.FromDays(1);
			view.End.DateTime = DateTime.Now + TimeSpan.FromDays(8);
			view.IndefiniteTime.IsChecked = false;

			UpdateUiOnActionTypeChange(view.ActionType.Selected);
		}

		public void LoadFromModel(ServiceOrderItemsInstance instance)
		{
			instanceToReturn = instance;
			isEdit = true;

			// Load correct types
			LoadFromModel(0);

			view.BtnAdd.Text = "Edit Service Order Item";
			view.TboxName.Text = instance.Name;
			view.ActionType.Selected = Enum.TryParse(instance.ServiceOrderItemInfo.Action, true, out ServiceOrderItemView.ActionTypeEnum action)
				? action
				: ServiceOrderItemView.ActionTypeEnum.NoChange;

			view.Start.DateTime = instance.ServiceOrderItemInfo.ServiceStartTime ?? DateTime.Now;
			view.End.DateTime = instance.ServiceOrderItemInfo.ServiceEndTime ?? DateTime.Now + TimeSpan.FromDays(7);
			view.IndefiniteTime.IsChecked = instance.ServiceOrderItemInfo.ServiceIndefiniteRuntime ?? false;
			if (view.IndefiniteTime.IsChecked)
			{
				view.End.IsEnabled = false;
			}

			ServiceCategoryInstance serviceCategoryInstance = repo.AllCategories.FirstOrDefault(x => x?.ID.Id == instance.ServiceOrderItemServiceInfo.ServiceCategory);
			if (serviceCategoryInstance != null && view.Category.Values.Contains(serviceCategoryInstance))
			{
				view.Category.Selected = serviceCategoryInstance;
			}

			ServicesInstance serviceInstance = repo.AllServices.FirstOrDefault(x => x.ID.Id == instance.ServiceOrderItemServiceInfo.Service);
			if (serviceInstance != null && view.Service.Values.Contains(serviceInstance))
			{
				view.Service.Selected = serviceInstance;
			}

			ServiceSpecificationsInstance serviceSpecificationsInstance = repo.AllSpecs.FirstOrDefault(x => x?.ID.Id == instance.ServiceOrderItemServiceInfo.ServiceSpecification);
			if (serviceSpecificationsInstance != null && view.Specification.Values.Contains(serviceSpecificationsInstance))
			{
				view.Specification.Selected = serviceSpecificationsInstance;
			}
			else
			{
				view.Specification.Selected = view.Specification.Values.FirstOrDefault(x => x?.ID.Id == view.Service.Selected?.ServiceInfo.ServiceSpecifcation);
			}

			UpdateUiOnActionTypeChange(view.ActionType.Selected);
		}

		public bool Validate()
		{
			bool ok = true;

			ok &= ValidateLabel(Name);

			if (view.ActionType.Selected == ServiceOrderItemView.ActionTypeEnum.Add && view.Specification.Selected == null)
			{
				ok = false;
				view.ErrorSpecification.Text = "Selection is mandatory!";
			}
			else
			{
				view.ErrorSpecification.Text = String.Empty;
			}

			if ((view.ActionType.Selected == ServiceOrderItemView.ActionTypeEnum.Modify || view.ActionType.Selected == ServiceOrderItemView.ActionTypeEnum.Delete) && view.Service.Selected == null)
			{
				ok = false;
				view.ErrorService.Text = "Selection is mandatory!";
			}
			else
			{
				view.ErrorService.Text = String.Empty;
			}

			return ok;
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

		private void UpdateUiOnActionTypeChange(ServiceOrderItemView.ActionTypeEnum actionTypeSelected)
		{
			if (actionTypeSelected == ServiceOrderItemView.ActionTypeEnum.Add)
			{
				view.Service.IsEnabled = false;
				view.Specification.IsEnabled = true;
			}
			else if (actionTypeSelected == ServiceOrderItemView.ActionTypeEnum.Delete || actionTypeSelected == ServiceOrderItemView.ActionTypeEnum.Modify)
			{
				view.Service.IsEnabled = true;
				view.Specification.IsEnabled = false;
			}
			else
			{
				view.Service.IsEnabled = true;
				view.Specification.IsEnabled = true;
			}
		}

		private bool ValidateLabel(string newValue)
		{
			if (String.IsNullOrWhiteSpace(newValue))
			{
				view.ErrorName.Text = "Placeholder will be used.";
				return true;
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