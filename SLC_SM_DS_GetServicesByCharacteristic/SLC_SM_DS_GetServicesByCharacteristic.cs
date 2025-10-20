namespace SLCSMDSGetServicesByCharacteristic
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Library;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;

	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "SLC_SM_DS_GetServicesByCharacteristic")]
	public sealed class SLCSMDSGetServicesByCharacteristic : IGQIDataSource, IGQIInputArguments, IGQIOnInit
	{
		private readonly GQIStringArgument serviceCharacteristicArg = new GQIStringArgument("Service Characteristic") { IsRequired = true };
		private readonly GQIStringArgument serviceCharacteristicValueArg = new GQIStringArgument("Service Characteristic Value") { IsRequired = true };

		private string _serviceCharacteristic;
		private string _serviceCharacteristicValue;

		private DataHelpersServiceManagement _serviceHelper;

		private GQIDMS gqiDms;
		private IConnection _connection;
		private IDms _dms;
		private IDma _agent;

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
			// Define data source rows
			// See: https://aka.dataminer.services/igqidatasource-getnextpage
			return new GQIPage(BuildPage())
			{
				HasNextPage = false,
			};
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
			gqiDms = args.DMS;

			_connection = gqiDms.GetConnection();
			_dms = _connection.GetDms();
			_agent = _dms.GetAgents().FirstOrDefault();
			if (_agent == null)
			{
				throw new InvalidOperationException("This operation is supported only on single agent dataminer systems");
			}

			_serviceHelper = new DataHelpersServiceManagement(args.DMS.GetConnection());

			return default;
		}

		private GQIRow[] BuildPage()
		{
			List<Models.Service> returnedServices;

			if (_serviceCharacteristic == null && _serviceCharacteristicValue == null)
			{
				// fetch all
				returnedServices = _serviceHelper.Services.Read();
			}
			else if (_serviceCharacteristic != null && _serviceCharacteristicValue == null)
			{
				// characteristc provided but no value
				return Array.Empty<GQIRow>();
			}
			else
			{
				// both provided
				returnedServices = _serviceHelper.Services.GetServicesByCharacteristic(_serviceCharacteristic, null, _serviceCharacteristicValue);
			}

			return returnedServices.Select(BuildRow).ToArray();
		}

		private GQIRow BuildRow(Models.Service service)
		{

			var domInstanceId = new DomInstanceId(service.ID) { ModuleId = "(slc)servicemanagement" };
			var objectRefMetadata = new ObjectRefMetadata { Object = domInstanceId };

			var alarmLevel = TryGetAlarmLevel(service);

			return new GQIRow(
				new[]
				{
						new GQICell { Value = domInstanceId?.Id.ToString() ?? String.Empty},
						new GQICell { Value = service.ServiceID ?? String.Empty},
						new GQICell { Value = service.Name ?? String.Empty },
						new GQICell { Value = service.StartTime?.ToUniversalTime() },
						new GQICell { Value = service.EndTime?.ToUniversalTime() },
						new GQICell { Value = service.Category?.Name ?? String.Empty },
						new GQICell { Value = service.Icon ?? String.Empty },
						new GQICell { Value = service.ServiceSpecificationId?.ToString() ?? String.Empty },
						new GQICell { Value = (int) alarmLevel },
				})
			{ Metadata = new GenIfRowMetadata(new[] { objectRefMetadata }) };
		}

		private AlarmLevel TryGetAlarmLevel(Models.Service service)
		{
			if (_agent.ServiceExistsSafe(service.Name))
			{
				return _agent.GetService(service.Name).GetState().Level;
			}

			return AlarmLevel.Undefined;
		}
	}

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
