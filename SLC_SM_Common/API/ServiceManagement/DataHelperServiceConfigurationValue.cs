namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcConfigurations;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations;

	/// <inheritdoc />
	public class DataHelperServiceConfigurationValue : DataHelper<Models.ServiceConfigurationValue>
	{
		/// <inheritdoc />
		public DataHelperServiceConfigurationValue(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceConfigurationValue)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceConfigurationValue item)
		{
			var instance = new ServiceConfigurationValueInstance(New(item.ID));
			instance.ServiceConfigurationValue.MandatoryAtServiceLevel = item.Mandatory;

			if (item.ConfigurationParameter.ID == Guid.Empty)
			{
				item.ConfigurationParameter.ID = Guid.NewGuid();
			}

			var dataHelperConfigurationParameters = new DataHelperConfigurationParameterValue(_connection);
			dataHelperConfigurationParameters.CreateOrUpdate(item.ConfigurationParameter);
			instance.ServiceConfigurationValue.ConfigurationParameterValue = item.ConfigurationParameter.ID;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ServiceConfigurationValue item)
		{
			bool ok = TryDelete(item.ConfigurationParameter.ID);
			return ok && TryDelete(item.ID);
		}

		protected override List<Models.ServiceConfigurationValue> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ServiceConfigurationValueInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ServiceConfigurationValue>();
			}

			var configurationParameters = GetRequiredConfigurationParameterValues(instances);

			return instances.Select(
					x => new Models.ServiceConfigurationValue
					{
						ID = x.ID.Id,
						Mandatory = x.ServiceConfigurationValue.MandatoryAtServiceLevel ?? false,
						ConfigurationParameter = configurationParameters.Find(p => p.ID == x.ServiceConfigurationValue.ConfigurationParameterValue),
					})
				.ToList();
		}

		private List<Configurations.Models.ConfigurationParameterValue> GetRequiredConfigurationParameterValues(List<ServiceConfigurationValueInstance> instances)
		{
			FilterElement<Configurations.Models.ConfigurationParameterValue> filter = new ORFilterElement<Configurations.Models.ConfigurationParameterValue>();
			var guids = instances.Where(i => i?.ServiceConfigurationValue?.ConfigurationParameterValue != null).Select(i => i.ServiceConfigurationValue.ConfigurationParameterValue.Value).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filter = filter.OR(ConfigurationParameterValueExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperConfigurationParameterValue(_connection).Read(filter) : new List<Configurations.Models.ConfigurationParameterValue>();
		}
	}
}