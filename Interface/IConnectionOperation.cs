
namespace ArLeiBurke.NodeEditor.Interface
{
	public interface IConnectionOperation
	{

		public void CreateConnection(Connector Source,Connector Target);

		public void DeleteConnection(Connector SelectedConnector);	

		public void DeleteConnection(Connection SelectedConnection);	


	}
}
