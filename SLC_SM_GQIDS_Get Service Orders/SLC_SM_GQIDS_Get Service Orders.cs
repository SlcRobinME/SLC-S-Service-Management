namespace SLC_SM_GQIDS_Get_Service_Orders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using SLC_SM_Common.Extensions;
	using Models = Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement.Models;

	// Required to mark the interface as a GQI data source
	[GQIMetaData(Name = "Get_ServiceOrders")]
	public class GetServiceOrders : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		// defining input argument, will be converted to guid by OnArgumentsProcessed
		private GQIDMS _dms;

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("Order ID"),
				new GQIStringColumn("Name"),
				new GQIStringColumn("Description"),
				new GQIStringColumn("Priority"),
				new GQIStringColumn("External ID"),
				new GQIStringColumn("Related Organization"),
				new GQIStringColumn("State"),
			};
		}

		public GQIArgument[] GetInputArguments()
		{
			return Array.Empty<GQIArgument>();
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			try
			{
				return new GQIPage(GetMultiSection())
				{
					HasNextPage = false,
				};
			}
			catch (Exception e)
			{
				_dms.GenerateInformationMessage("GQIDS|Get Service Orders Exception: " + e);
				return new GQIPage(Enumerable.Empty<GQIRow>().ToArray());
			}
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			return new OnArgumentsProcessedOutputArgs();
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			_dms = args.DMS;

			return default;
		}

		private GQIRow BuildRow(Models.ServiceOrder item, List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.Organization> organizations)
		{
			return new GQIRow(
				item.ID.ToString(),
				new[]
				{
					new GQICell { Value = item.ID.ToString() },
					new GQICell { Value = item.Name ?? String.Empty },
					new GQICell { Value = item.Description ?? String.Empty },
					new GQICell { Value = item.Priority?.ToString() ?? "Low" },
					new GQICell { Value = item.ExternalID ?? String.Empty },
					new GQICell { Value = item.OrganizationId.HasValue ? organizations.Find(x => x.ID == item.OrganizationId)?.Name ?? String.Empty : String.Empty },
					new GQICell { Value = item.Status.ToString() },
				});
		}

		private GQIRow[] GetMultiSection()
		{
			IConnection connection = _dms.GetConnection();
			List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.PeopleAndOrganization.Models.Organization> organizations = new DataHelperOrganization(connection).Read();

			var instances = new DataHelperServiceOrder(connection).Read();
			return instances.Select(item => BuildRow(item, organizations)).ToArray();
		}
	}
}