using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArLeiBurke.NodeEditor.EventArgs
{

	//  响应路由事件的时候需要进行传递的参数！！！
	public class EventArgsGeneric<T> : RoutedEventArgs
	{
		public T Component { get; set;}		

		public EventArgsGeneric(T Input,RoutedEvent Event):base(Event)
		{
			Component = Input;
		}

	}
}
