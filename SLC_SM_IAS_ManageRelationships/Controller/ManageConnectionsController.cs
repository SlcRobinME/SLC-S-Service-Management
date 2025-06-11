namespace SLC_SM_IAS_ManageRelationships.Controller
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using SLC_SM_IAS_ManageRelationships.Model;
	using SLC_SM_IAS_ManageRelationships.View;
	using SLCSMIASManageRelationships;

	public class ManageConnectionsController
	{
		private IEngine _engine;
		private InteractiveController _controller;
		private ManageConnectionsModel _model;
		private ManageConnectionDialog _dialog;
		private List<ServiceItemLinkMap> _linkMap;
		private ScriptData _data;

		private int _nextPairIndex = 0;

		public ManageConnectionsController(IEngine engine, ScriptData data)
		{
			_engine = engine;
			_data = data;
			_controller = new InteractiveController(engine);
			_model = new ManageConnectionsModel(engine);
			_linkMap = new List<ServiceItemLinkMap>();
		}

		public EventHandler<EventArgs> OnCancel => (sender, args) => _engine.ExitSuccess(string.Empty);

		public EventHandler<EventArgs> OnValidate => (sender, args) => Validate_Pressed();

		public EventHandler<EventArgs> OnPrevious => (sender, args) => Previous_Pressed();

		public void HandleNext()
		{
			while (HasMorePairs())
			{
				var currentPair = GetNextValidPair();
				if (currentPair == null)
					break;

				if (currentPair.IsOneToOne)
				{
					HandleOneToOnePair(currentPair);
					continue;
				}

				ShowDialogForPair(currentPair);
				return;
			}

			Save();
		}

		public void BuildLinkMap()
		{
			var instanceBase = _model.GetDomInstance(_data.DomId);
			var serviceItemNodes = _model.GetServiceItems(instanceBase, _data.ServiceIds);
			var sequentialPairs = _model.ToSequentialPairs(serviceItemNodes);

			var connections = sequentialPairs.Select(pair => CreateLinkMapFromPair(instanceBase, pair));

			foreach (var connection in connections)
				_linkMap.Add(connection);
		}

		public void CreateServiceItemFromWorkflow()
		{
			_data.ServiceIds.Add(_model.CreateServiceItemFromWorkflow(_data.DomId, _data.WorkflowName));
		}

		private bool HasMorePairs()
		{
			return _nextPairIndex < _linkMap.Count;
		}

		private ServiceItemLinkMap GetNextValidPair()
		{
			while (_nextPairIndex < _linkMap.Count)
			{
				var pair = _linkMap[_nextPairIndex++];
				if (pair.HasSources && pair.HasDestinations)
					return pair;
			}

			return null;
		}

		private void HandleOneToOnePair(ServiceItemLinkMap pair)
		{
			CreateOneToOneLink(pair);

			if (IsLast())
				Save();
		}

		private void ShowDialogForPair(ServiceItemLinkMap pair)
		{
			ConfigureDialog(pair);
			_controller.ShowDialog(_dialog);
		}

		private void CreateOneToOneLink(ServiceItemLinkMap currentPair)
		{
			var sourceInterface = currentPair.AvailableSources.Single();
			var destinationInterface = currentPair.AvailableDestinations.Single();

			if (!currentPair.HasLink(sourceInterface, destinationInterface))
				currentPair.AddLink(sourceInterface.NodeID, destinationInterface.NodeID);
		}

		private void ConfigureDialog(ServiceItemLinkMap currentPair)
		{
			var canShowNext = CanShowNext();
			var canShowPrevious = CanShowPrevious();

			_dialog = new ManageConnectionDialog(_engine, new ServiceItemLinkMapContext()
			{
				Pair = currentPair,
				ShowNext = canShowNext,
				ShowPrevious = canShowPrevious,
			});
			_dialog.ButtonValidate.Pressed += OnValidate;
			_dialog.ButtonCancel.Pressed += OnCancel;
			_dialog.ButtonPrevious.Pressed += OnPrevious;
		}

		private bool CanShowNext() => HasNavigablePair(i => i < _linkMap.Count, i => i + 1, _nextPairIndex);

		private bool CanShowPrevious() => HasNavigablePair(i => i >= 0, i => i - 1, _nextPairIndex - 2);

		private bool IsLast() => _nextPairIndex == _linkMap.Count();

		private bool HasNavigablePair(Func<int, bool> condition, Func<int, int> increment, int startIndex)
		{
			for (int i = startIndex; condition(i); i = increment(i))
			{
				var pair = _linkMap[i];
				if (pair.HasSources && !pair.IsOneToOne)
					return true;
			}

			return false;
		}

		private ServiceItemLinkMap CreateLinkMapFromPair(DomInstanceBase instance, (ServiceItemsSection, ServiceItemsSection) pair)
		{
			var links = _model.FindRelationshipsBetweenPair(instance, pair);

			var sourceWorkflowName = pair.Item1.DefinitionReference;
			var destinationWorkflowName = pair.Item2.DefinitionReference;

			var sourceWorkflow = _model.GetWorkflowbyName(sourceWorkflowName);
			var destinationWorkflow = _model.GetWorkflowbyName(destinationWorkflowName);

			return new ServiceItemLinkMap
			{
				SourceNode = pair.Item1,
				DestinationNode = pair.Item2,
				AvailableSources = _model.GetAvailableOutputs(pair.Item1, sourceWorkflow),
				AvailableDestinations = _model.GetAvailableInputs(pair.Item2, destinationWorkflow),
				Links = links,
			};
		}

		private void Previous_Pressed()
		{
			_dialog.UpdatePairFromDialog();
			_nextPairIndex -= 2;
			HandleNext();
		}

		private void Validate_Pressed()
		{
			_dialog.UpdatePairFromDialog();
			HandleNext();
		}

		private void Save()
		{
			_model.Update(_linkMap);
			_engine.ExitSuccess(string.Empty);
		}
	}
}
