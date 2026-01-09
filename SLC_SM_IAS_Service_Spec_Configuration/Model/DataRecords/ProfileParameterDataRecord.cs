namespace SLC_SM_IAS_Service_Spec_Configuration.Model.DataRecords
{
	public partial class ServiceConfigurationPresenter
	{
		internal sealed class ProfileParameterDataRecord : IParameterDataRecord
		{
			public State State { get; set; }

			public Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameterValue ConfigurationParamValue { get; set; }

			public Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter ConfigurationParam { get; set; }

			public Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ReferencedConfigurationParameters ReferencedConfiguration { get; set; }

			internal static ProfileParameterDataRecord BuildParameterDataRecord(Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameterValue currentConfig, Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter configParam, Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ReferencedConfigurationParameters refConfigParam)
			{
				var dataRecord = new ProfileParameterDataRecord
				{
					State = State.Update,
					ConfigurationParamValue = currentConfig,
					ConfigurationParam = configParam,
					ReferencedConfiguration = refConfigParam,
				};
				return dataRecord;
			}
		}
	}
}