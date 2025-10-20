namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcConfigurations;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	/// <inheritdoc />
	public class DataHelperConfigurationUnit : DataHelper<Models.ConfigurationUnit>
	{
		/// <inheritdoc />
		public DataHelperConfigurationUnit(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.ConfigurationUnit)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ConfigurationUnit item)
		{
			var instance = new ConfigurationUnitInstance(New(item.ID));
			instance.ConfigurationUnitInfo.UnitName = item.Name;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ConfigurationUnit item)
		{
			return TryDelete(item.ID);
		}

		/// <inheritdoc />
		protected override List<Models.ConfigurationUnit> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ConfigurationUnitInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ConfigurationUnit>();
			}

			return instances.Select(
					x => new Models.ConfigurationUnit
					{
						ID = x.ID.Id,
						Name = x.ConfigurationUnitInfo.UnitName,
					})
				.ToList();
		}
	}
}