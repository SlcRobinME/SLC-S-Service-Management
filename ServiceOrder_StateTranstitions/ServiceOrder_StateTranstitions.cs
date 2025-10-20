namespace ServiceOrder_StateTranstitions_1
{
	using System;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Actions;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions;

	public class Script
	{
		[AutomationEntryPoint(AutomationEntryPointType.Types.OnDomAction)]
		public void OnDomActionMethod(IEngine engine, ExecuteScriptDomActionContext context)
		{
			// DO NOT REMOVE
			// engine.ShowUI();

			var instanceId = context.ContextId as DomInstanceId;
			var previousState = engine.ReadScriptParamFromApp("PreviousState");
			var nextState = engine.ReadScriptParamFromApp("NextState");

			////engine.GenerateInformation($"EventStateTransition: Input parameters instaceId: {instanceId.ToString()}, PreviousState: {previousState}, NextState: {nextState}");

			////engine.GenerateInformation("Starting DOM Action with script EventStateTransitions");

			////engine.GenerateInformation(previousState);
			////engine.GenerateInformation(nextState);

			var domHelper = new DomHelper(engine.SendSLNetMessages, instanceId.ModuleId);

			////engine.GenerateInformation("Start Event Transition");

			string transitionId = String.Empty;

			switch (previousState)
			{
				case "new":
					transitionId = GetTransitionIdNew(nextState, previousState);
					break;

				case "acknowledged":
					transitionId = GetTransitionIdAck(nextState, previousState);
					break;

				case "inprogress":
					transitionId = GetTransitionIdInProgress(nextState, previousState);
					break;

				case "pending":
					transitionId = GetTransitionIdPending(nextState, previousState);
					break;

				case "held":
					transitionId = GetTransitionIdHeld(nextState, previousState);
					break;

				case "assesscancellation":
					transitionId = GetTransitionIdAssess(nextState, previousState);
					break;

				case "pendingcancellation":
					transitionId = GetransitionIdPendingCancel(nextState, previousState);
					break;

				default:
					throw new NotSupportedException($"previousState '{previousState}' is not supported");
			}

			engine.GenerateInformation($"Service Order Status Transition starting: previousState: {previousState}, nextState: {nextState}");
			domHelper.DomInstances.DoStatusTransition(instanceId, transitionId);
		}

		private static string GetransitionIdPendingCancel(string nextState, string previousState)
		{
			string transitionId;
			switch (nextState)
			{
				case "cancelled":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Pendingcancellation_To_Cancelled;
					break;

				default:
					throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
			}

			return transitionId;
		}

		private static string GetTransitionIdAssess(string nextState, string previousState)
		{
			string transitionId;
			switch (nextState)
			{
				case "pendingcancellation":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Assesscancellation_To_Pendingcancellation;
					break;

				case "held":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Assesscancellation_To_Held;
					break;

				case "pending":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Assesscancellation_To_Pending;
					break;

				default:
					throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
			}

			return transitionId;
		}

		private static string GetTransitionIdHeld(string nextState, string previousState)
		{
			string transitionId;
			switch (nextState)
			{
				case "assesscancellation":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Held_To_Assesscancellation;
					break;

				case "inprogress":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Held_To_Inprogress;
					break;

				default:
					throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
			}

			return transitionId;
		}

		private static string GetTransitionIdPending(string nextState, string previousState)
		{
			string transitionId;
			switch (nextState)
			{
				case "assesscancellation":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Pending_To_Assesscancellation;
					break;

				case "inprogress":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Pending_To_Inprogress;
					break;

				default:
					throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
			}

			return transitionId;
		}

		private static string GetTransitionIdInProgress(string nextState, string previousState)
		{
			string transitionId;
			switch (nextState)
			{
				case "completed":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Inprogress_To_Completed;
					break;

				case "failed":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Inprogress_To_Failed;
					break;

				case "partial":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Inprogress_To_Partial;
					break;

				case "held":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Inprogress_To_Held;
					break;

				case "pending":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Inprogress_To_Pending;
					break;

				default:
					throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
			}

			return transitionId;
		}

		private static string GetTransitionIdAck(string nextState, string previousState)
		{
			string transitionId;
			switch (nextState)
			{
				case "inprogress":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Acknowledged_To_Inprogress;
					break;

				case "rejected":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Acknowledged_To_Rejected;
					break;

				default:
					throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
			}

			return transitionId;
		}

		private static string GetTransitionIdNew(string nextState, string previousState)
		{
			string transitionId;
			switch (nextState)
			{
				case "acknowledged":

					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.New_To_Acknowledged;
					break;

				case "rejected":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.New_To_Rejected;
					break;

				default:
					throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
			}

			return transitionId;
		}

		/// <summary>
		///     The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(Engine engine)
		{
			// DO NOT REMOVE
			// engine.ShowUI();
			engine.ExitFail("This script should be executed using the 'OnDomAction' entry point");
		}
	}
}