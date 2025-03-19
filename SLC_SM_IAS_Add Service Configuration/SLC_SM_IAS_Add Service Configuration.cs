
//---------------------------------
// SLC_SM_IAS_Add Service Configuration_1.cs
//---------------------------------
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
    Tel.    : +32 51 31 35 69
    Fax.    : +32 51 31 01 29
    E-mail    : info@skyline.be
    Web        : www.skyline.be
    Contact    : Ben Vandenberghe

****************************************************************************
Revision History:

DATE        VERSION        AUTHOR            COMMENTS

dd/mm/2025    1.0.0.1        XXX, Skyline    Initial version
****************************************************************************
*/
namespace SLC_SM_IAS_Add_Service_Configuration_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DomHelpers.SlcServicemanagement;
    using Library.Views;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using SLC_SM_IAS_Add_Service_Configuration_1.Presenters;
    using SLC_SM_IAS_Add_Service_Configuration_1.Views;

    /// <summary>
    ///     Represents a DataMiner Automation script.
    /// </summary>
    public class Script
    {
        private InteractiveController _controller;
        private IEngine _engine;

        private enum Action
        {
            Add,
            Edit,
        }

        /// <summary>
        ///     The script entry point.
        /// </summary>
        /// <param name="engine">Link with SLAutomation process.</param>
        public void Run(IEngine engine)
        {
            /*
            * Note:
            * Do not remove the commented methods below!
            * The lines are needed to execute an interactive automation script from the non-interactive automation script or from Visio!
            *
            * engine.ShowUI();
            */
            if (engine.IsInteractive)
            {
                engine.FindInteractiveClient("Failed to run script in interactive mode", 1);
            }

            try
            {
                _engine = engine;
                _controller = new InteractiveController(engine);
                RunSafe();
            }
            catch (ScriptAbortException)
            {
                // Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
            }
            catch (ScriptForceAbortException)
            {
                // Catch forced abort exceptions, caused via external maintenance messages.
            }
            catch (ScriptTimeoutException)
            {
                // Catch timeout exceptions for when a script has been running for too long.
            }
            catch (InteractiveUserDetachedException)
            {
                // Catch a user detaching from the interactive script by closing the window.
                // Only applicable for interactive scripts, can be removed for non-interactive scripts.
            }
            catch (Exception e)
            {
                var errorView = new ErrorView(engine, "Error", e.Message, e.ToString());
                _controller.ShowDialog(errorView);
            }
        }

        private static void AddOrUpdateServiceConfigurationToInstance(DomHelper helper, ServiceConfigurationInstance domInstance, ServiceConfigurationParametersValuesSection newSection, string oldLabel)
        {
            // Remove old instance first in case of edit
            var oldItems = domInstance.ServiceConfigurationParametersValues
                .Where(x => x.Label == oldLabel || String.IsNullOrEmpty(x.Label))
                .ToList();
            if (oldItems.Any())
            {
                foreach (var oldItem in oldItems)
                {
                    domInstance.ServiceConfigurationParametersValues.Remove(oldItem);
                }
            }

            domInstance.ServiceConfigurationParametersValues.Add(newSection);
            domInstance.Save(helper);
        }

        private static ServiceConfigurationInstance GetServiceConfigurationSection(DomHelper domHelper, DomInstance domInstance)
        {
            if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
            {
                var instance = new ServicesInstance(domInstance);
                var sci = GetServiceConfigurationValueInstance(domHelper, instance.ServiceInfo.ServiceConfiguration);

                if (instance.ServiceInfo.ServiceConfiguration != sci.ID.Id)
                {
                    // Link created configuration value
                    instance.ServiceInfo.ServiceConfiguration = sci.ID.Id;
                    instance.Save(domHelper);
                }

                return sci;
            }

            if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
            {
                var instance = new ServiceSpecificationsInstance(domInstance);
                var sci = GetServiceConfigurationValueInstance(domHelper, instance.ServiceSpecificationInfo.ServiceConfiguration);

                if (instance.ServiceSpecificationInfo.ServiceConfiguration != sci.ID.Id)
                {
                    // Link created configuration value
                    instance.ServiceSpecificationInfo.ServiceConfiguration = sci.ID.Id;
                    instance.Save(domHelper);
                }

                return sci;
            }

            if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceOrderItems.Id)
            {
                var instance = new ServiceOrderItemsInstance(domInstance);
                var sci = GetServiceConfigurationValueInstance(domHelper, instance.ServiceOrderItemServiceInfo.Configuration);

                if (instance.ServiceOrderItemServiceInfo.Configuration != sci.ID.Id)
                {
                    // Link created configuration value
                    instance.ServiceOrderItemServiceInfo.Configuration = sci.ID.Id;
                    instance.Save(domHelper);
                }

                return sci;
            }

            throw new InvalidOperationException($"No Service Property item found linked to DOM Instance '{domInstance.ID.Id}'");
        }

        private static ServiceConfigurationInstance GetServiceConfigurationValueInstance(DomHelper domHelper, Guid? id)
        {
            Guid usedPropertyValue;
            if (!id.HasValue)
            {
                var propertyValue = new ServiceConfigurationInstance();
                propertyValue.ServiceConfigurationParametersValues.Add(new ServiceConfigurationParametersValuesSection());
                propertyValue.Save(domHelper);

                usedPropertyValue = propertyValue.ID.Id;
            }
            else
            {
                usedPropertyValue = id.Value;
            }

            var domInstance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(usedPropertyValue)).FirstOrDefault()
                              ?? throw new InvalidOperationException($"DOM Instance with ID '{usedPropertyValue}' does not exist on the system");

            return new ServiceConfigurationInstance(domInstance);
        }

        private static string[] GetServiceConfigurationLabels(DomHelper domHelper, DomInstance domInstance, string oldLbl)
        {
            List<string> items = new List<string>();
            Guid usedConfig = Guid.Empty;
            if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
            {
                var instance = new ServicesInstance(domInstance);
                usedConfig = instance.ServiceInfo.ServiceConfiguration.Value;
            }
            else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
            {
                var instance = new ServiceSpecificationsInstance(domInstance);
                usedConfig = instance.ServiceSpecificationInfo.ServiceConfiguration.Value;
            }
            else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceOrderItems.Id)
            {
                var instance = new ServiceOrderItemsInstance(domInstance);
                usedConfig = instance.ServiceOrderItemServiceInfo.Configuration.Value;
            }
            else
            {
                return items.ToArray();
            }

            var configInstance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(usedConfig)).FirstOrDefault()
                              ?? throw new InvalidOperationException($"DOM Instance with ID '{usedConfig}' does not exist on the system");

            items.AddRange(new ServiceConfigurationInstance(configInstance).ServiceConfigurationParametersValues.Select(x => x.Label));

            items.Remove(oldLbl);
            return items.ToArray();
        }

        private void RunSafe()
        {
            string domIdRaw = _engine.GetScriptParam("DOM ID").Value;
            Guid domId = JsonConvert.DeserializeObject<List<Guid>>(domIdRaw).FirstOrDefault();
            if (domId == Guid.Empty)
            {
                throw new InvalidOperationException("No DOM ID provided as input to the script");
            }

            string actionRaw = _engine.GetScriptParam("Action").Value.Trim('"', '[', ']');
            if (!Enum.TryParse(actionRaw, true, out Action action))
            {
                throw new InvalidOperationException("No Action provided as input to the script");
            }

            var domHelper = new DomHelper(_engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);
            var domInstance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domId)).FirstOrDefault()
                              ?? throw new InvalidOperationException($"No DOM Instance with ID '{domId}' found on the system!");

            string label = JsonConvert.DeserializeObject<List<string>>(_engine.GetScriptParam("Service Configuration Label").Value).FirstOrDefault()
                           ?? throw new InvalidOperationException("No Label provided as script input");

            // Get or create the service property value
            var configurationInstance = GetServiceConfigurationSection(domHelper, domInstance);

            // Init views
            var view = new ServiceConfigurationView(_engine);
            var presenter = new ServiceConfigurationPresenter(view, GetServiceConfigurationLabels(domHelper, domInstance, label));

            // Events
            view.BtnCancel.Pressed += (sender, args) => throw new ScriptAbortException("OK");
            view.BtnAdd.Pressed += (sender, args) =>
            {
                if (presenter.Validate())
                {
                    AddOrUpdateServiceConfigurationToInstance(domHelper, configurationInstance, presenter.Section, label);
                    throw new ScriptAbortException("OK");
                }
            };

            if (action == Action.Add)
            {
                presenter.LoadFromModel();
            }
            else
            {
                var section = configurationInstance.ServiceConfigurationParametersValues.FirstOrDefault(x => x.Label == label)
                    ?? throw new InvalidOperationException($"No Service Configuration exits with label '{label}'");
                presenter.LoadFromModel(section);
            }

            // Run interactive
            _controller.ShowDialog(view);
        }
    }
}

