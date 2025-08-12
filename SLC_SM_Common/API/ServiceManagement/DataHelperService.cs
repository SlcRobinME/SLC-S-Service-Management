namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;

	/// <inheritdoc />
	public class DataHelperService : DataHelper<Models.Service>
	{
		/// <inheritdoc />
		public DataHelperService(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.Services)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.Service item)
		{
			DomInstance domInstance = New(item.ID);
			var existingStatusId = _domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domInstance.ID)).FirstOrDefault()?.StatusId;
			if (existingStatusId != null)
			{
				domInstance.StatusId = existingStatusId;
			}

			var instance = new ServicesInstance(domInstance);
			instance.ServiceInfo.ServiceName = item.Name;
			instance.ServiceInfo.Description = item.Description;
			instance.ServiceInfo.ServiceID = item.ServiceID;
			instance.ServiceInfo.ServiceStartTime = item.StartTime;
			instance.ServiceInfo.ServiceEndTime = item.EndTime;
			instance.ServiceInfo.ServiceProperties = item.Properties?.ID;
			instance.ServiceInfo.ServiceCategory = item.Category?.ID;
			instance.ServiceInfo.ServiceSpecifcation = item.ServiceSpecificationId;
			instance.ServiceInfo.Icon = item.Icon;
			instance.ServiceInfo.RelatedOrganization = item.OrganizationId;

			if (item.Properties != null)
			{
				var dataHelperProperties = new DataHelperServicePropertyValues(_connection);
				dataHelperProperties.CreateOrUpdate(item.Properties);
			}

			if (item.Category != null)
			{
				var dataHelperServiceCategory = new DataHelperServiceCategory(_connection);
				dataHelperServiceCategory.CreateOrUpdate(item.Category);
			}

			instance.ServiceItemRelationship.Clear();
			if (item.ServiceItemsRelationships != null)
			{
				foreach (var relationship in item.ServiceItemsRelationships)
				{
					instance.ServiceItemRelationship.Add(
						new ServiceItemRelationshipSection
						{
							ParentServiceItem = relationship.ParentServiceItem,
							ParentServiceItemInterfaceID = relationship.ParentServiceItemInterfaceId,
							ChildServiceItem = relationship.ChildServiceItem,
							ChildServiceItemInterfaceID = relationship.ChildServiceItemInterfaceId,
							Type = relationship.Type,
						});
				}
			}

			if (!instance.ServiceItemRelationship.Any())
			{
				instance.ServiceItemRelationship.Add(new ServiceItemRelationshipSection());
			}

			var dataHelperConfigurations = new DataHelperServiceConfigurationValue(_connection);
			instance.ServiceInfo.ServiceConfigurationParameters.Clear();
			if (item.Configurations != null)
			{
				foreach (var config in item.Configurations.Where(c => c?.ConfigurationParameter != null))
				{
					instance.ServiceInfo.ServiceConfigurationParameters.Add(dataHelperConfigurations.CreateOrUpdate(config));
				}
			}

			instance.ServiceItems.Clear();
			if (item.ServiceItems != null)
			{
				foreach (var si in item.ServiceItems)
				{
					instance.ServiceItems.Add(
						new ServiceItemsSection
						{
							ServiceItemID = si.ID,
							Label = si.Label,
							ServiceItemScript = si.Script,
							ServiceItemType = si.Type,
							DefinitionReference = si.DefinitionReference,
							ImplementationReference = si.ImplementationReference,
						});
				}
			}

			if (!instance.ServiceItems.Any())
			{
				instance.ServiceItems.Add(new ServiceItemsSection());
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override List<Models.Service> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id));
			return Read(instances);
		}

		/// <inheritdoc />
		public List<Models.Service> Read(FilterElement<Models.Service> filter)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			var domFilter = FilterTranslator.TranslateFullFilter(filter);
			return Read(_domHelper.DomInstances.Read(domFilter));
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.Service item)
		{
			bool b = true;

			if (item.Properties != null)
			{
				b &= TryDelete(item.Properties.ID);
			}

			if (item.Configurations != null)
			{
				foreach (var config in item.Configurations)
				{
					b &= TryDelete(config.ID);
				}
			}

			return b && TryDelete(item.ID);
		}

		public string UniqueServiceId()
		{
			return UniqueServiceId(Read());
		}

		public string UniqueServiceId(List<Models.Service> services)
		{
			var serviceIds = services.Where(x => x?.ServiceID != null).Select(x => Int32.TryParse(x.ServiceID.Split('-').Last(), out int res) ? res : 0).ToArray();
			int max = serviceIds.Length > 0 ? serviceIds.Max() : 0;
			return $"SERVICE-{max + 1:00000}";
		}

		private static Models.Service FromInstance(
			ServicesInstance domInstance,
			List<Models.ServiceCategory> serviceCategories,
			List<Models.ServicePropertyValues> serviceProperties,
			List<Models.ServiceConfigurationValue> serviceConfigurations)
		{
			return new Models.Service
			{
				ID = domInstance.ID.Id,
				Name = domInstance.ServiceInfo.ServiceName,
				ServiceID = domInstance.ServiceInfo.ServiceID,
				Description = domInstance.ServiceInfo.Description,
				StartTime = domInstance.ServiceInfo.ServiceStartTime,
				EndTime = domInstance.ServiceInfo.ServiceEndTime,
				Icon = domInstance.ServiceInfo.Icon,
				Category = serviceCategories.Find(c => c.ID == domInstance.ServiceInfo.ServiceCategory),
				ServiceSpecificationId = domInstance.ServiceInfo.ServiceSpecifcation,
				OrganizationId = domInstance.ServiceInfo.RelatedOrganization,
				Properties = serviceProperties.Find(p => p.ID == domInstance.ServiceInfo.ServiceProperties) ?? new Models.ServicePropertyValues { Values = new List<Models.ServicePropertyValue>() },
				Configurations = serviceConfigurations.Where(p => domInstance.ServiceInfo.ServiceConfigurationParameters.Contains(p.ID)).ToList(),
				ServiceItems = domInstance.ServiceItems.Select(
						s => new Models.ServiceItem
						{
							ID = s.ServiceItemID ?? 1,
							Label = s.Label ?? String.Empty,
							Type = s.ServiceItemType ?? SlcServicemanagementIds.Enums.ServiceitemtypesEnum.SRMBooking,
							Script = s.ServiceItemScript ?? String.Empty,
							DefinitionReference = s.DefinitionReference ?? String.Empty,
							ImplementationReference = s.ImplementationReference ?? String.Empty,
						})
					.Where(s => !String.IsNullOrEmpty(s.Label))
					.ToList(),
				ServiceItemsRelationships = domInstance.ServiceItemRelationship.Select(
						r => new Models.ServiceItemRelationShip
						{
							ParentServiceItem = r.ParentServiceItem,
							ParentServiceItemInterfaceId = r.ParentServiceItemInterfaceID,
							ChildServiceItem = r.ChildServiceItem,
							ChildServiceItemInterfaceId = r.ChildServiceItemInterfaceID,
							Type = r.Type,
						})
					.ToList(),
			};
		}

		private List<Models.Service> Read(List<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ServicesInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.Service>();
			}

			var dataHelperServicePropertyValues = new DataHelperServicePropertyValues(_connection);
			var serviceProperties = dataHelperServicePropertyValues.Read();
			var dataHelperServiceConfigurations = new DataHelperServiceConfigurationValue(_connection);
			var serviceConfigurations = dataHelperServiceConfigurations.Read();
			var dataHelperServiceCategory = new DataHelperServiceCategory(_connection);
			var serviceCategories = dataHelperServiceCategory.Read();

			return instances.Select(
					x => FromInstance(x, serviceCategories, serviceProperties, serviceConfigurations))
				.ToList();
		}
	}
}