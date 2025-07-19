using PropertyChanged;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ArLeiBurke.NodeEditor
{

	[AddINotifyPropertyChangedInterface]
	public class Connection
	{

		public Point SourceLocation { get; set; }
		public Point TargetLocation { get; set; } 
		public Point SourceTransitionLocation { get; set; } 
		public Point TargetTransitionLocation { get; set; } 

		public Connector SourceConnector { get; set; }

		public Connector TargetConnector { get; set; }


		public Brush Color { get; set; } = Brushes.Orange;


		public Connection(Connector s, Connector t)
		{
			SourceConnector = s;
			TargetConnector = t;

			CreateEventSubscribe();
		}

		public Connection(Connector Source,Connector Target,Brush ConnectionColor) :this(Source,Target)
		{
			Color = ConnectionColor;
		}


		private void OnSourceLocationChanged(object sender, System.EventArgs e)
		{
			var Connector = sender as Connector;
			SourceLocation = Connector.Location;

			var temp = (SourceLocation.Y + TargetLocation.Y)/2;
			//下面这个 曲线比较平滑一点
			SourceTransitionLocation = new Point(SourceLocation.X, temp);
			TargetTransitionLocation = new Point(TargetLocation.X, temp);

			//下面这个曲线的话  有点陡峭！！！
			//SourceTransitionLocation = new Point(SourceLocation.X, TargetLocation.Y);
			//TargetTransitionLocation = new Point(TargetLocation.X, SourceLocation.Y);
		}

		private void OnTargetLocationChanged(object sender, System.EventArgs e)
		{
			var Connector = sender as Connector;
			TargetLocation = Connector.Location;

			var temp = (SourceLocation.Y + TargetLocation.Y) / 2;

			SourceTransitionLocation = new Point(SourceLocation.X, temp);
			TargetTransitionLocation = new Point(TargetLocation.X, temp);

			//SourceTransitionLocation = new Point(SourceLocation.X, TargetLocation.Y);
			//TargetTransitionLocation = new Point(TargetLocation.X, SourceLocation.Y);

		}

		private void CreateEventSubscribe()
		{
			SourceConnector.LocationChanged += OnSourceLocationChanged;
			TargetConnector.LocationChanged += OnTargetLocationChanged;
		}

		public void DeleteEventSubscribe()
		{
			SourceConnector.LocationChanged -= OnSourceLocationChanged;
			TargetConnector.LocationChanged -= OnTargetLocationChanged;
		}


	}

}
