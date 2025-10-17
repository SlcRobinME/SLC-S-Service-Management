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
	public class DataHelperServiceSpecification : DataHelper<Models.ServiceSpecification>
	{
		/// <inheritdoc />
		public DataHelperServiceSpecification(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceSpecifications)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceSpecification item)
		{
			var instance = new ServiceSpecificationsInstance(New(item.ID));
			instance.ServiceSpecificationInfo.SpecificationName = item.Name;
			instance.ServiceSpecificationInfo.Description = item.Description;
			instance.ServiceSpecificationInfo.Icon = item.Icon;

			if (item.ServiceItemsRelationships != null)
			{
				foreach (var relationship in item.ServiceItemsRelationships.Where(r => r != null))
				{
					instance.ServiceItemRelationships.Add(
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

			if (!instance.ServiceItemRelationships.Any())
			{
				instance.ServiceItemRelationships.Add(new ServiceItemRelationshipSection());
			}

			var dataHelperConfigurations = new DataHelperServiceSpecificationConfigurationValue(_connection);
			if (item.Configurations != null)
			{
				foreach (var config in item.Configurations.Where(c => c?.ConfigurationParameter != null))
				{
					instance.ServiceSpecificationInfo.ServiceSpecificationConfigurationParameters.Add(dataHelperConfigurations.CreateOrUpdate(config));
				}
			}

			if (item.ServiceItems != null)
			{
				foreach (var si in item.ServiceItems.Where(s => s != null))
				{
					instance.ServiceItemses.Add(
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

			if (!instance.ServiceItemses.Any())
			{
				instance.ServiceItemses.Add(new ServiceItemsSection());
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override List<Models.ServiceSpecification> Read()
		{
			var instances = _domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(_defId.Id));
			return Read(instances);
		}

		/// <inheritdoc />
		public List<Models.ServiceSpecification> Read(FilterElement<Models.ServiceSpecification> filter)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			var domFilter = FilterTranslator.TranslateFullFilter(filter);
			return Read(_domHelper.DomInstances.Read(domFilter));
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ServiceSpecification item)
		{
			bool b = true;

			if (item.Configurations != null)
			{
				var helperConfigs = new DataHelperServiceSpecificationConfigurationValue(_connection);
				foreach (var config in item.Configurations)
				{
					b &= helperConfigs.TryDelete(config.ID);
				}
			}

			return b && TryDelete(item.ID);
		}

		private List<Models.ServiceSpecification> Read(List<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ServiceSpecificationsInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ServiceSpecification>();
			}

			var dataHelperServiceConfigurations = new DataHelperServiceSpecificationConfigurationValue(_connection);
			var serviceConfigurations = dataHelperServiceConfigurations.Read();

			return instances.Select(
					x => new Models.ServiceSpecification
					{
						ID = x.ID.Id,
						Name = x.ServiceSpecificationInfo.SpecificationName,
						Description = x.ServiceSpecificationInfo.Description,
						Icon = x.ServiceSpecificationInfo.Icon,
						Configurations = serviceConfigurations.Where(p => x.ServiceSpecificationInfo.ServiceSpecificationConfigurationParameters.Contains(p.ID)).ToList(),
						ServiceItems = x.ServiceItemses.Select(
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
						ServiceItemsRelationships = x.ServiceItemRelationships.Select(
								r => new Models.ServiceItemRelationShip
								{
									ParentServiceItem = r.ParentServiceItem,
									ParentServiceItemInterfaceId = r.ParentServiceItemInterfaceID,
									ChildServiceItem = r.ChildServiceItem,
									ChildServiceItemInterfaceId = r.ChildServiceItemInterfaceID,
									Type = r.Type,
								})
							.ToList(),
					})
				.ToList();
		}
	}
}