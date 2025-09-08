namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS
{
	using Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Components;

	public static class AutomationData
	{
		public static readonly string InitialDropdownValue = "- Select -";

		public static Choice<T> CreateDefaultDropDownOption<T>()
		{
			return Choice.Create<T>(default, InitialDropdownValue);
		}
	}
}
