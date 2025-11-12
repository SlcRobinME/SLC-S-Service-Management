namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.SDM;

	/// <inheritdoc />
	public class DataHelperServiceSpecificationConfigurationValue : DataHelper<Models.ServiceSpecificationConfigurationValue>
	{
		/// <inheritdoc />
		public DataHelperServiceSpecificationConfigurationValue(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceSpecificationConfigurationValue)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceSpecificationConfigurationValue item)
		{
			var instance = new ServiceSpecificationConfigurationValueInstance(New(item.ID));
			instance.ServiceSpecificationConfigurationValue.MandatoryAtServiceOrderLevel = item.MandatoryAtServiceOrder;
			instance.ServiceSpecificationConfigurationValue.MandatoryAtServiceLevel = item.MandatoryAtService;
			instance.ServiceSpecificationConfigurationValue.ExposeAtServiceOrderLevel = item.ExposeAtServiceOrder;

			if (item.ConfigurationParameter.ID == Guid.Empty)
			{
				item.ConfigurationParameter.ID = Guid.NewGuid();
			}

			var dataHelperConfigurationParameters = new DataHelperConfigurationParameterValue(_connection);
			instance.ServiceSpecificationConfigurationValue.ConfigurationParameterValue = dataHelperConfigurationParameters.CreateOrUpdate(item.ConfigurationParameter);

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(IEnumerable<Models.ServiceSpecificationConfigurationValue> items)
		{
			if (items == null)
			{
				return true;
			}

			var lst = items.ToList();
			if (lst.Count < 1)
			{
				return true;
			}

			var helper = new DataHelperConfigurationParameterValue(_connection);
			bool b = helper.TryDelete(lst.Where(i => i?.ConfigurationParameter != null).Select(i => i.ConfigurationParameter));
			b &= TryDelete(lst.Where(i => i != null).Select(i => i.ID));

			return b;
		}

		/// <inheritdoc />
		internal override List<Models.ServiceSpecificationConfigurationValue> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ServiceSpecificationConfigurationValueInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ServiceSpecificationConfigurationValue>();
			}

			var configurationParameters = GetRequiredConfigurationParameterValues(instances);

			return instances.Select(
					x => new Models.ServiceSpecificationConfigurationValue
					{
						ID = x.ID.Id,
						MandatoryAtService = x.ServiceSpecificationConfigurationValue.MandatoryAtServiceLevel ?? false,
						MandatoryAtServiceOrder = x.ServiceSpecificationConfigurationValue.MandatoryAtServiceOrderLevel ?? false,
						ExposeAtServiceOrder = x.ServiceSpecificationConfigurationValue.ExposeAtServiceOrderLevel ?? false,
						ConfigurationParameter = configurationParameters.Find(p => p.ID == x.ServiceSpecificationConfigurationValue.ConfigurationParameterValue),
					})
				.ToList();
		}

		private List<Configurations.Models.ConfigurationParameterValue> GetRequiredConfigurationParameterValues(List<ServiceSpecificationConfigurationValueInstance> instances)
		{
			FilterElement<Configurations.Models.ConfigurationParameterValue> filter = new ORFilterElement<Configurations.Models.ConfigurationParameterValue>();
			var guids = instances.Where(i => i?.ServiceSpecificationConfigurationValue?.ConfigurationParameterValue != null).Select(i => i.ServiceSpecificationConfigurationValue.ConfigurationParameterValue.Value).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filter = filter.OR(ConfigurationParameterValueExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperConfigurationParameterValue(_connection).Read(filter) : new List<Configurations.Models.ConfigurationParameterValue>();
		}
	}
}