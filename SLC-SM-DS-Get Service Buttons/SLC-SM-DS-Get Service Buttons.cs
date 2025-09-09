namespace SLCSMDSGetServiceButtons
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
	[GQIMetaData(Name = "SLC-SM-DS-Get Service Buttons")]
	public sealed class SLCSMDSGetServiceButtons : IGQIDataSource
		, IGQIOnInit
		, IGQIInputArguments
	{

		private readonly GQIStringArgument serviceReferenceArg = new GQIStringArgument("ServiceReference") { IsRequired = true };
		private Guid serviceReference;

		private GQIDMS _dms;

		private readonly List<ItemState> itemStateList = new List<ItemState>
		{
			ItemStates.newState, ItemStates.designedState, ItemStates.reservedState, ItemStates.activeState, ItemStates.terminatedState, ItemStates.retiredState,
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
				serviceReferenceArg,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			// Process input argument values
			// See: https://aka.dataminer.services/igqiinputarguments-onargumentsprocessed
			if (!Guid.TryParse(args.GetArgumentValue(serviceReferenceArg), out serviceReference))
				serviceReference = Guid.Empty;

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

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			// Define data source rows
			// See: https://aka.dataminer.services/igqidatasource-getnextpage
			var domHelper = new DomHelper(_dms.SendMessages, SlcServicemanagementIds.ModuleId);
			var filter = DomInstanceExposers.Id.Equal(serviceReference);
			var instance = domHelper.DomInstances.Read(filter).FirstOrDefault() ?? throw new Exception($"Could not find service order with id {serviceReference}");
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
					Id = "new_to_designed",
					Name = "Designed",
					ApplicableStates = new List<ItemState> { ItemStates.newState },
					ScriptToExecute = "Service_StateTransitions",
					PreviousState = ItemStates.newState,
					NextState = ItemStates.designedState,
					IsHappyFlow = true,
				},

				new ButtonConfig
				{
					Id = "designed_to_reserved",
					Name = "Reserve",
					ApplicableStates = new List<ItemState> { ItemStates.designedState },
					ScriptToExecute = "Service_StateTransitions",
					PreviousState = ItemStates.designedState,
					NextState = ItemStates.reservedState,
					IsHappyFlow = true,
				},

				new ButtonConfig
				{
					Id = "reserved_to_active",
					Name = "Activate",
					ApplicableStates = new List<ItemState> { ItemStates.reservedState },
					ScriptToExecute = "Service_StateTransitions",
					PreviousState = ItemStates.reservedState,
					NextState = ItemStates.activeState,
					IsHappyFlow = true,
				},
				new ButtonConfig
				{
					Id = "active_to_terminated",
					Name = "Terminate",
					ApplicableStates = new List<ItemState> { ItemStates.activeState },
					ScriptToExecute = "Service_StateTransitions",
					PreviousState = ItemStates.activeState,
					NextState = ItemStates.terminatedState,
					IsHappyFlow = true,
				},
				new ButtonConfig
				{
					Id = "terminated_to_retired",
					Name = "Retire",
					ApplicableStates = new List<ItemState> { ItemStates.terminatedState },
					ScriptToExecute = "Service_StateTransitions",
					PreviousState = ItemStates.terminatedState,
					NextState = ItemStates.retiredState,
					IsHappyFlow = true,
				},
				new ButtonConfig
				{
					Id = "new_to_retired",
					Name = "Retire",
					ApplicableStates = new List<ItemState> { ItemStates.newState },
					ScriptToExecute = "Service_StateTransitions",
					PreviousState = ItemStates.newState,
					NextState = ItemStates.retiredState,
					IsHappyFlow = false,
				},
				new ButtonConfig
				{
					Id = "designed_to_retired",
					Name = "Retire",
					ApplicableStates = new List<ItemState> { ItemStates.designedState },
					ScriptToExecute = "Service_StateTransitions",
					PreviousState = ItemStates.designedState,
					NextState = ItemStates.retiredState,
					IsHappyFlow = false,
				},
				new ButtonConfig
				{
					Id = "reserved_to_retired",
					Name = "Retire",
					ApplicableStates = new List<ItemState> { ItemStates.reservedState },
					ScriptToExecute = "Service_StateTransitions",
					PreviousState = ItemStates.reservedState,
					NextState = ItemStates.retiredState,
					IsHappyFlow = false,
				},
				new ButtonConfig
				{
					Id = "terminated_to_active",
					Name = "Activate",
					ApplicableStates = new List<ItemState> { ItemStates.terminatedState },
					ScriptToExecute = "Service_StateTransitions",
					PreviousState = ItemStates.terminatedState,
					NextState = ItemStates.activeState,
					IsHappyFlow = false,
				},
			};
		}

		public static class ItemStates
		{
			public static readonly ItemState newState = new ItemState { Name = "New", NameId = "new", Id = SlcServicemanagementIds.Behaviors.Service_Behavior.Statuses.New };
			public static readonly ItemState designedState = new ItemState { Name = "Designed", NameId = "designed", Id = SlcServicemanagementIds.Behaviors.Service_Behavior.Statuses.Designed };
			public static readonly ItemState reservedState = new ItemState { Name = "Reserved", NameId = "reserved", Id = SlcServicemanagementIds.Behaviors.Service_Behavior.Statuses.Reserved };
			public static readonly ItemState activeState = new ItemState { Name = "Active", NameId = "active", Id = SlcServicemanagementIds.Behaviors.Service_Behavior.Statuses.Active };
			public static readonly ItemState terminatedState = new ItemState { Name = "Terminated", NameId = "terminated", Id = SlcServicemanagementIds.Behaviors.Service_Behavior.Statuses.Terminated };
			public static readonly ItemState retiredState = new ItemState { Name = "Retired", NameId = "Retired", Id = SlcServicemanagementIds.Behaviors.Service_Behavior.Statuses.Retired };
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
