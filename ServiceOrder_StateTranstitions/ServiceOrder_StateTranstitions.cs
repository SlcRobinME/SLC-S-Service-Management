
//---------------------------------
// ServiceOrder_StateTranstitions_1.cs
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

namespace ServiceOrder_StateTranstitions_1
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Actions;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using DomHelpers.SlcServicemanagement;


        /// <summary>
        /// The script entry point.
        /// </summary>
        /// <param name="engine">Link with SLAutomation process.</param>
        public class Script
        {
            /// <summary>
            /// The Script entry point.
            /// </summary>
            /// <param name="engine">Link with SLAutomation process.</param>
            public void Run(Engine engine)
            {
                // DO NOT REMOVE
                // engine.ShowUI();
                engine.ExitFail("This script should be executed using the 'OnDomAction' entry point");
            }

            [AutomationEntryPoint(AutomationEntryPointType.Types.OnDomAction)]
            public void OnDomActionMethod(IEngine engine, ExecuteScriptDomActionContext context)
            {
                // DO NOT REMOVE
                // engine.ShowUI();

                var instanceId = context.ContextId as DomInstanceId;
                var previousState = engine.GetScriptParam("PreviousState")?.Value;
                var nextState = engine.GetScriptParam("NextState")?.Value;

                //engine.GenerateInformation($"EventStateTransition: Input parameters instaceId: {instanceId.ToString()}, PreviousState: {previousState}, NextState: {nextState}");

                //engine.GenerateInformation("Starting DOM Action with script EventStateTransitions");

                //engine.GenerateInformation(previousState);
                //engine.GenerateInformation(nextState);

                if (!ValidateArguments(instanceId, previousState, nextState))
                {
                    //engine.GenerateInformation($"{nextState} and {previousState}");
                    engine.ExitFail("Input is not valid");
                }

                var domHelper = new DomHelper(engine.SendSLNetMessages, instanceId.ModuleId);

                //engine.GenerateInformation("Start Event Transition");

                string transitionId = String.Empty;

                switch (previousState)
                {
                    case "new":
                        switch (nextState)
                        {
                            case "acknowledged":

                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.New_To_Acknowledged;
                                break;
                            case "rejected":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.New_To_Rejected;
                                break;
                            default:
                                throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
                        }

                        break;
                    case "acknowledged":
                        switch (nextState)
                        {
                            case "inprogress":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Acknowledged_To_Inprogress;
                                break;
                            case "rejected":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Acknowledged_To_Rejected;
                                break;
                            default:
                                throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
                        }
                        break;
                    case "inprogress":
                        switch (nextState)
                        {
                            case "completed":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Inprogress_To_Completed;
                                break;
                            case "failed":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Inprogress_To_Failed;
                                break;
                            case "partial":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Inprogress_To_Partial;
                                break;
                            case "held":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Inprogress_To_Held;
                                break;
                            case "pending":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Inprogress_To_Pending;
                                break;
                            default:
                                throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
                        }

                        break;
                    case "pending":
                        switch (nextState)
                        {
                            case "assesscancellation":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Pending_To_Assesscancellation;
                                break;
                            case "inprogress":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Pending_To_Inprogress;
                                break;
                        default:
                                throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
                        }

                        break;
                case "held":
                    switch (nextState)
                    {
                        case "assesscancellation":
                            transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Held_To_Assesscancellation;
                            break;
                        case "inprogress":
                            transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Held_To_Inprogress;
                            break;
                        default:
                            throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
                    }

                    break;
                case "assesscancellation":
                        switch (nextState)
                        {
                            case "pendingcancellation":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Assesscancellation_To_Pendingcancellation;
                                break;
                            case "held":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Assesscancellation_To_Held;
                                break;
                            case "pending":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Assesscancellation_To_Pending;
                                break;
                        default:
                                throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
                        }

                        break;
                    case "pendingcancellation":
                        switch (nextState)
                        {
                            case "cancelled":
                                transitionId = SlcServicemanagementIds.Behaviors.Serviceorder_Behavior.Transitions.Pendingcancellation_To_Cancelled;
                                break;
                            default:
                                throw new NotSupportedException($"The provided previousState '{previousState}' is not supported for nextState '{nextState}'");
                        }

                        break;
                    default:
                        throw new NotSupportedException($"previousState '{previousState}' is not supported");
                }


                engine.GenerateInformation($"Service Order Status Transition starting: previousState: {previousState}, nextState: {nextState}");
                domHelper.DomInstances.DoStatusTransition(instanceId, transitionId);
            }

            private static bool ValidateArguments(DomInstanceId domInstanceId, string scriptParamValue, string scriptParamValue2)
            {
                if (domInstanceId == null)
                {
                    return false;
                }

                if (String.IsNullOrEmpty(scriptParamValue))
                {
                    return false;
                }

                if (String.IsNullOrEmpty(scriptParamValue2))
                {
                    return false;
                }

                return true;
            }
        }
}
