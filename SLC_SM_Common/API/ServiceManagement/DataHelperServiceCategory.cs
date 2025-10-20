namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	/// <inheritdoc />
	public class DataHelperServiceCategory : DataHelper<Models.ServiceCategory>
	{
		/// <inheritdoc />
		public DataHelperServiceCategory(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceCategory)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceCategory item)
		{
			var instance = new ServiceCategoryInstance(New(item.ID));
			instance.ServiceCategoryInfo.Name = item.Name;
			instance.ServiceCategoryInfo.Type = item.Type;
			instance.ServiceCategoryInfo.Icon = item.Icon;

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ServiceCategory item)
		{
			return TryDelete(item.ID);
		}

		/// <inheritdoc />
		protected override List<Models.ServiceCategory> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ServiceCategoryInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ServiceCategory>();
			}

			return instances.Select(
					x => new Models.ServiceCategory
					{
						ID = x.ID.Id,
						Name = x.ServiceCategoryInfo.Name,
						Type = x.ServiceCategoryInfo.Type,
						Icon = x.ServiceCategoryInfo.Icon,
					})
				.ToList();
		}
	}
}