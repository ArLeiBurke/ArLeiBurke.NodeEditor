using ArLeiBurke.Core.Service;
using System;
using System.Windows;

namespace ArLeiBurke.NodeEditor.Service
{
	public class SelectedNodeService: ISelectedNodeService
	{

		public void RegisterSelectedNodeService(UIElement Element, Action<object, RoutedEventArgs> action)
		{
			Element.AddHandler(NodeEditor.SelectedNodeChangedEvent, new RoutedEventHandler(action));

		}

	}
}
