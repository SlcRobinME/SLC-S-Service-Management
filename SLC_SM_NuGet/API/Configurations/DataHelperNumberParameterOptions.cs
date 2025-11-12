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
	public class DataHelperNumberParameterOptions : DataHelper<Models.NumberParameterOptions>
	{
		/// <inheritdoc />
		public DataHelperNumberParameterOptions(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.NumberParameterOptions)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.NumberParameterOptions item)
		{
			var instance = new NumberParameterOptionsInstance(New(item.ID));
			instance.NumberParameterOptions.MinRange = item.MinRange;
			instance.NumberParameterOptions.MaxRange = item.MaxRange;
			instance.NumberParameterOptions.StepSize = item.StepSize;
			instance.NumberParameterOptions.Decimals = item.Decimals;
			instance.NumberParameterOptions.DefaultValue = item.DefaultValue;

			var dataHelperUnits = new DataHelperConfigurationUnit(_connection);
			var units = dataHelperUnits.Read();

			if (item.DefaultUnit != null)
			{
				var value = units.Find(u => u.ID == item.DefaultUnit.ID || u.Name == item.DefaultUnit.Name);
				if (value == null)
				{
					item.DefaultUnit.ID = dataHelperUnits.CreateOrUpdate(item.DefaultUnit);
				}
				else
				{
					item.DefaultUnit.ID = value.ID;
				}

				instance.NumberParameterOptions.DefaultUnit = item.DefaultUnit.ID;
				units = dataHelperUnits.Read();
			}

			if (item.Units != null)
			{
				foreach (var unit in item.Units)
				{
					var value = units.Find(u => u.ID == unit.ID || u.Name == unit.Name);
					if (value == null)
					{
						unit.ID = dataHelperUnits.CreateOrUpdate(unit);
					}
					else
					{
						unit.ID = value.ID;
					}

					instance.NumberParameterOptions.Units.Add(unit.ID);
				}
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(IEnumerable<Models.NumberParameterOptions> items)
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

			return TryDelete(lst.Where(i => i != null).Select(i => i.ID));
		}

		internal override List<Models.NumberParameterOptions> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new NumberParameterOptionsInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.NumberParameterOptions>();
			}

			List<Models.ConfigurationUnit> units = GetRequiredUnits(instances);

			return instances.Select(
					x => new Models.NumberParameterOptions
					{
						ID = x.ID.Id,
						Units = units.Where(u => x.NumberParameterOptions.Units?.Contains(u.ID) == true).ToList(),
						DefaultUnit = units.Find(u => u.ID == x.NumberParameterOptions.DefaultUnit),
						MinRange = x.NumberParameterOptions.MinRange,
						MaxRange = x.NumberParameterOptions.MaxRange,
						StepSize = x.NumberParameterOptions.StepSize,
						Decimals = x.NumberParameterOptions.Decimals,
						DefaultValue = x.NumberParameterOptions.DefaultValue,
					})
				.ToList();
		}

		private List<Models.ConfigurationUnit> GetRequiredUnits(List<NumberParameterOptionsInstance> instances)
		{
			FilterElement<Models.ConfigurationUnit> filter = new ORFilterElement<Models.ConfigurationUnit>();
			var guids = instances.Where(i => i?.NumberParameterOptions?.Units != null).SelectMany(i => i.NumberParameterOptions?.Units).Distinct().ToList();
			foreach (var guid in guids)
			{
				filter = filter.OR(ConfigurationUnitExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperConfigurationUnit(_connection).Read(filter) : new List<Models.ConfigurationUnit>();
		}
	}
}