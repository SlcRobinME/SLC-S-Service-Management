using System;
using System.Collections.Generic;
using DomHelpers.SlcServicemanagement;
using Skyline.DataMiner.Analytics.GenericInterface;

namespace SLCSMDSGetServiceButtons
{
	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "SLC-SM-DS-Get Service Buttons")]
	public sealed class SLCSMDSGetServiceButtons : IGQIDataSource
		, IGQIOnInit
		, IGQIInputArguments
		, IGQIOptimizableDataSource
	{
		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			// Initialize the data source
			// See: https://aka.dataminer.services/igqioninit-oninit
			return default;
		}

		public GQIArgument[] GetInputArguments()
		{
			// Define data source input arguments
			// See: https://aka.dataminer.services/igqiinputarguments-getinputarguments
			return Array.Empty<GQIArgument>();
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			// Process input argument values
			// See: https://aka.dataminer.services/igqiinputarguments-onargumentsprocessed
			return default;
		}

		public GQIColumn[] GetColumns()
		{
			// Define data source columns
			// See: https://aka.dataminer.services/igqidatasource-getcolumns
			return Array.Empty<GQIColumn>();
		}

		public IGQIQueryNode Optimize(IGQIDataSourceNode currentNode, IGQICoreOperator nextOperator)
		{
			// Inspect, optimize or customize behavior for applied operators
			// See: https://aka.dataminer.services/igqioptimizabledatasource-optimize
			return currentNode.Append(nextOperator);
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			// Define data source rows
			// See: https://aka.dataminer.services/igqidatasource-getnextpage
			return new GQIPage(Array.Empty<GQIRow>())
			{
				HasNextPage = false,
			};
		}

		public static class ItemStates
		{
			public static readonly ItemState designedState = new ItemState { Name = "Designed", NameId = "designed", Id = SlcServicemanagementIds.Behaviors.Service_Behavior.Statuses.Designed };
			public static readonly ItemState reservedState = new ItemState { Name = "Reserved", NameId = "reserved", Id = SlcServicemanagementIds.Behaviors.Service_Behavior.Statuses.Reserved };
			public static readonly ItemState activeState = new ItemState { Name = "Active", NameId = "active", Id = SlcServicemanagementIds.Behaviors.Service_Behavior.Statuses.Active };
			public static readonly ItemState terminatedState = new ItemState { Name = "Terminated", NameId = "terminated", Id = SlcServicemanagementIds.Behaviors.Service_Behavior.Statuses.Teminated };
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
