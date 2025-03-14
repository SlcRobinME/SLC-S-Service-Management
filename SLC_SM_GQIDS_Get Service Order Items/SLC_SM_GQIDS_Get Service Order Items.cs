//---------------------------------
// SLC_SM_GQIDS_Get Service Order Items_1.cs
//---------------------------------
namespace SLC_SM_GQIDS_Get_Service_Order_Items_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Library;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	// Required to mark the interface as a GQI data source
	[GQIMetaData(Name = "Get_ServiceOrderItems")]
	public class EventManagerGetMultipleSections : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		// defining input argument, will be converted to guid by OnArgumentsProcessed
		private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = false };
		private GQIDMS _dms;
		private DomHelper _domHelper;

		// TO BE removed when we can easily fetch this using the DOM Code Generated code (backlog of Fiber squad, Arne Maes) 
		private static List<ServiceOrderItemStatus> serviceOrderItemStatuseList = new List<ServiceOrderItemStatus>() {
			new ServiceOrderItemStatus {Id = "06df5562-cd9b-4b0b-bd45-c58560a8b22a", Name = "New" },
			new ServiceOrderItemStatus {Id = "d917fc53-2638-4ab9-9ac6-651ec5312bac", Name = "Acknowledged" },
			new ServiceOrderItemStatus {Id = "331dc1c2-1950-4c00-a4ae-0aba674a30e6", Name = "In Progress" },
			new ServiceOrderItemStatus {Id = "260a7073-e54e-4482-a8a7-2b4f2e49c42e", Name = "Completed" },
			new ServiceOrderItemStatus {Id = "6a01a480-4c38-4db7-b545-72ba05742a7e", Name = "Rejected" },
			new ServiceOrderItemStatus {Id = "7f13d019-29de-43cb-a510-ab2b2a77e785", Name = "Failed" },
			new ServiceOrderItemStatus {Id = "f8a8d853-faaf-401c-9865-71e314614023", Name = "Partially Failed" },
			new ServiceOrderItemStatus {Id = "310ea9e9-f65c-4e11-8b1b-e2c34688ef44", Name = "Held" },
			new ServiceOrderItemStatus {Id = "23f9fa75-32b8-4e4a-bd65-06a7344d1902", Name = "Pending" },
			new ServiceOrderItemStatus {Id = "f7e93ddd-cddf-4755-a3e5-0f6ff885dcf5", Name = "Assess Cancellation" },
			new ServiceOrderItemStatus {Id = "15d08c01-fe63-4d5f-8544-e5b4d66439f5", Name = "Pending Cancellation" },
			new ServiceOrderItemStatus {Id = "61b80d48-d555-462e-baae-a52b17c85ddb", Name = "Cancelled" },
		};

		// variable where input argument will be stored
		private Guid _instanceDomId;

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("ID"),
				new GQIStringColumn("Name"),
				new GQIStringColumn("Start"),
				new GQIStringColumn("End"),
				new GQIStringColumn("Action"),
				new GQIStringColumn("Category"),
				new GQIStringColumn("Service Specification"),
				new GQIStringColumn("Service"),
				new GQIStringColumn("Property"),
				new GQIStringColumn("Configuration"),
				new GQIStringColumn("Status"),
				new GQIStringColumn("StatusId"),
			};
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[] { domIdArg };
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			return new GQIPage(GetMultiSection())
			{
				HasNextPage = false,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			// adds the input argument to private variable
			if (!Guid.TryParse(args.GetArgumentValue(domIdArg), out _instanceDomId))
			{
				_instanceDomId = Guid.Empty;
			}

			return new OnArgumentsProcessedOutputArgs();
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			_dms = args.DMS;

			return default;
		}

		private static GQIRow BuildRow(ServiceOrderItemsInstance item, Repo repo)
		{
			return new GQIRow(item.ID.Id.ToString(),
				new[] {
					new GQICell { Value = item.ID.Id.ToString() },
					new GQICell { Value = item.ServiceOrderItemInfo.Name },
					new GQICell { Value = item.ServiceOrderItemInfo.ServiceStartTime.HasValue ? item.ServiceOrderItemInfo.ServiceStartTime.ToString() : "No Start Time" },
					new GQICell { Value = item.ServiceOrderItemInfo.ServiceEndTime.HasValue ? item.ServiceOrderItemInfo.ServiceEndTime.ToString() : "No End Time" },
					new GQICell { Value = item.ServiceOrderItemInfo.Action },
					new GQICell
					{
						Value = item.ServiceOrderItemServiceInfo.ServiceCategory.HasValue
							? repo.AllCategories.FirstOrDefault(x => x.ID.Id == item.ServiceOrderItemServiceInfo.ServiceCategory)?.Name ?? String.Empty
							: String.Empty,
					},
					new GQICell
					{
						Value = item.ServiceOrderItemServiceInfo.ServiceSpecification.HasValue
							? repo.AllSpecs.FirstOrDefault(x => x.ID.Id == item.ServiceOrderItemServiceInfo.ServiceSpecification)?.Name ?? String.Empty
							: String.Empty,
					},
					new GQICell
					{
						Value = item.ServiceOrderItemServiceInfo.Service.HasValue
							? repo.AllServices.FirstOrDefault(x => x.ID.Id == item.ServiceOrderItemServiceInfo.Service)?.ServiceInfo.ServiceName ?? String.Empty
							: String.Empty,
					},
					new GQICell
					{
						Value = item.ServiceOrderItemServiceInfo.Properties.HasValue
							? repo.AllProperties.FirstOrDefault(x => x.ID.Id == item.ServiceOrderItemServiceInfo.Properties)?.ServicePropertyInfo.Name ?? String.Empty
							: String.Empty,
					},
					new GQICell
					{
						Value = item.ServiceOrderItemServiceInfo.Configuration.HasValue
							? repo.AllConfigurations.FirstOrDefault(x => x.ID.Id == item.ServiceOrderItemServiceInfo.Configuration)?.ID.Id.ToString()
							: String.Empty,
					},
					new GQICell
					{
						Value = serviceOrderItemStatuseList.FirstOrDefault<ServiceOrderItemStatus>(status => status.Id == item.StatusId)?.Name.ToString() ?? "No status mapping",
					},
					new GQICell
					{
						Value = serviceOrderItemStatuseList.FirstOrDefault<ServiceOrderItemStatus>(status => status.Id == item.StatusId)?.Id.ToString() ?? "No status mapping",
					},
				})
			{
				Metadata = new GenIfRowMetadata(new[] { new ObjectRefMetadata { Object = item.ID } }),
			};
		}

		private GQIRow[] GetMultiSection()
		{
			if (_instanceDomId == Guid.Empty)
			{
				// return th empty list
				return Array.Empty<GQIRow>();
			}

			// will initiate DomHelper
			LoadApplicationHandlersAndHelpers();

			// create filter to filter event instances with specific dom event ids
			var domInstance = _domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(_instanceDomId)).FirstOrDefault();
			if (domInstance == null)
			{
				return Array.Empty<GQIRow>();
			}

			var instance = new ServiceOrdersInstance(domInstance);

			var linkedIds = instance.ServiceOrderItems
				.Where(x => x.ServiceOrderItem.HasValue && x.ServiceOrderItem != Guid.Empty)
				.Select(x => x.ServiceOrderItem.Value)
				.ToArray();

			if (!linkedIds.Any())
			{
				return Array.Empty<GQIRow>();
			}

			FilterElement<DomInstance> filter = new ORFilterElement<DomInstance>();
			foreach (Guid linkedId in linkedIds)
			{
				filter = filter.OR(DomInstanceExposers.Id.Equal(linkedId));
			}

			var instances = _domHelper.DomInstances.Read(filter)
				.Select(
					x =>
					{
						try
						{
							return new ServiceOrderItemsInstance(x);
						}
						catch (Exception)
						{
							return null;
						}
					})
				.Where(x => x != null)
				.ToArray();

			var repo = new Repo(_domHelper);

			return instances.Select(item => BuildRow(item, repo)).ToArray();
		}

		private void LoadApplicationHandlersAndHelpers()
		{
			_domHelper = new DomHelper(_dms.SendMessages, SlcServicemanagementIds.ModuleId);
		}


		public class ServiceOrderItemStatus
		{
			public string Id { get; set; } = string.Empty;

			public string Name { get; set; } = string.Empty;
		}

	}
}
