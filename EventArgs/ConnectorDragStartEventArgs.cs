using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArLeiBurke.NodeEditor.EventArgs
{
	public class ConnectorDragStartEventArgs : RoutedEventArgs
	{
		public Connector Connector { get; set; }

		public ConnectorDragStartEventArgs(RoutedEvent routedEvent, Connector Sneder) : base(routedEvent)
		{
			Connector = Sneder;
		}
	}
}
