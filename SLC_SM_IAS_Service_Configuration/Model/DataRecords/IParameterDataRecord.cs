namespace SLC_SM_IAS_Service_Configuration.Presenters
{
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations;

	internal interface IParameterDataRecord
	{
		Models.ConfigurationParameter ConfigurationParam { get; set; }

		Models.ConfigurationParameterValue ConfigurationParamValue { get; set; }

		ServiceConfigurationPresenter.State State { get; set; }
	}
}