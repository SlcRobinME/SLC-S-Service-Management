namespace SLCSMButtonStateTransitions
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
				throw; // Comment if it should be treated as a normal exit of the script.
			}
			catch (ScriptForceAbortException)
			{
				// Catch forced abort exceptions, caused via external maintenance messages.
				throw;
			}
			catch (ScriptTimeoutException)
			{
				// Catch timeout exceptions for when a script has been running for too long.
				throw;
			}
			catch (InteractiveUserDetachedException)
			{
				// Catch a user detaching from the interactive script by closing the window.
				// Only applicable for interactive scripts, can be removed for non-interactive scripts.
				throw;
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			var domHelper = new DomHelper(engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
			var serviceOrderReference = engine.GetScriptParam("ServiceOrderReference").Value.Trim('[', ']').Trim('"', '"');
			var previousState = engine.GetScriptParam("PreviousState").Value.Trim('[', ']').Trim('"', '"').ToLower();
			var nextState = engine.GetScriptParam("NextState").Value.Trim('[', ']').Trim('"', '"').ToLower();

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

			domHelper.DomInstances.DoStatusTransition(new DomInstanceId(Guid.Parse(serviceOrderReference)), transitionId);
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
	}
}
