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
	public class DataHelperProtocolTest : DataHelper<Models.ProtocolTest>
	{
		/// <inheritdoc />
		public DataHelperProtocolTest(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.ProtocolTest)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ProtocolTest item)
		{
			var instance = new ProtocolTestInstance(New(item.ID));
			var info = instance.ProtocolTestInfo;

			info.ProtocolName = item.ProtocolName;
			info.ProtocolVersion = item.ProtocolVersion;
			info.State = item.State;
			info.ScriptVersion = item.ScriptVersion;
			info.Date = item.Date;

			if (item.Script != null)
			{
				var dataHelperScript = new DataHelperScript(_connection);
				var existingScript = dataHelperScript.Read(ScriptExposers.ID.Equal(item.Script.ID)).SingleOrDefault();

				info.Script = existingScript?.ID ?? dataHelperScript.CreateOrUpdate(item.Script);
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(IEnumerable<Models.ProtocolTest> items)
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
		internal override List<Models.ProtocolTest> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ProtocolTestInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ProtocolTest>();
			}

			List<Models.Script> scripts = GetRequiredScripts(instances);

			return instances.Select(
					x => new Models.ProtocolTest
					{
						ID = x.ID.Id,
						ProtocolName = x.ProtocolTestInfo.ProtocolName,
						ProtocolVersion = x.ProtocolTestInfo.ProtocolVersion,
						State = x.ProtocolTestInfo.State.Value,
						Script = scripts.Find(s => s.ID == x.ProtocolTestInfo.Script),
						Date = x.ProtocolTestInfo.Date.Value,
						ScriptVersion = x.ProtocolTestInfo.ScriptVersion,
					})
				.ToList();
		}

		private List<Models.Script> GetRequiredScripts(List<ProtocolTestInstance> instances)
		{
			FilterElement<Models.Script> filter = new ORFilterElement<Models.Script>();
			List<Guid> guids = instances
				.Where(i => i?.ProtocolTestInfo.Script != null)
				.Select(i => i.ProtocolTestInfo.Script.Value)
				.Distinct()
				.ToList();

			foreach (Guid guid in guids)
			{
				filter = filter.OR(ScriptExposers.ID.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperScript(_connection).Read(filter) : new List<Models.Script>();
		}
	}
}