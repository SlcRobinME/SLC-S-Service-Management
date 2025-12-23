namespace Library.Ownership
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.SDM;
	using Models = Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement.Models;

	public static class OwnershipExtensions
	{
		public static bool TakeOwnershipForOrder(this Models.ServiceOrder order, IEngine engine)
		{
			var person = InitializeCurrentPerson(engine);
			if (person == null)
			{
				order.Owner = default(Guid?);
				return false;
			}

			order.Owner = person.ID;
			return true;
		}

		private static Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.People InitializeCurrentPerson(IEngine engine)
		{
			string userName = engine.UserDisplayName;
			if (String.IsNullOrEmpty(userName))
			{
				return null;
			}

			var dataHelper = new DataHelpersPeopleAndOrganizations(engine.GetUserConnection());

			var person = dataHelper.People.Read(PeopleExposers.Name.Equal(userName)).FirstOrDefault();
			if (person != null)
			{
				return person;
			}

			var experience = dataHelper.Experiences.Read(ExperienceLevelExposers.Value.Equal("Engineer")).FirstOrDefault();
			if (experience == null)
			{
				Guid expId = dataHelper.Experiences.CreateOrUpdate(
					new Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.ExperienceLevel
					{
						Value = "Engineer",
					});
				experience = dataHelper.Experiences.Read(ExperienceLevelExposers.Guid.Equal(expId)).FirstOrDefault();
			}

			// Try create a new user
			Guid personId = dataHelper.People.CreateOrUpdate(
				new Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.People
				{
					FullName = userName,
					ExperienceLevel = experience,
				});
			return dataHelper.People.Read(PeopleExposers.Guid.Equal(personId)).FirstOrDefault();
		}
	}
}