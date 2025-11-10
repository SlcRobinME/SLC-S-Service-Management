namespace SLC_SM_IAS_ManageRelationships.Controller
{
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	internal class WorkflowsInstanceAdapter : IDefinitionObject
	{
		private readonly Models.ServiceItem _serviceItem;
		private readonly WorkflowsInstance _instance;
		private readonly IList<Models.ServiceItemRelationShip> _existingItemRelationShips;

		internal WorkflowsInstanceAdapter(Models.ServiceItem serviceItem, WorkflowsInstance instance, IList<Models.ServiceItemRelationShip> existingItemRelationShips)
		{
			_instance = instance;
			_existingItemRelationShips = existingItemRelationShips;
			_serviceItem = serviceItem;
		}

		public IEnumerable<NodesSection> GetAvailableInputs()
		{
			var inputsInuse = new HashSet<string>(
				_existingItemRelationShips
					.Where(r => r.ChildServiceItem == _serviceItem.ID.ToString())
					.Select(r => r.ChildServiceItemInterfaceId));

			var availableInputs = _instance.Nodeses
				.Where(
					n =>
						n.NodeType == SlcWorkflowIds.Enums.Nodetype.Source &&
						!inputsInuse.Contains(n.NodeID));

			return availableInputs;
		}

		public IEnumerable<NodesSection> GetAvailableOutputs()
		{
			var availableOutputs = _instance.Nodeses
				.Where(
					n =>
						n.NodeType == SlcWorkflowIds.Enums.Nodetype.Destination
					/*&& !outputsInUse.Contains(n.NodeID)*/);

			return availableOutputs;
		}
	}
}