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
	public class DataHelperDiscreteParameterOptions : DataHelper<Models.DiscreteParameterOptions>
	{
		/// <inheritdoc />
		public DataHelperDiscreteParameterOptions(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.DiscreteParameterOptions)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.DiscreteParameterOptions item)
		{
			var instance = new DiscreteParameterOptionsInstance(New(item.ID));

			var dataHelperDiscreteValues = new DataHelperDiscreteValues(_connection);
			var discretes = dataHelperDiscreteValues.Read();

			if (item.Default != null)
			{
				var value = discretes.Find(d => d.ID == item.ID || d.Value == item.Default.Value);
				if (value == null)
				{
					item.Default.ID = dataHelperDiscreteValues.CreateOrUpdate(item.Default);
				}
				else
				{
					item.Default.ID = value.ID;
				}

				instance.DiscreteParameterOptions.DefaultDiscreteValue = item.Default.ID;
				discretes = dataHelperDiscreteValues.Read();
			}

			if (item.DiscreteValues != null)
			{
				foreach (var discreteValue in item.DiscreteValues)
				{
					var value = discretes.Find(d => d.ID == discreteValue.ID || d.Value == discreteValue.Value);
					if (value == null)
					{
						discreteValue.ID = dataHelperDiscreteValues.CreateOrUpdate(discreteValue);
					}
					else
					{
						discreteValue.ID = value.ID;
					}

					instance.DiscreteParameterOptions.DiscreteValues.Add(discreteValue.ID);
				}
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.DiscreteParameterOptions item)
		{
			return TryDelete(item.ID);
		}

		/// <inheritdoc />
		protected override List<Models.DiscreteParameterOptions> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new DiscreteParameterOptionsInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.DiscreteParameterOptions>();
			}

			List<Models.DiscreteValue> discretes = GetRequiredDiscreteValues(instances);

			return instances.Select(
					x => new Models.DiscreteParameterOptions
					{
						ID = x.ID.Id,
						Default = discretes.Find(d => d.ID == x.DiscreteParameterOptions.DefaultDiscreteValue),
						DiscreteValues = discretes.Where(d => x.DiscreteParameterOptions.DiscreteValues?.Contains(d.ID) == true).ToList(),
					})
				.ToList();
		}

		private List<Models.DiscreteValue> GetRequiredDiscreteValues(List<DiscreteParameterOptionsInstance> instances)
		{
			FilterElement<Models.DiscreteValue> filter = new ORFilterElement<Models.DiscreteValue>();
			List<Guid> guids = instances.Where(i => i?.DiscreteParameterOptions?.DiscreteValues != null).SelectMany(i => i.DiscreteParameterOptions.DiscreteValues).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filter = filter.OR(DiscreteValueExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperDiscreteValues(_connection).Read(filter) : new List<Models.DiscreteValue>();
		}
	}
}