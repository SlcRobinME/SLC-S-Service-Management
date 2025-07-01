namespace SLC_SM_IAS_ManageRelationships.Controller
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;

	internal class SRMBooking : IDefinitionObject
	{
		private readonly ServiceItemsSection _serviceItem;
		private readonly IList<ServiceItemRelationshipSection> _existingRelationships;

		public SRMBooking(ServiceItemsSection serviceItem, IList<ServiceItemRelationshipSection> existingRelationships)
		{
			_serviceItem = serviceItem;
			_existingRelationships = existingRelationships;
		}

		IEnumerable<NodesSection> IDefinitionObject.GetAvailableInputs() =>
			_existingRelationships.Any(r => r.ChildServiceItem == _serviceItem.ServiceItemID.ToString())
				? Enumerable.Empty<NodesSection>()
				: new[] { new NodesSection { NodeID = "0", NodeAlias = "Default SRM Input" } };

		IEnumerable<NodesSection> IDefinitionObject.GetAvailableOutputs() =>
			new[] { new NodesSection { NodeID = "1", NodeAlias = "Default SRM Output" } };

	}
}
