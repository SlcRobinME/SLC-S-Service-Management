namespace SLC_SM_IAS_ManageRelationships.Controller
{
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;

	internal class WorkflowsInstanceAdapter : IDefinitionObject
	{
		private readonly ServiceItemsSection _serviceItem;
		private readonly WorkflowsInstance _instance;
		private readonly IList<ServiceItemRelationshipSection> _existingRelationships;

		internal WorkflowsInstanceAdapter(ServiceItemsSection serviceItem, WorkflowsInstance instance, IList<ServiceItemRelationshipSection> existingRelationships)
		{
			_instance = instance;
			_serviceItem = serviceItem;
			_existingRelationships = existingRelationships;
		}

		public IEnumerable<NodesSection> GetAvailableInputs()
		{
			var inputsInuse = new HashSet<string>(
				_existingRelationships
					.Where(r => r.ChildServiceItem == _serviceItem.ServiceItemID.ToString())
					.Select(r => r.ChildServiceItemInterfaceID));

			var availableInputs = _instance.Nodeses
				.Where(n =>
					n.NodeType == SlcWorkflowIds.Enums.Nodetype.Source &&
					!inputsInuse.Contains(n.NodeID));

			return availableInputs;
		}

		public IEnumerable<NodesSection> GetAvailableOutputs()
		{
			var availableOutputs = _instance.Nodeses
				.Where(n =>
					n.NodeType == SlcWorkflowIds.Enums.Nodetype.Destination
					/*&& !outputsInUse.Contains(n.NodeID)*/);

			return availableOutputs;
		}
	}

}
