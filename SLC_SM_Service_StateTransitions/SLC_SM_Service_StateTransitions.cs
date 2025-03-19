using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Actions;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using DomHelpers.SlcServicemanagement;

namespace SLCSMServiceStateTransitions
{
	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{
			// DO NOT REMOVE
			// engine.ShowUI();
			engine.ExitFail("This script should be executed using the 'OnDomAction' entry point");
		}

		[AutomationEntryPoint(AutomationEntryPointType.Types.OnDomAction)]
		public void OnDomActionMethod(IEngine engine, ExecuteScriptDomActionContext context)
		{
			// DO NOT REMOVE
			// engine.ShowUI();

			var instanceId = context.ContextId as DomInstanceId;
			var previousState = engine.GetScriptParam("PreviousState")?.Value;
			var nextState = engine.GetScriptParam("NextState")?.Value;

			//engine.GenerateInformation($"EventStateTransition: Input parameters instaceId: {instanceId.ToString()}, PreviousState: {previousState}, NextState: {nextState}");

			//engine.GenerateInformation("Starting DOM Action with script EventStateTransitions");

			//engine.GenerateInformation(previousState);
			//engine.GenerateInformation(nextState);

			if (!ValidateArguments(instanceId, previousState, nextState))
			{
				//engine.GenerateInformation($"{nextState} and {previousState}");
				engine.ExitFail("Input is not valid");
			}

			var domHelper = new DomHelper(engine.SendSLNetMessages, instanceId.ModuleId);

			//engine.GenerateInformation("Start Event Transition");

			string transitionId = String.Empty;

			switch (previousState)
			{
				case "new":
					switch (nextState)
					{
						case "designed":

							transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.New_To_Acknowledged;
							break;
						default:
							throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
					}

					break;
				case "designed":
					switch (nextState)
					{
						case "reserved":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Acknowledged_To_Inprogress;
							break;
						default:
							throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
					}
					break;
				case "reserved":
					switch (nextState)
					{
						case "terminated":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Inprogress_To_Completed;
							break;
						default:
							throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
					}

					break;
				case "terminated":
					switch (nextState)
					{
						case "retired":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Pending_To_Assesscancellation;
							break;
						default:
							throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
					}

					break;
				default:
					throw new NotSupportedException($"previousState '{previousState}' is not supported");
			}


			engine.GenerateInformation($"Service Order Status Transition starting: previousState: {previousState}, nextState: {nextState}");
			domHelper.DomInstances.DoStatusTransition(instanceId, transitionId);
		}

		private static bool ValidateArguments(DomInstanceId domInstanceId, string scriptParamValue, string scriptParamValue2)
		{
			if (domInstanceId == null)
			{
				return false;
			}

			if (String.IsNullOrEmpty(scriptParamValue))
			{
				return false;
			}

			if (String.IsNullOrEmpty(scriptParamValue2))
			{
				return false;
			}

			return true;
		}
	}
}
