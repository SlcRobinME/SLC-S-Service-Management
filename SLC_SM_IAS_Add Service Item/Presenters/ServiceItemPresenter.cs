namespace SLC_SM_IAS_Add_Service_Item_1.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.MediaOps.Common.IOData.Scheduling.Scripts.JobHandler;
	using Skyline.DataMiner.Utils.MediaOps.Helpers.Workflows;
	using SLC_SM_IAS_Add_Service_Item_1.Views;

	public class ServiceItemPresenter
	{
		private readonly IServiceInstanceBase domInstance;
		private readonly IEngine engine;
		private readonly string[] getServiceItemLabels;
		private readonly List<Models.Service> services = new List<Models.Service>();
		private readonly List<Models.ServiceSpecification> specifications = new List<Models.ServiceSpecification>();
		private readonly ServiceItemView view;
		private readonly Workflow[] workflows;
		private readonly List<Option<string>> allScripts;
		private List<ServiceReservationInstance> bookings = new List<ServiceReservationInstance>();

		public ServiceItemPresenter(IEngine engine, ServiceItemView view, string[] getServiceItemLabels, IServiceInstanceBase domInstance)
		{
			this.engine = engine;
			this.view = view;
			this.getServiceItemLabels = getServiceItemLabels;
			this.domInstance = domInstance;

			var workflowHelper = new WorkflowHelper(engine);
			workflows = workflowHelper.GetAllWorkflows().ToArray();

			allScripts = (engine.SendSLNetSingleResponseMessage(new GetInfoMessage(InfoType.Scripts)) as GetScriptsResponseMessage)?.Scripts.OrderBy(x => x).Select(x => new Option<string>(x)).ToList() ?? new List<Option<string>>();
			allScripts.Insert(0, new Option<string>("-None-", null));

			view.TboxLabel.Changed += (sender, args) => ValidateLabel(args.Value);
			view.ServiceItemType.Changed += (sender, args) => OnUpdateServiceItemType(args.Selected);
			view.DefinitionReferences.Changed += (sender, args) => OnUpdateDefinitionReference(args.Selected, view.ServiceItemType.Selected);
		}

		public string Name => String.IsNullOrWhiteSpace(view.TboxLabel.Text) ? view.TboxLabel.PlaceHolder : view.TboxLabel.Text;

		public ServiceItemsSection Section => new ServiceItemsSection
		{
			Label = Name,
			ServiceItemType = view.ServiceItemType.Selected,
			DefinitionReference = view.DefinitionReferences.Selected ?? String.Empty,
			ServiceItemScript = view.ScriptSelection.Selected ?? String.Empty,
			ImplementationReference = GetImplementationReference(),
		};

		private string GetImplementationReference()
		{
			if (view.ServiceItemType.Selected == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Service)
			{
				return services.Find(s => view.ImplementationReferences.Selected == GetServiceDropDownLabel(s))?.ID.ToString() ?? String.Empty;
			}

			if (view.ServiceItemType.Selected == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking)
			{
				return bookings.Find(x => view.ImplementationReferences.Selected == GetBookingDropDownLabel(x))?.ID.ToString() ?? String.Empty;
			}

			return String.Empty;
		}

		public void LoadFromModel()
		{
			// Load correct types
			view.ServiceItemType.SetOptions(
				new List<Option<SlcServicemanagementIds.Enums.ServiceitemtypesEnum>>
				{
					new Option<SlcServicemanagementIds.Enums.ServiceitemtypesEnum>(
						SlcServicemanagementIds.Enums.Serviceitemtypes.Workflow,
						SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow),
					new Option<SlcServicemanagementIds.Enums.ServiceitemtypesEnum>(
						SlcServicemanagementIds.Enums.Serviceitemtypes.Service,
						SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Service),
					new Option<SlcServicemanagementIds.Enums.ServiceitemtypesEnum>(
						SlcServicemanagementIds.Enums.Serviceitemtypes.SRMBooking,
						SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking),
				});
			OnUpdateServiceItemType(view.ServiceItemType.Selected);
		}

		public void LoadFromModel(ServiceItemsSection section)
		{
			// Load correct types
			LoadFromModel();

			view.BtnAdd.Text = "Save";
			view.TboxLabel.Text = section.Label;

			if (section.ServiceItemType.HasValue)
			{
				view.ServiceItemType.Selected = section.ServiceItemType.Value;
				OnUpdateServiceItemType(section.ServiceItemType.Value);
			}

			if (!String.IsNullOrEmpty(section.DefinitionReference))
			{
				view.DefinitionReferences.Selected = section.DefinitionReference;
				OnUpdateDefinitionReference(view.DefinitionReferences.Selected, view.ServiceItemType.Selected);
			}

			if (!String.IsNullOrEmpty(section.ImplementationReference))
			{
				if (services.Exists(s => s.ID.ToString() == section.ImplementationReference))
				{
					view.ImplementationReferences.Selected = GetServiceDropDownLabel(services.Find(s => s.ID.ToString() == section.ImplementationReference));
				}
				else if (bookings.Exists(b => b.ID.ToString() == section.ImplementationReference))
				{
					view.ImplementationReferences.Selected = GetBookingDropDownLabel(bookings.Find(s => s.ID.ToString() == section.ImplementationReference));
				}
				else
				{
					// future reference
				}
			}

			if (!String.IsNullOrEmpty(section.ServiceItemScript) && view.ScriptSelection.Options.Any(o => o.Value == section.ServiceItemScript))
			{
				view.ScriptSelection.Selected = section.ServiceItemScript;
			}
		}

		public string UpdateJobForWorkFlow(string label)
		{
			if (view.ServiceItemType.Selected != SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow)
			{
				return String.Empty;
			}

			var job = GetJobForOrder(label);
			if (job == null)
			{
				return String.Empty;
			}

			var action = new EditJobAction
			{
				DomJobId = job.ID.Id,
				End = domInstance.GetEndTime(),
			};

			// Only add start update if the job is not already running
			if (job.JobInfo.JobStart <= DateTime.UtcNow)
			{
				action.Start = domInstance.GetStartTime();
			}

			action.SendToJobHandler(engine, true);

			return job.ID.Id.ToString();
		}

		public bool Validate()
		{
			bool ok = true;

			ok &= ValidateLabel(Name);

			return ok;
		}

		private static bool FallsWithTimeRange(Models.Service service, DateTime? currentStart, DateTime? currentEnd)
		{
			bool endTimeOk = service.EndTime == null || (currentEnd != null && currentEnd < service.EndTime);

			return endTimeOk && currentStart >= service.StartTime;
		}

		private static string GetServiceDropDownLabel(Models.Service s)
		{
			if (s == null)
			{
				return String.Empty;
			}

			return $"{s.Name} ({s.ServiceID})";
		}

		private static string GetBookingDropDownLabel(ServiceReservationInstance reservation)
		{
			if (reservation == null)
			{
				return String.Empty;
			}

			return reservation.Name;
		}

		private JobsInstance GetJobForOrder(string label)
		{
			var jobFilter = DomInstanceExposers.FieldValues.DomInstanceField(SlcWorkflowIds.Sections.JobInfo.JobDescription)
				.Equal($"{domInstance.GetId()} | {label}")
				.OR(DomInstanceExposers.FieldValues.DomInstanceField(SlcWorkflowIds.Sections.JobInfo.JobDescription).Equal($"{domInstance.GetId()}|{label}"));

			var domHelper = new DomHelper(engine.SendSLNetMessages, SlcWorkflowIds.ModuleId);
			return domHelper.DomInstances.Read(jobFilter)
				.Select(x => new JobsInstance(x))
				.FirstOrDefault();
		}

		private void OnUpdateDefinitionReference(string selectedDefinitionReference, SlcServicemanagementIds.Enums.ServiceitemtypesEnum serviceItemType)
		{
			if (String.IsNullOrEmpty(selectedDefinitionReference))
			{
				view.ScriptSelection.Selected = null;
				return;
			}

			if (serviceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking)
			{
				UpdateImplementationReferenceForTypeSrmBooking(selectedDefinitionReference);

				var el = engine.FindElement(selectedDefinitionReference);
				if (el == null)
				{
					view.ScriptSelection.Selected = null;
					return;
				}

				var scriptName = Convert.ToString(el.GetParameter(195));
				if (!view.ScriptSelection.Options.Any(o => o.Value == scriptName))
				{
					view.ScriptSelection.Selected = null;
					return;
				}

				view.ScriptSelection.Selected = scriptName;
			}
			else if (serviceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Service)
			{
				var selectedSpec = specifications.Find(s => s.Name == selectedDefinitionReference);
				UpdateImplementationReferenceForTypeService(selectedSpec);
				UpdateLabelPlaceholder(selectedDefinitionReference);
			}
			else
			{
				// Not required
			}
		}

		private void OnUpdateServiceItemType(SlcServicemanagementIds.Enums.ServiceitemtypesEnum serviceItemType)
		{
			if (serviceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow)
			{
				var workflowOptions = workflows.Select(x => x.Name).OrderBy(x => x).ToList();
				view.DefinitionReferences.SetOptions(workflowOptions);
				if (workflowOptions.Exists(x => x == "Default"))
				{
					view.DefinitionReferences.Selected = "Default";
				}

				view.ScriptSelection.SetOptions(new List<string>());
				view.ScriptSelection.IsEnabled = false;
			}
			else if (serviceItemType == SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Service)
			{
				if (specifications.Count < 1)
				{
					specifications.AddRange(new DataHelperServiceSpecification(engine.GetUserConnection()).Read());
				}

				List<string> specOptions = specifications.Select(x => x.Name).OrderBy(x => x).ToList();
				specOptions.Insert(0, "-None-");
				view.DefinitionReferences.SetOptions(specOptions);
				var selectedSpec = specifications.Find(s => s.Name == view.DefinitionReferences.Selected);

				UpdateImplementationReferenceForTypeService(selectedSpec);

				view.ScriptSelection.SetOptions(new List<string>());
				view.ScriptSelection.IsEnabled = false;
			}
			else
			{
				view.ScriptSelection.SetOptions(allScripts);
				view.ScriptSelection.IsEnabled = true;

				var bookingManagers = engine.FindElementsByProtocol("Skyline Booking Manager").Where(x => x.IsActive).Select(x => x.ElementName).ToArray();
				view.DefinitionReferences.SetOptions(bookingManagers);
				OnUpdateDefinitionReference(view.DefinitionReferences.Selected, serviceItemType);
			}

			view.LblImplementationReference.IsVisible = serviceItemType != SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow;
			view.ImplementationReferences.IsVisible = serviceItemType != SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Workflow;

			UpdateLabelPlaceholder(view.DefinitionReferences.Selected);
		}

		private void UpdateImplementationReferenceForTypeSrmBooking(string selectedDefinitionReference)
		{
			if (!String.IsNullOrEmpty(selectedDefinitionReference))
			{
				bookings = new ResourceManagerHelper(engine.SendSLNetSingleResponseMessage).GetReservationInstances(
					ReservationInstanceExposers.Properties.DictStringField("Booking Manager").Equal(selectedDefinitionReference))
					.OfType<ServiceReservationInstance>()
					.Where(x => x.ContributingResourceID == Guid.Empty)
					.ToList();
			}
			else
			{
				bookings.Clear();
			}

			var options = bookings.Select(GetBookingDropDownLabel).OrderBy(x => x).ToList();
			options.Insert(0, "-None-");
			view.ImplementationReferences.SetOptions(options);
		}

		private void UpdateImplementationReferenceForTypeService(Models.ServiceSpecification selectedSpec)
		{
			services.Clear();
			if (selectedSpec != null)
			{
				services.AddRange(new DataHelperService(engine.GetUserConnection()).Read(ServiceExposers.ServiceSpecifcation.Equal(selectedSpec.ID)));
			}
			else
			{
				services.AddRange(new DataHelperService(engine.GetUserConnection()).Read());
			}

			DateTime? currentStart = domInstance.GetStartTime();
			DateTime? currentEnd = domInstance.GetEndTime();
			var serviceOptions = services.Where(x => domInstance.GetId().Id != x.ID && FallsWithTimeRange(x, currentStart, currentEnd))
				.Select(GetServiceDropDownLabel)
				.OrderBy(s => s)
				.ToList();
			serviceOptions.Insert(0, "-None-");
			view.ImplementationReferences.SetOptions(serviceOptions);
		}

		private void UpdateLabelPlaceholder(string definitionReference)
		{
			string tboxLabelPlaceHolder = $"{definitionReference}";
			while (getServiceItemLabels.Contains(tboxLabelPlaceHolder))
			{
				tboxLabelPlaceHolder += " (1)";
			}

			view.TboxLabel.PlaceHolder = tboxLabelPlaceHolder;
		}

		private bool ValidateLabel(string newValue)
		{
			if (String.IsNullOrWhiteSpace(newValue))
			{
				view.ErrorLabel.Text = "Placeholder will be used";
				return true;
			}

			if (getServiceItemLabels.Contains(newValue, StringComparer.InvariantCultureIgnoreCase))
			{
				view.ErrorLabel.Text = "Label already exists!";
				return false;
			}

			view.ErrorLabel.Text = String.Empty;
			return true;
		}
	}
}