namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcPeople_Organizations;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	/// <inheritdoc />
	public class DataHelperPersonalExperience : DataHelper<Models.ExperienceLevel>
	{
		/// <inheritdoc />
		public DataHelperPersonalExperience(IConnection connection) : base(connection, SlcPeople_OrganizationsIds.Definitions.Experience)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ExperienceLevel item)
		{
			var instance = new ExperienceInstance(New(item.ID));
			instance.ExperienceInformation.Experience = item.Value;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(IEnumerable<Models.ExperienceLevel> items)
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
		internal override List<Models.ExperienceLevel> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ExperienceInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ExperienceLevel>();
			}

			return instances.Select(
					x => new Models.ExperienceLevel
					{
						ID = x.ID.Id,
						Value = x.ExperienceInformation.Experience,
					})
				.ToList();
		}
	}
}