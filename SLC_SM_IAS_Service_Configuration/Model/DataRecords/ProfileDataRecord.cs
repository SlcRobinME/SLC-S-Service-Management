namespace SLC_SM_IAS_Service_Configuration.Presenters
{
	using System.Collections.Generic;

	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	public partial class ServiceConfigurationPresenter
	{
		internal sealed class ProfileDataRecord
		{
			public State State { get; set; }

			public bool CanBeDeleted { get; set; }

			public Models.ServiceProfile ServiceProfileConfig { get; set; }

			public List<ProfileParameterDataRecord> ProfileParameterConfigs { get; set; }

			public Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.Profile Profile { get; set; }

			public Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ProfileDefinition ProfileDefinition { get; set; }

			internal static ProfileDataRecord BuildProfileRecord(Models.ServiceProfile currentConfig, List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter> configParams, Models.ServiceSpecification serviceSpecifivation, Skyline.DataMiner.Automation.IEngine engine)
			{
				engine.GenerateInformation("Building profile data record for profile: " + currentConfig.Profile.Name);
				var dataRecord = new ProfileDataRecord
				{
					State = State.Update,
					CanBeDeleted = serviceSpecifivation?.ConfigurationProfiles?.Exists(p => p?.ProfileDefinition?.ID == currentConfig?.ProfileDefinition?.ID) ?? true,
					ServiceProfileConfig = currentConfig,
					ProfileParameterConfigs = new List<ProfileParameterDataRecord>(),
					Profile = currentConfig.Profile,
					ProfileDefinition = currentConfig.ProfileDefinition,
				};

				foreach (var currentParameterConfig in currentConfig.Profile.ConfigurationParameterValues)
				{
					var configParam = configParams.Find(x => x.ID == currentParameterConfig?.ConfigurationParameterId);
					if (configParam == null)
					{
						continue;
					}

					ProfileParameterDataRecord dataParameterRecord = ProfileParameterDataRecord.BuildParameterDataRecord(currentParameterConfig, configParam);
					dataRecord.ProfileParameterConfigs.Add(dataParameterRecord);
				}

				return dataRecord;
			}
		}
	}
}