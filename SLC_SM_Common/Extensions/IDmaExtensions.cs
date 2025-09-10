namespace SLC_SM_Common.Extensions
{
	using Skyline.DataMiner.Core.DataMinerSystem.Common;

	public static class DmaExtensions
	{
		public static bool ServiceExistsSafe(this IDma agent, string serviceName)
		{
			try
			{
				return agent.ServiceExists(serviceName);
			}
			catch
			{
				return false;
			}
		}
	}
}
