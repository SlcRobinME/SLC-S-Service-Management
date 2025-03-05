namespace Library
{
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public sealed class Repo
	{
		public Repo(DomHelper domHelper)
		{
			AllCategories = GetAllCategories(domHelper);
			AllSpecs = GetAllSpecs(domHelper);
			AllServices = GetAllSpecServices(domHelper);
			AllProperties = GetAllProperties(domHelper);
			AllPropertyValues = GetAllPropertyValues(domHelper);
			AllServiceOrderItems = GetAllServiceOrderItems(domHelper);
			AllConfigurations = GetAllConfigurationItems(domHelper);
		}

		public ServiceOrderItemsInstance[] AllServiceOrderItems { get; }

		public ServiceCategoryInstance[] AllCategories { get; }

		public ServiceSpecificationsInstance[] AllSpecs { get; }

		public ServicesInstance[] AllServices { get; }

		public ServicePropertiesInstance[] AllProperties { get; }

		public ServicePropertyValuesInstance[] AllPropertyValues { get; }

		public ServiceConfigurationInstance[] AllConfigurations { get; }

		private ServiceConfigurationInstance[] GetAllConfigurationItems(DomHelper domHelper)
		{
			return domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcServicemanagementIds.Definitions.ServiceConfiguration.Id))
				.Select(x => new ServiceConfigurationInstance(x))
				.ToArray();
		}

		private ServiceOrderItemsInstance[] GetAllServiceOrderItems(DomHelper domHelper)
		{
			return domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcServicemanagementIds.Definitions.ServiceOrderItems.Id))
				.Select(x => new ServiceOrderItemsInstance(x))
				.ToArray();
		}

		private ServiceCategoryInstance[] GetAllCategories(DomHelper domHelper)
		{
			return domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcServicemanagementIds.Definitions.ServiceCategory.Id))
				.Select(x => new ServiceCategoryInstance(x))
				.ToArray();
		}

		private ServicePropertiesInstance[] GetAllProperties(DomHelper domHelper)
		{
			return domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcServicemanagementIds.Definitions.ServiceProperties.Id))
				.Select(x => new ServicePropertiesInstance(x))
				.ToArray();
		}

		private ServicePropertyValuesInstance[] GetAllPropertyValues(DomHelper domHelper)
		{
			return domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcServicemanagementIds.Definitions.ServicePropertyValues.Id))
				.Select(x => new ServicePropertyValuesInstance(x))
				.ToArray();
		}

		private ServiceSpecificationsInstance[] GetAllSpecs(DomHelper domHelper)
		{
			return domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcServicemanagementIds.Definitions.ServiceSpecifications.Id))
				.Select(x => new ServiceSpecificationsInstance(x))
				.ToArray();
		}

		private ServicesInstance[] GetAllSpecServices(DomHelper domHelper)
		{
			return domHelper.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcServicemanagementIds.Definitions.Services.Id))
				.Select(x => new ServicesInstance(x))
				.ToArray();
		}
	}
}