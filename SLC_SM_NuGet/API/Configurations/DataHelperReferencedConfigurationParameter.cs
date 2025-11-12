namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcConfigurations;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	/// <inheritdoc />
	public class DataHelperReferencedConfigurationParameter : DataHelper<Models.ReferencedConfigurationParameters>
	{
		/// <inheritdoc />
		public DataHelperReferencedConfigurationParameter(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.ReferencedConfigurationParameters)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ReferencedConfigurationParameters item)
		{
			var instance = new ReferencedConfigurationParametersInstance(New(item.ID));
			var info = instance.ReferencedConfigurationParametersInfo;

			info.ConfigurationParameter = item.ConfigurationParameter;
			info.Mandatory = item.Mandatory;
			info.AllowMultiple = item.AllowMultiple;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(IEnumerable<Models.ReferencedConfigurationParameters> items)
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

		/// <inheritdoc />
		internal override List<Models.ReferencedConfigurationParameters> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ReferencedConfigurationParametersInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ReferencedConfigurationParameters>();
			}

			return instances.Select(
					x => new Models.ReferencedConfigurationParameters
					{
						ID = x.ID.Id,
						ConfigurationParameter = x.ReferencedConfigurationParametersInfo.ConfigurationParameter.Value,
						Mandatory = x.ReferencedConfigurationParametersInfo.Mandatory.Value,
						AllowMultiple = x.ReferencedConfigurationParametersInfo.AllowMultiple.Value,
					})
				.ToList();
		}
	}
}