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

namespace SLCSMPopupMessage
{
	using System;
	using System.Net.NetworkInformation;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			// DO NOT REMOVE THIS COMMENTED-OUT CODE OR THE SCRIPT WON'T RUN!
			// DataMiner evaluates if the script needs to launch in interactive mode.
			// This is determined by a simple string search looking for "engine.ShowUI" in the source code.
			// However, because of the toolkit NuGet package, this string cannot be found here.
			// So this comment is here as a workaround.
			//// engine.ShowUI();
			///
			try
			{
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
				// throw; // Comment if it should be treated as a normal exit of the script.
			}
			catch (InteractiveUserDetachedException)
			{
				// Catch a user detaching from the interactive script by closing the window.
				// Only applicable for interactive scripts, can be removed for non-interactive scripts.
				// throw;
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			var title = engine.GetScriptParam("Title").Value;
			var message = engine.GetScriptParam("Message").Value;
			var buttonLabel = engine.GetScriptParam("ButtonLabel").Value;

			InteractiveController controller = new InteractiveController(engine);
			PopupDialog dialog = new PopupDialog(engine, title, message, buttonLabel);
			dialog.ButtonOk.Pressed += (sender, args) => engine.ExitSuccess(string.Empty);

			controller.ShowDialog(dialog);
		}
	}

	public class PopupDialog : Dialog
	{
		public PopupDialog(IEngine engine, string title, string message, string button) : base(engine)
		{
			Title = title;

			Label label = new Label(message);
			label.SetWidthAuto();

			ButtonOk = new Button(button);
			ButtonOk.Style = ButtonStyle.CallToAction;

			int row = 0;
			AddWidget(label, row++, 0);
			AddWidget(new WhiteSpace { Height = 25 }, row++, 0);
			AddWidget(ButtonOk, row++, 1, HorizontalAlignment.Right);

			SetColumnWidth(0, 300);
			SetColumnWidth(1, 100);
		}

		public Button ButtonOk { get; private set; }
	}
}
