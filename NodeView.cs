using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace ArLeiBurke.NodeEditor
{
	public class NodeView : Control
	{
		static NodeView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeView), new FrameworkPropertyMetadata(typeof(NodeView)));
		}

		public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(NodeView), new PropertyMetadata(default(DataTemplate)));

		public static readonly DependencyProperty OutputTemplateProperty = DependencyProperty.Register("OutputTemplate", typeof(DataTemplate), typeof(NodeView), new PropertyMetadata(default(DataTemplate)));

		public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(NodeView), new PropertyMetadata(default(DataTemplate)));

		public static readonly DependencyProperty InputTemplateProperty = DependencyProperty.Register("InputTemplate", typeof(DataTemplate), typeof(NodeView), new PropertyMetadata(default(DataTemplate)));

		public static readonly DependencyProperty FootNoteTemplateProperty = DependencyProperty.Register("FootNoteTemplate", typeof(DataTemplate), typeof(NodeView), new PropertyMetadata(default(DataTemplate)));

		public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(CornerRadius), typeof(NodeView), new PropertyMetadata(default(CornerRadius)));



		public DataTemplate HeaderTemplate
		{
			get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
			set { SetValue(HeaderTemplateProperty, value); }
		}
		public DataTemplate ContentTemplate
		{
			get { return (DataTemplate)GetValue(ContentTemplateProperty); }
			set { SetValue(ContentTemplateProperty, value); }
		}

		public DataTemplate InputTemplate
		{
			get { return (DataTemplate)GetValue(InputTemplateProperty); }
			set { SetValue(InputTemplateProperty, value); }
		}

		public DataTemplate OutputTemplate
		{
			get { return (DataTemplate)GetValue(OutputTemplateProperty); }
			set { SetValue(OutputTemplateProperty, value); }
		}

		public DataTemplate FootNoteTemplate
		{
			get { return (DataTemplate)GetValue(FootNoteTemplateProperty); }
			set { SetValue(FootNoteTemplateProperty, value); }
		}


		public CornerRadius Radius
		{
			get { return (CornerRadius)GetValue(RadiusProperty); }
			set { SetValue(RadiusProperty, value); }
		}

	}
}
