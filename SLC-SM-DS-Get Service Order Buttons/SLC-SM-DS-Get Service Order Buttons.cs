namespace SLCSMDSGetServiceOrderButtons
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "SLC-SM-DS-Get Service Order Buttons")]
	public sealed class SLCSMDSGetServiceOrderButtons : IGQIDataSource
		, IGQIOnInit
		, IGQIInputArguments
	{
		private readonly GQIStringArgument serviceOrderReferenceArg = new GQIStringArgument("ServiceOrderReference") { IsRequired = true};
		private Guid serviceOrderReference;

		private GQIDMS _dms;

		private readonly List<ItemState> itemStateList = new List<ItemState>
		{
			ItemStates.newState, ItemStates.acknowledgedState, ItemStates.inprogressState, ItemStates.rejectState, ItemStates.failedState, ItemStates.partialState, ItemStates.heldState,
			ItemStates.pendingState, ItemStates.completedState, ItemStates.assesscancellationState, ItemStates.pendingcancellationState, ItemStates.cancelledState,
		};

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			// Initialize the data source
			// See: https://aka.dataminer.services/igqioninit-oninit
			_dms = args.DMS;
			return default;
		}

		public GQIArgument[] GetInputArguments()
		{
			// Define data source input arguments
			// See: https://aka.dataminer.services/igqiinputarguments-getinputarguments
			return new GQIArgument[]
			{
				serviceOrderReferenceArg,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			// Process input argument values
			// See: https://aka.dataminer.services/igqiinputarguments-onargumentsprocessed
			if (!Guid.TryParse(args.GetArgumentValue(serviceOrderReferenceArg), out serviceOrderReference))
				serviceOrderReference = Guid.Empty;

			return new OnArgumentsProcessedOutputArgs();
		}

		public GQIColumn[] GetColumns()
		{
			// Define data source columns
			// See: https://aka.dataminer.services/igqidatasource-getcolumns
			return new GQIColumn[]
			{
				new GQIStringColumn("Button Label"),
				new GQIStringColumn("Script"),
				new GQIStringColumn("PreviousState"),
				new GQIStringColumn("NextState"),
				new GQIBooleanColumn("IsHappyFlow"),
			};
		}

		public OnPrepareFetchOutputArgs OnPrepareFetch(OnPrepareFetchInputArgs args)
		{
			// Prepare data source for fetching
			// See: https://aka.dataminer.services/igqionpreparefetch-onpreparefetch
			return default;
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			// Define data source rows
			// See: https://aka.dataminer.services/igqidatasource-getnextpage

			var domHelper = new DomHelper(_dms.SendMessages, SlcServicemanagementIds.ModuleId);
			var filter = DomInstanceExposers.Id.Equal(serviceOrderReference);
			var instance = domHelper.DomInstances.Read(filter).FirstOrDefault() ?? throw new Exception($"Could not find service order with id {serviceOrderReference}");
			ItemState currentState = itemStateList.SingleOrDefault(state => state.Id == instance.StatusId);

			List<ButtonConfig> activeButtons = ButtonCollection.ButtonList.Where(button => button.ApplicableStates.Contains(currentState)).ToList();

			List<GQIRow> rows = activeButtons.Select(
					button => new GQIRow(
						new[]
						{
							new GQICell { Value = button.Name },
							new GQICell { Value = button.ScriptToExecute },
							new GQICell { Value = button.PreviousState.NameId },
							new GQICell { Value = button.NextState.NameId },
							new GQICell { Value = button.IsHappyFlow },
						}))
				.ToList();

			return new GQIPage(rows.ToArray())
			{
				HasNextPage = false,
			};
		}

		public static class ButtonCollection
		{
			public static readonly List<ButtonConfig> ButtonList = new List<ButtonConfig>
			{
				new ButtonConfig
				{
					Id = "new_to_acknowledged",
					Name = "Acknowledge",
					ApplicableStates = new List<ItemState> { ItemStates.newState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.newState,
					NextState = ItemStates.acknowledgedState,
					IsHappyFlow = true,
				},

				new ButtonConfig
				{
					Id = "new_to_reject",
					Name = "Reject",
					ApplicableStates = new List<ItemState> { ItemStates.newState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.newState,
					NextState = ItemStates.acknowledgedState,
					IsHappyFlow = false,
				},

				new ButtonConfig
				{
					Id = "acknowledged_to_inprogress",
					Name = "Initialize",
					ApplicableStates = new List<ItemState> { ItemStates.acknowledgedState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.acknowledgedState,
					NextState = ItemStates.inprogressState,
					IsHappyFlow = true,
				},
				new ButtonConfig
				{
					Id = "acknowledged_to_reject",
					Name = "Reject",
					ApplicableStates = new List<ItemState> { ItemStates.acknowledgedState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.acknowledgedState,
					NextState = ItemStates.rejectState,
					IsHappyFlow = false,
				},
				new ButtonConfig
				{
					Id = "inprogress_to_failed",
					Name = "Failed",
					ApplicableStates = new List<ItemState> { ItemStates.inprogressState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.inprogressState,
					NextState = ItemStates.failedState,
					IsHappyFlow = false,
				},
				new ButtonConfig
				{
					Id = "inprogress_to_failed",
					Name = "Partially Failed",
					ApplicableStates = new List<ItemState> { ItemStates.inprogressState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.inprogressState,
					NextState = ItemStates.partialState,
					IsHappyFlow = false,
				},
				new ButtonConfig
				{
					Id = "inprogress_to_completed",
					Name = "Complete",
					ApplicableStates = new List<ItemState> { ItemStates.inprogressState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.inprogressState,
					NextState = ItemStates.completedState,
					IsHappyFlow = true,
				},
				new ButtonConfig
				{
					Id = "inprogress_to_held",
					Name = "Issue",
					ApplicableStates = new List<ItemState> { ItemStates.inprogressState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.inprogressState,
					NextState = ItemStates.heldState,
					IsHappyFlow = false,
				},
				new ButtonConfig
				{
					Id = "inprogress_to_pending",
					Name = "Information Missing",
					ApplicableStates = new List<ItemState> { ItemStates.inprogressState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.inprogressState,
					NextState = ItemStates.pendingState,
					IsHappyFlow = false,
				},
				new ButtonConfig
				{
					Id = "pending_to_assesscancellation",
					Name = "Request Cancellation",
					ApplicableStates = new List<ItemState> { ItemStates.pendingState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.pendingState,
					NextState = ItemStates.assesscancellationState,
					IsHappyFlow = false,
				},
				new ButtonConfig
				{
					Id = "held_to_assesscancellation",
					Name = "Request Cancellation",
					ApplicableStates = new List<ItemState> { ItemStates.heldState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.heldState,
					NextState = ItemStates.assesscancellationState,
					IsHappyFlow = false,
				},
				new ButtonConfig
				{
					Id = "assesscancellation_to_pendingcancellation",
					Name = "Confirm Cancellation",
					ApplicableStates = new List<ItemState> { ItemStates.assesscancellationState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.assesscancellationState,
					NextState = ItemStates.pendingcancellationState,
					IsHappyFlow = false,
				},
				new ButtonConfig
				{
					Id = "pendingcancellation_to_cancelled",
					Name = "Cancel",
					ApplicableStates = new List<ItemState> { ItemStates.pendingcancellationState },
					ScriptToExecute = "ServiceOrderItem_StateTransitions",
					PreviousState = ItemStates.pendingcancellationState,
					NextState = ItemStates.cancelledState,
					IsHappyFlow = false,
				},
			};
		}

		public static class ItemStates
		{
			public static readonly ItemState acknowledgedState = new ItemState { Name = "Acknowledged", NameId = "acknowledged", Id = "d917fc53-2638-4ab9-9ac6-651ec5312bac" };
			public static readonly ItemState assesscancellationState = new ItemState { Name = "Assess Cancellation", NameId = "assesscancellation", Id = "f7e93ddd-cddf-4755-a3e5-0f6ff885dcf5" };
			public static readonly ItemState cancelledState = new ItemState { Name = "Cancelled", NameId = "cancelled", Id = "61b80d48-d555-462e-baae-a52b17c85ddb" };
			public static readonly ItemState completedState = new ItemState { Name = "Completed", NameId = "completed", Id = "f8a8d853-faaf-401c-9865-71e314614023" };
			public static readonly ItemState failedState = new ItemState { Name = "Failed", NameId = "failed", Id = "6a01a480-4c38-4db7-b545-72ba05742a7e" };
			public static readonly ItemState heldState = new ItemState { Name = "Held", NameId = "held", Id = "310ea9e9-f65c-4e11-8b1b-e2c34688ef44" };
			public static readonly ItemState inprogressState = new ItemState { Name = "In Progress", NameId = "inprogress", Id = "331dc1c2-1950-4c00-a4ae-0aba674a30e6" };
			public static readonly ItemState newState = new ItemState { Name = "New", NameId = "new", Id = "06df5562-cd9b-4b0b-bd45-c58560a8b22a" };
			public static readonly ItemState partialState = new ItemState { Name = "Partial", NameId = "partial", Id = "7f13d019-29de-43cb-a510-ab2b2a77e785" };
			public static readonly ItemState pendingcancellationState = new ItemState { Name = "Pending Cancellation", NameId = "pendingcancellation", Id = "15d08c01-fe63-4d5f-8544-e5b4d66439f5" };
			public static readonly ItemState pendingState = new ItemState { Name = "Pending", NameId = "pending", Id = "23f9fa75-32b8-4e4a-bd65-06a7344d1902" };
			public static readonly ItemState rejectState = new ItemState { Name = "Rejected", NameId = "rejected", Id = "260a7073-e54e-4482-a8a7-2b4f2e49c42e" };
		}

		public class ButtonConfig
		{
			public string Id { get; set; }

			public string Name { get; set; }

			public List<ItemState> ApplicableStates { get; set; }

			public string ScriptToExecute { get; set; }

			public ItemState PreviousState { get; set; }

			public ItemState NextState { get; set; }

			public bool IsHappyFlow { get; set; }
		}

		public class ItemState
		{
			public string Name { get; set; }

			public string NameId { get; set; }

			public string Id { get; set; }
		}
	}
}
