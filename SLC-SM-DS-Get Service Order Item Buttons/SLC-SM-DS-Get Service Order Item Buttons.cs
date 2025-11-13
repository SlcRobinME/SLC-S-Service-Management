namespace SLCSMDSGetServiceOrderItemButtons
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using SLC_SM_Common.Extensions;
	using static DomHelpers.SlcServicemanagement.SlcServicemanagementIds.Behaviors.Serviceorderitem_Behavior;

	/// <summary>
	///     Represents a data source.
	///     See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = DataSourceName)]
	public sealed class SLCSMDSGetServiceOrderItemButtons : IGQIDataSource, IGQIOnInit, IGQIInputArguments
	{
		private const string DataSourceName = "SLC-SM-DS-Get Service Order Item Buttons";

		private static readonly Dictionary<TransitionsEnum, string> ButtonNames = new Dictionary<TransitionsEnum, string>
		{
			{ TransitionsEnum.New_To_Acknowledged, "Acknowledge" },
			{ TransitionsEnum.New_To_Rejected, "Reject" },
			{ TransitionsEnum.Acknowledged_To_Rejected, "Reject" },
			{ TransitionsEnum.Acknowledged_To_Inprogress, "Initialize" },
			{ TransitionsEnum.Inprogress_To_Completed, "Complete" },
			{ TransitionsEnum.Inprogress_To_Failed, "Failed" },
			{ TransitionsEnum.Inprogress_To_Partial, "Partially Failed" },
			{ TransitionsEnum.Inprogress_To_Held, "Issue" },
			{ TransitionsEnum.Inprogress_To_Pending, "Information Missing" },
			{ TransitionsEnum.Pending_To_Assesscancellation, "Request Cancellation" },
			{ TransitionsEnum.Assesscancellation_To_Pendingcancellation, "Confirm Cancellation" },
			{ TransitionsEnum.Pendingcancellation_To_Cancelled, "Cancel" },
			{ TransitionsEnum.Held_To_Inprogress, "In Progress" },
			{ TransitionsEnum.Assesscancellation_To_Held, "Issue" },
			{ TransitionsEnum.Assesscancellation_To_Pending, "Information Missing" },
			{ TransitionsEnum.Held_To_Assesscancellation, "Request Cancellation" },
			{ TransitionsEnum.Pending_To_Inprogress, "In Progress" },
		};

		private readonly GQIStringArgument currentStateArg = new GQIStringArgument("Current State") { IsRequired = true };
		private string currentStateIdInput;
		private GQIDMS _dms;
		private IGQILogger _logger;

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("Button Label"),
				new GQIStringColumn("Script"),
				new GQIStringColumn("PreviousState"),
				new GQIStringColumn("NextState"),
			};
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[]
			{
				currentStateArg,
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			return _logger.PerformanceLogger(nameof(GetNextPage), BuildupRows);
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			currentStateIdInput = args.GetArgumentValue(currentStateArg);
			return default;
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			_dms = args.DMS;
			_logger = args.Logger;
			_logger.MinimumLogLevel = GQILogLevel.Debug;
			return default;
		}

		private GQIPage BuildupRows()
		{
			try
			{
				StatusesEnum currentState = Statuses.ToEnum(currentStateIdInput);

				return _logger.PerformanceLogger(
					"Build Rows",
					() =>
					{
						var transitions = Enum.GetValues(typeof(TransitionsEnum))
							.Cast<TransitionsEnum>()
							.Where(t => t.ToString().StartsWith($"{currentState}_to_", StringComparison.OrdinalIgnoreCase))
							.ToList();

						List<GQIRow> rows = transitions.Select(
								transition => new GQIRow(
									new[]
									{
										new GQICell { Value = ButtonNames.TryGetValue(transition, out string button) ? button : Transitions.ToValue(transition) },
										new GQICell { Value = "ServiceOrderItem_StateTransitions" },
										new GQICell { Value = currentState.ToString() },
										new GQICell { Value = transition.ToString().Split('_').Last() },
									}))
							.ToList();

						return new GQIPage(rows.ToArray())
						{
							HasNextPage = false,
						};
					});
			}
			catch (Exception e)
			{
				_dms.GenerateInformationMessage($"GQIDS|{nameof(DataSourceName)}|Exception: {e}");
				_logger.Error($"GQIDS|{nameof(DataSourceName)}|Exception: {e}");
				return new GQIPage(Array.Empty<GQIRow>());
			}
		}
	}
}