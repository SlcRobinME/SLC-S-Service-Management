namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcConfigurations;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.SDM;

	/// <inheritdoc />
	public class DataHelperConfigurationParameterValue : DataHelper<Models.ConfigurationParameterValue>
	{
		/// <inheritdoc />
		public DataHelperConfigurationParameterValue(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.ConfigurationParameterValue)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ConfigurationParameterValue item)
		{
			var instance = new ConfigurationParameterValueInstance(New(item.ID));
			instance.ConfigurationParameterValue.Label = item.Label;
			instance.ConfigurationParameterValue.Type = item.Type;
			instance.ConfigurationParameterValue.StringValue = item.StringValue;
			instance.ConfigurationParameterValue.DoubleValue = item.DoubleValue;
			instance.ConfigurationParameterValue.LinkedInstanceReference = item.LinkedConfigurationReference;
			instance.ConfigurationParameterValue.ValueFixed = item.ValueFixed.ToString();
			instance.ConfigurationParameterValue.ConfigurationParameterReference = item.ConfigurationParameterId;

			if (item.NumberOptions != null)
			{
				var numberOptionsHelper = new DataHelperNumberParameterOptions(_connection);
				instance.ConfigurationParameterValue.NumberValueOptions = numberOptionsHelper.CreateOrUpdate(item.NumberOptions);
			}

			if (item.DiscreteOptions != null)
			{
				var discreteOptionsHelper = new DataHelperDiscreteParameterOptions(_connection);
				instance.ConfigurationParameterValue.DiscreteValueOptions = discreteOptionsHelper.CreateOrUpdate(item.DiscreteOptions);
			}

			if (item.TextOptions != null)
			{
				var textOptionsHelper = new DataHelperTextParameterOptions(_connection);
				instance.ConfigurationParameterValue.TextValueOptions = textOptionsHelper.CreateOrUpdate(item.TextOptions);
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(IEnumerable<Models.ConfigurationParameterValue> items)
		{
			if (items == null)
			{
				return true;
			}

			var lst = items.ToList();
			if (lst.Count < 1)
			{
				return true;
			}

			var numberHelper = new DataHelperNumberParameterOptions(_connection);
			bool b = numberHelper.TryDelete(lst.Where(i => i?.NumberOptions != null).Select(i => i.NumberOptions));

			var discreteHelper = new DataHelperDiscreteParameterOptions(_connection);
			b &= discreteHelper.TryDelete(lst.Where(i => i?.DiscreteOptions != null).Select(i => i.DiscreteOptions));

			var textHelper = new DataHelperTextParameterOptions(_connection);
			b &= textHelper.TryDelete(lst.Where(i => i?.TextOptions != null).Select(i => i.TextOptions));

			b &= TryDelete(lst.Where(i => i != null).Select(i => i.ID));

			return b;
		}

		internal override List<Models.ConfigurationParameterValue> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ConfigurationParameterValueInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ConfigurationParameterValue>();
			}

			List<Models.ConfigurationParameter> configurationParameters = GetRequiredConfigurationParameters(instances);
			List<Models.NumberParameterOptions> numberOptions = GetRequiredNumberOptions(instances);
			List<Models.DiscreteParameterOptions> discreteOptions = GetRequiredDiscreteOptions(instances);
			List<Models.TextParameterOptions> textOptions = GetRequiredTextOptions(instances);

			return instances.Select(
					x =>
					{
						var configParameter = configurationParameters.FirstOrDefault(p => p.ID == x.ConfigurationParameterValue.ConfigurationParameterReference);
						Models.NumberParameterOptions numberOption = null;
						if (x.ConfigurationParameterValue.NumberValueOptions != null)
						{
							numberOption = numberOptions.Find(o => o.ID == x.ConfigurationParameterValue.NumberValueOptions);
						}

						Models.DiscreteParameterOptions discreteOption = null;
						if (x.ConfigurationParameterValue.DiscreteValueOptions != null)
						{
							discreteOption = discreteOptions.Find(o => o.ID == x.ConfigurationParameterValue.DiscreteValueOptions);
						}

						Models.TextParameterOptions textOption = null;
						if (x.ConfigurationParameterValue.TextValueOptions != null)
						{
							textOption = textOptions.Find(o => o.ID == x.ConfigurationParameterValue.TextValueOptions);
						}

						return new Models.ConfigurationParameterValue
						{
							ID = x.ID.Id,
							Label = x.ConfigurationParameterValue.Label,
							ConfigurationParameterId = x.ConfigurationParameterValue.ConfigurationParameterReference ?? Guid.Empty,
							Type = x.ConfigurationParameterValue.Type ?? configParameter?.Type ?? SlcConfigurationsIds.Enums.Type.Text,
							StringValue = x.ConfigurationParameterValue.StringValue,
							DoubleValue = x.ConfigurationParameterValue.DoubleValue,
							NumberOptions = numberOption,
							DiscreteOptions = discreteOption,
							TextOptions = textOption,
							LinkedConfigurationReference = x.ConfigurationParameterValue.LinkedInstanceReference,
							ValueFixed = x.ConfigurationParameterValue.ValueFixed == "true",
						};
					})
				.ToList();
		}

		private List<Models.ConfigurationParameter> GetRequiredConfigurationParameters(List<ConfigurationParameterValueInstance> instances)
		{
			FilterElement<Models.ConfigurationParameter> filterConfigurationParameter = new ORFilterElement<Models.ConfigurationParameter>();
			var guids = instances.Where(i => i?.ConfigurationParameterValue.ConfigurationParameterReference != null).Select(i => i.ConfigurationParameterValue.ConfigurationParameterReference.Value).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filterConfigurationParameter = filterConfigurationParameter.OR(ConfigurationParameterExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperConfigurationParameter(_connection).Read(filterConfigurationParameter) : new List<Models.ConfigurationParameter>();
		}

		private List<Models.DiscreteParameterOptions> GetRequiredDiscreteOptions(List<ConfigurationParameterValueInstance> instances)
		{
			FilterElement<Models.DiscreteParameterOptions> filter = new ORFilterElement<Models.DiscreteParameterOptions>();
			var guids = instances.Where(i => i?.ConfigurationParameterValue?.DiscreteValueOptions != null).Select(i => i.ConfigurationParameterValue.DiscreteValueOptions.Value).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filter = filter.OR(DiscreteParameterOptionExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperDiscreteParameterOptions(_connection).Read(filter) : new List<Models.DiscreteParameterOptions>();
		}

		private List<Models.NumberParameterOptions> GetRequiredNumberOptions(List<ConfigurationParameterValueInstance> instances)
		{
			FilterElement<Models.NumberParameterOptions> filter = new ORFilterElement<Models.NumberParameterOptions>();
			var guids = instances.Where(i => i?.ConfigurationParameterValue?.NumberValueOptions != null).Select(i => i.ConfigurationParameterValue.NumberValueOptions.Value).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filter = filter.OR(NumberParameterOptionExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperNumberParameterOptions(_connection).Read(filter) : new List<Models.NumberParameterOptions>();
		}

		private List<Models.TextParameterOptions> GetRequiredTextOptions(List<ConfigurationParameterValueInstance> instances)
		{
			FilterElement<Models.TextParameterOptions> filter = new ORFilterElement<Models.TextParameterOptions>();
			var guids = instances.Where(i => i?.ConfigurationParameterValue?.TextValueOptions != null).Select(i => i.ConfigurationParameterValue.TextValueOptions.Value).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filter = filter.OR(TextParameterOptionExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperTextParameterOptions(_connection).Read(filter) : new List<Models.TextParameterOptions>();
		}
	}
}