
//---------------------------------
// Get Service Order Items Multisection_1.cs
//---------------------------------
namespace Get_ServiceOrderItemsMultipleSections_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    // Required to mark the interface as a GQI data source 
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Sections;

    using System.Linq;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Apps.Sections.Sections;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Sections;

    using DomHelpers;
    using DomHelpers.SlcServicemanagement;
    using Skyline.DataMiner.Net.Helper;

    [GQIMetaData(Name = "Get_ServiceOrderItemsMultipleSections")]
    public class EventManagerGetMultipleSections : IGQIDataSource, IGQIInputArguments, IGQIOnInit
    {
        // defining input argument, will be converted to guid by OnArgumentsProcessed
        private readonly GQIStringArgument domIdArg = new GQIStringArgument("DOM ID") { IsRequired = false };
        // private readonly GQIStringArgument sectionNameArg = new GQIStringArgument("Section") { IsRequired = true };
        // variable where input argument will be stored
        private Guid instanceDomId;
        //private string sectionName;

        private GQIDMS dms;

        private DomInstance _domInstance;

        private DomHelper _DomHelper;


        public GQIColumn[] GetColumns()
        {

            return new GQIColumn[]
                {
                    new GQIStringColumn("Service Order Item"),
                    new GQIIntColumn("Priority Order"),
                };
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] {
                domIdArg,
                //,
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

            //sectionName = args.GetArgumentValue(sectionNameArg);

            return new OnArgumentsProcessedOutputArgs();
        }

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            dms = args.DMS;

            return default;
        }

        private GQIRow[] GetMultiSection()
        {
            //GenerateInformationEvent("Get Service Items Multisection started");

            // define output list
            var rows = new List<GQIRow>();

            if (instanceDomId == Guid.Empty)
            {
                // return th empty list
                return rows.ToArray();
            }

            // will initiate DomHelper 
            LoadApplicationHandlersAndHelpers();

            var domEventIntanceId = new DomInstanceId(instanceDomId);
            // create filter to filter event instances with specific dom event ids
            var filter = DomInstanceExposers.Id.Equal(domEventIntanceId);

            _domInstance = _DomHelper.DomInstances.Read(filter).First<DomInstance>();

            var instance = new ServiceOrdersInstance(_domInstance);

            var serviceItems = instance.ServiceOrderItems;

            serviceItems.ForEach(item =>
            {
                rows.Add(
                    new GQIRow(new[] {
                                new GQICell{ Value = item.ServiceOrderItem.ToString() },
                                new GQICell{ Value = (int)(item.PriorityOrder ?? 0) },
                    })
                    );

            });

            return rows.ToArray();

        }

        private void LoadApplicationHandlersAndHelpers()
        {
            _DomHelper = new DomHelper(dms.SendMessages, SlcServicemanagementIds.ModuleId);
        }

        public DMSMessage GenerateInformationEvent(string message)
        {
            var generateAlarmMessage = new GenerateAlarmMessage(GenerateAlarmMessage.AlarmSeverity.Information, message);
            return dms.SendMessage(generateAlarmMessage);
        }
    }
}
