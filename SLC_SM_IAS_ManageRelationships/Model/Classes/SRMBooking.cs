namespace SLC_SM_IAS_ManageRelationships.Controller
{
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	internal class SRMBooking : IDefinitionObject
	{
		private readonly IList<Models.ServiceItemRelationShip> _existingRelationships;
		private readonly Models.ServiceItem _serviceItem;

		public SRMBooking(Models.ServiceItem serviceItem, IList<Models.ServiceItemRelationShip> existingRelationships)
		{
			_serviceItem = serviceItem;
			_existingRelationships = existingRelationships;
		}

		IEnumerable<NodesSection> IDefinitionObject.GetAvailableInputs()
		{
			return _existingRelationships.Any(r => r.ChildServiceItem == _serviceItem.ID.ToString())
				? Enumerable.Empty<NodesSection>()
				: new[] { new NodesSection { NodeID = "0", NodeAlias = "Default SRM Input" } };
		}

		IEnumerable<NodesSection> IDefinitionObject.GetAvailableOutputs()
		{
			return new[] { new NodesSection { NodeID = "1", NodeAlias = "Default SRM Output" } };
		}
	}
}