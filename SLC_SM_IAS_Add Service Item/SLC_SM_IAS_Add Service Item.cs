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
namespace SLC_SM_IAS_Add_Service_Item_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcServicemanagement;
	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.Relationship;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.IAS;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Dialogs;
	using SLC_SM_IAS_Add_Service_Item_1.Presenters;
	using SLC_SM_IAS_Add_Service_Item_1.Views;
	using Models = Skyline.DataMiner.ProjectApi.ServiceManagement.API.Relationship.Models;

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
				_controller = new InteractiveController(engine) { ScriptAbortPopupBehavior = ScriptAbortPopupBehavior.HideAlways };
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
				engine.ShowErrorDialog(e);
			}
		}

		private static void AddOrUpdateServiceItemToInstance(DomHelper helper, DomInstance domInstance, ServiceItemsSection newSection, string oldLabel)
		{
			if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.Services.Id)
			{
				var instance = new ServicesInstance(domInstance);

				// Remove old instance first in case of edit
				var oldItem = instance.ServiceItemses.FirstOrDefault(x => x.Label == oldLabel);
				if (oldItem != null)
				{
					newSection.ServiceItemID = oldItem.ServiceItemID;
					instance.ServiceItemses.Remove(oldItem);
				}

				if (newSection.ServiceItemID == null)
				{
					// Auto assign new ID
					long[] ids = instance.ServiceItemses.Where(x => x.ServiceItemID.HasValue).Select(x => x.ServiceItemID.Value).OrderBy(x => x).ToArray();
					newSection.ServiceItemID = ids.Any() ? ids.Max() + 1 : 0;
				}

				newSection.Icon = instance.ServiceInfo.Icon; // inherit icon from service.

				AddServiceLink(instance.ID.Id, instance.ServiceInfo.ServiceName, newSection);

				instance.ServiceItemses.Add(newSection);
				instance.Save(helper);
			}
			else if (domInstance.DomDefinitionId.Id == SlcServicemanagementIds.Definitions.ServiceSpecifications.Id)
			{
				var instance = new ServiceSpecificationsInstance(domInstance);

				// Remove old instance first in case of edit
				var oldItem = instance.ServiceItemses.FirstOrDefault(x => x.Label == oldLabel);
				if (oldItem != null)
				{
					newSection.ServiceItemID = oldItem.ServiceItemID;
					instance.ServiceItemses.Remove(oldItem);
				}

				if (newSection.ServiceItemID == null)
				{
					// Auto assign new ID
					long[] ids = instance.ServiceItemses.Where(x => x.ServiceItemID.HasValue).Select(x => x.ServiceItemID.Value).OrderBy(x => x).ToArray();
					newSection.ServiceItemID = ids.Any() ? ids.Max() + 1 : 0;
				}

				instance.ServiceItemses.Add(newSection);
				instance.Save(helper);
			}
			else
			{
				throw new InvalidOperationException($"DOM definition '{domInstance.DomDefinitionId}' not supported (yet).");
			}
		}

		private static void AddServiceLink(Guid serviceInstanceId, string serviceInstanceName, ServiceItemsSection newSection)
		{
			if (newSection.ServiceItemType != SlcServicemanagementIds.Enums.ServiceitemtypesEnum.Service)
			{
				return;
			}

			var dataHelper = new DataHelperLink(Engine.SLNetRaw);
			var link = dataHelper.Read().Find(x => x.ParentID == serviceInstanceId.ToString() && x.ChildID == newSection.ImplementationReference);
			if (link != null)
			{
				// Already linked OK
				return;
			}

			dataHelper.CreateOrUpdate(
				new Models.Link
				{
					ParentID = serviceInstanceId.ToString(),
					ParentName = serviceInstanceName,
					ChildID = newSection.ImplementationReference,
					ChildName = newSection.DefinitionReference,
				});
		}

		private static string[] GetServiceItemLabels(IServiceInstanceBase domInstance, string oldLbl)
		{
			var items = domInstance.GetServiceItems().Select(x => x.Label).ToList();
			items.Remove(oldLbl);

			return items.ToArray();
		}

		private static ServiceItemsSection GetServiceItemSection(DomInstance domInstance, string label)
		{
			IServiceInstanceBase serviceInstanceBase = ServiceInstancesExtentions.GetTypedInstance(domInstance);
			return serviceInstanceBase.GetServiceItems()?.FirstOrDefault(x => x.Label == label)
				?? throw new InvalidOperationException($"No Service Item with label '{label}' exists under {serviceInstanceBase.GetName()}");
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
			IServiceInstanceBase serviceInstance = ServiceInstancesExtentions.GetTypedInstance(domInstance);

			string label = _engine.GetScriptParam("Service Item Label").Value.Trim('"', '[', ']');

			// Init views
			var view = new ServiceItemView(_engine) { IsEnabled = false };
			view.Show(false);
			view.IsEnabled = true;
			var presenter = new ServiceItemPresenter(_engine, view, GetServiceItemLabels(serviceInstance, label), serviceInstance);

			// Events
			view.BtnCancel.Pressed += (sender, args) => throw new ScriptAbortException("OK");
			view.BtnAdd.Pressed += (sender, args) =>
			{
				if (presenter.Validate())
				{
					var section = presenter.Section;
					string jobId = presenter.UpdateJobForWorkFlow(label);
					if (!String.IsNullOrEmpty(jobId))
					{
						section.ImplementationReference = jobId;
					}

					AddOrUpdateServiceItemToInstance(domHelper, domInstance, section, label);
					throw new ScriptAbortException("OK");
				}
			};

			if (action == Action.Add)
			{
				presenter.LoadFromModel();
			}
			else
			{
				presenter.LoadFromModel(GetServiceItemSection(domInstance, label));
			}

			// Run interactive
			_controller.ShowDialog(view);
		}
	}
}