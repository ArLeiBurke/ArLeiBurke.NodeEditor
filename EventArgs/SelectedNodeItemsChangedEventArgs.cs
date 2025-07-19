using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArLeiBurke.NodeEditor.EventArgs
{
	public class SelectedNodeItemsChangedEventArgs :System.EventArgs
	{

		public object SelectedItem { get; set; }	

		public NotifyCollectionChangedAction Action { get; set; }	


		public SelectedNodeItemsChangedEventArgs(object SelectedItem, NotifyCollectionChangedAction action)
		{
			this.SelectedItem = SelectedItem;
			Action = action;
		}

	}
}
