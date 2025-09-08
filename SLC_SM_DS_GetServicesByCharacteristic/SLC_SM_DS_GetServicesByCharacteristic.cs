using System;
using System.Collections.Generic;
using DomHelpers.SlcConfigurations;
using Library;
using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
//using Skyline.DataMiner.ProjectApi.ServiceManagement;

namespace SLCSMDSGetServicesByCharacteristic
{
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

		DataHelpersServiceManagement _serviceHelper;

		private GQIDMS dms;

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

			//if (!Guid.TryParse(args.GetArgumentValue(domIdArg), out instanceDomId))
			//{
			//	instanceDomId = Guid.Empty;
			//}

			return new OnArgumentsProcessedOutputArgs();
		}

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			dms = args.DMS;

			_serviceHelper = new DataHelpersServiceManagement(args.DMS.GetConnection());

			return default;
		}

		private GQIRow[] BuildPage()
		{
			var rows = new List<GQIRow>();

			// if one of the input strings is not provided --> return empty list (can be changed to fetch all services that contain a service characteristic) 
			if (_serviceCharacteristic == null) {
			}
			else if (_serviceCharacteristicValue == null)
			{
				return Array.Empty<GQIRow>();
			} else
			{

			}

			var returnedServices = new List<Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement.Models.Service>();

			if (_serviceCharacteristic == null)
			{
				returnedServices = _serviceHelper.Services.Read();
			}
			else if (_serviceCharacteristicValue == null)
			{
				returnedServices = _serviceHelper.Services.GetServicesByCharacteristic(_serviceCharacteristic, null, null);
			}
			else
			{
				returnedServices = _serviceHelper.Services.GetServicesByCharacteristic(_serviceCharacteristic, null, _serviceCharacteristicValue);
			}

			foreach (var service in returnedServices)
			{
				BuildRow(service, rows);
			}

			return rows.ToArray();
		}

		private void BuildRow(Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement.Models.Service service, List<GQIRow> rows)
		{

			//var testId = Guid.Parse("1bb40d7f-cf16-4f26-8426-cb3e910322a5");

			var domInstanceId = new DomInstanceId(service.ID) { ModuleId = "(slc)servicemanagement"};

			// var domInstanceId = new DomInstanceId(testId);

			// domInstanceId.ModuleId = "(slc)servicemanagement"; 

			var objectRefMetadata = new ObjectRefMetadata { Object = domInstanceId };

			rows.Add(
				new GQIRow(
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
					})
				{ Metadata = new GenIfRowMetadata(new[] { objectRefMetadata } )}
				);
		}
	}
}
