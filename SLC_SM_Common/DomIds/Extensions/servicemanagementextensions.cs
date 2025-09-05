namespace DomHelpers.SlcServicemanagement
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	public interface IServiceInstanceBase
	{
		IList<ServiceItemsSection> GetServiceItems();

		IList<ServiceItemRelationshipSection> GetServiceItemRelationships();

		DomInstanceId GetId();

		string GetName();

		DateTime? GetStartTime();

		DateTime? GetEndTime();

		void Save(DomHelper domHelper);
	}

	public partial class ServiceSpecificationsInstance : IServiceInstanceBase
	{
		public IList<ServiceItemsSection> GetServiceItems() => ServiceItemses;

		public IList<ServiceItemRelationshipSection> GetServiceItemRelationships() => ServiceItemRelationships;

		public DomInstanceId GetId() => ID;

		public string GetName() => ServiceSpecificationInfo.SpecificationName;

		public DateTime? GetStartTime() => null;

		public DateTime? GetEndTime() => null;
	}

	public partial class ServicesInstance : IServiceInstanceBase
	{
		public IList<ServiceItemsSection> GetServiceItems() => ServiceItemses;

		public IList<ServiceItemRelationshipSection> GetServiceItemRelationships() => ServiceItemRelationships;

		public DomInstanceId GetId() => ID;

		public string GetName() => ServiceInfo.ServiceName;

		public DateTime? GetStartTime() => ServiceInfo.ServiceStartTime;

		public DateTime? GetEndTime() => ServiceInfo.ServiceEndTime;
	}

	public class ServiceInstancesExtentions
	{
		public static IServiceInstanceBase GetTypedInstance(DomInstance instance)
		{
			if (instance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
				return new ServiceSpecificationsInstance(instance);

			if (instance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
				return new ServicesInstance(instance);

			throw new InvalidOperationException($"Unsupported Dom Definition: {instance.DomDefinitionId.Id}");
		}
	}
}
