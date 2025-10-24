namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcConfigurations;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <inheritdoc />
	public class DataHelperConfigurationParameter : DataHelper<Models.ConfigurationParameter>
	{
		/// <inheritdoc />
		public DataHelperConfigurationParameter(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.ConfigurationParameters)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ConfigurationParameter item)
		{
			var instance = new ConfigurationParametersInstance(New(item.ID));
			instance.ConfigurationParameterInfo.ParameterName = item.Name;
			instance.ConfigurationParameterInfo.Type = item.Type;

			if (item.NumberOptions != null)
			{
				var numberOptionsHelper = new DataHelperNumberParameterOptions(_connection);
				instance.ConfigurationParameterInfo.NumberOptions = numberOptionsHelper.CreateOrUpdate(item.NumberOptions);
			}

			if (item.DiscreteOptions != null)
			{
				var discreteOptionsHelper = new DataHelperDiscreteParameterOptions(_connection);
				instance.ConfigurationParameterInfo.DiscreteOptions = discreteOptionsHelper.CreateOrUpdate(item.DiscreteOptions);
			}

			if (item.TextOptions != null)
			{
				var textOptionsHelper = new DataHelperTextParameterOptions(_connection);
				instance.ConfigurationParameterInfo.TextOptions = textOptionsHelper.CreateOrUpdate(item.TextOptions);
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ConfigurationParameter item)
		{
			return TryDelete(item.ID);
		}

		/// <inheritdoc />
		protected override List<Models.ConfigurationParameter> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ConfigurationParametersInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ConfigurationParameter>();
			}

			List<Models.NumberParameterOptions> numberOptions = GetRequiredNumberOptions(instances);
			List<Models.DiscreteParameterOptions> discreteOptions = GetRequiredDiscreteOptions(instances);
			List<Models.TextParameterOptions> textOptions = GetRequiredTextOptions(instances);

			return instances.Select(
					x => new Models.ConfigurationParameter
					{
						ID = x.ID.Id,
						Name = x.ConfigurationParameterInfo.ParameterName,
						Type = x.ConfigurationParameterInfo.Type ?? SlcConfigurationsIds.Enums.Type.Text,
						NumberOptions = numberOptions.Find(o => o.ID == x.ConfigurationParameterInfo.NumberOptions),
						DiscreteOptions = discreteOptions.Find(o => o.ID == x.ConfigurationParameterInfo.DiscreteOptions),
						TextOptions = textOptions.Find(o => o.ID == x.ConfigurationParameterInfo.TextOptions),
					})
				.ToList();
		}

		private List<Models.DiscreteParameterOptions> GetRequiredDiscreteOptions(List<ConfigurationParametersInstance> instances)
		{
			FilterElement<Models.DiscreteParameterOptions> filter = new ORFilterElement<Models.DiscreteParameterOptions>();
			var guids = instances.Where(i => i?.ConfigurationParameterInfo?.DiscreteOptions != null).Select(i => i.ConfigurationParameterInfo.DiscreteOptions.Value).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filter = filter.OR(DiscreteParameterOptionExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperDiscreteParameterOptions(_connection).Read(filter) : new List<Models.DiscreteParameterOptions>();
		}

		private List<Models.NumberParameterOptions> GetRequiredNumberOptions(List<ConfigurationParametersInstance> instances)
		{
			FilterElement<Models.NumberParameterOptions> filter = new ORFilterElement<Models.NumberParameterOptions>();
			var guids = instances.Where(i => i?.ConfigurationParameterInfo?.NumberOptions != null).Select(i => i.ConfigurationParameterInfo.NumberOptions.Value).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filter = filter.OR(NumberParameterOptionExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperNumberParameterOptions(_connection).Read(filter) : new List<Models.NumberParameterOptions>();
		}

		private List<Models.TextParameterOptions> GetRequiredTextOptions(List<ConfigurationParametersInstance> instances)
		{
			FilterElement<Models.TextParameterOptions> filter = new ORFilterElement<Models.TextParameterOptions>();
			var guids = instances.Where(i => i?.ConfigurationParameterInfo?.TextOptions != null).Select(i => i.ConfigurationParameterInfo.TextOptions.Value).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filter = filter.OR(TextParameterOptionExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperTextParameterOptions(_connection).Read(filter) : new List<Models.TextParameterOptions>();
		}
	}
}