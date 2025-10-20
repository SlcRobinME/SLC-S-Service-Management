/*
****************************************************************************
*  Copyright (c) 2025,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

16/06/2025	1.0.0.1		RCA, Skyline	Initial version
****************************************************************************
*/

namespace SLCSMDSGetWorkflows
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcProperties;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "SLC_SM_DS_GetWorkflows")]
	public sealed class SLCSMDSGetWorkflows : IGQIDataSource
		, IGQIOnInit
	{
		private GQIDMS _dms;

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			// Initialize the data source
			// See: https://aka.dataminer.services/igqioninit-oninit
			_dms = args.DMS;

			return default;
		}

		public GQIColumn[] GetColumns()
		{
			// Define data source columns
			// See: https://aka.dataminer.services/igqidatasource-getcolumns
			return new GQIColumn[]
			{
				new GQIStringColumn("ID"),
				new GQIStringColumn("Name"),
				new GQIStringColumn("Category"),
				new GQIStringColumn("Type"),
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			var workflowsDomHelper = new DomHelper(_dms.SendMessages, SlcWorkflowIds.ModuleId);
			var workflowsResult = workflowsDomHelper.DomInstances
				.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcWorkflowIds.Definitions.Workflows.Id)
				.AND(DomInstanceExposers.StatusId.Equal(SlcWorkflowIds.Behaviors.Workflow_Behavior.Statuses.Complete)));

			if (workflowsResult == null)
				return ReturnEmptyResult();

			var workflowPropertyValues = new DomHelper(_dms.SendMessages, SlcPropertiesIds.ModuleId)
				.DomInstances
				.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcPropertiesIds.Definitions.PropertyValues.Id))
				.Select(p => new PropertyValuesInstance(p));

			var workflows = workflowsResult
				.Select(w => new WorkflowsInstance(w))
				.Select(wf => BuildRow(wf, FetchWorkflowCategory(wf, workflowPropertyValues)))
				.ToArray();

			var bookings = _dms.SendMessages(new GetLiteElementInfo { ProtocolName = "Skyline Booking Manager" })
				.OfType<LiteElementInfoEvent>()
				.ToArray();

			var getPropertyMessages = bookings
				.Select(b => new GetPropertyValueMessage
				{
					ObjectID = $"{b.DataMinerID}/{b.ElementID}",
					ObjectType = "Element",
					PropertyName = "Category",
				})
				.Cast<DMSMessage>()
				.ToArray();

			var bookingPropertyValues = _dms.SendMessages(getPropertyMessages)
				.OfType<PropertyChangeEventMessage>();

			var bookingRows = bookings
				.Select(b => BuildRow(b, FetchBookingCategory(b, bookingPropertyValues)))
				.ToArray();

			return new GQIPage(workflows.Concat(bookingRows).ToArray());
		}

		private string FetchBookingCategory(LiteElementInfoEvent liteElementInfoEvent, IEnumerable<PropertyChangeEventMessage> properties)
		{
			return properties
				.FirstOrDefault(p =>
					p.DataMinerID == liteElementInfoEvent.DataMinerID &&
					p.ElementID == liteElementInfoEvent.ElementID)?.Value ?? String.Empty;
		}

		private string FetchWorkflowCategory(WorkflowsInstance workflow, IEnumerable<PropertyValuesInstance> propertyValues)
		{
			return propertyValues
				.FirstOrDefault(p => p.PropertyValueInfo.LinkedObjectID == workflow.ID.Id.ToString())?
				.PropertyValues.FirstOrDefault(v => v.PropertyName == "Category")?.Value ?? String.Empty;
		}

		private GQIRow BuildRow(WorkflowsInstance workflow, string category)
		{
			return new GQIRow(
				new[]
				{
					new GQICell { Value = workflow.ID.Id.ToString() },
					new GQICell { Value = workflow.Name },
					new GQICell { Value = category },
					new GQICell { Value = "Workflow" },
				});
		}

		private GQIRow BuildRow(LiteElementInfoEvent liteElement, string category)
		{
			return new GQIRow(
				new[]
				{
					new GQICell { Value = $"{liteElement.DataMinerID}/{liteElement.ElementID}" },
					new GQICell { Value = liteElement.Name },
					new GQICell { Value = category },
					new GQICell { Value = "SRMBooking" },
				});
		}

		private GQIPage ReturnEmptyResult()
		{
			return new GQIPage(Array.Empty<GQIRow>())
			{
				HasNextPage = false,
			};
		}
	}
}