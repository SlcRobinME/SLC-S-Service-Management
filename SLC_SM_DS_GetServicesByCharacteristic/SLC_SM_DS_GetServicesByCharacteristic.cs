namespace SLCSMDSGetServicesByCharacteristic
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using SLC_SM_Common.Extensions;

	/// <summary>
	///     Represents a data source.
	///     See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = DataSourceName)]
	public sealed class SLCSMDSGetServicesByCharacteristic : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		private const string DataSourceName = "SLC_SM_DS_GetServicesByCharacteristic";
		private readonly GQIStringArgument serviceCharacteristicArg = new GQIStringArgument("Service Characteristic") { IsRequired = true };
		private readonly GQIStringArgument serviceCharacteristicValueArg = new GQIStringArgument("Service Characteristic Value") { IsRequired = true };
		private string _serviceCharacteristic;
		private string _serviceCharacteristicValue;
		private DataHelpersServiceManagement _serviceHelper;
		private GQIDMS _gqiDms;
		private IDms _dms;
		private IGQILogger _logger;

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[]
			{
				new GQIStringColumn("DOM ID"),
				new GQIStringColumn("Service ID"),
				new GQIStringColumn("Service Name"),
				new GQIDateTimeColumn("Service Start"),
				new GQIDateTimeColumn("Service End"),
				new GQIStringColumn("Service Category"),
				new GQIStringColumn("Service Logo"),
				new GQIStringColumn("Service Specification"),
				new GQIIntColumn("Alarm Level"),
			};
		}

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[]
			{
				serviceCharacteristicArg,
				serviceCharacteristicValueArg,
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			return _logger.PerformanceLogger(nameof(GetNextPage), BuildupRows);
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			// adds the input argument to private variable
			_serviceCharacteristic = args.GetArgumentValue(serviceCharacteristicArg);
			_serviceCharacteristicValue = args.GetArgumentValue(serviceCharacteristicValueArg);

			return new OnArgumentsProcessedOutputArgs();
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			_gqiDms = args.DMS;
			_logger = args.Logger;
			_logger.MinimumLogLevel = GQILogLevel.Debug;

			IConnection connection = _gqiDms.GetConnection();
			_dms = connection.GetDms();
			_serviceHelper = new DataHelpersServiceManagement(connection);

			return default;
		}

		private GQIRow[] BuildPage()
		{
			if (_serviceCharacteristic != null && _serviceCharacteristicValue == null)
			{
				// characteristic provided but no value
				return Array.Empty<GQIRow>();
			}

			List<Models.Service> returnedServices = _logger.PerformanceLogger(
				"Get Services For Characteristic",
				() =>
					_serviceCharacteristic == null && _serviceCharacteristicValue == null
						? _serviceHelper.Services.Read() // fetch all
						: _serviceHelper.Services.GetServicesByCharacteristic(_serviceCharacteristic, null, _serviceCharacteristicValue));

			return returnedServices.Select(BuildRow).ToArray();
		}

		private GQIRow BuildRow(Models.Service service)
		{
			var domInstanceId = new DomInstanceId(service.ID) { ModuleId = SlcServicemanagementIds.ModuleId };
			var objectRefMetadata = new ObjectRefMetadata { Object = domInstanceId };

			var alarmLevel = _logger.PerformanceLogger("Get Alarm Level", () => TryGetAlarmLevel(service));

			return new GQIRow(
					new[]
					{
						new GQICell { Value = service.ID.ToString() },
						new GQICell { Value = service.ServiceID ?? String.Empty },
						new GQICell { Value = service.Name ?? String.Empty },
						new GQICell { Value = service.StartTime?.ToUniversalTime() },
						new GQICell { Value = service.EndTime?.ToUniversalTime() },
						new GQICell { Value = service.Category?.Name ?? String.Empty },
						new GQICell { Value = service.Icon ?? String.Empty },
						new GQICell { Value = service.ServiceSpecificationId?.ToString() ?? String.Empty },
						new GQICell { Value = (int)alarmLevel },
					})
			{ Metadata = new GenIfRowMetadata(new[] { objectRefMetadata }) };
		}

		private GQIPage BuildupRows()
		{
			try
			{
				return new GQIPage(BuildPage())
				{
					HasNextPage = false,
				};
			}
			catch (Exception e)
			{
				_gqiDms.GenerateInformationMessage($"GQIDS|{nameof(DataSourceName)}|Exception: {e}");
				_logger.Error($"GQIDS|{nameof(DataSourceName)}|Exception: {e}");
				return new GQIPage(Enumerable.Empty<GQIRow>().ToArray());
			}
		}

		private AlarmLevel TryGetAlarmLevel(Models.Service service)
		{
			if (_dms.ServiceExistsSafe(service.Name, out IDmsService srv))
			{
				return srv.GetState().Level;
			}

			return AlarmLevel.Undefined;
		}
	}
}