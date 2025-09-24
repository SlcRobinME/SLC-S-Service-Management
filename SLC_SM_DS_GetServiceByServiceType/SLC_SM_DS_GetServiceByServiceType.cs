/*
****************************************************************************
*  Copyright (c) 2025,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

11/09/2025	1.0.0.1		RCA, Skyline	Initial version
****************************************************************************
*/

namespace SLCSMDSGetServiceByServiceType
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcConfigurations;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using static DomHelpers.SlcConfigurations.SlcConfigurationsIds.Sections;

	/// <summary>
	/// Represents a data source.
	/// See: https://aka.dataminer.services/gqi-external-data-source for a complete example.
	/// </summary>
	[GQIMetaData(Name = "SLC_SM_DS_GetServiceByServiceType")]
	public sealed class SLCSMDSGetServiceByServiceType : IGQIDataSource, IGQIOnInit, IGQIInputArguments
	{
		private readonly Arguments _arguments = new Arguments();
		private DomHelper _serviceMangerDomHelper;
		private DomHelper _configurationDomHelper;
		private IGQILogger _logger;
		private Skyline.DataMiner.Net.IConnection _connection;
		private IDms _dms;
		private IDma _agent;

		public OnInitOutputArgs OnInit(OnInitInputArgs args)
		{
			// Initialize the data source
			// See: https://aka.dataminer.services/igqioninit-oninit
			_serviceMangerDomHelper = new DomHelper(args.DMS.SendMessages, SlcServicemanagementIds.ModuleId);
			_configurationDomHelper = new DomHelper(args.DMS.SendMessages, SlcConfigurationsIds.ModuleId);
			_logger = args.Logger;

			var gqiDms = args.DMS;
			_connection = gqiDms.GetConnection();
			_dms = _connection.GetDms();
			_agent = _dms.GetAgents().SingleOrDefault();
			if (_agent == null)
			{
				throw new InvalidOperationException("This operation is supported only on single agent dataminer systems");
			}

			return new OnInitOutputArgs();
		}

		public GQIArgument[] GetInputArguments()
		{
			// Define data source input arguments
			// See: https://aka.dataminer.services/igqiinputarguments-getinputarguments
			return _arguments.GetInputArguments();
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			// Process input argument values
			// See: https://aka.dataminer.services/igqiinputarguments-onargumentsprocessed
			return _arguments.OnArgumentsProcessed(args);
		}

		public GQIColumn[] GetColumns()
		{
			// Define data source columns
			// See: https://aka.dataminer.services/igqidatasource-getcolumns
			return new GQIColumn[]
			{
				new GQIStringColumn("Service Id"),
				new GQIStringColumn("Service Name"),
				new GQIStringColumn("Icon"),
				new GQIStringColumn("Status"),
				new GQIIntColumn("Alarm Level"),
				new GQIStringColumn("Service Type"),
				new GQIStringColumn("Reception Type"),
				new GQIStringColumn("Channel ID"),
				new GQIStringColumn("Video Format"),
				new GQIStringColumn("Distribution Type"),
				new GQIStringColumn("Region"),
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			// Define data source rows
			// See: https://aka.dataminer.services/igqidatasource-getnextpage

			var configurationaParameterInfos = GetConfigurationParameterInfos();
			var configurationParametersValues = GetConfigurationParameterValues(configurationaParameterInfos);
			var serviceConfigurationValues = GetServiceConfigurationValues(configurationParametersValues);
			var services = GetServices(serviceConfigurationValues)
				.Select(s => new ServiceRow(s))
				.ToList();

			var configurationParameterNames = new[]
			{
				"Service Type",
				"Reception Type",
				"Channel ID",
				"Video Format",
				"Distribution Type",
				"Region",
			};
			foreach (var serviceRow in services)
			{
				Compose(
					serviceRow,
					serviceConfigurationValues,
					configurationParametersValues,
					configurationaParameterInfos,
					configurationParameterNames);
			}

			return new GQIPage(services
				.Where(s => s.ParameterValues.TryGetValue("Service Type", out _))
				.Select(BuildRow).ToArray());
		}

		private void Compose(
			ServiceRow serviceRow,
			List<ServiceConfigurationValueInstance> allServiceConfigurationValues,
			List<ConfigurationParameterValueInstance> allConfigurationParametersValues,
			List<ConfigurationParametersInstance> allConfigurationParameterInfos,
			string[] targetParameterNames)
		{
			// Pre-index for performance
			var scvLookup = allServiceConfigurationValues.ToDictionary(scv => scv.ID.Id);
			var cvLookup = allConfigurationParametersValues.ToDictionary(cv => cv.ID.Id);
			var infoLookup = allConfigurationParameterInfos
				.Where(info => targetParameterNames.Contains(info.ConfigurationParameterInfo.ParameterName))
				.ToDictionary(info => info.ID.Id, info => info.ConfigurationParameterInfo.ParameterName);

			foreach (var scvid in serviceRow.Service.ServiceInfo.ServiceConfigurationParameters)
			{
				if (!scvLookup.TryGetValue(scvid, out var serviceConfig))
					continue;

				// unwrap nullable Guid
				var cvId = serviceConfig.ServiceConfigurationValue.ConfigurationParameterValue;
				if (!cvId.HasValue)
					continue;

				if (!cvLookup.TryGetValue(cvId.Value, out var cv))
					continue;

				var referenceId = cv.ConfigurationParameterValue.ConfigurationParameterReference;
				if (referenceId.HasValue && infoLookup.TryGetValue(referenceId.Value, out var paramName))
				{
					serviceRow.ParameterValues[paramName] = cv.ConfigurationParameterValue.StringValue;
				}
			}
		}

		private GQIRow BuildRow(ServiceRow service)
		{
			return new GQIRow(new[]
			{
				new GQICell { Value = service.Service.ID.Id.ToString() },
				new GQICell { Value = service.Service.ServiceInfo.ServiceName },
				new GQICell { Value = service.Service.ServiceInfo.Icon },
				new GQICell { Value = service.Service.Status.ToString() },
				new GQICell { Value = (int) TryGetAlarmLevel(service) },
				new GQICell { Value = service.ParameterValues.TryGetValue("Service Type", out var type) ? type : string.Empty },
				new GQICell { Value = service.ParameterValues.TryGetValue("Reception Type", out var receptionType) ? receptionType : string.Empty },
				new GQICell { Value = service.ParameterValues.TryGetValue("Channel ID", out var channelId) ? channelId : string.Empty },
				new GQICell { Value = service.ParameterValues.TryGetValue("Video Format", out var videoFormat) ? videoFormat : string.Empty },
				new GQICell { Value = service.ParameterValues.TryGetValue("Distribution Type", out var distType) ? distType : string.Empty },
				new GQICell { Value = service.ParameterValues.TryGetValue("Region", out var region) ? region : string.Empty },
			});
		}

		private List<ConfigurationParametersInstance> GetConfigurationParameterInfos()
		{
			var filters = new[]
			{
				(FilterElement<DomInstance>) DomInstanceExposers.FieldValues.DomInstanceField(ConfigurationParameterInfo.ParameterName).Equal("Service Type"),
				(FilterElement<DomInstance>) DomInstanceExposers.FieldValues.DomInstanceField(ConfigurationParameterInfo.ParameterName).Equal("Reception Type"),
				(FilterElement<DomInstance>) DomInstanceExposers.FieldValues.DomInstanceField(ConfigurationParameterInfo.ParameterName).Equal("Service Type"),
				(FilterElement<DomInstance>) DomInstanceExposers.FieldValues.DomInstanceField(ConfigurationParameterInfo.ParameterName).Equal("Channel ID"),
				(FilterElement<DomInstance>) DomInstanceExposers.FieldValues.DomInstanceField(ConfigurationParameterInfo.ParameterName).Equal("Video Format"),
				(FilterElement<DomInstance>) DomInstanceExposers.FieldValues.DomInstanceField(ConfigurationParameterInfo.ParameterName).Equal("Distribution Type"),
				(FilterElement<DomInstance>) DomInstanceExposers.FieldValues.DomInstanceField(ConfigurationParameterInfo.ParameterName).Equal("Region"),
			};

			var results = _configurationDomHelper.DomInstances.Read(filters.Aggregate((f1, f2) => f1.OR(f2)));
			return results.Select(r => new ConfigurationParametersInstance(r)).ToList();
		}

		private List<ConfigurationParameterValueInstance> GetConfigurationParameterValues(List<ConfigurationParametersInstance> configurationParameterInfos)
		{
			var serviceTypeInfo = configurationParameterInfos
				.SingleOrDefault(i => i.ConfigurationParameterInfo.ParameterName == "Service Type");

			var allFilters = configurationParameterInfos
				.Where(info => serviceTypeInfo == null || info.ID.Id != serviceTypeInfo.ID.Id)
				.Select(info =>
					(FilterElement<DomInstance>)DomInstanceExposers.FieldValues
						.DomInstanceField(ConfigurationParameterValue.ConfigurationParameterReference)
						.Equal(info.ID.Id))
				.ToList();

			FilterElement<DomInstance> filterForOthers = null;
			if (allFilters.Any())
			{
				filterForOthers = allFilters.Aggregate((f1, f2) => f1.OR(f2));
			}

			FilterElement<DomInstance> finalFilter = filterForOthers;

			if (serviceTypeInfo != null)
			{
				var serviceTypeFilter =
					DomInstanceExposers.FieldValues.DomInstanceField(ConfigurationParameterValue.ConfigurationParameterReference)
						.Equal(serviceTypeInfo.ID.Id)
					.AND(
					DomInstanceExposers.FieldValues.DomInstanceField(ConfigurationParameterValue.StringValue)
						.Equal(_arguments.ServiceType));

				if (filterForOthers != null)
				{
					finalFilter = filterForOthers.OR(serviceTypeFilter);
				}
				else
				{
					finalFilter = serviceTypeFilter;
				}
			}

			// If no filters at all, return empty sequence
			if (finalFilter == null)
				return new List<ConfigurationParameterValueInstance>();

			var results = _configurationDomHelper.DomInstances.Read(finalFilter);
			return results.Select(r => new ConfigurationParameterValueInstance(r)).ToList();
		}

		private List<ServiceConfigurationValueInstance> GetServiceConfigurationValues(List<ConfigurationParameterValueInstance> configurationParameterValues)
		{
			var filters = configurationParameterValues
				.Select(configValue =>
					(FilterElement<DomInstance>)DomInstanceExposers.FieldValues
						.DomInstanceField(SlcServicemanagementIds.Sections.ServiceConfigurationValue.ConfigurationParameterValue)
						.Equal(configValue.ID.Id))
				.ToList();

			var filter = filters.Any()
				? filters.Aggregate((f1, f2) => f1.OR(f2))
				: new FALSEFilterElement<DomInstance>();

			var results = _serviceMangerDomHelper.DomInstances.Read(filter);
			return results.Select(r => new ServiceConfigurationValueInstance(r)).ToList();
		}

		private IEnumerable<ServicesInstance> GetServices(
			IEnumerable<ServiceConfigurationValueInstance> serviceConfigurationValues)
		{
			var filters = serviceConfigurationValues
				.Select(serviceConfigValue =>
					(FilterElement<DomInstance>)DomInstanceExposers.FieldValues
						.DomInstanceField(SlcServicemanagementIds.Sections.ServiceInfo.ServiceConfigurationParameters)
						.Contains(serviceConfigValue.ID.Id))
				.ToList();

			var filter = filters.Any()
				? filters.Aggregate((f1, f2) => f1.OR(f2))
				: new FALSEFilterElement<DomInstance>();

			var result = _serviceMangerDomHelper.DomInstances.Read(filter);
			return result.Select(r => new ServicesInstance(r));
		}

		private Skyline.DataMiner.Core.DataMinerSystem.Common.AlarmLevel TryGetAlarmLevel(ServiceRow service)
		{
			var serviceName = service.Service.ServiceInfo.ServiceName;
			if (_agent.ServiceExistsSafe(serviceName))
			{
				return _agent.GetService(serviceName).GetState().Level;
			}

			return Skyline.DataMiner.Core.DataMinerSystem.Common.AlarmLevel.Undefined;
		}
	}

	internal class ServiceRow
	{
		public ServiceRow(ServicesInstance service)
		{
			Service = service;
		}

		public ServicesInstance Service { get; }

		public Dictionary<string, string> ParameterValues { get; } = new Dictionary<string, string>();
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
