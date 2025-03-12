using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Actions;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using DomHelpers.SlcServicemanagement;
using Newtonsoft.Json;
using System.Linq;

namespace ServiceOrderItemStateTranstitions
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
			OnButtonActionMethod(engine);
		}

		public void OnButtonActionMethod(IEngine engine)
		{

			var domInstanceIdInput = engine.GetScriptParam("Id")?.Value;
			string domInstanceId = JsonConvert.DeserializeObject<List<string>>(domInstanceIdInput).FirstOrDefault();
			var previousStateInput = engine.GetScriptParam("PreviousState")?.Value;
			string previousState = JsonConvert.DeserializeObject<List<string>>(previousStateInput).FirstOrDefault();
			var nextStateInput = engine.GetScriptParam("NextState")?.Value;
			string nextState = JsonConvert.DeserializeObject<List<string>>(nextStateInput).FirstOrDefault();

			//engine.GenerateInformation($"EventStateTransition: Input parameters instaceId: {instanceId.ToString()}, PreviousState: {previousState}, NextState: {nextState}");

			//engine.GenerateInformation("Starting DOM Action with script EventStateTransitions");

			//engine.GenerateInformation(previousState);
			//engine.GenerateInformation(nextState);

			if (!ValidateArguments(domInstanceId, previousState, nextState))
			{
				//engine.GenerateInformation($"{nextState} and {previousState}");
				engine.ExitFail("Input is not valid");
			}

			var domHelper = new DomHelper(engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);

			//engine.GenerateInformation("Start Event Transition");

			string transitionId = String.Empty;

			switch (previousState)
			{
				case "new":
					switch (nextState)
					{
						case "acknowledged":

							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.New_To_Acknowledged;
							break;
						case "rejected":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.New_To_Rejected;
							break;
						default:
							throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
					}

					break;
				case "acknowledged":
					switch (nextState)
					{
						case "inprogress":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Acknowledged_To_Inprogress;
							break;
						case "rejected":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Acknowledged_To_Rejected;
							break;
						default:
							throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
					}
					break;
				case "inprogress":
					switch (nextState)
					{
						case "completed":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Inprogress_To_Completed;
							break;
						case "failed":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Inprogress_To_Failed;
							break;
						case "partial":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Inprogress_To_Partial;
							break;
						case "held":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Inprogress_To_Held;
							break;
						case "pending":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Inprogress_To_Pending;
							break;
						default:
							throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
					}

					break;
				case "pending":
					switch (nextState)
					{
						case "assesscancellation":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Pending_To_Assesscancellation;
							break;
						case "inprogress":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Pending_To_Inprogress;
							break;
						default:
							throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
					}

					break;
				case "held":
					switch (nextState)
					{
						case "assesscancellation":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Held_To_Assesscancellation;
							break;
						case "inprogress":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Held_To_Inprogress;
							break;
						default:
							throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
					}

					break;
				case "assesscancellation":
					switch (nextState)
					{
						case "pendingcancellation":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Assesscancellation_To_Pendingcancellation;
							break;
						case "held":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Assesscancellation_To_Held;
							break;
						case "pending":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Assesscancellation_To_Pending;
							break;
						default:
							throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
					}

					break;
				case "pendingcancellation":
					switch (nextState)
					{
						case "cancelled":
							transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Pendingcancellation_To_Cancelled;
							break;
						default:
							throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
					}

					break;
				default:
					throw new NotSupportedException($"previousState '{previousState}' is not supported");
			}


			engine.GenerateInformation($"Service Order Status Transition starting: previousState: {previousState}, nextState: {nextState}");

			domHelper.DomInstances.DoStatusTransition(new DomInstanceId(Guid.Parse(domInstanceId)), transitionId);
		}

		private static bool ValidateArguments(string domInstanceIdGuid, string scriptParamValue, string scriptParamValue2)
		{
			if (!Guid.TryParse(domInstanceIdGuid, out var domInstanceId))
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
