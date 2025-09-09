namespace ServiceStateTransitions
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
			var serviceReference = engine.GetScriptParam("ServiceReference").Value.Trim('[', ']').Trim('"', '"');
			var previousState = engine.GetScriptParam("PreviousState").Value.Trim('[', ']').Trim('"', '"').ToLower();
			var nextState = engine.GetScriptParam("NextState").Value.Trim('[', ']').Trim('"', '"').ToLower();

			string transitionId = String.Empty;

			switch (previousState)
			{
				case "new":
					transitionId = GetTransitionIdNew(previousState, nextState);
					break;

				case "designed":
					transitionId = GetTransitionIdDesigned(previousState, nextState);
					break;

				case "reserved":
					transitionId = GetTransitionIdReserved(previousState, nextState);
					break;

				case "active":
					transitionId = GetTransitionIdActive(previousState, nextState);
					break;

				case "terminated":
					transitionId = GetTransitionIdTerminated(previousState, nextState);
					break;

				default:
					throw new NotSupportedException($"previousState '{previousState}' is not supported");
			}

			engine.GenerateInformation($"Service Order Status Transition starting: previousState: {previousState}, nextState: {nextState}");

			domHelper.DomInstances.DoStatusTransition(new DomInstanceId(Guid.Parse(serviceReference)), transitionId);
		}

		private static string GetTransitionIdTerminated(string previousState, string nextState)
		{
			string transitionId;
			switch (nextState)
			{
				case "retired":
					transitionId = SlcServicemanagementIds.Behaviors.Service_Behavior.Transitions.Terminated_To_Retired;
					break;

				case "active":
					transitionId = SlcServicemanagementIds.Behaviors.Service_Behavior.Transitions.Terminated_To_Active;
					break;

				default:
					throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
			}

			return transitionId;
		}

		private static string GetTransitionIdActive(string previousState, string nextState)
		{
			string transitionId;
			switch (nextState)
			{
				case "terminated":
					transitionId = SlcServicemanagementIds.Behaviors.Service_Behavior.Transitions.Active_To_Terminated;
					break;

				default:
					throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
			}

			return transitionId;
		}

		private static string GetTransitionIdReserved(string previousState, string nextState)
		{
			string transitionId;
			switch (nextState)
			{
				case "active":
					transitionId = SlcServicemanagementIds.Behaviors.Service_Behavior.Transitions.Reserved_To_Active;
					break;

				case "retired":
					transitionId = SlcServicemanagementIds.Behaviors.Service_Behavior.Transitions.Reserved_To_Retired;
					break;

				default:
					throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
			}

			return transitionId;
		}

		private static string GetTransitionIdDesigned(string previousState, string nextState)
		{
			string transitionId;
			switch (nextState)
			{
				case "reserved":
					transitionId = SlcServicemanagementIds.Behaviors.Service_Behavior.Transitions.Designed_To_Reserved;
					break;

				case "retired":
					transitionId = SlcServicemanagementIds.Behaviors.Service_Behavior.Transitions.Designed_To_Retired;
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
				case "designed":
					transitionId = SlcServicemanagementIds.Behaviors.Service_Behavior.Transitions.New_To_Designed;
					break;

				case "retired":
					transitionId = SlcServicemanagementIds.Behaviors.Service_Behavior.Transitions.New_To_Retired;
					break;

				default:
					throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
			}

			return transitionId;
		}
	}
}
