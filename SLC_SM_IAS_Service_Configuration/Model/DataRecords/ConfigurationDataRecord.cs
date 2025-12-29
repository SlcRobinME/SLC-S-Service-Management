namespace SLC_SM_IAS_Service_Configuration.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	public partial class ServiceConfigurationPresenter
	{
		internal sealed class ConfigurationDataRecord
		{
			public State State { get; set; }

			public Models.ServiceConfigurationVersion ServiceConfigurationVersion { get; set; }

			public List<StandaloneParameterDataRecord> ServiceParameterConfigs { get; set; }

			public List<ProfileDataRecord> ServiceProfileConfigs { get; set; }

			internal static ConfigurationDataRecord BuildConfigurationDataRecordRecord(
				Models.ServiceConfigurationVersion currentConfig,
				List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter> configParams,
				List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ReferencedConfigurationParameters> referencedConfigParams,
				Models.ServiceSpecification serviceSpecifivation,
				State state = State.Update)
			{
				var dataRecord = new ConfigurationDataRecord
				{
					State = state,
					ServiceConfigurationVersion = currentConfig,
					ServiceParameterConfigs = new List<StandaloneParameterDataRecord>(),
					ServiceProfileConfigs = currentConfig.Profiles.Select(profile => ProfileDataRecord.BuildProfileRecord(profile, configParams, referencedConfigParams, state)).ToList(),
				};

				foreach (var currentParameterConfig in currentConfig.Parameters)
				{
					var configParam = configParams.Find(x => x.ID == currentParameterConfig?.ConfigurationParameter?.ConfigurationParameterId);
					if (configParam == null)
					{
						continue;
					}

					StandaloneParameterDataRecord dataParameterRecord = StandaloneParameterDataRecord.BuildParameterDataRecord(currentParameterConfig, configParam, state);
					dataRecord.ServiceParameterConfigs.Add(dataParameterRecord);
				}

				return dataRecord;
			}
		}
	}
}