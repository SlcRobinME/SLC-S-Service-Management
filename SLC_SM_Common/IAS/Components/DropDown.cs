namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Components
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DropDown<T> : DropDown
	{
		private readonly Dictionary<string, Choice<T>> _mapping = new Dictionary<string, Choice<T>>();

		public new IEnumerable<Choice<T>> Options
		{
			get
			{
				return _mapping.Values;
			}

			set
			{
				var list = value.ToList();

				_mapping.Clear();
				foreach (var option in list)
					_mapping[option.DisplayValue ?? string.Empty] = option;

				SetOptions(list.Select(x => x.DisplayValue ?? string.Empty));
			}
		}

		public Choice<T> SelectedOption
		{
			get
			{
				if (Selected == null)
				{
					return default;
				}

				_mapping.TryGetValue(Selected, out var selectedOption);
				return selectedOption;
			}

			set
			{
				Selected = value.DisplayValue;
			}
		}

		public T SelectedValue
		{
			get
			{
				return SelectedOption.Value;
			}

			set
			{
				var item = _mapping.Values.FirstOrDefault(x => Equals(x.Value, value));
				Selected = item.DisplayValue;
			}
		}

		public void Clear()
		{
			Options = Enumerable.Empty<Choice<T>>();
		}
	}
}