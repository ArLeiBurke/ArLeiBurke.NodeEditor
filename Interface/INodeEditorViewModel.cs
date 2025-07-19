using ArLeiBurke.NodeEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace ArLeiBurke.NodeEditor.Interface
{
    public interface INodeEditorViewModel<T>
    {

		// 主控件
		NodeEditor MainEditor { get; set; }

		// 主控件所需要绑定的集合！！！
		ObservableCollection<T> Nodes { get; set; }

		ICommand OnCreateConnectionCommand { get; set; }

		ICommand OnDeleteConnectionCommand { get; set; }

		void InitializeNodes();

		// 执行反序列化！！！！
		void ExecuteDeSerialize();

		void InitializeCommand();

		void OnNodesChanged(T NewValue);




	}
}
