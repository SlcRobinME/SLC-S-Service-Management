namespace Get_Service_Properties_1
{
    using System;
    using System.Linq;
    using DomHelpers.SlcServicemanagement;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

    [GQIMetaData(Name = "Get_ServiceProperties")]
    public class EventManagerGetServiceProperties : IGQIDataSource, IGQIInputArguments, IGQIOnInit
    {
        // defining input argument, will be converted to guid by OnArgumentsProcessed
        private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = true };
        private GQIDMS _dms;
        private DomHelper _domHelper;

        // variable where input argument will be stored
        private Guid _instanceDomId;

        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[]
            {
                new GQIStringColumn("Property ID"),
                new GQIStringColumn("Property Name"),
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
            return new GQIPage(GetServicePropertiesForDomInstance())
            {
                HasNextPage = false,
            };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            // adds the input argument to private variable
            if (!Guid.TryParse(args.GetArgumentValue(domIdArg), out _instanceDomId))
            {
                _instanceDomId = Guid.Empty;
            }

            return new OnArgumentsProcessedOutputArgs();
        }

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            _dms = args.DMS;

            return new OnInitOutputArgs();
        }

        private DomInstance FetchDomInstance(Guid instanceId)
        {
            var filter = DomInstanceExposers.Id.Equal(new DomInstanceId(instanceId));
            return _domHelper.DomInstances.Read(filter).FirstOrDefault();
        }

        private GQIRow[] GetServicePropertiesForDomInstance()
        {
            if (_instanceDomId == Guid.Empty)
            {
                // return th empty list
                return Array.Empty<GQIRow>();
            }

            // will initiate DomHelper
            LoadApplicationHandlersAndHelpers();

            var domInstance = FetchDomInstance(_instanceDomId);
            if (domInstance == null)
            {
                // DOM Instance does not exist
                return Array.Empty<GQIRow>();
            }

            Guid servicePropertyInstanceId = Guid.Empty;

            if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
            {
                var instance = new ServicesInstance(domInstance);
                servicePropertyInstanceId = instance.ServiceInfo.ServiceProperties ?? Guid.Empty;
            }
            else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
            {
                var instance = new ServiceSpecificationsInstance(domInstance);
                servicePropertyInstanceId = instance.ServiceSpecificationInfo.ServiceProperties ?? Guid.Empty;
            }
            else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceOrderItems.Id)
            {
                var instance = new ServiceOrderItemsInstance(domInstance);
                servicePropertyInstanceId = instance.ServiceOrderItemServiceInfo.Properties ?? Guid.Empty;
            }
            else
            {
                // No other options to configure
            }

            if (servicePropertyInstanceId == Guid.Empty)
            {
                return Array.Empty<GQIRow>(); // No properties configured
            }

            var propertyDomInstance = FetchDomInstance(servicePropertyInstanceId);
            if (propertyDomInstance == null)
            {
                return Array.Empty<GQIRow>();
            }

            var propertiesInstance = new ServicePropertyValuesInstance(propertyDomInstance);
            return propertiesInstance.ServicePropertyValue
                .Where(x => x.Property.HasValue)
                .Select(BuildRow)
                .ToArray();
        }

        private static GQIRow BuildRow(ServicePropertyValueSection item)
        {
            return new GQIRow(
                new[]
                {
                    new GQICell { Value = item.Property.ToString() },
                    new GQICell { Value = item.PropertyName },
                    new GQICell { Value = item.Value },
                });
        }

        private void LoadApplicationHandlersAndHelpers()
        {
            _domHelper = new DomHelper(_dms.SendMessages, SlcServicemanagementIds.ModuleId);
        }
    }
}

