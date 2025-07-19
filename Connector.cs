using ArLeiBurke.NodeEditor.EventArgs;
using PropertyChanged;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace ArLeiBurke.NodeEditor
{

	[AddINotifyPropertyChangedInterface]
	public class Connector : Control
	{
		static Connector()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Connector), new FrameworkPropertyMetadata(typeof(Connector)));
		}

		public Connector()
		{
			this.Loaded += Connector_Loaded;
		}


		public event EventHandler LocationChanged;

		// 当前 Connector 所在 的Editor
		private NodeEditor Editor { get; set; }

		// 当前 Connector 所属的 NodeEditorContainer
		public NodeEditorContainer EditorContainer { get; set; }

		private Panel MainPanel { get; set; }

		public void RefreshConnector()
		{
			Location = GetLocation();
			LocationChanged?.Invoke(this, null);

		}
		public static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location", typeof(Point), typeof(Connector), new PropertyMetadata(default(Point)));

		public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(Connector), new PropertyMetadata(Brushes.Blue));

		public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(Connector), new PropertyMetadata(default(string)));


		public Point Location
		{
			get { return (Point)GetValue(LocationProperty); }
			set { SetValue(LocationProperty, value); }
		}

		public Brush Stroke
		{
			get { return (Brush)GetValue(StrokeProperty); }
			set { SetValue(StrokeProperty, value); }
		}

		public string Category
		{
			get { return (string)GetValue(CategoryProperty); }
			set { SetValue(CategoryProperty, value); }
		}


		public static readonly RoutedEvent ConnectorDragStartEvent = EventManager.RegisterRoutedEvent("ConnectorDragStart", RoutingStrategy.Bubble, typeof(EventHandler<ConnectorDragStartEventArgs>), typeof(Connector));

		public static readonly RoutedEvent ConnectorDragCompletedEvent = EventManager.RegisterRoutedEvent("ConnectorDragCompleted", RoutingStrategy.Bubble, typeof(EventHandler<ConnectorDragCompletedEventArgs>), typeof(Connector));

		public event EventHandler<ConnectorDragStartEventArgs> ConnectorDragStart
		{
			add { this.AddHandler(ConnectorDragStartEvent, value); }
			remove { this.RemoveHandler(ConnectorDragStartEvent, value); }
		}

		public event EventHandler<ConnectorDragCompletedEventArgs> ConnectorDragCompleted
		{
			add { this.AddHandler(ConnectorDragCompletedEvent, value); }
			remove { this.RemoveHandler(ConnectorDragCompletedEvent, value); }
		}
		public void PrintParent(DependencyObject element)
		{
			DependencyObject parent = VisualTreeHelper.GetParent(element);
			if (parent != null)
			{
				//Debug.WriteLine(parent.GetType().Name);
				// 递归打印父元素
				PrintParent(parent);
			}
		}


		Point GetLocation()
		{
			GeneralTransform transform = this.TransformToAncestor(Editor.ItemsHost); ;
			Point positionInCanvas = transform.Transform(new Point(0, 0));
			// 添加补偿！！！
			var ConnectorRendersize = this.RenderSize;
			Vector Offset = new Vector(0, ConnectorRendersize.Height / 2);
			positionInCanvas += Offset;
			return positionInCanvas;
		}

		private void Connector_Loaded(object sender, RoutedEventArgs e)
		{
			RefreshConnector();
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			EditorContainer = this.GetParentOfType<NodeEditorContainer>();
			Editor = EditorContainer?.Editor ?? this.GetParentOfType<NodeEditor>();
			MainPanel = Editor.ItemsHost;
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				RaiseEvent(new ConnectorDragStartEventArgs(ConnectorDragStartEvent, this));
				e.Handled = true;
			}
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Released)
			{
				//GeneralTransform transform = this.TransformToAncestor(Editor.ItemsHost);
				//Point positionInCanvas = transform.Transform(new Point(0, 0));
				//Debug.WriteLine("End Point " + positionInCanvas);

				RaiseEvent(new ConnectorDragCompletedEventArgs(ConnectorDragCompletedEvent, this));
				e.Handled = true;
			}
		}
	}


}
