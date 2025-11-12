namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcConfigurations;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	/// <inheritdoc />
	public class DataHelperReferencedProfileDefinition : DataHelper<Models.ReferencedProfileDefinitions>
	{
		/// <inheritdoc />
		public DataHelperReferencedProfileDefinition(IConnection connection) : base(connection, SlcConfigurationsIds.Definitions.ReferencedProfileDefinitions)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ReferencedProfileDefinitions item)
		{
			var instance = new ReferencedProfileDefinitionsInstance(New(item.ID));
			var info = instance.ReferencedProfileDefinitionsInfo;

			info.ProfileDefinition = item.ProfileDefinitionReference;
			info.Mandatory = item.Mandatory;
			info.AllowMultiple = item.AllowMultiple;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(IEnumerable<Models.ReferencedProfileDefinitions> items)
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
		internal override List<Models.ReferencedProfileDefinitions> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ReferencedProfileDefinitionsInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ReferencedProfileDefinitions>();
			}

			return instances.Select(
					x => new Models.ReferencedProfileDefinitions
					{
						ID = x.ID.Id,
						ProfileDefinitionReference = x.ReferencedProfileDefinitionsInfo.ProfileDefinition.Value,
						Mandatory = x.ReferencedProfileDefinitionsInfo.Mandatory.Value,
						AllowMultiple = x.ReferencedProfileDefinitionsInfo.AllowMultiple.Value,
					}).ToList();
		}
	}
}