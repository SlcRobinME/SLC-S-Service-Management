namespace SLC_SM_IAS_Manage_Discretes.Presenters
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Automation;

	using SLC_SM_Common.API.ConfigurationsApi;

	using SLC_SM_IAS_Manage_Discretes.Views;

	public class DiscreteMgmtAddPresenter
	{
		private readonly IEngine engine;
		private readonly DiscreteMgmtAddView view;
		private readonly List<Models.DiscreteValue> discreteValues;

		public DiscreteMgmtAddPresenter(IEngine engine, DiscreteMgmtAddView view, List<Models.DiscreteValue> discreteValues)
		{
			this.engine = engine;
			this.view = view;
			this.discreteValues = discreteValues;

			Validate();

			view.Value.Changed += (sender, args) => Validate(args.Value);
		}

		public bool Validate()
		{
			return Validate(view.Value.Text);
		}

		public bool Validate(string value)
		{
			bool ok = true;

			if (String.IsNullOrEmpty(value))
			{
				view.ErrorValue.Text = "Please enter a valid value";
				ok = false;
			}
			else if (discreteValues.Exists(x => x.Value == value))
			{
				view.ErrorValue.Text = "Entered value already exists";
				ok = false;
			}
			else
			{
				view.ErrorValue.Text = String.Empty;
			}

			return ok;
		}
	}
}