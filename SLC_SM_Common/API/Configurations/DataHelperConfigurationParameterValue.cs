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

			if (item.ConfigurationParameterId == Guid.Empty)
			{
				item.ConfigurationParameterId = Guid.NewGuid();
			}

			var dataHelperConfigurationParameter = new DataHelperConfigurationParameter(_connection);
			var configurationParameters = dataHelperConfigurationParameter.Read();
			if (!configurationParameters.Exists(p => p.ID == item.ConfigurationParameterId))
			{
				// Create a default configuration parameter?
				dataHelperConfigurationParameter.CreateOrUpdate(
					new Models.ConfigurationParameter
					{
						ID = item.ConfigurationParameterId,
						Name = item.Label,
						Type = item.Type,
						NumberOptions = item.NumberOptions,
						DiscreteOptions = item.DiscreteOptions,
						TextOptions = item.TextOptions,
					});
			}

			instance.ConfigurationParameterValue.ConfigurationParameterReference = item.ConfigurationParameterId;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ConfigurationParameterValue item)
		{
			bool b = true;
			if (item.DiscreteOptions != null)
			{
				b &= TryDelete(item.DiscreteOptions.ID);
			}

			if (item.NumberOptions != null)
			{
				b &= TryDelete(item.NumberOptions.ID);
			}

			if (item.TextOptions != null)
			{
				b &= TryDelete(item.TextOptions.ID);
			}

			return b && TryDelete(item.ID);
		}

		protected override List<Models.ConfigurationParameterValue> Read(IEnumerable<DomInstance> domInstances)
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
						Models.NumberParameterOptions numberOption;
						if (x.ConfigurationParameterValue.NumberValueOptions != null)
						{
							numberOption = numberOptions.Find(o => o.ID == x.ConfigurationParameterValue.NumberValueOptions);
						}
						else if (configParameter?.NumberOptions != null)
						{
							// Create duplicate of the pre-configured Configuration Parameter option
							numberOption = configParameter.NumberOptions;
							numberOption.ID = Guid.NewGuid();
						}
						else
						{
							numberOption = new Models.NumberParameterOptions
							{
								ID = Guid.NewGuid(),
							};
						}

						Models.DiscreteParameterOptions discreteOption;
						if (x.ConfigurationParameterValue.DiscreteValueOptions != null)
						{
							discreteOption = discreteOptions.Find(o => o.ID == x.ConfigurationParameterValue.DiscreteValueOptions);
						}
						else if (configParameter?.DiscreteOptions != null)
						{
							// Create duplicate of the pre-configured Configuration Parameter option
							discreteOption = configParameter.DiscreteOptions;
							discreteOption.ID = Guid.NewGuid();
						}
						else
						{
							discreteOption = new Models.DiscreteParameterOptions
							{
								ID = Guid.NewGuid(),
							};
						}

						Models.TextParameterOptions textOption;
						if (x.ConfigurationParameterValue.TextValueOptions != null)
						{
							textOption = textOptions.Find(o => o.ID == x.ConfigurationParameterValue.TextValueOptions);
						}
						else if (configParameter?.TextOptions != null)
						{
							// Create duplicate of the pre-configured Configuration Parameter option
							textOption = configParameter.TextOptions;
							textOption.ID = Guid.NewGuid();
						}
						else
						{
							textOption = new Models.TextParameterOptions
							{
								ID = Guid.NewGuid(),
							};
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