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
	public class DataHelperServiceOrderItemConfigurationValue : DataHelper<Models.ServiceOrderItemConfigurationValue>
	{
		/// <inheritdoc />
		public DataHelperServiceOrderItemConfigurationValue(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceOrderItemConfigurationValue)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceOrderItemConfigurationValue item)
		{
			var instance = new ServiceOrderItemConfigurationValueInstance(New(item.ID));
			instance.ServiceOrderItemConfigurationValue.MandatoryAtServiceOrderLevel = item.Mandatory;

			if (item.ConfigurationParameter.ID == Guid.Empty)
			{
				item.ConfigurationParameter.ID = Guid.NewGuid();
			}

			var dataHelperConfigurationParameters = new DataHelperConfigurationParameterValue(_connection);
			instance.ServiceOrderItemConfigurationValue.ConfigurationParameterValue = dataHelperConfigurationParameters.CreateOrUpdate(item.ConfigurationParameter);

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ServiceOrderItemConfigurationValue item)
		{
			bool ok = TryDelete(item.ConfigurationParameter.ID);
			return ok && TryDelete(item.ID);
		}

		/// <inheritdoc />
		protected override List<Models.ServiceOrderItemConfigurationValue> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ServiceOrderItemConfigurationValueInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ServiceOrderItemConfigurationValue>();
			}

			var configurationParameters = GetRequiredConfigurationParameterValues(instances);

			return instances.Select(
					x => new Models.ServiceOrderItemConfigurationValue
					{
						ID = x.ID.Id,
						Mandatory = x.ServiceOrderItemConfigurationValue.MandatoryAtServiceOrderLevel ?? false,
						ConfigurationParameter = configurationParameters.Find(p => p.ID == x.ServiceOrderItemConfigurationValue.ConfigurationParameterValue),
					})
				.ToList();
		}

		private List<Configurations.Models.ConfigurationParameterValue> GetRequiredConfigurationParameterValues(List<ServiceOrderItemConfigurationValueInstance> instances)
		{
			FilterElement<Configurations.Models.ConfigurationParameterValue> filter = new ORFilterElement<Configurations.Models.ConfigurationParameterValue>();
			var guids = instances.Where(i => i?.ServiceOrderItemConfigurationValue?.ConfigurationParameterValue != null).Select(i => i.ServiceOrderItemConfigurationValue.ConfigurationParameterValue.Value).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filter = filter.OR(ConfigurationParameterValueExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperConfigurationParameterValue(_connection).Read(filter) : new List<Configurations.Models.ConfigurationParameterValue>();
		}
	}
}