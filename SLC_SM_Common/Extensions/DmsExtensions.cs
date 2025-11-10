namespace SLC_SM_Common.Extensions
{
	using Skyline.DataMiner.Core.DataMinerSystem.Common;

	public static class DmsExtensions
	{
		public static bool ServiceExistsSafe(this IDms dms, string serviceName, out IDmsService service)
		{
			try
			{
				service = dms.GetService(serviceName);
				return true;
			}
			catch
			{
				service = default;
				return false;
			}
		}
	}
}
