namespace SLC_SM_IAS_ManageRelationships.Controller
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public class IssueCollector
	{
		private readonly List<string> _issues = new List<string>();

		public bool HasIssues => _issues.Count > 0;

		public void Add(string message)
		{
			_issues.Add(message);
		}

		public void Add(Exception ex)
		{
			_issues.Add(ex.Message);
		}

		public string PrintReport()
		{
			var sb = new StringBuilder();
			sb.AppendLine("Summary of connection remarks:\n");
			foreach (var issue in _issues)
			{
				sb.AppendLine($"\t⚠\t {issue}");
			}

			return sb.ToString();
		}
	}
}