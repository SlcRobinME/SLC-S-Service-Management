namespace Skyline.DataMiner.Utils.ServiceManagement.Common.IAS.Components
{
	using System;
	using System.Collections.Generic;

	// using Skyline.DataMiner.Utils.MediaOps.Common.Tools;

	public readonly struct Choice<T> : IEquatable<Choice<T>>
	{
		public static readonly Choice<T> Empty = default;

		public Choice(T value, string displayValue)
		{
			Value = value;
			DisplayValue = displayValue;
		}

		public T Value { get; }

		public string DisplayValue { get; }

		public static bool operator ==(Choice<T> left, Choice<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Choice<T> left, Choice<T> right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return obj is Choice<T> option && Equals(option);
		}

		public bool Equals(Choice<T> other)
		{
			return EqualityComparer<T>.Default.Equals(Value, other.Value) &&
				   DisplayValue == other.DisplayValue;
		}

		public override int GetHashCode()
		{
			return 0;

			/// return HashCode.Combine(Value, DisplayValue);
		}
	}

	public static class Choice
	{
		public static Choice<T> Create<T>(T value, string displayValue)
		{
			return new Choice<T>(value, displayValue);
		}
	}
}