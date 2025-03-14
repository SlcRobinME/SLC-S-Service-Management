namespace SLC_SM_Create_Service_Inventory_Item.Presenters
{
	using System;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Library;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using SLC_SM_Create_Service_Inventory_Item.Views;

	public class ServicePresenter
	{
		private readonly IEngine engine;
		private readonly Repo repo;
		private readonly ServiceView view;
		private readonly string[] getServiceLabels;
		private ServicesInstance instanceToReturn;

		public ServicePresenter(IEngine engine, Repo repo, ServiceView view, string[] getServiceLabels)
		{
			this.engine = engine;
			this.repo = repo;
			this.view = view;
			this.getServiceLabels = getServiceLabels;
			instanceToReturn = new ServicesInstance();

			view.IndefiniteRuntime.Changed += (sender, args) => view.End.IsEnabled = !args.IsChecked;
			view.TboxName.Changed += (sender, args) => ValidateLabel(args.Value);
		}

		public ServicesInstance Instance
		{
			get
			{
				instanceToReturn.ServiceInfo.ServiceName = view.TboxName.Text;
				instanceToReturn.ServiceInfo.ServiceStartTime = view.Start.DateTime.ToUniversalTime();
				instanceToReturn.ServiceInfo.ServiceEndTime = view.IndefiniteRuntime.IsChecked ? default(DateTime?) : view.End.DateTime.ToUniversalTime();
				instanceToReturn.ServiceInfo.Icon = instanceToReturn.ServiceInfo.Icon ?? String.Empty;
				instanceToReturn.ServiceInfo.Description = instanceToReturn.ServiceInfo.Description ?? String.Empty;
				instanceToReturn.ServiceInfo.ServiceCategory = view.ServiceCategory.Selected?.ID.Id;
				instanceToReturn.ServiceInfo.ServiceSpecifcation = view.Specs.Selected?.ID.Id;

				return instanceToReturn;
			}
		}

		public void LoadFromModel()
		{
			// Load correct types
			view.ServiceCategory.SetOptions(repo.AllCategories.OrderBy(x => x.Name).Select(x => new Option<ServiceCategoryInstance>(x.Name, x)));

			var specs = repo.AllSpecs.OrderBy(x => x.Name).Select(x => new Option<ServiceSpecificationsInstance>(x.Name, x)).ToList();
			specs.Insert(0, new Option<ServiceSpecificationsInstance>("-None-", null));
			view.Specs.SetOptions(specs);

			view.Start.DateTime = DateTime.Now + TimeSpan.FromHours(1);
			view.End.DateTime = view.Start.DateTime + TimeSpan.FromDays(7);
		}

		public void LoadFromModel(ServicesInstance instance)
		{
			var section = instance.ServiceInfo;
			instanceToReturn = instance;

			// Load correct types
			LoadFromModel();

			view.BtnAdd.Text = "Edit Service Inventory Item";
			view.TboxName.Text = section.ServiceName;
			if (instance.ServiceInfo.ServiceStartTime.HasValue)
			{
				view.Start.DateTime = instance.ServiceInfo.ServiceStartTime.Value;
			}

			if (instance.ServiceInfo.ServiceEndTime.HasValue)
			{
				view.End.DateTime = instance.ServiceInfo.ServiceEndTime.Value;
				view.IndefiniteRuntime.IsChecked = false;
			}
			else
			{
				view.End.DateTime = view.Start.DateTime + TimeSpan.FromDays(7);
				view.IndefiniteRuntime.IsChecked = true;
			}

			if (section.ServiceCategory.HasValue && repo.AllCategories.Any(x => x.ID.Id == section.ServiceCategory.Value))
			{
				view.ServiceCategory.Selected = repo.AllCategories.FirstOrDefault(x => x.ID.Id == section.ServiceCategory.Value);
			}

			if (section.ServiceSpecifcation.HasValue && repo.AllSpecs.Any(x => x.ID.Id == section.ServiceSpecifcation.Value))
			{
				view.Specs.Selected = repo.AllSpecs.FirstOrDefault(x => x.ID.Id == section.ServiceSpecifcation.Value);
			}

			if (instance.ServiceInfo.ServiceStartTime.HasValue)
			{
				view.Start.DateTime = instance.ServiceInfo.ServiceStartTime.Value.ToLocalTime();
			}

			if (instance.ServiceInfo.ServiceEndTime.HasValue)
			{
				view.End.DateTime = instance.ServiceInfo.ServiceEndTime.Value.ToLocalTime();
			}
		}

		public bool Validate()
		{
			bool ok = true;

			ok &= ValidateLabel(view.TboxName.Text);

			if (view.Start.DateTime < DateTime.Now)
			{
				ok = false;
				view.ErrorStart.Text = "Please make a selection which doesn't lie in the past.";
			}
			else if (!view.IndefiniteRuntime.IsChecked && view.End.DateTime < view.Start.DateTime)
			{
				ok = false;
				view.ErrorStart.Text = "End Time must come after the start time.";
			}
			else
			{
				view.ErrorStart.Text = String.Empty;
			}

			return ok;
		}

		private bool ValidateLabel(string newValue)
		{
			if (String.IsNullOrWhiteSpace(newValue))
			{
				view.ErrorName.Text = "Please enter a value!";
				return false;
			}

			if (getServiceLabels.Contains(newValue, StringComparer.InvariantCultureIgnoreCase))
			{
				view.ErrorName.Text = "Name already exists!";
				return false;
			}

			view.ErrorName.Text = String.Empty;
			return true;
		}
	}
}