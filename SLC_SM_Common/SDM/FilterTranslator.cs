namespace Skyline.DataMiner.SDM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;
	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using Comparer = Skyline.DataMiner.Net.Messages.SLDataGateway.Comparer;

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
				[ServiceExposers.ServiceItemsSection.Label.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.Label), comparer, (string)value),
				[ServiceExposers.ServiceItemsSection.ServiceItemID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ServiceItemID), comparer, (long)value),
				[ServiceExposers.ServiceItemsSection.ServiceItemType.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ServiceItemType), comparer, (int)value),
				[ServiceExposers.ServiceItemsSection.DefinitionReference.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.DefinitionReference), comparer, (string)value),
				[ServiceExposers.ServiceItemsSection.ServiceItemScript.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ServiceItemScript), comparer, (string)value),
				[ServiceExposers.ServiceItemsSection.ImplementationReference.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItems.ImplementationReference), comparer, (string)value),
				[ServiceExposers.ServiceItemRelationshipsSection.Type.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.Type), comparer, (string)value),
				[ServiceExposers.ServiceItemRelationshipsSection.ParentServiceItem.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ParentServiceItem), comparer, (string)value),
				[ServiceExposers.ServiceItemRelationshipsSection.ChildServiceItem.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ChildServiceItem), comparer, (string)value),
				[ServiceExposers.ServiceItemRelationshipsSection.ParentServiceItemInterfaceID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ParentServiceItemInterfaceID), comparer, (string)value),
				[ServiceExposers.ServiceItemRelationshipsSection.ChildServiceItemInterfaceID.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceItemRelationship.ChildServiceItemInterfaceID), comparer, (string)value),

				/*SERVICE SPECIFICATION*/
				[ServiceSpecificationExposers.Guid.fieldName] = HandleGuid,
				[ServiceSpecificationExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceSpecificationInfo.SpecificationName), comparer, (string)value),

				/*SERVICE SPECIFICATION CONFIGURATION VALUE*/
				[ServiceSpecificationConfigurationValueExposers.Guid.fieldName] = HandleGuid,

				/*SERVICE ORDER*/
				[ServiceOrderExposers.Guid.fieldName] = HandleGuid,
				[ServiceOrderExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceOrderInfo.Name), comparer, (string)value),

				/*SERVICE ORDER ITEM*/
				[ServiceOrderItemExposers.Guid.fieldName] = HandleGuid,
				[ServiceOrderItemExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcServicemanagementIds.Sections.ServiceOrderItemInfo.Name), comparer, (string)value),

				/*SERVICE ORDER ITEM*/
				[ServiceOrderItemConfigurationValueExposers.Guid.fieldName] = HandleGuid,

				/*SERVICE CATEGORY*/
				[ServiceCategroyExposers.Guid.fieldName] = HandleGuid,

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

				/*CONFIGURATION PARAM*/
				[ConfigurationUnitExposers.Guid.fieldName] = HandleGuid,
				[ConfigurationUnitExposers.Name.fieldName] = (comparer, value) => FilterElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(SlcConfigurationsIds.Sections.ConfigurationUnitInfo.UnitName), comparer, (string)value),

				/*CONFIGURATION NUMBER OPTIONS*/
				[NumberParameterOptionExposers.Guid.fieldName] = HandleGuid,

				/*CONFIGURATION TEXT OPTIONS*/
				[TextParameterOptionExposers.Guid.fieldName] = HandleGuid,

				/*CONFIGURATION DSICRETE OPTIONS*/
				[DiscreteParameterOptionExposers.Guid.fieldName] = HandleGuid,

				/*CONFIGURATION DSICRETE VALUE OPTIONS*/
				[DiscreteValueExposers.Guid.fieldName] = HandleGuid,
			};

		public static FilterElement<DomInstance> TranslateFullFilter<T>(FilterElement<T> filter) where T : class
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			FilterElement<DomInstance> translated;
			if (filter is ANDFilterElement<T> and)
			{
				translated = new ANDFilterElement<DomInstance>(and.subFilters.Select(TranslateFullFilter<T>).ToArray());
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