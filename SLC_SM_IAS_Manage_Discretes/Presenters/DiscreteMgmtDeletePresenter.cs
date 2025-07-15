namespace SLC_SM_IAS_Manage_Discretes.Presenters
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	using SLC_SM_Common.API.ConfigurationsApi;

	using SLC_SM_IAS_Manage_Discretes.Views;

	public class DiscreteMgmtDeletePresenter
	{
		private readonly IEngine engine;
		private readonly DiscreteMgmtDeleteView view;
		private readonly List<Models.DiscreteParameterOptions> discreteParameterOptions;

		public DiscreteMgmtDeletePresenter(IEngine engine, DiscreteMgmtDeleteView view, List<Models.DiscreteValue> discreteValues, List<Models.DiscreteParameterOptions> discreteParameterOptions)
		{
			this.engine = engine;
			this.view = view;
			this.discreteParameterOptions = discreteParameterOptions;

			view.Value.SetOptions(discreteValues.Select(x => new Option<Models.DiscreteValue>(x.Value, x)).OrderBy(x => x.DisplayValue));

			Validate();

			view.Value.Changed += (sender, args) => Validate(args.Selected);
		}

		public bool Validate()
		{
			return Validate(view.Value.Selected);
		}

		public bool Validate(Models.DiscreteValue value)
		{
			bool ok = true;

			if (value == null)
			{
				view.Info.Text = "Please select a discrete value.";
				ok = false;
			}
			else if (discreteParameterOptions.Exists(x => x.Default?.ID == view.Value.Selected.ID || x.DiscreteValues.Exists(d => d.ID == view.Value.Selected.ID)))
			{
				view.Info.Text = "Discrete option still in use!\r\n\r\nWARNING: removing the discrete option will remove the discrete from every characteristic where it is used,\r\nthis action could impact orchestration.";
			}
			else
			{
				view.Info.Text = $"Discrete option '{value?.Value}' is not used and can be safely removed.";
			}

			return ok;
		}
	}
}