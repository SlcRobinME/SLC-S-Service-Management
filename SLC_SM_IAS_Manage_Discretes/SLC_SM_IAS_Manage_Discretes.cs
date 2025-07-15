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

15/07/2025	1.0.0.1		RME, Skyline	Initial version
****************************************************************************
*/
namespace SLC_SM_IAS_Manage_Discretes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Library.Views;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using SLC_SM_Common.API.ConfigurationsApi;

	using SLC_SM_IAS_Manage_Discretes.Presenters;
	using SLC_SM_IAS_Manage_Discretes.Views;

	/// <summary>
	///     Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private InteractiveController _controller;
		private IEngine _engine;

		internal enum Action
		{
			Add,
			Delete,
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

		private void RunAddWindow(DataHelperDiscreteValues helperDiscretes, List<Models.DiscreteValue> discreteValues)
		{
			// Init views
			var view = new DiscreteMgmtAddView(_engine);
			var presenter = new DiscreteMgmtAddPresenter(_engine, view, discreteValues);

			// Events
			view.BtnClose.Pressed += (sender, args) => throw new ScriptAbortException("OK");
			view.BtnAdd.Pressed += (sender, args) =>
			{
				if (!presenter.Validate())
				{
					return;
				}

				helperDiscretes.CreateOrUpdate(
					new Models.DiscreteValue
					{
						Value = view.Value.Text,
					});

				throw new ScriptAbortException("OK");
			};

			// Run interactive
			_controller.ShowDialog(view);
		}

		private void RunDeleteWindow(DataHelperDiscreteValues helperDiscretes, List<Models.DiscreteValue> discreteValues)
		{
			var discreteParameterOptionsHelper = new DataHelperDiscreteParameterOptions(Engine.SLNetRaw);
			var discreteParameterOptions = discreteParameterOptionsHelper.Read();

			// Init views
			var view = new DiscreteMgmtDeleteView(_engine);
			var presenter = new DiscreteMgmtDeletePresenter(_engine, view, discreteValues, discreteParameterOptions);

			// Events
			view.BtnClose.Pressed += (sender, args) => throw new ScriptAbortException("OK");
			view.BtnRemove.Pressed += (sender, args) =>
			{
				if (!presenter.Validate())
				{
					return;
				}

				Guid selectedId = view.Value.Selected.ID;
				foreach (var discreteOption in discreteParameterOptions.Where(x => x.Default?.ID == selectedId || x.DiscreteValues.Exists(d => d.ID == selectedId)))
				{
					if (discreteOption.Default?.ID == selectedId)
					{
						discreteOption.Default.ID = discreteOption.DiscreteValues.FirstOrDefault(d => d.ID != selectedId)?.ID ?? Guid.Empty;
					}

					discreteOption.DiscreteValues.RemoveAll(d => d.ID == selectedId);
					discreteParameterOptionsHelper.CreateOrUpdate(discreteOption);
				}

				helperDiscretes.TryDelete(view.Value.Selected);

				throw new ScriptAbortException("OK");
			};

			// Run interactive
			_controller.ShowDialog(view);
		}

		private void RunSafe()
		{
			var action = Enum.TryParse(_engine.GetScriptParam("Action").Value, true, out Action raw)
				? raw
				: throw new InvalidOperationException("Failed to retrieve the 'Action' script input parameter");

			var helperDiscretes = new DataHelperDiscreteValues(Engine.SLNetRaw);
			List<Models.DiscreteValue> discreteValues = helperDiscretes.Read();

			if (action == Action.Add)
			{
				RunAddWindow(helperDiscretes, discreteValues);
			}
			else
			{
				RunDeleteWindow(helperDiscretes, discreteValues);
			}
		}
	}
}