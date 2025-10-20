namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcPeople_Organizations;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	/// <inheritdoc />
	public class DataHelperCategory : DataHelper<Models.Category>
	{
		/// <inheritdoc />
		public DataHelperCategory(IConnection connection) : base(connection, SlcPeople_OrganizationsIds.Definitions.Category)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.Category item)
		{
			var instance = new CategoryInstance(New(item.ID));
			instance.CategoryInformation.Category = item.Name;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.Category item)
		{
			return TryDelete(item.ID);
		}

		/// <inheritdoc />
		protected override List<Models.Category> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new CategoryInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.Category>();
			}

			return instances.Select(
					x => new Models.Category
					{
						ID = x.ID.Id,
						Name = x.CategoryInformation.Category,
					})
				.ToList();
		}
	}
}