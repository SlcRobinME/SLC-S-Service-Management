namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcConfigurations;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	/// <inheritdoc />
	public class DataHelperTextParameterOptions : DataHelper<Models.TextParameterOptions>
	{
		/// <inheritdoc />
		public DataHelperTextParameterOptions(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.TextParameterOptions)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.TextParameterOptions item)
		{
			var instance = new TextParameterOptionsInstance(New(item.ID));
			instance.TextParameterOptions.Default = item.Default;
			instance.TextParameterOptions.Regex = item.Regex;
			instance.TextParameterOptions.UserMessage = item.UserMessage;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(IEnumerable<Models.TextParameterOptions> items)
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
		internal override List<Models.TextParameterOptions> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new TextParameterOptionsInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.TextParameterOptions>();
			}

			return instances.Select(
					x => new Models.TextParameterOptions
					{
						ID = x.ID.Id,
						Regex = x.TextParameterOptions.Regex,
						Default = x.TextParameterOptions.Default,
						UserMessage = x.TextParameterOptions.UserMessage,
					})
				.ToList();
		}
	}
}