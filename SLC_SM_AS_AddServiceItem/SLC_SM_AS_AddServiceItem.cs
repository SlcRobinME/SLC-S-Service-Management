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

27/05/2025	1.0.0.1		RCA, Skyline	Initial version
****************************************************************************
*/

namespace SLCSMASAddServiceItem
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private Guid _serviceSpecificationId;
		//private string _label;
		private string _serviceItemType;
		private string _definitionReference;
		//private string _serviceItemScript;
		//private string _implementationReference;

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
				throw; // Comment if it should be treated as a normal exit of the script.
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			LoadParameters(engine);

			// TODO
		}

		private void LoadParameters(IEngine engine)
		{
			string domIdRaw = engine.GetScriptParam("DOM ID").Value;
			_serviceSpecificationId = JsonConvert.DeserializeObject<List<Guid>>(domIdRaw).FirstOrDefault();
			if (_serviceSpecificationId == Guid.Empty)
			{
				throw new ArgumentException("No DOM ID provided as input to the script");
			}

			//_label = engine.GetScriptParam("Label").Value.Trim('"', '[', ']');
			//_serviceItemScript = engine.GetScriptParam("ServiceItemScript").Value.Trim('"', '[', ']');
			//_implementationReference = engine.GetScriptParam("ImplementationReference").Value.Trim('"', '[', ']');

			_serviceItemType = engine.GetScriptParam("ServiceItemType").Value.Trim('"', '[', ']');
			if (string.IsNullOrEmpty(_serviceItemType))
			{
				throw new ArgumentException("No Service Item type provided as input to the script");
			}

			_definitionReference = engine.GetScriptParam("DefinitionReference").Value.Trim('"', '[', ']');
			if (string.IsNullOrEmpty(_definitionReference))
			{
				throw new ArgumentException("No Definition Reference provided as input to the script");
			}
		}
	}
}
