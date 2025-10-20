namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcPeople_Organizations;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	/// <inheritdoc />
	public class DataHelperOrganization : DataHelper<Models.Organization>
	{
		/// <inheritdoc />
		public DataHelperOrganization(IConnection connection) : base(connection, SlcPeople_OrganizationsIds.Definitions.Organizations)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.Organization item)
		{
			var instance = new OrganizationsInstance(New(item.ID));
			instance.OrganizationInformation.OrganizationName = item.Name;
			instance.OrganizationInformation.Category = item.CategoryId;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.Organization item)
		{
			return TryDelete(item.ID);
		}

		/// <inheritdoc />
		protected override List<Models.Organization> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new OrganizationsInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.Organization>();
			}

			return instances.Select(
					x => new Models.Organization
					{
						ID = x.ID.Id,
						Name = x.OrganizationInformation.OrganizationName,
						CategoryId = x.OrganizationInformation.Category,
					})
				.ToList();
		}
	}
}