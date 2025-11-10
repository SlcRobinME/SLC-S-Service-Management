namespace Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions
{
	using System;
	using System.Text;

	public static class StringExtensions
	{
		public static string Wrap(this string text, int lineWidth)
		{
			if (String.IsNullOrEmpty(text) || lineWidth < 1)
			{
				return text;
			}

			var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
			var sb = new StringBuilder();

			foreach (var originalLine in lines)
			{
				var words = originalLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				var currentLine = new StringBuilder();

				foreach (var word in words)
				{
					// If adding this word would exceed the limit, start a new line
					if (currentLine.Length + word.Length > lineWidth)
					{
						if (currentLine.Length > 0)
						{
							sb.AppendLine(currentLine.ToString().TrimEnd());
						}

						currentLine.Clear();
					}

					currentLine.Append(word + " ");
				}

				// Add remaining text in the current line
				if (currentLine.Length > 0)
				{
					sb.AppendLine(currentLine.ToString().TrimEnd());
				}
				else
				{
					sb.AppendLine(); // preserve empty lines
				}
			}

			return sb.ToString().TrimEnd(); // remove trailing newline
		}
	}
}