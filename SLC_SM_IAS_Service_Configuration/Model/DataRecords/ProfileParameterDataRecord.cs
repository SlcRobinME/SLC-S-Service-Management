namespace SLC_SM_IAS_Service_Configuration.Presenters
{
	public partial class ServiceConfigurationPresenter
	{
		internal sealed class ProfileParameterDataRecord : IParameterDataRecord
		{
			public State State { get; set; }

			public bool Mandatory { get; set; }

			public Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameterValue ConfigurationParamValue { get; set; }

			public Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter ConfigurationParam { get; set; }

			internal static ProfileParameterDataRecord BuildParameterDataRecord(
				Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameterValue currentConfig,
				Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter configParam,
				Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ReferencedConfigurationParameters referencedParam,
				State state = State.Update)
			{
				var dataRecord = new ProfileParameterDataRecord
				{
					State = state,
					ConfigurationParamValue = currentConfig,
					ConfigurationParam = configParam,
				};

				if (referencedParam != null)
				{
					dataRecord.Mandatory = referencedParam.Mandatory;
				}
				else
				{
					dataRecord.Mandatory = false;
				}

				return dataRecord;
			}
		}
	}
}