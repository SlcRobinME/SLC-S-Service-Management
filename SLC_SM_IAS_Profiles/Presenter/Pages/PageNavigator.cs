namespace SLC_SM_IAS_Profiles.Presenters
{
	using System.Collections.Generic;
	using System.Linq;

	public interface IReadOnlyNavigator
	{
		IEnumerable<DataRecordPage> GetPathPages();

		IEnumerable<DataRecord> GetAllRecords();

		DataRecordPage GetCurrentPage();

		bool CanGoBack();
	}

	public class PageNavigator : IReadOnlyNavigator
	{
		private DataRecordPage _root;

		public DataRecordPage CurrentPage { get; private set; }

		public bool CanGoBack => CurrentPage?.Previous != null;

		public IEnumerable<DataRecordPage> GetPathPages()
		{
			var visited = new HashSet<DataRecordPage>();
			foreach (var page in Traverse(_root, visited))
			{
				yield return page;
			}
		}

		public IEnumerable<DataRecord> GetAllRecords() => GetPathPages().SelectMany(p => p.Records);

		public DataRecordPage CreateRootPage(IEnumerable<DataRecord> records)
		{
			var root = new RootPage(records);
			_root = root;
			CurrentPage = root;
			return root;
		}

		public void AddRecordToCurrentPage(DataRecord record)
		{
			CurrentPage.AddRecord(record);
		}

		public DataRecordPage PushChildPage(ProfileDataRecord parentRecord, List<DataRecord> records)
		{
			if (CurrentPage == null)
				return CreateRootPage(records);

			var parentId = parentRecord.Profile.ID;
			var child = CurrentPage.Children
				.OfType<ProfilePage>()
				.FirstOrDefault(p => p.ProfileDataRecord.Profile.ID == parentId);

			if (child == null)
			{
				child = new ProfilePage(records);
				CurrentPage.AddChild(child);
			}
			else
			{
				child.SetRecords(records);
			}

			child.SetProfileDefinition(parentRecord);
			return CurrentPage = child;
		}

		public void GoBack(List<DataRecord> records)
		{
			if (CanGoBack)
				CurrentPage = CurrentPage.Previous;

			CurrentPage.SetRecords(records);
		}

		public DataRecordPage GetCurrentPage()
		{
			return CurrentPage;
		}

		bool IReadOnlyNavigator.CanGoBack()
		{
			return CanGoBack;
		}

		private IEnumerable<DataRecordPage> Traverse(DataRecordPage page, HashSet<DataRecordPage> visited)
		{
			if (page == null || visited.Contains(page))
				yield break;

			visited.Add(page);
			yield return page;

			foreach (var child in page.Children)
			{
				foreach (var c in Traverse(child, visited))
				{
					yield return c;
				}
			}
		}
	}
}
