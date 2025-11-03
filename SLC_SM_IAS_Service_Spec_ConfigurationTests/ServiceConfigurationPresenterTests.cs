namespace SLC_SM_IAS_Service_Spec_Configuration.Tests.Presenters
{
	using System.Reflection;
	using Moq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using SLC_SM_IAS_Service_Spec_Configuration.Presenters;
	using SLC_SM_IAS_Service_Spec_Configuration.Views;

	[TestClass]
	public class ServiceConfigurationPresenterTests
	{
		[TestMethod]
		public void AddConfigModel_AddsConfiguration()
		{
			// Arrange
			var engine = Mock.Of<IEngine>();
			var view = new ServiceConfigurationView(engine);
			var serviceSpecification = new Models.ServiceSpecification { Configurations = new List<Models.ServiceSpecificationConfigurationValue>() };
			var presenter = new ServiceConfigurationPresenter(engine, new InteractiveController(engine), view, serviceSpecification);

			var param = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter
			{
				ID = Guid.NewGuid(),
				Name = "TestParam",
			};

			// Act
			var addConfigModelMethod = typeof(ServiceConfigurationPresenter)
				.GetMethod("AddConfigModel", BindingFlags.NonPublic | BindingFlags.Instance);
			addConfigModelMethod.Invoke(presenter, new object[] { param });

			// Assert
			Assert.AreEqual(1, serviceSpecification.Configurations.Count);
			Assert.AreEqual(param.ID, serviceSpecification.Configurations[0].ConfigurationParameter.ConfigurationParameterId);
		}
	}
}