using ArLeiBurke.NodeEditor.EventArgs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ArLeiBurke.NodeEditor
{

	public class NodeEditorContainer : ContentControl
	{
		static NodeEditorContainer()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeEditorContainer), new FrameworkPropertyMetadata(typeof(NodeEditorContainer)));
		}

		public NodeEditor Editor { get; set; }

		public NodeEditorContainer(NodeEditor editor)
		{
			Editor = editor;
			RenderTransform = new TranslateTransform();

			ConnectorCollections = new List<Connector>();


			this.LayoutUpdated += NodeEditorContainer_LayoutUpdated;
			this.Unloaded += NodeEditorContainer_Unloaded;


		}

		
		private void NodeEditorContainer_Unloaded(object sender, RoutedEventArgs e)
		{
			// 当NodeEditor 上面的某个Item 被删除的时候，与该项相关联的容器（即 ItemTemplate 中定义的内容）会被从视觉树中移除。

		}

		private bool Initialized = false;

		private void NodeEditorContainer_LayoutUpdated(object? sender, System.EventArgs e)
		{

			/*
			 有几个疑问
			1.为什么我鼠标放在这界面上面，仅仅是滑动了几下，就触发了 LayoutUpdated 事件，，这是什么情况呢？？
			 
			 */


			// 当我删除 NodeEditor 上面的 某个 Item 的时候！！！就会进入到 这个方法里面来！！！！
			if (this.DataContext.ToString() == "{DisconnectedItem}")
			{
				//Debug.WriteLine("DataContext is DisconnectedItem");
				return;
			}

			//只会加载一次！
			if (Initialized == false)
			{
				// 找到 NodeEditorContainer 下面所有的 Connector ，，然后添加到指定的集合里面去！
				var ChildrenOfTypeConnector = VisualTreeHelperExtensions.FindChildrenOfType<Connector>(this);
				if (ChildrenOfTypeConnector.Count == 0)
					return;
				ConnectorCollections.AddRange(ChildrenOfTypeConnector);
				Initialized = true;

			}
			else
			{
				RefreshConnectorLocation(ConnectorCollections);
				//RefreshNodeEditorContainerLocation();

			}

		}

		// 刷新一下 NodeEditorContainer 的 Location 属性！！！
		private void RefreshNodeEditorContainerLocation()
		{
			/*
			 没有这个方法的话，，，会有下面几种影响！
			1.当我切换界面的时候，NodeEditor 中 NodeContainer 的坐标会跑到初始位置！ 窗口的左上角！
			 
			 */
			var Position = this.TransformToAncestor(Editor.ItemsHost).Transform(new Point(0, 0));
			this.Location = Position;

		}

		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(NodeEditorContainer), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{

			var container = (d as NodeEditorContainer);

			bool NewValue = (bool)e.NewValue;

			container.RaiseEvent(new RoutedEventArgs(IsSelectedChangedEvent, container));
		}


		public static readonly RoutedEvent IsSelectedChangedEvent = EventManager.RegisterRoutedEvent("IsSelectedChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NodeEditor));

		public event RoutedEventHandler IsSelectedChanged
		{
			add { AddHandler(IsSelectedChangedEvent, value); }
			remove { RemoveHandler(IsSelectedChangedEvent, value); }
		}





		public static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location", typeof(Point), typeof(NodeEditorContainer), new PropertyMetadata(default(Point)));

		public static readonly RoutedEvent DragStartedEvent = EventManager.RegisterRoutedEvent(nameof(DragStarted), RoutingStrategy.Bubble, typeof(DragStartedEventHandler), typeof(NodeEditorContainer));

		public static readonly RoutedEvent DragCompletedEvent = EventManager.RegisterRoutedEvent(nameof(DragCompleted), RoutingStrategy.Bubble, typeof(DragCompletedEventHandler), typeof(NodeEditorContainer));

		public static readonly RoutedEvent DragDeltaEvent = EventManager.RegisterRoutedEvent(nameof(DragDelta), RoutingStrategy.Bubble, typeof(DragDeltaEventHandler), typeof(NodeEditorContainer));

		public static readonly RoutedEvent RefreshConnectorEvent = EventManager.RegisterRoutedEvent(nameof(RefreshConnector), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NodeEditorContainer));

		public event DragDeltaEventHandler RefreshConnector
		{
			add => AddHandler(RefreshConnectorEvent, value);
			remove => RemoveHandler(RefreshConnectorEvent, value);
		}

		public event DragStartedEventHandler DragStarted
		{
			add => AddHandler(DragStartedEvent, value);
			remove => RemoveHandler(DragStartedEvent, value);
		}

		public event DragDeltaEventHandler DragDelta
		{
			add => AddHandler(DragDeltaEvent, value);
			remove => RemoveHandler(DragDeltaEvent, value);
		}

		public event DragCompletedEventHandler DragCompleted
		{
			add => AddHandler(DragCompletedEvent, value);
			remove => RemoveHandler(DragCompletedEvent, value);
		}

		public bool IsSelected
		{
			get { return (bool)GetValue(IsSelectedProperty); }
			set { SetValue(IsSelectedProperty, value); }
		}

		public Point Location
		{
			get { return (Point)GetValue(LocationProperty); }
			set { SetValue(LocationProperty, value); }
		}


		bool IsDragging;
		Point InitializePosition;
		Point CurrentPosition;
		Point PreviousPosition;



		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				this.CaptureMouse();

				// 通过按住 Ctrl 键的方式 可以选择多个 Container ！！！！
				if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					foreach (var item in Editor.Items)
					{
						var container = Editor.ItemContainerGenerator.ContainerFromItem(item) as NodeEditorContainer;
						if (container == this)
							continue;
						container.IsSelected = false;
					}

				IsSelected = true;
				IsDragging = true;
				InitializePosition = e.GetPosition(Editor.ItemsHost);

				RaiseEvent(new DragStartedEventArgs(InitializePosition.X, InitializePosition.Y)
				{
					RoutedEvent = NodeEditorContainer.DragStartedEvent
				});

				PreviousPosition = InitializePosition;
				e.Handled = true;
			}
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Released)
			{
				this.ReleaseMouseCapture();
				IsDragging = false;

				var delta = CurrentPosition - InitializePosition;
				RaiseEvent(new DragCompletedEventArgs(delta.X, delta.Y, true)
				{
					RoutedEvent = NodeEditorContainer.DragCompletedEvent
				});
			}

		}


		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (IsDragging)
			{
				RefreshConnectorLocation(ConnectorCollections);

				CurrentPosition = e.GetPosition(Editor.ItemsHost);
				var delta = CurrentPosition - PreviousPosition;
				RaiseEvent(new DragDeltaEventArgs(delta.X, delta.Y)
				{
					RoutedEvent = NodeEditorContainer.DragDeltaEvent
				});

				RaiseEvent(new RoutedEventArgs(RefreshConnectorEvent));

				PreviousPosition = CurrentPosition;
				e.Handled = true;
			}
		}


		List<Connector> ConnectorCollections { get; set; }

		void RefreshConnectorLocation(List<Connector> Collections)
		{
			foreach (Connector item in ConnectorCollections)
			{
				item.RefreshConnector();
			}
		}
	}
}
