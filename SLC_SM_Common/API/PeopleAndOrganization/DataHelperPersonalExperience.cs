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
		public override bool TryDelete(Models.ExperienceLevel item)
		{
			return TryDelete(item.ID);
		}

		/// <inheritdoc />
		protected override List<Models.ExperienceLevel> Read(IEnumerable<DomInstance> domInstances)
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