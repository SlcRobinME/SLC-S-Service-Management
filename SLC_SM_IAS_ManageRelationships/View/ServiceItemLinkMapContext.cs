namespace SLC_SM_IAS_ManageRelationships.View
{
	using SLC_SM_IAS_ManageRelationships.Controller;

	public class ServiceItemLinkMapContext
	{
		public ServiceItemLinkMap Pair { get; set;  }

		public bool ShowPrevious { get; set; }

		public bool ShowNext { get; set; }
	}
}
