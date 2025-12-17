namespace SLC_SM_IAS_Service_Configuration.Presenters
{
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	public partial class ServiceConfigurationPresenter
	{
		internal sealed class StandaloneParameterDataRecord : IParameterDataRecord
		{
			public State State { get; set; }

			public Models.ServiceConfigurationValue ServiceParameterConfig { get; set; }

			public Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameterValue ConfigurationParamValue { get; set; }

			public Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter ConfigurationParam { get; set; }

			internal static StandaloneParameterDataRecord BuildParameterDataRecord(Models.ServiceConfigurationValue currentConfig, Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter configParam)
			{
				var dataRecord = new StandaloneParameterDataRecord
				{
					State = State.Update,
					ServiceParameterConfig = currentConfig,
					ConfigurationParamValue = currentConfig.ConfigurationParameter,
					ConfigurationParam = configParam,
				};
				return dataRecord;
			}
		}
	}
}