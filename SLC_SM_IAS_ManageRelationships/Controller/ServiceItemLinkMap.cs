namespace SLC_SM_IAS_ManageRelationships.Controller
{
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	public class ServiceItemLinkMap
	{
		public Models.ServiceItem SourceNode { get; set; }

		public Models.ServiceItem DestinationNode { get; set; }

		public IEnumerable<NodesSection> AvailableSources { get; set; }

		public IEnumerable<NodesSection> AvailableDestinations { get; set; }

		public List<Models.ServiceItemRelationShip> Links { get; set; }

		public bool HasSources => AvailableSources.Any();

		public bool HasDestinations => AvailableDestinations.Any();

		public bool HasSingleSourceInterface => AvailableSources.Count() == 1;

		public bool HasSingleDestinationInterface => AvailableDestinations.Count() == 1;

		public bool IsOneToOne => HasSingleSourceInterface && HasSingleDestinationInterface;

		public bool HasLink(NodesSection source, NodesSection destination)
		{
			return Links.Any(l => l.ParentServiceItemInterfaceId == source.NodeID && l.ChildServiceItemInterfaceId == destination.NodeID);
		}

		public void AddLink(string sourceInterface, string destinationInterface)
		{
			Links.Add(new Models.ServiceItemRelationShip
			{
				Type = "Connection",
				ParentServiceItem = SourceNode.ID.ToString(),
				ParentServiceItemInterfaceId = sourceInterface,
				ChildServiceItem = DestinationNode.ID.ToString(),
				ChildServiceItemInterfaceId = destinationInterface,
			});
		}

		public Models.ServiceItemRelationShip FindLinkBySource(string sourceInterface)
		{
			return Links.FirstOrDefault(l => l.ParentServiceItemInterfaceId == sourceInterface);
		}

		public void RemoveLink(Models.ServiceItemRelationShip link)
		{
			Links.Remove(link);
		}

		public void ClearLinks()
		{
			Links.Clear();
		}
	}
}
