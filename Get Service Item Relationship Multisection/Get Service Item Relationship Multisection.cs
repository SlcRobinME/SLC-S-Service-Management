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

27/05/2025	1.0.0.1		RCA, Skyline	Initial version
****************************************************************************
*/

namespace GetServiceItemRelationshipMultisection
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "Get Service Item Relationship Multisection")]
	public sealed class GetServiceItemRelationshipMultisection : IGQIDataSource
		, IGQIOnInit
		, IGQIInputArguments
	{
		private readonly Dictionary<string, string> _workflowNameCache = new Dictionary<string, string>();
		private readonly Dictionary<(string, string), string> _nodeAliasCache = new Dictionary<(string, string), string>();

		private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = false };
		private Guid _specificationId;

		private DomInstance _domInstance;

		private DomHelper _smDomHelper;
		private DomHelper _wfDomHelper;
		private GQIDMS dms;

		private IEnumerable<WorkflowsInstance> _workflows;

		private Func<string, string> _getWorkflowName;

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			dms = args.DMS;

			return default;
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[]
			{
				domIdArg,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			if (!Guid.TryParse(args.GetArgumentValue(domIdArg), out _specificationId))
				_specificationId = Guid.Empty;

			return new OnArgumentsProcessedOutputArgs();
		}

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("ID"),
				new GQIStringColumn("Type"),
				new GQIStringColumn("Child"),
				new GQIStringColumn("Parent"),
				new GQIStringColumn("Child Interface"),
				new GQIStringColumn("Parent Interface"),
				new GQIStringColumn("Source"),
				new GQIStringColumn("Destination"),
				new GQIStringColumn("Source Interface"),
				new GQIStringColumn("Destination Interface"),
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			if (_specificationId == Guid.Empty)
				return EmptyPage();

			Init();

			if (_domInstance == null)
				return EmptyPage();

			var relationships = GetServiceRelationships();

			return new GQIPage(relationships
				.Where(r => !r.IsEmpty)
				.Select(BuildRow)
				.ToArray());
		}

		private void Init()
		{
			_smDomHelper = new DomHelper(dms.SendMessages, SlcServicemanagementIds.ModuleId);
			_wfDomHelper = new DomHelper(dms.SendMessages, SlcWorkflowIds.ModuleId);

			_domInstance = _smDomHelper.DomInstances
				.Read(DomInstanceExposers.Id.Equal(_specificationId))
				.FirstOrDefault();

			_workflows = _wfDomHelper.DomInstances
				.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcWorkflowIds.Definitions.Workflows.Id))
				.Select(w => new WorkflowsInstance(w));

			InitInstanceSpecific();
		}

		private void InitInstanceSpecific()
		{
			var defId = _domInstance.DomDefinitionId.Id;

			if (defId == SlcServicemanagementIds.Definitions.Services.Id)
			{
				var servicesInstance = new ServicesInstance(_domInstance);
				_getWorkflowName = serviceItem => servicesInstance.ServiceItems
					.FirstOrDefault(i => i.ServiceItemID.ToString() == serviceItem)?.DefinitionReference ?? string.Empty;
			}
			else if (defId == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
			{
				var serviceSpecsInstance = new ServiceSpecificationsInstance(_domInstance);
				_getWorkflowName = serviceItem => serviceSpecsInstance.ServiceItems
					.FirstOrDefault(i => i.ServiceItemID.ToString() == serviceItem)?.DefinitionReference ?? string.Empty;
			}
			else
			{
				_getWorkflowName = _ => string.Empty;
			}
		}

		private IList<ServiceItemRelationshipSection> GetServiceRelationships()
		{
			var defId = _domInstance.DomDefinitionId.Id;

			if (defId == SlcServicemanagementIds.Definitions.Services.Id)
				return new ServicesInstance(_domInstance).ServiceItemRelationship;

			if (defId == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
				return new ServiceSpecificationsInstance(_domInstance).ServiceItemRelationship;

			return new List<ServiceItemRelationshipSection>();
		}

		private GQIRow BuildRow(ServiceItemRelationshipSection r)
		{
			return new GQIRow(new[]
			{
				new GQICell { Value = r.ID.Id.ToString() },
				new GQICell { Value = r.Type },
				new GQICell { Value = r.ChildServiceItem },
				new GQICell { Value = r.ParentServiceItem },
				new GQICell { Value = r.ChildServiceItemInterfaceID },
				new GQICell { Value = r.ParentServiceItemInterfaceID },
				new GQICell { Value = GetCachedWorkflowName(r.ParentServiceItem) },
				new GQICell { Value = GetCachedWorkflowName(r.ChildServiceItem) },
				new GQICell { Value = GetCachedNodeAlias(r.ParentServiceItem, r.ParentServiceItemInterfaceID) },
				new GQICell { Value = GetCachedNodeAlias(r.ChildServiceItem, r.ChildServiceItemInterfaceID) },
			});
		}

		private string GetCachedWorkflowName(string serviceItem)
		{
			if (_workflowNameCache.TryGetValue(serviceItem, out var name))
				return name;

			name = _getWorkflowName(serviceItem);
			_workflowNameCache[serviceItem] = name;
			return name;
		}

		private string GetCachedNodeAlias(string serviceItem, string interfaceId)
		{
			var key = (serviceItem, interfaceId);
			if (_nodeAliasCache.TryGetValue(key, out var alias))
				return alias;

			var workflowName = GetCachedWorkflowName(serviceItem);
			var node = _workflows
				.FirstOrDefault(w => w.Name == workflowName)?.Nodes
				.FirstOrDefault(n => n.NodeID == interfaceId);

			alias = node?.NodeAlias ?? string.Empty;
			_nodeAliasCache[key] = alias;

			return alias;
		}

		private GQIPage EmptyPage()
		{
			return new GQIPage(Array.Empty<GQIRow>());
		}
	}
}
