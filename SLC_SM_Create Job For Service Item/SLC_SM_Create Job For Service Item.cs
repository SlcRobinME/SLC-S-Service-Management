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

13/03/2025    1.0.0.1        RME, Skyline    Initial version
****************************************************************************
*/
namespace SLCSMCreateJobForServiceItem
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;
	using DomHelpers.SlcWorkflow;
	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.MediaOps.Common.IOData.Scheduling.Scripts.JobHandler;
	using Skyline.DataMiner.Utils.MediaOps.Helpers.Relationships;
	using Skyline.DataMiner.Utils.MediaOps.Helpers.Workflows;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.IAS;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs;
	using OutputData = Skyline.DataMiner.Utils.MediaOps.Common.IOData.Scheduling.Scripts.JobHandler.OutputData;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
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
			try
			{
				RunSafe(engine);
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
				engine.ShowErrorDialog(e);
				engine.Log(e.ToString());
			}
		}

		private void RunSafe(IEngine engine)
		{
			Guid domId = engine.ReadScriptParamFromApp<Guid>("DOM ID");
			if (domId == Guid.Empty)
			{
				throw new InvalidOperationException("No DOM ID provided as input to the script");
			}

			var domHelper = new DomHelper(engine.SendSLNetMessages, SlcServicemanagementIds.ModuleId);

			var domInstance = domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domId)).FirstOrDefault()
							  ?? throw new InvalidOperationException($"No DOM Instance with ID '{domId}' found on the system!");

			string label = engine.ReadScriptParamFromApp("Service Item Label");

			var instance = ServiceInstancesExtentions.GetTypedInstance(domInstance);
			ServiceItemsSection serviceItemsSection = instance.GetServiceItems().SingleOrDefault(s => s.Label == label);
			if (serviceItemsSection == null)
			{
				throw new InvalidOperationException($"Could not find the service item section with label {label}");
			}

			var workflowHelper = new WorkflowHelper(engine);
			var workflow = workflowHelper.GetAllWorkflows().FirstOrDefault(x => x.Name == serviceItemsSection.DefinitionReference)
						   ?? throw new InvalidOperationException($"No Workflow found on the system with name '{serviceItemsSection.DefinitionReference}'");

			if (instance.GetEndTime() < DateTime.UtcNow)
			{
				throw new InvalidOperationException($"End time lies in the past ({instance.GetEndTime()}), not possible to create a job for a past event");
			}

			engine.Log("Gonna create job configuration");

			CreateJobAction jobConfiguration = CreateJobConfiguration(instance, serviceItemsSection, workflow);

			engine.Log("Gonna send to job handler");
			OutputData sendToJobHandler = jobConfiguration.SendToJobHandler(engine, true);

			engine.Log("Returned from job handler");

			if (sendToJobHandler == null)
			{
				engine.Log("Failed to create the job");
				engine.Log($"This is the exception: {sendToJobHandler.ExceptionInfo.SourceException}");
				throw new InvalidOperationException("Failure on creating the job from the workflow");
			}

			if (sendToJobHandler.HasException)
			{
				engine.ExitFail(sendToJobHandler.ExceptionInfo.SourceException.Message);
				return;
			}

			var outputData = (CreateJobActionOutput)sendToJobHandler.ActionOutput;
			if (outputData == null)
			{
				throw new InvalidOperationException("Failure on creating the job from the workflow");
			}

			var jobId = outputData.DomJobId;

			var transitionJobToTentativeInputData = new ExecuteJobAction
			{
				DomJobId = jobId,
				JobAction = JobAction.SaveAsTentative,
			};
			transitionJobToTentativeInputData.SendToJobHandler(engine, true);

			var domWorkflowHelper = new DomHelper(engine.SendSLNetMessages, SlcWorkflowIds.ModuleId);
			var job = FindJob(domWorkflowHelper, jobId);

			CreateLink(engine, instance, job);
			TrySetMonitoringSettingsForJob(instance, job);

			job.Save(domWorkflowHelper);

			serviceItemsSection.ImplementationReference = jobId.ToString();
			AddOrUpdateServiceItemToInstance(domHelper, instance, serviceItemsSection, label);
		}

		private void CreateLink(IEngine engine, IServiceInstanceBase instance, JobsInstance job)
		{
			var relationshipHelper = new RelationshipsHelper(engine);

			var serviceObjectType = GetOrCreateObjectType(relationshipHelper, "Service");
			var jobObjectType = GetOrCreateObjectType(relationshipHelper, "Job");

			var linkDetailsConfiguration = CreateLinkDetailsConfiguration(instance, job, serviceObjectType, jobObjectType);
			relationshipHelper.CreateLink(linkDetailsConfiguration);
		}

		private LinkConfiguration CreateLinkDetailsConfiguration(IServiceInstanceBase instance, JobsInstance job, Guid serviceObjectType, Guid jobObjectType)
		{
			var linkConfiguration = new LinkConfiguration()
			{
				Child = new LinkDetailsConfiguration()
				{
					DomObjectTypeId = serviceObjectType,
					ObjectId = instance.GetId().Id.ToString(),
					ObjectName = instance.GetName(),
					URL = "Link to open the service panel on service iventory app",
				},
				Parent = new LinkDetailsConfiguration()
				{
					DomObjectTypeId = jobObjectType,
					ObjectId = job.ID.Id.ToString(),
					ObjectName = job.Name,
				},
			};

			return linkConfiguration;
		}

		private CreateJobAction CreateJobConfiguration(IServiceInstanceBase instance, ServiceItemsSection serviceItemsSection, Workflow workflow)
		{
			return new CreateJobAction
			{
				Name = $"{instance.GetName()} | {serviceItemsSection.Label}",
				Description = $"{instance.GetId()} | {serviceItemsSection.Label}",
				DomWorkflowId = workflow.Id,
				Source = "Scheduling",
				DesiredJobStatus = DesiredJobStatus.Tentative,
				Start = instance.GetStartTime() ?? throw new InvalidOperationException("No Start Time configured to create the job from"),
				End = instance.GetEndTime() ?? ReservationInstance.PermanentEnd,
			};
		}

		private Guid GetOrCreateObjectType(RelationshipsHelper relationshipHelper, string name)
		{
			var objectType = relationshipHelper.GetObjectType(name);
			if (objectType == null)
			{
				return relationshipHelper.CreateObjectType(new ObjectTypeConfiguration()
				{
					Name = name,
				});
			}

			return objectType.Id;
		}

		private void TrySetMonitoringSettingsForJob(IServiceInstanceBase instance, JobsInstance job)
		{
			job.MonitoringSettings.AtJobStart = SlcWorkflowIds.Enums.Atjobstart.CreateServiceAtWorkflowStart;
			job.MonitoringSettings.AtJobEnd = SlcWorkflowIds.Enums.Atjobend.DeleteServiceIfOneExists;
		}

		private JobsInstance FindJob(DomHelper domWorkflowHelper, Guid jobId)
		{
			var filter = DomInstanceExposers.Id.Equal(jobId);
			var instance = domWorkflowHelper.DomInstances.Read(filter).FirstOrDefault();
			if (instance != null)
			{
				return new JobsInstance(instance);
			}

			return default;
		}

		private void AddOrUpdateServiceItemToInstance(DomHelper helper, IServiceInstanceBase instance, ServiceItemsSection newSection, string oldLabel)
		{
			var oldItem = instance.GetServiceItems().FirstOrDefault(x => x.Label == oldLabel);
			if (oldItem != null)
			{
				instance.GetServiceItems().Remove(oldItem);
			}
			else
			{
				long[] ids = instance.GetServiceItems().Where(x => x.ServiceItemID.HasValue).Select(x => x.ServiceItemID.Value).OrderBy(x => x).ToArray();
				newSection.ServiceItemID = ids.Any() ? ids.Max() + 1 : 0;
			}

			instance.GetServiceItems().Add(newSection);
			instance.Save(helper);
		}
	}
}
