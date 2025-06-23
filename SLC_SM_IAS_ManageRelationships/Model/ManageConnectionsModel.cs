namespace SLC_SM_IAS_ManageRelationships.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers;
	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using SLC_SM_IAS_ManageRelationships.Controller;

	internal class ManageConnectionsModel
	{
		private readonly IEngine _engine;
		private readonly DomHelper _smDomHelper;
		private readonly DomHelper _wfDomHelper;

		public ManageConnectionsModel(IEngine engine)
		{
			_engine = engine;
			_smDomHelper = new DomHelper(_engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
			_wfDomHelper = new DomHelper(_engine.SendSLNetMessages, SlcWorkflowIds.ModuleId);
		}

		public DomInstanceBase DomInstance { get; set; }

		public WorkflowsInstance GetWorkflowbyId(Guid workflowId)
		{
			var domInstance = _wfDomHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(workflowId)).FirstOrDefault();
			if (domInstance == null)
				throw new Exception($"Could not find workflow with id {workflowId}");

			return new WorkflowsInstance(domInstance);
		}

		public WorkflowsInstance GetWorkflowbyName(string workflowName)
		{
			var domInstance = _wfDomHelper.DomInstances.Read(DomInstanceExposers.Name.Equal(workflowName)).FirstOrDefault();
			if (domInstance == null)
				throw new Exception($"Could not find workflow with id {workflowName}");

			return new WorkflowsInstance(domInstance);
		}

		public IEnumerable<NodesSection> GetAvailableOutputs(ServiceItemsSection source, WorkflowsInstance workflow)
		{
			var relationships = GetRelationships(DomInstance);

			//var outputsInUse = new HashSet<string>(
			//	relationships
			//		.Where(r => r.ParentServiceItem == source.ServiceItemID.ToString())
			//		.Select(r => r.ParentServiceItemInterfaceID));

			var availableOutputs = workflow.Nodes
				.Where(n =>
					n.NodeType == SlcWorkflowIds.Enums.Nodetype.Destination
					/*&& !outputsInUse.Contains(n.NodeID)*/);

			return availableOutputs;
		}

		public IEnumerable<NodesSection> GetAvailableInputs(ServiceItemsSection destination, WorkflowsInstance workflow)
		{
			var relationships = GetRelationships(DomInstance);

			var inputsInuse = new HashSet<string>(
				relationships
					.Where(r => r.ChildServiceItem == destination.ServiceItemID.ToString())
					.Select(r => r.ChildServiceItemInterfaceID));

			var availableInputs = workflow.Nodes
				.Where(n =>
					n.NodeType == SlcWorkflowIds.Enums.Nodetype.Source &&
					!inputsInuse.Contains(n.NodeID));

			return availableInputs;
		}

		/// <summary>
		/// Turns a list of Service Items (A,B,C,D) to a list of pairs (A,B), (B,C), (C,D).
		/// This allows building the relationships between Service Items in the same order the user selected them.
		/// </summary>
		/// <param name="source">A list of Service Items to be connected in sequence.</param>
		/// <returns>A list of Service Item pairs between which a relationship will be built. </returns>
		public IEnumerable<(ServiceItemsSection, ServiceItemsSection)> ToSequentialPairs(IEnumerable<ServiceItemsSection> source)
		{
			using (var enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
					yield break;

				var previous = enumerator.Current;
				while (enumerator.MoveNext())
				{
					yield return (previous, enumerator.Current);
					previous = enumerator.Current;
				}
			}
		}

		public List<ServiceItemRelationshipSection> FindRelationshipsBetweenPair(
			DomInstanceBase domInstance,
			(ServiceItemsSection, ServiceItemsSection) pair)
		{
			var relationships = GetRelationships(domInstance);
			var parentId = pair.Item1.ServiceItemID.ToString();
			var childId = pair.Item2.ServiceItemID.ToString();

			return relationships.Where(r =>
				r.ParentServiceItem == parentId &&
				r.ChildServiceItem == childId).ToList();
		}

		public DomInstanceBase GetDomInstance(Guid domId)
		{
			var instance = _smDomHelper.DomInstances
				.Read(DomInstanceExposers.Id.Equal(domId))
				.FirstOrDefault();

			if (instance == null)
				throw new Exception($"Could not find the DOM instance with id {domId}");

			DomInstance = CreateTypedDomInstance(instance);
			return DomInstance;
		}

		public IEnumerable<ServiceItemsSection> GetServiceItems(
			DomInstanceBase domInstance,
			IEnumerable<string> serviceItemIds)
		{
			var items = GetServiceItemList(domInstance);
			return GetServiceItemsFromList(items, serviceItemIds);
		}

		public void Update(List<ServiceItemLinkMap> linkMap)
		{
			var relationships = GetRelationshipsMutable(DomInstance);

			foreach (var link in linkMap.SelectMany(pair => pair.Links))
			{
				var existing = relationships
					.FirstOrDefault(r => r.ParentServiceItem == link.ParentServiceItem &&
					r.ChildServiceItem == link.ChildServiceItem &&
					r.ParentServiceItemInterfaceID == link.ParentServiceItemInterfaceID);

				if (existing != null)
					relationships.Remove(existing);

				if (!string.IsNullOrEmpty(link.ChildServiceItemInterfaceID))
					relationships.Add(link);
			}

			DomInstance.Save(_smDomHelper);
		}

		public string CreateServiceItemFromWorkflow(Guid domId, string workflowName)
		{
			var addServiceItemScript = _engine.PrepareSubScript("SLC_SM_AS_AddServiceItem");
			addServiceItemScript.SelectScriptParam("DOM ID", $"[\"{domId}\"]");
			addServiceItemScript.SelectScriptParam("ServiceItemType", "Workflow");
			addServiceItemScript.SelectScriptParam("DefinitionReference", workflowName);
			addServiceItemScript.Synchronous = true;
			addServiceItemScript.InheritScriptOutput = true;
			addServiceItemScript.StartScript();

			if (addServiceItemScript.HadError)
				throw new Exception($"Error creating the service item:{addServiceItemScript.GetErrorMessages()}");

			return _engine.GetScriptOutput("ServiceItemId");
		}

		private DomInstanceBase CreateTypedDomInstance(DomInstance domInstance)
		{
			if (IsServicesInstance(domInstance))
				return new ServicesInstance(domInstance);

			if (IsServiceSpecificationsInstance(domInstance))
				return new ServiceSpecificationsInstance(domInstance);

			throw new NotSupportedException($"Unsupported DOM definition ID: {domInstance.DomDefinitionId.Id}");
		}

		private bool IsServicesInstance(DomInstance domInstance)
		{
			return domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id;
		}

		private bool IsServiceSpecificationsInstance(DomInstance domInstance)
		{
			return domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id;
		}

		private IEnumerable<ServiceItemsSection> GetServiceItemList(DomInstanceBase instance)
		{
			var services = instance as ServicesInstance;
			if (services != null)
				return services.ServiceItems;

			var specs = instance as ServiceSpecificationsInstance;
			if (specs != null)
				return specs.ServiceItems;

			throw new NotSupportedException($"Unsupported DomInstance type: {instance.GetType().Name}");
		}

		private IEnumerable<ServiceItemsSection> GetServiceItemsFromList(
			IEnumerable<ServiceItemsSection> serviceItems,
			IEnumerable<string> serviceItemIds)
		{
			var itemsById = serviceItems
				.Where(n => n.ServiceItemID.HasValue)
				.ToDictionary(n => n.ServiceItemID.Value.ToString());

			return serviceItemIds
				.Select(id => itemsById.TryGetValue(id, out var item) ? item : null)
				.Where(item => item != null);
		}

		private IList<ServiceItemRelationshipSection> GetRelationshipsMutable(DomInstanceBase domInstance)
		{
			if (domInstance is ServicesInstance services)
				return services.ServiceItemRelationship;

			if (domInstance is ServiceSpecificationsInstance specs)
				return specs.ServiceItemRelationship;

			throw new InvalidOperationException("Unsupported DomInstance type.");
		}

		private IList<ServiceItemRelationshipSection> GetRelationships(DomInstanceBase domInstance)
		{
			return GetRelationshipsMutable(domInstance).ToList(); // Copy
		}
	}
}