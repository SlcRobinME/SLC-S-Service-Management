namespace SLC_SM_IAS_Add_Service_Item.ScriptModels
{
	using System;

	public interface IScriptModel
	{
		Guid ID { get; set; }

		DateTime? Start { get; set; }

		DateTime? End { get; set; }
	}

	public class ScriptScriptModel : IScriptModel
	{
		public Guid ID { get; set; }

		public DateTime? Start { get; set; }

		public DateTime? End { get; set; }
	}
}