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
	public class DataHelperProfileDefinition : DataHelper<Models.ProfileDefinition>
	{
		/// <inheritdoc />
		public DataHelperProfileDefinition(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.ProfileDefinition)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ProfileDefinition item)
		{
			var instance = new ProfileDefinitionInstance(New(item.ID));
			var info = instance.ProfileDefinitionInfo;

			info.Name = item.Name;

			foreach (var configurationParameter in item.ConfigurationParameters)
			{
				info.ConfigurationParameters.Add(configurationParameter);
			}

			if (item.Scripts != null && item.Scripts.Count > 0)
			{
				CreateOrUpdateScripts(item);
			}

			if (item.ProfileDefinitions != null && item.ProfileDefinitions.Count > 0)
			{
				CreateOrUpdateReferencedProfileDefinitions(item);
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(IEnumerable<Models.ProfileDefinition> items)
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

			var dataHelperScript = new DataHelperScript(_connection);
			bool b = dataHelperScript.TryDelete(lst.Where(i => i?.Scripts != null).SelectMany(i => i.Scripts));

			var dataHelperReferencedProfileDefinitions = new DataHelperReferencedProfileDefinition(_connection);
			b &= dataHelperReferencedProfileDefinitions.TryDelete(lst.Where(i => i?.ProfileDefinitions != null).SelectMany(i => i.ProfileDefinitions));

			b &= TryDelete(lst.Where(i => i != null).Select(i => i.ID));

			return b;
		}

		/// <inheritdoc />
		internal override List<Models.ProfileDefinition> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ProfileDefinitionInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ProfileDefinition>();
			}

			List<Models.Script> scripts = GetRequiredScripts(instances);
			List<Models.ReferencedProfileDefinitions> referencedProfileDefinitions = GetRequiredReferencedProfileDefinitions(instances);

			return instances.Select(
					x => new Models.ProfileDefinition
					{
						ID = x.ID.Id,
						Name = x.Name,
						ConfigurationParameters = x.ProfileDefinitionInfo.ConfigurationParameters.ToList(),
						Scripts = scripts.Where(s => x.ProfileDefinitionInfo.Scripts.Contains(s.ID)).ToList(),
						ProfileDefinitions = referencedProfileDefinitions.Where(r => x.ProfileDefinitionInfo.ProfileDefinitions.Contains(r.ID)).ToList(),
					})
				.ToList();
		}

		private List<Models.ReferencedProfileDefinitions> GetRequiredReferencedProfileDefinitions(List<ProfileDefinitionInstance> instances)
		{
			var filter = new ORFilterElement<Models.ReferencedProfileDefinitions>();
			var ids = instances
				.Where(i => i?.ProfileDefinitionInfo.ProfileDefinitions != null)
				.SelectMany(i => i.ProfileDefinitionInfo.ProfileDefinitions)
				.Distinct()
				.ToList();

			foreach (var id in ids)
			{
				filter.OR(ReferencedProfileDefinitionsExposers.ID.Equal(id));
			}

			return ids.Count > 0 ? new DataHelperReferencedProfileDefinition(_connection).Read(filter) : new List<Models.ReferencedProfileDefinitions> ();
		}

		private List<Models.Script> GetRequiredScripts(List<ProfileDefinitionInstance> instances)
		{
			FilterElement<Models.Script> filter = new ORFilterElement<Models.Script>();
			List<Guid> guids = instances
				.Where(i => i?.ProfileDefinitionInfo.Scripts != null)
				.SelectMany(i => i.ProfileDefinitionInfo.Scripts)
				.Distinct()
				.ToList();

			foreach (Guid guid in guids)
			{
				filter = filter.OR(ScriptExposers.ID.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperScript(_connection).Read(filter) : new List<Models.Script>();
		}

		private void CreateOrUpdateReferencedProfileDefinitions(Models.ProfileDefinition item)
		{
			var helper = new DataHelperReferencedProfileDefinition(_connection);
			var filter = new ORFilterElement<Models.ReferencedProfileDefinitions>();
			foreach (var referencedProfileDefinition in item.ProfileDefinitions)
			{
				filter.OR(ReferencedProfileDefinitionsExposers.ID.Equal(referencedProfileDefinition.ID));
			}

			var existingReferencedProfileDefinitions = helper.Read(filter);

			foreach (var referencedProfileDefinition in item.ProfileDefinitions)
			{
				var existingReferencedProfileDefinition = existingReferencedProfileDefinitions.Find(r => r.ID == referencedProfileDefinition.ID);
				if (existingReferencedProfileDefinition == null)
				{
					referencedProfileDefinition.ID = helper.CreateOrUpdate(referencedProfileDefinition);
				}
				else
				{
					referencedProfileDefinition.ID = existingReferencedProfileDefinition.ID;
				}
			}
		}

		private void CreateOrUpdateScripts(Models.ProfileDefinition item)
		{
			var helper = new DataHelperScript(_connection);

			var filter = new ORFilterElement<Models.Script>();
			foreach (var script in item.Scripts)
			{
				filter.OR(ScriptExposers.ID.Equal(script.ID));
			}

			var existingScripts = helper.Read(filter);

			foreach (var script in item.Scripts)
			{
				var existingScript = existingScripts.Find(s => s.ID == script.ID);
				if (existingScript == null)
				{
					script.ID = helper.CreateOrUpdate(script);
				}
				else
				{
					script.ID = existingScript.ID;
				}
			}
		}
	}
}