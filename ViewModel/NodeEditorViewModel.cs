using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArLeiBurke.NodeEditor.ViewModel
{


	public abstract class NodeEditorViewModel<T>
	{
		public ObservableCollection<T> Nodes { get; set; }

		public abstract ICommand OnCreateConnectionCommand { get; set; }

		public abstract ICommand OnDeleteConnectionCommand { get; set; }

		public virtual void InitializeNodes() { }

		public NodeEditorViewModel()
		{
			Nodes = new ObservableCollection<T>();
			InitializeNodes();
		}

	}
}
