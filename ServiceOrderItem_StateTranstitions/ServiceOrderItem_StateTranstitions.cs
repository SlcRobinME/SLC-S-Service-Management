namespace ServiceOrderItemStateTranstitions
{
	using System;
	using System.Linq;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.SDM;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions;
	using static DomHelpers.SlcServicemanagement.SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		public static void OnButtonActionMethod(IEngine engine)
		{
			Guid domInstanceId = engine.ReadScriptParamFromApp<Guid>("Id");
			string previousState = engine.ReadScriptParamFromApp("PreviousState").ToLower();
			string nextState = engine.ReadScriptParamFromApp("NextState").ToLower();

			TransitionsEnum transition = Enum.GetValues(typeof(TransitionsEnum))
				.Cast<TransitionsEnum?>()
				.FirstOrDefault(t => t.ToString().Equals($"{previousState}_to_{nextState}", StringComparison.OrdinalIgnoreCase))
				?? throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");

			var orderItemHelper = new DataHelperServiceOrderItem(engine.GetUserConnection());
			var service = orderItemHelper.Read(ServiceOrderItemExposers.Guid.Equal(domInstanceId)).FirstOrDefault()
						  ?? throw new NotSupportedException($"No Service with ID '{domInstanceId}' exists on the system");

			engine.GenerateInformation($"Service Order Item Status Transition starting: previousState: {previousState}, nextState: {nextState}");
			orderItemHelper.UpdateState(service, transition);
		}

		/// <summary>
		///     The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{
			OnButtonActionMethod(engine);
		}
	}
}