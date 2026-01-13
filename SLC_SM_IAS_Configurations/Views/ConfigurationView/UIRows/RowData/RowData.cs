namespace SLC_SM_IAS_Profiles.Views
{
	using System.Collections.Generic;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using SLC_SM_IAS_Profiles.Presenters;
	using SLC_SM_IAS_Profiles.Presenters;

	public abstract class RowData
	{
		protected RowData(
			DataRecordPage page,
			IEnumerable<Option<Models.ConfigurationUnit>> cachedUnits,
			EventHandlers callbacks,
			int rowIndex)
		{
			Page = page;
			CachedUnits = cachedUnits;
			Callbacks = callbacks;
			RowIndex = rowIndex;
		}

		public DataRecordPage Page { get; set; }

		public DataRecord Record { get; set; }

		public IEnumerable<Option<Models.ConfigurationUnit>> CachedUnits { get; set; }

		public EventHandlers Callbacks { get; set; }

		public int RowIndex { get; set; }

		public bool InRootPage => Page is RootPage;
	}

	public class ProfileDefinitionRowData : RowData
	{
		public ProfileDefinitionRowData(
			ProfileDefinitionDataRecord record,
			DataRecordPage page,
			IEnumerable<Option<Models.ConfigurationUnit>> cachedUnits,
			IEnumerable<Models.ProfileDefinition> referenceOptions,
			EventHandlers callbacks,
			int rowIndex)
			: base(page, cachedUnits, callbacks, rowIndex)
		{
			base.Record = record;
			ReferenceOptions = referenceOptions;
		}

		public new ProfileDefinitionDataRecord Record => base.Record as ProfileDefinitionDataRecord;

		public IEnumerable<Models.ProfileDefinition> ReferenceOptions { get; }
	}

	public class ConfigurationRowData : RowData
	{
		public ConfigurationRowData(
			ConfigurationDataRecord record,
			DataRecordPage page,
			IEnumerable<Option<Models.ConfigurationUnit>> cachedUnits,
			IEnumerable<Models.ConfigurationParameter> referenceOptions,
			EventHandlers callbacks,
			int rowIndex)
			: base(page, cachedUnits, callbacks, rowIndex)
		{
			base.Record = record;
			ReferenceOptions = referenceOptions;
		}

		public new ConfigurationDataRecord Record => base.Record as ConfigurationDataRecord;

		public IEnumerable<Models.ConfigurationParameter> ReferenceOptions { get; }
	}
}
