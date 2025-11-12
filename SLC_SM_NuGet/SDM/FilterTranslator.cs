namespace Skyline.DataMiner.ProjectApi.ServiceManagement.SDM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcConfigurations;
	using DomHelpers.SlcPeople_Organizations;
	using DomHelpers.SlcRelationships;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using Comparer = Skyline.DataMiner.Net.Messages.SLDataGateway.Comparer;

	/// <summary>
	/// Provides methods to translate filter elements for service management into filter elements for DOM instances.
	/// </summary>
	public static class FilterTranslator
	{
		private static readonly Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>> Handlers = new Dictionary<string, Func<Comparer, object, FilterElement<DomInstance>>>
		{
			/*SERVICE*/
			[ServiceExposers.Guid.fieldName] = HandleGuid,
			[ServiceExposers.ServiceName.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceName), comparer, (string)value),
			[ServiceExposers.Description.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.Description), comparer, (string)value),
			[ServiceExposers.ServiceStartTime.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceStartTime), comparer, (DateTime)value),
			[ServiceExposers.ServiceEndTime.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceEndTime), comparer, (DateTime)value),
			[ServiceExposers.Icon.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.Icon), comparer, (string)value),
			[ServiceExposers.ServiceSpecifcation.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceSpecifcation), comparer, (Guid)value),
			[ServiceExposers.RelatedOrganization.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.RelatedOrganization), comparer, (Guid)value),
			[ServiceExposers.ServiceCategory.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceCategory), comparer, (value as Models.ServiceCategory)?.ID ?? Guid.Empty),
			[ServiceExposers.ServiceConfigurationParameters.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceConfigurationParameters), comparer, (value as Models.ServiceConfigurationValue)?.ID ?? Guid.Empty),
			[ServiceExposers.ServiceID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceID), comparer, (string)value),
			[ServiceExposers.ServiceItemsExposers.Label.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.Label), comparer, (string)value),
			[ServiceExposers.ServiceItemsExposers.ServiceItemID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ServiceItemID), comparer, (long)value),
			[ServiceExposers.ServiceItemsExposers.ServiceItemType.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ServiceItemType), comparer, (int)value),
			[ServiceExposers.ServiceItemsExposers.DefinitionReference.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.DefinitionReference), comparer, (string)value),
			[ServiceExposers.ServiceItemsExposers.ServiceItemScript.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ServiceItemScript), comparer, (string)value),
			[ServiceExposers.ServiceItemsExposers.ImplementationReference.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ImplementationReference), comparer, (string)value),
			[ServiceExposers.ServiceItemRelationshipsExposers.Type.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.Type), comparer, (string)value),
			[ServiceExposers.ServiceItemRelationshipsExposers.ParentServiceItem.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ParentServiceItem), comparer, (string)value),
			[ServiceExposers.ServiceItemRelationshipsExposers.ChildServiceItem.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ChildServiceItem), comparer, (string)value),
			[ServiceExposers.ServiceItemRelationshipsExposers.ParentServiceItemInterfaceID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ParentServiceItemInterfaceID), comparer, (string)value),
			[ServiceExposers.ServiceItemRelationshipsExposers.ChildServiceItemInterfaceID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ChildServiceItemInterfaceID), comparer, (string)value),

			/*SERVICE SPECIFICATION*/
			[ServiceSpecificationExposers.Guid.fieldName] = HandleGuid,
			[ServiceSpecificationExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceSpecificationInfo.SpecificationName), comparer, (string)value),

			/*SERVICE SPECIFICATION CONFIGURATION VALUE*/
			[ServiceSpecificationConfigurationValueExposers.Guid.fieldName] = HandleGuid,
			[ServiceSpecificationConfigurationValueExposers.ConfigurationParameterID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceSpecificationConfigurationValue.ConfigurationParameterValue), comparer, (Guid)value),

			/*SERVICE ORDER*/
			[ServiceOrderExposers.Guid.fieldName] = HandleGuid,
			[ServiceOrderExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceOrderInfo.Name), comparer, (string)value),
			[ServiceOrderExposers.ExternalID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceOrderInfo.ExternalID), comparer, (string)value),
			[ServiceOrderExposers.ServiceOrderItemsExposers.ServiceOrderItem.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceOrderItems.ServiceOrderItem), comparer, (value as Models.ServiceOrderItem)?.ID ?? Guid.Empty),

			/*SERVICE ORDER ITEM*/
			[ServiceOrderItemExposers.Guid.fieldName] = HandleGuid,
			[ServiceOrderItemExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceOrderItemInfo.Name), comparer, (string)value),

			/*SERVICE ORDER ITEM CONFIGURATION VALUE*/
			[ServiceOrderItemConfigurationValueExposers.Guid.fieldName] = HandleGuid,
			[ServiceOrderItemConfigurationValueExposers.ConfigurationParameterID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceOrderItemConfigurationValue.ConfigurationParameterValue), comparer, (Guid)value),

			/*SERVICE CATEGORY*/
			[ServiceCategoryExposers.Guid.fieldName] = HandleGuid,

			/*SERVICE CONFIGURATION VALUE*/
			[ServiceConfigurationValueExposers.Guid.fieldName] = HandleGuid,
			[ServiceConfigurationValueExposers.ConfigurationParameterID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceConfigurationValue.ConfigurationParameterValue), comparer, (Guid)value),

			/*CONFIGURATION PARAM*/
			[ConfigurationParameterExposers.Guid.fieldName] = HandleGuid,
			[ConfigurationParameterExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterInfo.ParameterName), comparer, (string)value),

			/*CONFIGURATION VALUE*/
			[ConfigurationParameterValueExposers.Guid.fieldName] = HandleGuid,
			[ConfigurationParameterValueExposers.Label.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterValue.Label), comparer, (string)value),
			[ConfigurationParameterValueExposers.StringValue.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterValue.StringValue), comparer, (string)value),
			[ConfigurationParameterValueExposers.DoubleValue.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterValue.DoubleValue), comparer, (double?)value),
			[ConfigurationParameterValueExposers.Type.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterValue.Type), comparer, (SlcConfigurationsIds.Enums.Type)value),
			[ConfigurationParameterValueExposers.ConfigurationParameterID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationParameterValue.ConfigurationParameterReference), comparer, (Guid)value),

			/*CONFIGURATION UNIT*/
			[ConfigurationUnitExposers.Guid.fieldName] = HandleGuid,
			[ConfigurationUnitExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationUnitInfo.UnitName), comparer, (string)value),

			/*CONFIGURATION NUMBER OPTIONS*/
			[NumberParameterOptionExposers.Guid.fieldName] = HandleGuid,

			/*CONFIGURATION TEXT OPTIONS*/
			[TextParameterOptionExposers.Guid.fieldName] = HandleGuid,

			/*CONFIGURATION DISCRETE OPTIONS*/
			[DiscreteParameterOptionExposers.Guid.fieldName] = HandleGuid,

			/*CONFIGURATION DISCRETE VALUE OPTIONS*/
			[DiscreteValueExposers.Guid.fieldName] = HandleGuid,

			/*CONFIGURATION PROFILE DEFINITIONS*/
			[ProfileDefinitionExposers.ID.fieldName] = HandleGuid,

			/*CONFIGURATION PROFILES*/
			[ProfileExposers.ID.fieldName] = HandleGuid,

			/*CONFIGURATION REFERENCED OCNFIGURATION PARAMETERS*/
			[ReferencedConfigurationParametersExposers.ID.fieldName] = HandleGuid,

			/*CONFIGURATION REFERENCED PROFILE DEFINITIONS*/
			[ReferencedProfileDefinitionsExposers.ID.fieldName] = HandleGuid,

			/*CONFIGURATION PROTOCOL TESTS*/
			[ProtocolTestExposers.ID.fieldName] = HandleGuid,

			/*CONFIGURATION SCRIPTS*/
			[ScriptExposers.ID.fieldName] = HandleGuid,

			/*LINK*/
			[LinkExposers.Guid.fieldName] = HandleGuid,
			[LinkExposers.ChildID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcRelationshipsIds.Sections.LinkInfo.ChildObjectID), comparer, (string)value),
			[LinkExposers.ChildName.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcRelationshipsIds.Sections.LinkInfo.ChildObjectName), comparer, (string)value),
			[LinkExposers.ParentID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcRelationshipsIds.Sections.LinkInfo.ParentObjectID), comparer, (string)value),
			[LinkExposers.ParentName.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcRelationshipsIds.Sections.LinkInfo.ParentObjectName), comparer, (string)value),

			/*ORGANIZATION*/
			[OrganizationExposers.Guid.fieldName] = HandleGuid,
			[OrganizationExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.OrganizationInformation.OrganizationName), comparer, (string)value),
			[OrganizationExposers.Category.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.OrganizationInformation.Category), comparer, (Guid)value),

			[PeopleExposers.Guid.fieldName] = HandleGuid,
			[PeopleExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.PeopleInformation.FullName), comparer, (string)value),
			[PeopleExposers.Organization.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.Organization.Organization_57695f03), comparer, (Guid)value),
			[PeopleExposers.Experience.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.PeopleInformation.ExperienceLevel), comparer, (Guid)value),

			[CategoryExposers.Guid.fieldName] = HandleGuid,
			[CategoryExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.CategoryInformation.Category), comparer, (string)value),

			[ExperienceLevelExposers.Guid.fieldName] = HandleGuid,
			[ExperienceLevelExposers.Value.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcPeople_OrganizationsIds.Sections.ExperienceInformation.Experience), comparer, (string)value),
		};

		/// <summary>
		/// Translates a filter element of type <typeparamref name="T"/> into a filter element for <see cref="DomInstance"/>.
		/// </summary>
		/// <typeparam name="T">The type of the filter element to translate. Must be a class.</typeparam>
		/// <param name="filter">The filter element to translate.</param>
		/// <returns>A <see cref="FilterElement{DomInstance}"/> representing the translated filter.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is null.</exception>
		/// <exception cref="NotSupportedException">Thrown when the filter type is not supported.</exception>
		public static FilterElement<DomInstance> TranslateFullFilter<T>(FilterElement<T> filter) where T : class
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			FilterElement<DomInstance> translated;
			if (filter is ANDFilterElement<T> and)
			{
				translated = new ANDFilterElement<DomInstance>(and.subFilters.Select(TranslateFullFilter).ToArray());
			}
			else if (filter is ORFilterElement<T> or)
			{
				translated = new ORFilterElement<DomInstance>(or.subFilters.Select(TranslateFullFilter).ToArray());
			}
			else if (filter is NOTFilterElement<T> not)
			{
				translated = new NOTFilterElement<DomInstance>(TranslateFullFilter(not));
			}
			else if (filter is TRUEFilterElement<T>)
			{
				translated = new TRUEFilterElement<DomInstance>();
			}
			else if (filter is FALSEFilterElement<T>)
			{
				translated = new FALSEFilterElement<DomInstance>();
			}
			else if (filter is ManagedFilterIdentifier managedFilter)
			{
				translated = TranslateFilter(managedFilter);
			}
			else
			{
				throw new NotSupportedException($"Unsupported filter: {filter}");
			}

			return translated;
		}

		private static FilterElement<DomInstance> TranslateFilter(ManagedFilterIdentifier managedFilter)
		{
			if (managedFilter is null)
			{
				throw new ArgumentNullException(nameof(managedFilter));
			}

			var fieldName = managedFilter.getFieldName().fieldName;
			var comparer = managedFilter.getComparer();
			var value = managedFilter.getValue();
			var translated = CreateFilter(fieldName, comparer, value);
			return translated;
		}

		private static FilterElement<DomInstance> CreateFilter(string fieldName, Comparer comparer, object value)
		{
			if (!Handlers.ContainsKey(fieldName))
			{
				throw new NotSupportedException(fieldName);
			}

			return Handlers[fieldName].Invoke(comparer, value);
		}

		private static FilterElement<DomInstance> HandleGuid(Comparer comparer, object value)
		{
			return FilterElementFactory.Create(DomInstanceExposers.Id, comparer, (Guid)value);
		}
	}
}