namespace SLC_SM_IAS_Profiles.Views
{
	using System;
	using DomHelpers.SlcConfigurations;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public abstract class Row
	{
		protected Row(RowData data)
		{
			Data = data;
		}

		public RowData Data { get; set; }

		public abstract Row Configure();

		public abstract void BuildRow(Dialog view);
	}

	public class RowFactory
	{
		public static Row Create(RowData data)
		{
			if (data is ConfigurationRowData)
			{
				var rowData = (ConfigurationRowData)data;
				var configurationRecord = rowData.Record;
				switch (configurationRecord.ConfigurationParameterValue.Type)
				{
					case SlcConfigurationsIds.Enums.Type.Number:
						{
							return new NumberRow(rowData);
						}

					case SlcConfigurationsIds.Enums.Type.Text:
						{
							return new TextRow(rowData);
						}

					case SlcConfigurationsIds.Enums.Type.Discrete:
						{
							return new DiscreteRow(rowData);
						}

					default:
						throw new NotSupportedException($"Configuration parameter type: {configurationRecord.ConfigurationParameterValue.Type} is not supported");
				}
			}
			else if (data is ProfileRowData)
			{
				var rowData = (ProfileRowData)data;
				return new ProfileRow(rowData);
			}
			else
			{
				throw new NotSupportedException($"Row data of type {data.GetType().Name} is not supported");
			}
		}
	}
}
