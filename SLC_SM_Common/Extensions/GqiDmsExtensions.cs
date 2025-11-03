namespace SLC_SM_Common.Extensions
{
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Messages;

	public static class GqiDmsExtensions
	{
		public static void GenerateInformationMessage(this GQIDMS dms, string text)
		{
			dms.SendMessage(new GenerateAlarmMessage(GenerateAlarmMessage.AlarmSeverity.Information, text) { Status = GenerateAlarmMessage.AlarmStatus.Cleared });
		}
	}
}