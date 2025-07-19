using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArLeiBurke.NodeEditor
{

	public class NodeCanvas : Panel
	{
		protected override Size ArrangeOverride(Size arrangeSize)
		{
			UIElementCollection children = InternalChildren;

			foreach (UIElement Element in children)
			{
				var container = Element as NodeEditorContainer;
				Rect rect = new Rect(container.Location.X, container.Location.Y, Element.DesiredSize.Width, Element.DesiredSize.Height);
				Element.Arrange(rect);

			}

			return arrangeSize;

		}

		protected override Size MeasureOverride(Size constraint)
		{

			Size desiredSize = new Size();
			foreach (UIElement child in InternalChildren)
			{
				child.Measure(constraint);
				desiredSize.Width = Math.Max(desiredSize.Width, child.DesiredSize.Width);
				desiredSize.Height = child.DesiredSize.Height;
			}
			return desiredSize;

		}
	}

}
