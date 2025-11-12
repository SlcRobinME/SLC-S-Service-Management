namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcConfigurations;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.SDM;

	/// <inheritdoc />
	public class DataHelperProfile : DataHelper<Models.Profile>
	{
		/// <inheritdoc />
		public DataHelperProfile(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.Profile)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.Profile item)
		{
			var instance = new ProfileInstance(New(item.ID));
			var info = instance.ProfileInfo;

			info.Name = item.Name;
			info.ProfileDefinition = item.ProfileDefinitionReference;

			foreach (var profileId in item.Profiles)
			{
				info.Profiles.Add(profileId);
			}

			if (item.ConfigurationParameterValues != null && item.ConfigurationParameterValues.Count > 0)
			{
				CreateOrUpdateConfigurationParameterValues(item);
			}

			if (item.TestedProtocols != null && item.TestedProtocols.Count > 0)
			{
				CreateOrUpdateProtocolTests(item);
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(IEnumerable<Models.Profile> items)
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

			var dataHelperConfigurationParameterValue = new DataHelperConfigurationParameterValue(_connection);
			bool b = dataHelperConfigurationParameterValue.TryDelete(lst.Where(i => i?.ConfigurationParameterValues != null).SelectMany(i => i.ConfigurationParameterValues));

			var dataHelperProtocolTest = new DataHelperProtocolTest(_connection);
			b &= dataHelperProtocolTest.TryDelete(lst.Where(i => i?.TestedProtocols != null).SelectMany(i => i.TestedProtocols));

			b &= TryDelete(lst.Where(i => i != null).Select(i => i.ID));

			return b;
		}

		/// <inheritdoc />
		internal override List<Models.Profile> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ProfileInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.Profile>();
			}

			List<Models.ConfigurationParameterValue> configurationParameterValues = GetRequiredConfigurationParameterValues(instances);
			List<Models.ProtocolTest> protocolTests = GetRequiredProtocolTests(instances);

			return instances.Select(
					x => new Models.Profile
					{
						ID = x.ID.Id,
						Name = x.ProfileInfo.Name,
						Profiles = x.ProfileInfo.Profiles.ToList(),
						ProfileDefinitionReference = x.ProfileInfo.ProfileDefinition.Value,
						ConfigurationParameterValues = configurationParameterValues.Where(v => x.ProfileInfo.ConfigurationParameterValue.Contains(v.ID)).ToList(),
						TestedProtocols = protocolTests.Where(t => x.ProfileInfo.TestedProtocols.Contains(t.ID)).ToList(),
					})
				.ToList();
		}

		private void CreateOrUpdateProtocolTests(Models.Profile item)
		{
			var dataHelperProtocolTests = new DataHelperProtocolTest(_connection);
			var filter = new ORFilterElement<Models.ProtocolTest>();

			foreach (var id in item.TestedProtocols.Select(t => t.ID))
			{
				filter.OR(ProtocolTestExposers.ID.Equal(id));
			}

			var existingProtocolTests = dataHelperProtocolTests.Read(filter);

			foreach (var protocolTest in item.TestedProtocols)
			{
				var existingProtocolTest = existingProtocolTests.Find(t => t.ID == protocolTest.ID);
				if (existingProtocolTest == null)
				{
					protocolTest.ID = dataHelperProtocolTests.CreateOrUpdate(protocolTest);
				}
				else
				{
					protocolTest.ID = existingProtocolTest.ID;
				}
			}
		}

		private void CreateOrUpdateConfigurationParameterValues(Models.Profile item)
		{
			var helper = new DataHelperConfigurationParameterValue(_connection);

			var filter = new ORFilterElement<Models.ConfigurationParameterValue>();
			foreach (var id in item.ConfigurationParameterValues.Select(v => v.ID))
			{
				filter.OR(ConfigurationParameterValueExposers.Guid.Equal(id));
			}

			var existingConfigurationParameterValues = helper.Read(filter);

			foreach (var configurationParameterValue in item.ConfigurationParameterValues)
			{
				var existingConfigurationParameterValue = existingConfigurationParameterValues.Find(v => v.ID == configurationParameterValue.ID);
				if (existingConfigurationParameterValue == null)
				{
					configurationParameterValue.ID = helper.CreateOrUpdate(configurationParameterValue);
				}
				else
				{
					configurationParameterValue.ID = existingConfigurationParameterValue.ID;
				}
			}
		}

		private List<Models.ConfigurationParameterValue> GetRequiredConfigurationParameterValues(List<ProfileInstance> instances)
		{
			FilterElement<Models.ConfigurationParameterValue> filter = new ORFilterElement<Models.ConfigurationParameterValue>();
			var guids = instances
				.Where(i => i?.ProfileInfo.ConfigurationParameterValue != null)
				.SelectMany(i => i.ProfileInfo.ConfigurationParameterValue)
				.Distinct()
				.ToList();

			foreach (Guid guid in guids)
			{
				filter = filter.OR(ConfigurationParameterValueExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperConfigurationParameterValue(_connection).Read(filter) : new List<Models.ConfigurationParameterValue>();
		}

		private List<Models.ProtocolTest> GetRequiredProtocolTests(List<ProfileInstance> instances)
		{
			FilterElement<Models.ProtocolTest> filter = new ORFilterElement<Models.ProtocolTest>();
			var guids = instances
				.Where(i => i?.ProfileInfo.TestedProtocols != null)
				.SelectMany(i => i.ProfileInfo.TestedProtocols)
				.Distinct()
				.ToList();

			foreach (Guid guid in guids)
			{
				filter = filter.OR(ProtocolTestExposers.ID.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperProtocolTest(_connection).Read(filter) : new List<Models.ProtocolTest>();
		}
	}
}