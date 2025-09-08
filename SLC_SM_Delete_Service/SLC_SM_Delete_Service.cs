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

13/03/2025    1.0.0.1	   XXX, Skyline    Initial version
****************************************************************************
*/
namespace SLC_SM_Delete_Service_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Library;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.IAS;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private IEngine _engine;

		/// <summary>
		///     The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		/// // engine.ShowUI();
		public void Run(IEngine engine)
		{
			try
			{
				_engine = engine;
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
				engine.ExitFail(e.Message);
			}
		}

		private void RunSafe()
		{
			string domIdRaw = _engine.GetScriptParam("DOM ID").Value;
			_engine.GenerateInformation(domIdRaw);
			List<Guid> domIdList = JsonConvert.DeserializeObject<List<Guid>>(domIdRaw);
			if (domIdList.Count() == 0)
			{
				return;
				//throw new InvalidOperationException("No DOM ID provided as input to the script");
			}

			var serviceManagementHelper = new DataHelpersServiceManagement(Engine.SLNetRaw);

			// confirmation if the user wants to delete the services
			if (!_engine.ShowConfirmDialog($"Are you sure to you want to delete the selected {domIdList.Count} service(s) from the Inventory?"))
			{
				return;
			}

			var dms = _engine.GetDms();
			foreach (var domId in domIdList)
			{
				var service = serviceManagementHelper.Services.Read(ServiceExposers.Guid.Equal(domId)).FirstOrDefault();
				if (service != null)
				{
					if (service.GenerateMonitoringService == true && FindDmaService(dms, service, out IDmsService dmsService))
					{
						dmsService.Delete();
					}

					_engine.GenerateInformation($"Service that will be removed: {service.ID}/{service.Name}");
					serviceManagementHelper.Services.TryDelete(service);
				}
			}
		}

		private bool FindDmaService(IDms dms, Models.Service service, out IDmsService dmsService)
		{
			dmsService = null;
			try
			{
				if (dms.ServiceExists(service.Name))
				{
					dmsService = dms.GetService(service.Name);
					return true;
				}

				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}