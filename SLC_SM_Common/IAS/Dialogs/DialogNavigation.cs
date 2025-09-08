namespace Skyline.DataMiner.Utils.MediaOps.Common.IAS.Dialogs
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class DialogNavigation
	{
		private readonly Stack<Dialog> _stack = new Stack<Dialog>();

		public DialogNavigation(InteractiveController controller)
		{
			Controller = controller ?? throw new ArgumentNullException(nameof(controller));
		}

		public InteractiveController Controller { get; }

		public void ShowDialog(Dialog dialog)
		{
			if (dialog == null)
			{
				throw new ArgumentNullException(nameof(dialog));
			}

			if (_stack.Count == 0 || _stack.Peek() != dialog)
			{
				_stack.Push(dialog);
			}

			Controller.ShowDialog(dialog);
			//if (Controller.IsRunning)
			//{
			//	Controller.ShowDialog(dialog);
			//}
			//else
			//{
			//	Controller.Run(dialog);
			//}
		}

		public void Back()
		{
			if (_stack.Count < 2)
			{
				throw new InvalidOperationException("No dialog found to go back");
			}

			_stack.Pop();
			ShowDialog(_stack.Peek());
		}

		public void Clear()
		{
			_stack.Clear();
		}

		public void RegisterBackButton(Button button)
		{
			if (button == null)
			{
				throw new ArgumentNullException(nameof(button));
			}

			button.Pressed += (s, e) => Back();
		}
	}
}
