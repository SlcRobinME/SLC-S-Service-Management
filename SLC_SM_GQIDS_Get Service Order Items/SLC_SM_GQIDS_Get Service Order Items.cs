//---------------------------------
// SLC_SM_GQIDS_Get Service Order Items_1.cs
//---------------------------------
namespace SLC_SM_GQIDS_Get_Service_Order_Items_1
{
    using System;
    using System.Linq;
    using DomHelpers.SlcServicemanagement;
    using Library;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;

    // Required to mark the interface as a GQI data source
    [GQIMetaData(Name = "Get_ServiceOrderItems")]
    public class EventManagerGetMultipleSections : IGQIDataSource, IGQIInputArguments, IGQIOnInit
    {
        // defining input argument, will be converted to guid by OnArgumentsProcessed
        private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = false };
        private GQIDMS _dms;
        private DomHelper _domHelper;

        // variable where input argument will be stored
        private Guid _instanceDomId;

        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[]
            {
                new GQIStringColumn("ID"),
                new GQIStringColumn("Name"),
                new GQIStringColumn("Action"),
                new GQIStringColumn("Category"),
                new GQIStringColumn("Service Specification"),
                new GQIStringColumn("Service"),
                new GQIStringColumn("Property"),
                new GQIStringColumn("Configuration"),
            };
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] { domIdArg };
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            return new GQIPage(GetMultiSection())
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

            return default;
        }

        private static GQIRow BuildRow(ServiceOrderItemsInstance item, Repo repo)
        {
            return new GQIRow(
                new[]
                {
                    new GQICell { Value = item.ID.Id.ToString() },
                    new GQICell { Value = item.ServiceOrderItemInfo.Name },
                    new GQICell { Value = item.ServiceOrderItemInfo.Action },
                    new GQICell
                    {
                        Value = item.ServiceOrderItemServiceInfo.ServiceCategory.HasValue
                            ? repo.AllCategories.FirstOrDefault(x => x.ID.Id == item.ServiceOrderItemServiceInfo.ServiceCategory)?.Name ?? String.Empty
                            : String.Empty,
                    },
                    new GQICell
                    {
                        Value = item.ServiceOrderItemServiceInfo.ServiceSpecification.HasValue
                            ? repo.AllSpecs.FirstOrDefault(x => x.ID.Id == item.ServiceOrderItemServiceInfo.ServiceSpecification)?.Name ?? String.Empty
                            : String.Empty,
                    },
                    new GQICell
                    {
                        Value = item.ServiceOrderItemServiceInfo.Service.HasValue
                            ? repo.AllServices.FirstOrDefault(x => x.ID.Id == item.ServiceOrderItemServiceInfo.Service)?.ServiceInfo.ServiceName ?? String.Empty
                            : String.Empty,
                    },
                    new GQICell
                    {
                        Value = item.ServiceOrderItemServiceInfo.Properties.HasValue
                            ? repo.AllProperties.FirstOrDefault(x => x.ID.Id == item.ServiceOrderItemServiceInfo.Properties)?.ServicePropertyInfo.Name ?? String.Empty
                            : String.Empty,
                    },
                    new GQICell
                    {
                        Value = item.ServiceOrderItemServiceInfo.Configuration.HasValue
                            ? repo.AllConfigurations.FirstOrDefault(x => x.ID.Id == item.ServiceOrderItemServiceInfo.Properties)?.ID.Id.ToString()
                            : String.Empty,
                    },
                });
        }

        private GQIRow[] GetMultiSection()
        {
            if (_instanceDomId == Guid.Empty)
            {
                // return th empty list
                return Array.Empty<GQIRow>();
            }

            // will initiate DomHelper
            LoadApplicationHandlersAndHelpers();

            // create filter to filter event instances with specific dom event ids
            var domInstance = _domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(_instanceDomId)).FirstOrDefault();
            if (domInstance == null)
            {
                return Array.Empty<GQIRow>();
            }

            var instance = new ServiceOrdersInstance(domInstance);

            var linkedIds = instance.ServiceOrderItems
                .Where(x => x.ServiceOrderItem.HasValue && x.ServiceOrderItem != Guid.Empty)
                .Select(x => x.ServiceOrderItem.Value)
                .ToArray();

            if (!linkedIds.Any())
            {
                return Array.Empty<GQIRow>();
            }

            FilterElement<DomInstance> filter = new ORFilterElement<DomInstance>();
            foreach (Guid linkedId in linkedIds)
            {
                filter = filter.OR(DomInstanceExposers.Id.Equal(linkedId));
            }

            var instances = _domHelper.DomInstances.Read(filter)
                .Select(
                    x =>
                    {
                        try
                        {
                            return new ServiceOrderItemsInstance(x);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    })
                .Where(x => x != null)
                .ToArray();

            var repo = new Repo(_domHelper);

            return instances.Select(item => BuildRow(item, repo)).ToArray();
        }

        private void LoadApplicationHandlersAndHelpers()
        {
            _domHelper = new DomHelper(_dms.SendMessages, SlcServicemanagementIds.ModuleId);
        }
    }
}
