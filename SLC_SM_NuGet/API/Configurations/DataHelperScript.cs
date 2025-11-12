namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcConfigurations;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	/// <inheritdoc />
	public class DataHelperScript : DataHelper<Models.Script>
	{
		/// <inheritdoc />
		public DataHelperScript(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.Script)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.Script item)
		{
			var instance = new ScriptInstance(New(item.ID));

			instance.ScriptInfo.ScriptName = item.ScriptName;
			instance.ScriptInfo.Protocol = item.Protocol;
			instance.ScriptInfo.ProtocolVersion = item.ProtocolVersion;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(IEnumerable<Models.Script> items)
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
		internal override List<Models.Script> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ScriptInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.Script>();
			}

			return instances.Select(
					x => new Models.Script
					{
						ID = x.ID.Id,
						ScriptName = x.ScriptInfo.ScriptName,
						Protocol = x.ScriptInfo.Protocol,
						ProtocolVersion = x.ScriptInfo.ProtocolVersion,
					})
				.ToList();
		}
	}
}