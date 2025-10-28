namespace SLC_SM_GQIDS_Get_Service_Items
{
	// Used to process the Service Items
	using System;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.SDM;
	using SLDataGateway.API.Querying;

	// Required to mark the interface as a GQI data source
	[GQIMetaData(Name = "Get_ServiceItemsMultipleSections")]
	public class EventManagerGetMultipleSections : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		// defining input argument, will be converted to guid by OnArgumentsProcessed
		private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = true };
		private GQIDMS dms;

		// variable where input argument will be stored
		private Guid instanceDomId;

		public DMSMessage GenerateInformationEvent(string message)
		{
			var generateAlarmMessage = new GenerateAlarmMessage(GenerateAlarmMessage.AlarmSeverity.Information, message) { Status = GenerateAlarmMessage.AlarmStatus.Cleared };
			return dms.SendMessage(generateAlarmMessage);
		}

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("Label"),
				new GQIIntColumn("Service Item ID"),
				new GQIStringColumn("Service Item Type"),
				new GQIStringColumn("Definition Reference"),
				new GQIStringColumn("Service Item Script"),
				new GQIStringColumn("Implementation Reference"),
				new GQIStringColumn("Implementation Reference Name"),
				new GQIStringColumn("Implementation Reference Link"),
				new GQIBooleanColumn("Implementation Reference Has Value"),
				new GQIBooleanColumn("Implementation Reference Name Has Value"),
				new GQIBooleanColumn("Implementation Reference Link Has Value"),
				new GQIStringColumn("Implementation State"),
				new GQIStringColumn("Implementation Reference Custom Link"),
				new GQIBooleanColumn("Implementation Reference Custom Link Has Value"),
				new GQIStringColumn("Monitoring Service State"),
				new GQIStringColumn("Monitoring Service DMA ID/SID"),
			};
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[]
			{
				domIdArg,
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			////GenerateInformationEvent("GetNextPage started");
			return new GQIPage(GetMultiSection())
			{
				HasNextPage = false,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			// adds the input argument to private variable
			if (!Guid.TryParse(args.GetArgumentValue(domIdArg), out instanceDomId))
			{
				instanceDomId = Guid.Empty;
			}

			return new OnArgumentsProcessedOutputArgs();
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			dms = args.DMS;
			return default;
		}

		private GQIRow BuildRow(Models.ServiceItem item)
		{
			var implementationRef = GetImplementationRefName(item.ImplementationReference, item.DefinitionReference);
			return new GQIRow(
				Guid.NewGuid().ToString(),
				new[]
				{
					new GQICell { Value = item.Label },
					new GQICell { Value = (int)item.ID },
					new GQICell { Value = SlcServicemanagementIds.Enums.Serviceitemtypes.ToValue(item.Type) },
					new GQICell { Value = item.DefinitionReference ?? String.Empty },
					new GQICell { Value = item.Script ?? String.Empty },
					new GQICell { Value = item.ImplementationReference ?? String.Empty },
					new GQICell { Value = implementationRef.Name },
					new GQICell { Value = implementationRef.ServiceId },
					new GQICell { Value = !String.IsNullOrEmpty(item.ImplementationReference) },
					new GQICell { Value = !String.IsNullOrEmpty(implementationRef.Name) },
					new GQICell { Value = !String.IsNullOrEmpty(implementationRef.ServiceId) },
					new GQICell { Value = implementationRef.State },
					new GQICell { Value = implementationRef.CustomLink },
					new GQICell { Value = !String.IsNullOrEmpty(implementationRef.CustomLink) },
					new GQICell { Value = implementationRef.MonServiceState },
					new GQICell { Value = implementationRef.MonServiceDmaIdSid },
				});
		}

		private ImplementationItemInfo GetImplementationRefName(string referenceId, string definitionReference)
		{
			if (String.IsNullOrEmpty(referenceId) || !Guid.TryParse(referenceId, out Guid id))
			{
				return new ImplementationItemInfo();
			}

			var inst = new DomHelper(dms.SendMessages, SlcWorkflowIds.ModuleId).DomInstances.Read(DomInstanceExposers.Id.Equal(id)).FirstOrDefault();
			if (inst != null)
			{
				var jobInst = new JobsInstance(inst);
				return new ImplementationItemInfo
				{
					Name = inst.Name,
					State = jobInst.Status.ToString(),
				};
			}

			var serv = new DataHelperService(dms.GetConnection()).Read(ServiceExposers.Guid.Equal(id)).FirstOrDefault();
			if (serv != null)
			{
				return new ImplementationItemInfo
				{
					Name = serv.Name,
				};
			}

			var request = new ManagerStoreStartPagingRequest<ReservationInstance>(ReservationInstanceExposers.ID.Equal(id).ToQuery(), 10);
			var reservation = ((ManagerStorePagingResponse<ReservationInstance>)dms.SendMessage(request))?.Objects?.FirstOrDefault() as ServiceReservationInstance;
			if (reservation != null)
			{
				string customReference = null;
				if (!String.IsNullOrEmpty(definitionReference))
				{
					var liteElementInfoEvent = dms.SendMessage(new GetElementByNameMessage(definitionReference)) as ElementInfoEventMessage;
					customReference = liteElementInfoEvent?.GetPropertyValue("App Link");
				}

				var serviceInfoEventMessage = dms.SendMessage(new GetServiceStateMessage { DataMinerID = reservation.ServiceID.DataMinerID, ServiceID = reservation.ServiceID.SID }) as ServiceStateEventMessage;

				return new ImplementationItemInfo
				{
					Name = reservation.Name,
					ServiceId = reservation.ServiceID.ToString(),
					State = reservation.Status.ToString(),
					CustomLink = customReference ?? String.Empty,
					MonServiceState = serviceInfoEventMessage?.Level.ToString() ?? String.Empty,
					MonServiceDmaIdSid = serviceInfoEventMessage != null ? $"{serviceInfoEventMessage.DataMinerID}/{serviceInfoEventMessage.ServiceID}" : String.Empty,
				};
			}

			return new ImplementationItemInfo();
		}

		private GQIRow[] GetMultiSection()
		{
			////GenerateInformationEvent("Get Service Items Multisection started");
			if (instanceDomId == Guid.Empty)
			{
				// return th empty list
				return Array.Empty<GQIRow>();
			}

			var service = new DataHelperService(dms.GetConnection()).Read(ServiceExposers.Guid.Equal(instanceDomId)).FirstOrDefault();
			if (service != null)
			{
				return service.ServiceItems.Select(BuildRow).ToArray();
			}

			var spec = new DataHelperServiceSpecification(dms.GetConnection()).Read(ServiceSpecificationExposers.Guid.Equal(instanceDomId)).FirstOrDefault();
			if (spec != null)
			{
				return spec.ServiceItems.Select(BuildRow).ToArray();
			}

			return Array.Empty<GQIRow>();
		}
	}

	internal sealed class ImplementationItemInfo
	{
		public string Name { get; set; } = String.Empty;

		public string ServiceId { get; set; } = String.Empty;

		public string State { get; set; } = String.Empty;

		public string CustomLink { get; set; } = String.Empty;

		public string MonServiceState { get; set; } = String.Empty;

		public string MonServiceDmaIdSid { get; set; } = String.Empty;
	}
}