namespace SLC_SM_IAS_Service_Spec_Configuration.Model.DataRecords
{
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations;

	internal interface IParameterDataRecord
	{
		Models.ConfigurationParameter ConfigurationParam { get; set; }

		Models.ConfigurationParameterValue ConfigurationParamValue { get; set; }
	}
}