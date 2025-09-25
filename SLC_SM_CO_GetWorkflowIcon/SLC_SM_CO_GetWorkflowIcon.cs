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

20/06/2025	1.0.0.1		, Skyline	Initial version
****************************************************************************
*/

using System;
using System.Collections.Generic;
using System.Linq;
using DomHelpers.SlcProperties;
using DomHelpers.SlcWorkflow;
using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using Skyline.DataMiner.Net.Messages.SLDataGateway;

namespace SLCSMCOGetWorkflowIcon
{
	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "SLC_SM_CO_GetWorkflowIcon")]
	public class SLCSMCOGetWorkflowIcon : IGQIColumnOperator, IGQIRowOperator, IGQIOnInit, IGQIInputArguments
	{
		private readonly GQIStringColumn _iconColumn = new GQIStringColumn("Icon");

		private readonly GQIStringArgument _argWorkflowIdColumnName = new GQIStringArgument("Workflow ID Column Name") { IsRequired = true };
		private string _workflowIdColumnName = string.Empty;

		private GQIDMS _dms;
		private DomHelper _propertiesDomHelper;
		private DomHelper _wfDomHelper;

		private IEnumerable<PropertyValuesInstance> _propertyValues;

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[] { _argWorkflowIdColumnName };
		}

		public void HandleColumns(GQIEditableHeader header)
		{
			header.AddColumns(_iconColumn);
		}

		public void HandleRow(GQIEditableRow row)
		{
			var workflowId = Guid.Parse(row.GetValue(_workflowIdColumnName).ToString());

			var workflowsResult = _wfDomHelper.DomInstances
				.Read(DomInstanceExposers.Id.Equal(workflowId));

			var workflow = workflowsResult.FirstOrDefault();

			var icon = workflow != null
				? FetchWorkflowCategory(new WorkflowsInstance(workflow))
				: string.Empty;

			row.SetValue(_iconColumn, icon);
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			_workflowIdColumnName = args.GetArgumentValue(_argWorkflowIdColumnName);
			return new OnArgumentsProcessedOutputArgs();
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			_dms = args.DMS;
			_propertiesDomHelper = new DomHelper(_dms.SendMessages, SlcPropertiesIds.ModuleId);
			_wfDomHelper = new DomHelper(_dms.SendMessages, SlcWorkflowIds.ModuleId);

			_propertyValues = _propertiesDomHelper.DomInstances
				.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcPropertiesIds.Definitions.PropertyValues.Id))
				.Select(p => new PropertyValuesInstance(p));

			return default;
		}

		private string FetchWorkflowCategory(WorkflowsInstance workflow)
		{
			return _propertyValues
				.FirstOrDefault(p => p.PropertyValueInfo.LinkedObjectID == workflow.ID.Id.ToString())?
				.PropertyValues.FirstOrDefault(v => v.PropertyName == "Icon")?.Value ?? string.Empty;
		}
	}
}
