namespace Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Additional Exposers Extensions.
	/// </summary>
	public static class AdditionalExposerExtensions
	{
		/// <summary>
		/// Creates a filter element that checks if the value exposed by the specified Exposer is equal to any of the values in the provided collection.
		/// </summary>
		/// <typeparam name="T">The type of object exposed by the Exposer.</typeparam>
		/// <typeparam name="TF">The type of the filter values that implement the IEquatable&lt;TF&gt; interface.</typeparam>
		/// <param name="exposer">The Exposer containing the value to compare.</param>
		/// <param name="values">A collection of values to compare against the exposed value.</param>
		/// <returns>A filter element that represents the equality condition for any value in the collection.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="exposer"/> or <paramref name="values"/> is null.</exception>
		public static FilterElement<T> EqualMultiple<T, TF>(this Exposer<T, TF> exposer, IEnumerable<TF> values) where TF : IEquatable<TF>
		{
			if (exposer == null)
			{
				throw new ArgumentNullException(nameof(exposer));
			}

			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			values = values as ICollection<TF> ?? values.ToList();
			if (!values.Any())
			{
				return new TRUEFilterElement<T>();
			}

			var filters = new List<FilterElement<T>>();
			foreach (TF value in values)
			{
				filters.Add(exposer.Equal(value));
			}

			return new ORFilterElement<T>(filters.ToArray());
		}

		/// <summary>
		/// Creates a filter element that checks if the value exposed by the specified Exposer is equal to any of the values in the provided collection.
		/// </summary>
		/// <typeparam name="T">The type of object exposed by the Exposer.</typeparam>
		/// <param name="exposer">The Exposer containing the value to compare.</param>
		/// <param name="values">A collection of values to compare against the exposed value.</param>
		/// <returns>A filter element that represents the equality condition for any value in the collection.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="exposer"/> or <paramref name="values"/> is null.</exception>
		public static FilterElement<T> EqualMultiple<T>(this Exposer<T, object> exposer, IEnumerable<object> values)
		{
			if (exposer == null)
			{
				throw new ArgumentNullException(nameof(exposer));
			}

			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			values = values as ICollection<object> ?? values.ToList();
			if (!values.Any())
			{
				return new TRUEFilterElement<T>();
			}

			var filters = new List<FilterElement<T>>();
			foreach (object value in values)
			{
				filters.Add(exposer.Equal(value));
			}

			return new ORFilterElement<T>(filters.ToArray());
		}
	}
}