namespace ServiceOrderItemStateTranstitions
{
	using System;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		public void OnButtonActionMethod(IEngine engine)
		{
			string domInstanceId = engine.ReadScriptParamFromApp("Id");
			string previousState = engine.ReadScriptParamFromApp("PreviousState");
			string nextState = engine.ReadScriptParamFromApp("NextState");

			////engine.GenerateInformation($"EventStateTransition: Input parameters instaceId: {instanceId.ToString()}, PreviousState: {previousState}, NextState: {nextState}");

			////engine.GenerateInformation("Starting DOM Action with script EventStateTransitions");

			////engine.GenerateInformation(previousState);
			////engine.GenerateInformation(nextState);

			var domHelper = new DomHelper(engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);

			////engine.GenerateInformation("Start Event Transition");

			string transitionId = String.Empty;

			switch (previousState)
			{
				case "new":
					transitionId = GetTransitionIdNew(previousState, nextState);
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
					transitionId = GetTransitionIdPendingCancel(nextState, previousState);

					break;

				default:
					throw new NotSupportedException($"previousState '{previousState}' is not supported");
			}

			engine.GenerateInformation($"Service Order Status Transition starting: previousState: {previousState}, nextState: {nextState}");

			domHelper.DomInstances.DoStatusTransition(new DomInstanceId(Guid.Parse(domInstanceId)), transitionId);
		}

		private static string GetTransitionIdPendingCancel(string nextState, string previousState)
		{
			string transitionId;
			switch (nextState)
			{
				case "cancelled":
					transitionId = SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior.Transitions.Pendingcancellation_To_Cancelled;
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

			return transitionId;
		}

		private static string GetTransitionIdHeld(string nextState, string previousState)
		{
			string transitionId;
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

			return transitionId;
		}

		private static string GetTransitionIdPending(string nextState, string previousState)
		{
			string transitionId;
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

			return transitionId;
		}

		private static string GetTransitionIdInProgress(string nextState, string previousState)
		{
			string transitionId;
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

			return transitionId;
		}

		private static string GetTransitionIdAck(string nextState, string previousState)
		{
			string transitionId;
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

			return transitionId;
		}

		private static string GetTransitionIdNew(string previousState, string nextState)
		{
			string transitionId;
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

			return transitionId;
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