namespace SLC_SM_IAS_Service_Configuration.Presenters
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	public partial class ServiceConfigurationPresenter
	{
		internal sealed class ConfigurationDataRecord
		{
			public State State { get; set; }

			public Models.Configurations ServiceConfig { get; set; }

			public List<StandaloneParameterDataRecord> ServiceParameterConfigs { get; set; }

			public List<ProfileDataRecord> ServiceProfileConfigs { get; set; }

			internal static ConfigurationDataRecord BuildConfigurationDataRecordRecord(
				Models.Configurations currentConfig,
				List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter> configParams,
				Models.ServiceSpecification serviceSpecifivation,
				IEngine engine)
			{
				var dataRecord = new ConfigurationDataRecord
				{
					State = State.Update,
					ServiceConfig = currentConfig,
					ServiceParameterConfigs = new List<StandaloneParameterDataRecord>(),
					ServiceProfileConfigs = currentConfig.Profiles.Select(profile => ProfileDataRecord.BuildProfileRecord(profile, configParams, serviceSpecifivation, engine)).ToList(),
				};

				foreach (var currentParameterConfig in currentConfig.Parameters)
				{
					var configParam = configParams.Find(x => x.ID == currentParameterConfig?.ConfigurationParameter?.ConfigurationParameterId);
					if (configParam == null)
					{
						continue;
					}

					StandaloneParameterDataRecord dataParameterRecord = StandaloneParameterDataRecord.BuildParameterDataRecord(currentParameterConfig, configParam);
					dataRecord.ServiceParameterConfigs.Add(dataParameterRecord);
				}

				return dataRecord;
			}
		}
	}
}