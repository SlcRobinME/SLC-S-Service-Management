namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations;

	/// <summary>
	///     Provides access to data helpers for configuration parameters and options.
	/// </summary>
	public class DataHelpersConfigurations
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DataHelpersConfigurations" /> class.
		/// </summary>
		/// <param name="connection">The connection to use for data operations.</param>
		public DataHelpersConfigurations(IConnection connection)
		{
			ConfigurationParameterValues = new DataHelperConfigurationParameterValue(connection);
			ConfigurationParameters = new DataHelperConfigurationParameter(connection);
			NumberParameterOptions = new DataHelperNumberParameterOptions(connection);
			DiscreteParameterOptions = new DataHelperDiscreteParameterOptions(connection);
			TextParameterOptions = new DataHelperTextParameterOptions(connection);
			DiscreteValues = new DataHelperDiscreteValues(connection);
			ConfigurationUnits = new DataHelperConfigurationUnit(connection);
			ProfileDefinitions = new DataHelperProfileDefinition(connection);
			Profiles = new DataHelperProfile(connection);
			ReferencedConfigurationParameters = new DataHelperReferencedConfigurationParameter(connection);
			ReferencedProfileDefinitions = new DataHelperReferencedProfileDefinition(connection);
			ProtocolTests = new DataHelperProtocolTest(connection);
			Scripts = new DataHelperScript(connection);
		}

		/// <summary>
		///     Gets the data helper for configuration parameter values.
		/// </summary>
		public DataHelperConfigurationParameterValue ConfigurationParameterValues { get; }

		/// <summary>
		///     Gets the data helper for configuration parameters.
		/// </summary>
		public DataHelperConfigurationParameter ConfigurationParameters { get; }

		/// <summary>
		///     Gets the data helper for number parameter options.
		/// </summary>
		public DataHelperNumberParameterOptions NumberParameterOptions { get; }

		/// <summary>
		///     Gets the data helper for discrete parameter options.
		/// </summary>
		public DataHelperDiscreteParameterOptions DiscreteParameterOptions { get; }

		/// <summary>
		///     Gets the data helper for text parameter options.
		/// </summary>
		public DataHelperTextParameterOptions TextParameterOptions { get; }

		/// <summary>
		///     Gets the data helper for discrete values.
		/// </summary>
		public DataHelperDiscreteValues DiscreteValues { get; }

		/// <summary>
		///     Gets the data helper for configuration units.
		/// </summary>
		public DataHelperConfigurationUnit ConfigurationUnits { get; }

		/// <summary>
		/// 	Gets or sets the data helper for profile definitions.
		/// </summary>
		public DataHelperProfileDefinition ProfileDefinitions { get; set; }

		/// <summary>
		///     Gets or sets the data helper for profiles.
		/// </summary>
		public DataHelperProfile Profiles { get; set; }

		/// <summary>
		///     Gets or sets the data helper for referenced profile definitions.
		/// </summary>
		public DataHelperReferencedProfileDefinition ReferencedProfileDefinitions { get; set; }

		/// <summary>
		///     Gets or sets the data helper for referenced configuration parameters.
		/// </summary>
		public DataHelperReferencedConfigurationParameter ReferencedConfigurationParameters { get; set; }

		/// <summary>
		///     Gets or sets the data helper for protocol tests.
		/// </summary>
		public DataHelperProtocolTest ProtocolTests { get; set; }

		/// <summary>
		///     Gets or sets the data helper for scripts.
		/// </summary>
		public DataHelperScript Scripts { get; set; }
	}
}