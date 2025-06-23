namespace SLC_SM_IAS_ManageRelationships.Controller
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;

	public class ServiceItemLinkMap
	{
		public ServiceItemsSection SourceNode { get; set; }

		public ServiceItemsSection DestinationNode { get; set; }

		public IEnumerable<NodesSection> AvailableSources { get; set; }

		public IEnumerable<NodesSection> AvailableDestinations { get; set; }

		public List<ServiceItemRelationshipSection> Links { get; set; }

		public bool HasSources => AvailableSources.Any();

		public bool HasDestinations => AvailableDestinations.Any();

		public bool HasSingleSourceInterface => AvailableSources.Count() == 1;

		public bool HasSingleDestinationInterface => AvailableDestinations.Count() == 1;

		public bool IsOneToOne => HasSingleSourceInterface && HasSingleDestinationInterface;

		public bool HasLink(NodesSection source, NodesSection destination)
		{
			return Links.Any(l => l.ParentServiceItemInterfaceID == source.NodeID && l.ChildServiceItemInterfaceID == destination.NodeID);
		}

		public void AddLink(string sourceInterface, string destinationInterface)
		{
			Links.Add(new ServiceItemRelationshipSection
			{
				Type = "Connection",
				ParentServiceItem = SourceNode.ServiceItemID.ToString(),
				ParentServiceItemInterfaceID = sourceInterface,
				ChildServiceItem = DestinationNode.ServiceItemID.ToString(),
				ChildServiceItemInterfaceID = destinationInterface,
			});
		}

		public ServiceItemRelationshipSection FindLinkBySource(string sourceInterface)
		{
			return Links.FirstOrDefault(l => l.ParentServiceItemInterfaceID == sourceInterface);
		}

		public void RemoveLink(ServiceItemRelationshipSection link)
		{
			Links.Remove(link);
		}

		public void ClearLinks()
		{
			Links.Clear();
		}
	}
}
