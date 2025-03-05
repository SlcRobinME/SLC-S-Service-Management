
//---------------------------------
// Get Service Configuration_1.cs
//---------------------------------
namespace Get_ServiceConfiguration_1
{
    // Used to process the Service Items
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DomHelpers.SlcServicemanagement;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Helper;
    using Skyline.DataMiner.Net.Messages;

    // Required to mark the interface as a GQI data source

    [GQIMetaData(Name = "Get_ServiceConfiguration")]
    public class EventManagerGetMultipleSections : IGQIDataSource, IGQIInputArguments, IGQIOnInit
    {
        // defining input argument, will be converted to guid by OnArgumentsProcessed
        private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = true };
        private DomHelper _domHelper;
        private GQIDMS dms;

        // variable where input argument will be stored
        private Guid instanceDomId;

        public DMSMessage GenerateInformationEvent(string message)
        {
            var generateAlarmMessage = new GenerateAlarmMessage(GenerateAlarmMessage.AlarmSeverity.Information, message) { Status = GenerateAlarmMessage.AlarmStatus.Cleared };
            return dms.SendMessage(generateAlarmMessage);
        }

        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[]
            {
                new GQIStringColumn("Label"),
                new GQIStringColumn("Service parameter ID"),
                new GQIStringColumn("Profile parameter ID"),
                new GQIBooleanColumn("Mandatory"),
                new GQIStringColumn("Value"),
            };
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[]
            {
                domIdArg,
            };
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            //GenerateInformationEvent("GetNextPage started");

            return new GQIPage(GetMultiSection())
            {
                HasNextPage = false,
            };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            // adds the input argument to private variable
            if (!Guid.TryParse(args.GetArgumentValue(domIdArg), out instanceDomId))
            {
                instanceDomId = Guid.Empty;
            }

            return new OnArgumentsProcessedOutputArgs();
        }

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            dms = args.DMS;

            return default;
        }

        private DomInstance FetchDomInstance(Guid instanceDomId)
        {
            var domIntanceId = new DomInstanceId(instanceDomId);

            // create filter to filter event instances with specific dom event ids
            var filter = DomInstanceExposers.Id.Equal(domIntanceId);

            return _domHelper.DomInstances.Read(filter).FirstOrDefault();
        }

        private GQIRow[] GetMultiSection()
        {
            if (instanceDomId == Guid.Empty)
            {
                return Array.Empty<GQIRow>();
            }

            // will initiate DomHelper
            LoadApplicationHandlersAndHelpers();

            var domInstance = FetchDomInstance(instanceDomId);
            if (domInstance == null)
            {
                return Array.Empty<GQIRow>();
            }

            Guid serviceConfigurationGuid = Guid.Empty;

            if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
            {
                var instance = new ServicesInstance(domInstance);
                serviceConfigurationGuid = instance.ServiceInfo.ServiceConfiguration ?? Guid.Empty;
            }
            else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
            {
                var instance = new ServiceSpecificationsInstance(domInstance);
                serviceConfigurationGuid = instance.ServiceSpecificationInfo.ServiceConfiguration ?? Guid.Empty;
            }
            else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceOrderItems.Id)
            {
                var instance = new ServiceOrderItemsInstance(domInstance);
                serviceConfigurationGuid = instance.ServiceOrderItemServiceInfo.Configuration ?? Guid.Empty;
            }
            else
            {
                // For future options
            }

            if (serviceConfigurationGuid == Guid.Empty)
            {
                return Array.Empty<GQIRow>();
            }

            var configDomInstance = FetchDomInstance(serviceConfigurationGuid);
            if (configDomInstance == null)
            {
                return Array.Empty<GQIRow>();
            }

            var configInstance = new ServiceConfigurationInstance(configDomInstance);

            var configValues = configInstance.ServiceConfigurationParametersValues;

            var rows = new List<GQIRow>();
            configValues.ForEach(
                item =>
                {
                    rows.Add(
                        new GQIRow(
                            new[]
                            {
                                new GQICell { Value = item.Label ?? String.Empty },
                                new GQICell { Value = item.ServiceParameterID ?? String.Empty },
                                new GQICell { Value = item.ProfileParameterID ?? String.Empty },
                                new GQICell { Value = (bool)(item.Mandatory ?? false) },
                                new GQICell
                                {
                                    Value = !String.IsNullOrWhiteSpace(item.StringValue)
                                        ? item.StringValue
                                        : item.DoubleValue.HasValue
                                            ? Convert.ToString(item.DoubleValue.Value)
                                            : String.Empty,
                                },
                            })
                    );
                });

            return rows.ToArray();
        }

        private void LoadApplicationHandlersAndHelpers()
        {
            _domHelper = new DomHelper(dms.SendMessages, SlcServicemanagementIds.ModuleId);
        }
    }
}
