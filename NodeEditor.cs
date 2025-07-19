using ArLeiBurke.Core.Interface;
using ArLeiBurke.NodeEditor.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;



namespace ArLeiBurke.NodeEditor
{

	public class NodeEditor : ItemsControl
	{
		/*
		 下面是要实现的几个功能
		1. NodeEditor 上面节点的序列化和反序列化！！！

		2025.4.10
		有几个小bug 
		1.在相机界面 跟 NodeEditor 界面来回切换的时候，NodeEditor 中的 Container 都会跑到初始位置去，，而且 Connection 还会乱跑。
			这个问题解决了， NodeEditorContainer 的 默认Style 我有通过Setter 的方式 设置 Location 依赖属性(通过Binding 跟 TreeNode 中的Location 属性进行绑定)，但是我没有设置 Mode = TwoWay ! 设置完之后就好了。就没有这个问题了
		2.切回到NodeEditor 界面的时候会重新调用  GetContainerForItemOverride() 这个方法，导致我先前创建的 Connection 和 NodeEditorContainer全都没用了！！！然后我拖动NodeEditorContainer ，先前创建的Connection 是不会跟着一起动的
		 切换页面的时候还会导致 NodeEditor 的 ItemsSource 属性发生改变！！！
		 切换页面的时候倒不会导致 NodeEditor 的 DataContext 属性发生改变！
		目前能够想到的解决方案就是将 先前创建的 NodeEditorContainer 给 缓存起来，下次直接就是用！！！
		
		2025.4.11
			今天上午解决了昨天 "切换页面会导致NodeEditor 重新调用 GetContainerForItemOverride()"  通过缓存的方式解决了这个问题！
		1. NodeEditor 这个小控件还稍微有点迟CPU和 GPU资源，当我快速拖动整个界面的时候会 出现这种情况！！！ 这个可以尝试优化一下！
		 
		 */

		private static Type THIS_TYPE => typeof(NodeEditor);

		#region 事件聚合器！！！！

		public IEventAggregator Aggregator
		{
			get { return (IEventAggregator)GetValue(AggregatorProperty); }
			set { SetValue(AggregatorProperty, value); }
		}

		public static readonly DependencyProperty AggregatorProperty = DependencyProperty.Register("Aggregator", typeof(IEventAggregator), THIS_TYPE, new PropertyMetadata(default(IEventAggregator)));

		#endregion

		public string ID { get; set; }

		protected internal Panel ItemsHost { get; private set; }
		protected const string ElementItemsHost = "PART_ItemsHost";



		#region NavigationControl 上面会用到的两个属性！
		public string DisplayName => "Node Editor";

		public object GetControlInstance()
		{
			return default(object);
		}

		#endregion


		static NodeEditor()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeEditor), new FrameworkPropertyMetadata(typeof(NodeEditor)));
			// ItemsSource 发生改变的时候 会触发  OnItemsSourceChanged 这个方法，，通过这种方式能够监听一些  依赖属性！！！
			ItemsSourceProperty.OverrideMetadata(typeof(NodeEditor), new FrameworkPropertyMetadata(null, OnItemsSourceChanged));
		}

		// 不能删  预留！！！
		private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }


		public NodeEditor()
		{

			BindingOperations.SetBinding(this, ItemsSourceProperty, new Binding("BindingCollections") { Source = this, Mode = BindingMode.TwoWay });

			AddHandler(NodeEditorContainer.IsSelectedChangedEvent, new RoutedEventHandler(OnIsSelectedChanged));

			AddHandler(NodeEditorContainer.DragStartedEvent, new DragStartedEventHandler(OnItemsDragStarted));
			AddHandler(NodeEditorContainer.DragCompletedEvent, new DragCompletedEventHandler(OnItemsDragCompleted));
			AddHandler(NodeEditorContainer.DragDeltaEvent, new DragDeltaEventHandler(OnItemsDragDelta));
			// 双击 NodeEditor 上面的 某个 节点的时候，就会触发下面的 这个事件！！！然后会将 双击选中的节点给传递到NodeEditor 里面来
			AddHandler(NodeEditorContainer.MouseDoubleClickEvent, new RoutedEventHandler(OnNodeEditorContainerMouseDoubleClick));

			AddHandler(Connector.ConnectorDragStartEvent, new EventHandler<ConnectorDragStartEventArgs>(OnConnectorDragStart));
			AddHandler(Connector.ConnectorDragCompletedEvent, new EventHandler<ConnectorDragCompletedEventArgs>(OnConnectorDragCompleted));

			SetValue(ConnectionsProperty, new ObservableCollection<Connection>());

			var transform = new TransformGroup();
			transform.Children.Add(ScaleTransform);
			transform.Children.Add(TranslateTransform);

			SetValue(ViewportTransformPropertyKey, transform);

			DataContextChanged += NodeEditor_DataContextChanged;

		}




		#region  NodeEditor 背景 相关的依赖属性！！！

		// 显示在 NodeEditor 上面的 水印. 哈哈哈哈哈哈哈
		public Brush WaterMark
		{
			get { return (Brush)GetValue(WaterMarkProperty); }
			set { SetValue(WaterMarkProperty, value); }
		}

		// NodeEditor 上面大的十字网格
		public Brush LargeGridLines
		{
			get { return (Brush)GetValue(LargeGridLinesProperty); }
			set { SetValue(LargeGridLinesProperty, value); }
		}

		// NodeEditor 上面小的十字网格
		public Brush SmallGridLines
		{
			get { return (Brush)GetValue(SmallGridLinesProperty); }
			set { SetValue(SmallGridLinesProperty, value); }
		}

		public static readonly DependencyProperty WaterMarkProperty = DependencyProperty.Register("WaterMark", typeof(Brush), THIS_TYPE, new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty LargeGridLinesProperty = DependencyProperty.Register("LargeGridLines", typeof(Brush), THIS_TYPE, new PropertyMetadata(default(Brush)));

		public static readonly DependencyProperty SmallGridLinesProperty = DependencyProperty.Register("SmallGridLines", typeof(Brush), THIS_TYPE, new PropertyMetadata(default(Brush)));

		#endregion







		private void NodeEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{

		}

		public ObservableCollection<object> BindingCollections
		{
			get { return (ObservableCollection<object>)GetValue(BindingCollectionsProperty); }
			set { SetValue(BindingCollectionsProperty, value); }
		}

		public static readonly DependencyProperty BindingCollectionsProperty = DependencyProperty.Register("BindingCollections", typeof(ObservableCollection<object>), typeof(NodeEditor), new PropertyMetadata(null, OnBindingCollectionsChanged));


		private static void OnBindingCollectionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }



		#region 删除选中节点！！！

		protected override void OnKeyDown(KeyEventArgs e)
		{
			var Key = e.Key;

			//只有当前的 NodeEditor 获取到 焦点的时候，然后按下键盘上的键，才会进入到此方法里面来，，要不然不会进来的！！！！
			base.OnKeyDown(e);

			if (Key is Key.Delete)
			{
				/*
				1.删除节点之间的Mapping
				2.删除节点所关联的项
				3.删除Connection
				
				下面这几个方法 可以全都标记成异步的形式！！								特别声明一下！！！虽然说我把 NodeEditorContainer 和 Connections 给删除掉了，，但是他们并没有被GC回收掉，因为彼此之间有强引用关系,所以这个要特别注意一下！！！
				 */
				DeleteConnectionCommand?.Execute(null);
				DeleteConnections();
				DeleteSelectedNodeContainer();
			}

		}

		// 删除选中的节点！  删除选中节点的同时，也会将 NodeEditorContainer 所包裹的 Item 给删除掉！！！
		void DeleteSelectedNodeContainer()
		{

			foreach (var EditorContainer in SelectedNodeItems)
			{

				// 先判断一下 集合里面 是否存在对应的 Item
				var temp = BindingCollections.Contains(EditorContainer.DataContext);
				BindingCollections.Remove(EditorContainer.DataContext);
			}

			SelectedNodeItems.Clear();
		}

		// 删除节点所对应的 Connection
		void DeleteConnections()
		{
			// 简单的测试了一下，是没有问题的。 如果后面删掉 NodeEditorContainer,但是没有删除掉 Connection 的话，肯定就是下面的代码导致的问题！！！
			foreach (var EditorContainer in SelectedNodeItems)
			{
				List<Connection> DeleteConnections = new List<Connection>();

				for (int i = 0; i < Connections.Count; i++)
				{
					if (Connections[i].SourceConnector.EditorContainer == EditorContainer || Connections[i].TargetConnector.EditorContainer == EditorContainer)
						DeleteConnections.Add(Connections[i]);
				}

				foreach (var item in DeleteConnections)
				{
					Connections.Remove(item);
				}

			}
		}

		#endregion

		public static readonly RoutedEvent SelectedNodeChangedEvent = EventManager.RegisterRoutedEvent(nameof(SelectedNodeChangedEvent), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NodeEditor));

		public event DragStartedEventHandler SelectedNodeChanged
		{
			add => AddHandler(SelectedNodeChangedEvent, value);
			remove => RemoveHandler(SelectedNodeChangedEvent, value);
		}


		// 每次选中和取消节点的时候 都会触发一下 下面的这个节点！！！
		public event EventHandler<SelectedNodeItemsChangedEventArgs> SelectedNodeItemChangedEvent;


		private void OnIsSelectedChanged(object sender, RoutedEventArgs e)
		{
			NodeEditorContainer Container = e.OriginalSource as NodeEditorContainer;

			if (Container.IsSelected)
				AddSelectedNodeItems(Container);
			else
				RemoveSelectedNodeItems(Container);
		}

		void AddSelectedNodeItems(NodeEditorContainer Item)
		{
			SelectedNodeItems.Add(Item);
			RaiseSelectedItemsChanged(NotifyCollectionChangedAction.Add, Item.DataContext);
			RaiseEvent(new RoutedEventArgs()
			{
				RoutedEvent = NodeEditor.SelectedNodeChangedEvent,
			});

			// 通知一下其他类型的控件， NodeEditor SelectedNode 已经发生了改变！
			Aggregator.Publish<object>(Item.DataContext);

		}

		void RemoveSelectedNodeItems(NodeEditorContainer Item)
		{
			SelectedNodeItems.Remove(Item);
			RaiseSelectedItemsChanged(NotifyCollectionChangedAction.Remove, Item.DataContext);
			//RaiseEvent(new RoutedEventArgs()
			//{
			//	RoutedEvent = NodeEditor.SelectedNodeChangedEvent,
			//});


			// 如果想要在  未选中任何节点的时候  PropertyGrid 界面上 什么都不显示的话 就启用下面的这行代码！！！
			//Aggregator.Publish<object>(default(object));
		}

		// 选中的节点发生变化的话，要通知一下DockView，让DockView 切换对应的界面！！！！
		private void RaiseSelectedItemsChanged(NotifyCollectionChangedAction Action, object Component)
		{
			SelectedNodeItemChangedEvent?.Invoke(this, new SelectedNodeItemsChangedEventArgs(Component, Action));
		}


		public static readonly DependencyProperty SelectedNodeItemsProperty = DependencyProperty.Register("SelectedNodeItems", typeof(IList<NodeEditorContainer>), typeof(NodeEditor), new PropertyMetadata(new List<NodeEditorContainer>(), OnSelectedNodeItemsChanged));

		// 当前 NodeEditor 中 所选中的节点！！！支持多选！
		public IList<NodeEditorContainer> SelectedNodeItems
		{
			get { return (IList<NodeEditorContainer>)GetValue(SelectedNodeItemsProperty); }
			set { SetValue(SelectedNodeItemsProperty, value); }
		}

		private static void OnSelectedNodeItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// Do Nothing !
		}



		public static readonly DependencyProperty ConnectionDragStartCommandProperty = DependencyProperty.Register("ConnectionDragStartCommand", typeof(ICommand), typeof(NodeEditor), new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty CreateConnectionCommandProperty = DependencyProperty.Register("CreateConnectionCommand", typeof(ICommand), typeof(NodeEditor), new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty DeleteConnectionCommandProperty = DependencyProperty.Register("DeleteConnectionCommand", typeof(ICommand), typeof(NodeEditor), new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty MouseDownCommandProperty = DependencyProperty.Register("MouseDownCommand", typeof(ICommand), typeof(NodeEditor), new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty MouseWheelCommandProperty = DependencyProperty.Register("MouseWheelCommand", typeof(ICommand), typeof(NodeEditor), new PropertyMetadata(default(ICommand)));

		public static readonly DependencyProperty CheckCreateConnectionConditionCommandProperty = DependencyProperty.Register("CheckCreateConnectionConditionCommand", typeof(ICommand), typeof(NodeEditor), new PropertyMetadata(default(ICommand)));

		public ICommand ConnectionDragStartCommand
		{
			get { return (ICommand)GetValue(ConnectionDragStartCommandProperty); }
			set { SetValue(ConnectionDragStartCommandProperty, value); }
		}

		public ICommand CreateConnectionCommand
		{
			get { return (ICommand)GetValue(CreateConnectionCommandProperty); }
			set { SetValue(CreateConnectionCommandProperty, value); }
		}

		public ICommand DeleteConnectionCommand
		{
			get { return (ICommand)GetValue(DeleteConnectionCommandProperty); }
			set { SetValue(DeleteConnectionCommandProperty, value); }
		}

		public ICommand MouseDownCommand
		{
			get { return (ICommand)GetValue(MouseDownCommandProperty); }
			set { SetValue(MouseDownCommandProperty, value); }
		}

		public ICommand MouseWheelCommand
		{
			get { return (ICommand)GetValue(MouseWheelCommandProperty); }
			set { SetValue(MouseWheelCommandProperty, value); }
		}

		public ICommand CheckCreateConnectionConditionCommand
		{
			get { return (ICommand)GetValue(CheckCreateConnectionConditionCommandProperty); }
			set { SetValue(CheckCreateConnectionConditionCommandProperty, value); }
		}



		public static readonly RoutedEvent ConnectionCreatingEvent = EventManager.RegisterRoutedEvent("ConnectionCreating", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NodeEditor));

		public event RoutedEventHandler ConnectionCreating
		{
			add { AddHandler(ConnectionCreatingEvent, value); }
			remove { RemoveHandler(ConnectionCreatingEvent, value); }
		}

		public static readonly RoutedEvent NodeEditorContainerMouseDoubleClickEvent = EventManager.RegisterRoutedEvent("NodeEditorContainerMouseDoubleClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NodeEditor));

		public event RoutedEventHandler NodeEditorContainerMouseDoubleClick
		{
			add { AddHandler(NodeEditorContainerMouseDoubleClickEvent, value); }
			remove { RemoveHandler(NodeEditorContainerMouseDoubleClickEvent, value); }
		}





		#region 创建 Connection 相关逻辑！！！

		Connector Source { get; set; }

		Connector Target { get; set; }

		public virtual void OnConnectorDragStart(object sender, ConnectorDragStartEventArgs e)
		{
			Source = e.Connector;
			ConnectionDragStartCommand?.Execute(Source.DataContext);
		}

		public virtual void OnConnectorDragCompleted(object sender, ConnectorDragCompletedEventArgs e)
		{
			Target = e.Connector;

			//true 表示 创建Connectioan ， false 的话 表示不满足条件，不创建Connection。
			bool? IsCreateConnection = CheckCondition(Source, Target);
			if (IsCreateConnection is false)
				return;


			IsCreateConnection = CheckConditionDelegate?.Invoke();
			if (IsCreateConnection is false)
				return;

			CreateConnection(Source, Target);
		}

		// 需要把 创建 Connection 相关的逻辑 给暴露出去！！！！
		public Func<bool> CheckConditionDelegate { get;set; }	

		// 检查是否满足 创建 连接的条件！
		private bool CheckCondition(Connector Source, Connector Target)
		{
			/*
			根据一下几个条件判断是否能够创建连接。	简单来讲就是只能把输出赋值给输入！！！！
			1.Target 不能为null
			2.不能把自己的输出赋值给自己的输入
			3.不能把 A Container 的输出 赋值给 B Container 的输出 (A输出-B输出)
			4.不能把自己的输出赋值给自己的输出。
			 */


			if (Target is null || Source is null)
				return false;
			// 下面这行代码 偶尔会报错，，不知道什么原因导致的！  简单的看了一下 ，有的时候 Source 会为null
			if (Source.Location == Target.Location)
				return false;

			if (Source.DataContext == Target.DataContext)
				return false;

			return true;
		}

		private void CreateConnection(Connector Source, Connector Target)
		{
			/*
			 先检查一下是否满足 创建Connection 的条件！

			 */
			var IsCreateConnection = CheckCreateConnectionCondition(Source, Target);
			if (IsCreateConnection is false)
				return;

			/*
			1.创建节点间的事件订阅关系 
			2.在Target节点创建Mapping
			3.UI上面创建 贝塞尔曲线  创建Connection
			 */

			//	1 2 3
			CreateConnectionCommand?.Execute(new[] { Source, Target });

			// 触发对应的路由事件！！！
			RoutedEventArgs args = new(ConnectionCreatingEvent, new[] { Source, Target });
			RaiseEvent(args);

			//	4
			//  创建节点的逻辑 我给迁移到外面去了！！！
			Connections.Add(new Connection(Source, Target, Brushes.Blue));

		}

		// 判断是否满足 创建 Connection 的条件！！！  True 的话 为满足，，满足的话 才能够 创建 Connection
		public virtual bool CheckCreateConnectionCondition(Connector Source, Connector Target)
		{
			/*
			 1. A节点的输入不能 跟 A节点的输入创建连接。
			2. A节点的输出不能 跟 A节点的输出创建连接
			 3.不允许两个节点之间 交叉创建连接，，比如 A的输出跟B的输入创建连接，B的输出跟A的输入创建连接！！！这总情况目前是不允许的！
			4. 如果两个节点之间已经创建了连接的话，，，是不允许多次创建的！！！！
			 
			不满足上述条件的话 还要弹窗警示一下 才可以！！！
			 */

			if (Source.Category == Target.Category)
				return false;



			return true;
		}

		#endregion



		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			ItemsHost = GetTemplateChild(ElementItemsHost) as Panel ?? throw new InvalidOperationException("PART_ItemsHost is missing or is not of type Panel.");

			/*
			 * 
			 设置 NodeEditor 的背景！！！缺少下面三行代码时，当缩放NodeEdiotr 的时候，网格背景不会跟着一起缩放！！！！

			2025.5.12
			简单的优化了一下，现在能够实现多个图层显示在界面上面了！！一般就三个图层
			1.背景颜色		NodeEditor 设置
			2.大网格			外Border 设置
			3.小网格			SmallBorder 设置
			 */
			DrawingBrush SmallGridLinesDrawingBrush = this.FindResource("SmallGridLinesDrawingBrush") as DrawingBrush;
			SmallGridLinesDrawingBrush.Transform = this.ViewportTransform;
			var SmallBorder = Template.FindName("SmallBorder", this) as Border;
			SmallBorder.Background = SmallGridLinesDrawingBrush;

			//this.Background = NodeEditorBackground;

		}


		#region 缓存一下 NodeEditor 创建的 NodeEditorContainer ，，每次切换回界面的时候直接用原先创建的，，新创建的就不用了！！！


		Dictionary<object,NodeEditorContainer > Cache = new Dictionary<object, NodeEditorContainer>();

		object SelectedObject;

		protected override DependencyObject GetContainerForItemOverride()
		{
			if (SelectedObject != null)
				return Cache[SelectedObject];

			throw new Exception("hello worod");

		}


		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			bool IsCreateNewContainer = item is NodeEditorContainer;
			if(!IsCreateNewContainer)
			{
				SelectedObject = item;
				// 如果有创建缓存的话 就不创建了！
				if(Cache.ContainsKey(SelectedObject))
				{
					return IsCreateNewContainer;
				}
				Cache.Add(item, new NodeEditorContainer(this));	
			}

			return IsCreateNewContainer;

		}

		// 下面是原有的两行旧代码！！！！
		//protected override DependencyObject GetContainerForItemOverride() => new NodeEditorContainer(this);
		//protected override bool IsItemItsOwnContainerOverride(object item) => item is NodeEditorContainer;



		#endregion


		protected readonly ScaleTransform ScaleTransform = new ScaleTransform();

		protected readonly TranslateTransform TranslateTransform = new TranslateTransform();

		protected static readonly DependencyPropertyKey ViewportTransformPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ViewportTransform), typeof(Transform), typeof(NodeEditor), new FrameworkPropertyMetadata(new TransformGroup()));

		public static readonly DependencyProperty ViewportTransformProperty = ViewportTransformPropertyKey.DependencyProperty;

		public static readonly DependencyProperty ViewportZoomProperty = DependencyProperty.Register(nameof(ViewportZoom), typeof(double), typeof(NodeEditor), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnViewportZoomChanged));

		public static readonly DependencyProperty CanvasPositionProperty = DependencyProperty.Register("CanvasPosition", typeof(Point), typeof(NodeEditor), new PropertyMetadata(default(Point)));

		public static readonly DependencyProperty ViewportLocationProperty = DependencyProperty.Register(nameof(ViewportLocation), typeof(Point), typeof(NodeEditor), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnViewportLocationChanged));

		public static readonly DependencyProperty ConnectionsProperty = DependencyProperty.Register("Connections", typeof(IList<Connection>), typeof(NodeEditor), new PropertyMetadata(default(IList<Connection>)));

		public static readonly DependencyProperty MousePositionProperty = DependencyProperty.Register("MousePosition", typeof(Point), typeof(NodeEditor), new PropertyMetadata(default(Point)));

		public static readonly DependencyProperty ConnectionTemplateProperty = DependencyProperty.Register("ConnectionTemplate", typeof(DataTemplate), typeof(NodeEditor), new PropertyMetadata(default(DataTemplate)));

		public static readonly DependencyProperty DisableMouseInteractionProperty = DependencyProperty.Register("DisableMouseInteraction", typeof(bool), typeof(NodeEditor), new PropertyMetadata(default(bool)));

		public DataTemplate ConnectionTemplate
		{
			get { return (DataTemplate)GetValue(ConnectionTemplateProperty); }
			set { SetValue(ConnectionTemplateProperty, value); }
		}

		public bool DisableMouseInteraction
		{
			get { return (bool)GetValue(DisableMouseInteractionProperty); }
			set { SetValue(DisableMouseInteractionProperty, value); }
		}

		public double ViewportZoom
		{
			get => (double)GetValue(ViewportZoomProperty);
			set => SetValue(ViewportZoomProperty, value);
		}

		public IList<Connection> Connections
		{
			get { return (IList<Connection>)GetValue(ConnectionsProperty); }
			set { SetValue(ConnectionsProperty, value); }
		}

		public Point MousePosition
		{
			get => (Point)GetValue(MousePositionProperty);
			set => SetValue(MousePositionProperty, value);
		}

		public Point CanvasPosition
		{
			get { return (Point)GetValue(CanvasPositionProperty); }
			set { SetValue(CanvasPositionProperty, value); }
		}

		public Point ViewportLocation
		{
			get => (Point)GetValue(ViewportLocationProperty);
			set => SetValue(ViewportLocationProperty, value);
		}

		// 拖拽 或者 缩放 都会进入到下面的这个方法里面来！！！
		private static void OnViewportLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var editor = (NodeEditor)d;
			var translate = (Point)e.NewValue;

			editor.TranslateTransform.X = -translate.X * editor.ViewportZoom;
			editor.TranslateTransform.Y = -translate.Y * editor.ViewportZoom;

		}

		private static void OnViewportZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var editor = (NodeEditor)d;
			double zoom = (double)e.NewValue;

			editor.ScaleTransform.ScaleX = zoom;
			editor.ScaleTransform.ScaleY = zoom;

		}

		public Transform ViewportTransform => (Transform)GetValue(ViewportTransformProperty);

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);
			//让当前 NodeEditor 获取焦点！！！ 这样子 当我选中某个节点的时候，然后按下键盘上的 Delete 键才会将其删除掉，要不然不会删除的！
			this.Focus();
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (DisableMouseInteraction)
				return;

			if (e.LeftButton == MouseButtonState.Pressed)
			{
				foreach (var item in Items)
				{
					var container = ItemContainerGenerator.ContainerFromItem(item) as NodeEditorContainer;
					container.IsSelected = false;
				}
			}

			if (e.RightButton == MouseButtonState.Pressed)
			{
				isDragging = true;
				startMousePosition = e.GetPosition(this); // 记录鼠标按下的起始位置
				ItemsHost.CaptureMouse(); // 捕获鼠标
			}
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (DisableMouseInteraction)
				return;

			if (e.RightButton == MouseButtonState.Released)
			{
				isDragging = false;
				ItemsHost.ReleaseMouseCapture(); // 释放鼠标捕获
			}

		}

		bool isDragging = false; // 是否正在拖动
		Point startMousePosition; // 鼠标按下的位置

		protected override void OnMouseMove(MouseEventArgs e)
		{
			MousePosition = e.GetPosition(null);
			//下面这个 转换出来的结果 怪怪的，，不知道是什么原因导致的！！！
			//CanvasPosition = ItemsHost.TranslatePoint(MousePosition, this);

			if (DisableMouseInteraction)
				return;

			if (isDragging)
			{
				// 获取当前鼠标位置
				Point currentMousePosition = e.GetPosition(this);

				// 计算鼠标移动的偏移量
				double offsetX = currentMousePosition.X - startMousePosition.X;
				double offsetY = currentMousePosition.Y - startMousePosition.Y;

				// 更新 Canvas 的位置
				TranslateTransform.X += offsetX;
				TranslateTransform.Y += offsetY;


				// 下面这行代码很重要，删除的话会导致错位的情况(先拖拽界面，然后再去缩放的话，会明显看到有错位的情况！！！)  下面的这行代码 <<非常重要！！！>> <<非常重要！！！>> <<非常重要！！！>>
				// 拖拽界面的时候 对应的参考坐标系已经发生改变了。。所以需要下面这行代码 更新一下！！！
				this.ViewportLocation -= (currentMousePosition - startMousePosition) / ViewportZoom;

				// 更新起始位置为当前位置
				startMousePosition = currentMousePosition;

			}

		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{

			double zoom = Math.Pow(2.0, e.Delta / 3.0 / Mouse.MouseWheelDeltaForOneLine);


			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
				return;

			ZoomAtPosition(zoom, e.GetPosition(ItemsHost));
			if (e.Source is NodeEditor)
				e.Handled = true;
		}

		public void ZoomAtPosition(double zoom, Point location)
		{
			//Debug.WriteLine(location);
			double prevZoom = ViewportZoom;
			ViewportZoom *= zoom;

			if (Math.Abs(prevZoom - ViewportZoom) > 0.001)
			{
				// get the actual zoom value because Zoom might have been coerced
				zoom = ViewportZoom / prevZoom;
				Vector position = (Vector)location;

				var dist = position - (Vector)ViewportLocation;
				var zoomedDist = dist * zoom;
				var diff = zoomedDist - dist;
				ViewportLocation += diff / zoom;
			}
		}

		Vector _dragAccumulator;

		List<NodeEditorContainer> SelectedContainer;

		private void OnItemsDragStarted(object sender, DragStartedEventArgs e)
		{
			List<NodeEditorContainer> ContainerCollections = new List<NodeEditorContainer>();

			foreach (var item in Items)
			{
				var Container = ItemContainerGenerator.ContainerFromItem(item) as NodeEditorContainer;
				ContainerCollections.Add(Container);
			}

			SelectedContainer = ContainerCollections.Where(container => container.IsSelected is true).ToList();

			_dragAccumulator = new Vector(0, 0);

		}

		private void OnItemsDragDelta(object sender, DragDeltaEventArgs e)
		{
			Vector change = new Vector(e.HorizontalChange, e.VerticalChange);
			_dragAccumulator += change;
			var delta = new Vector((int)_dragAccumulator.X, (int)_dragAccumulator.Y);
			_dragAccumulator -= delta;


			if (delta.X != 0 || delta.Y != 0)
			{
				for (var i = 0; i < SelectedContainer.Count; i++)
				{
					NodeEditorContainer container = SelectedContainer[i];
					var r = (TranslateTransform)container.RenderTransform;

					r.X += delta.X; // Snapping without correction
					r.Y += delta.Y; // Snapping without correction

				}
			}
		}


		// NodeEditor 中 双击某个节点的时候 会进入到下面的这个方法，由下面的这个方法 将 双击选中的节点 传递出去！！！
		private void OnNodeEditorContainerMouseDoubleClick(object sender, RoutedEventArgs e)
		{
			//RaiseEvent(new RoutedEventArgs(NodeEditorContainerMouseDoubleClickEvent));
		}

		private void OnItemsDragCompleted(object sender, DragCompletedEventArgs e)
		{

			if (SelectedContainer is null)
				return;
			for (var i = 0; i < SelectedContainer.Count; i++)
			{
				NodeEditorContainer container = SelectedContainer[i];
				var r = (TranslateTransform)container.RenderTransform;

				Point result = container.Location + new Vector(r.X, r.Y);

				// Correct the final position
				if (r.X != 0 || r.Y != 0)
				{
					result.X = (int)result.X;
					result.Y = (int)result.Y;
				}

				container.Location = result;

				r.X = 0;
				r.Y = 0;
			}

			SelectedContainer.Clear();
			ItemsHost.InvalidateArrange();
		}





		#region 需要暴露在 ToolBar 或者 Menu 上面的几个功能

		/*
		 详情见下
		1.保存节点配置	下面这几个都要单独序列化！！不能全都序列化成一个文件。序列化成一个的话 看起来太乱了。
			1.1 NodeEidotr 上面的节点(都有哪些类型的节点，节点在NodeEditor上面的坐标)
			1.2 每个节点的参数
			1.3 节点之间的绑定关系！！
		2.加载节点配置

		3.保存单个节点的参数
			3.1	 做成一个单独的文件，到时候可以服用！！！
		4.加载单个节点的参数

		5.
		 
		 */


		#endregion





	}

}
