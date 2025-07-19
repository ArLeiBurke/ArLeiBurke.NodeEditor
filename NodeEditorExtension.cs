using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace ArLeiBurke.NodeEditor
{
	public static class NodeEditorExtension
	{
		public static T? GetParentOfType<T>(this DependencyObject child) where T : DependencyObject
		{
			DependencyObject? current = child;
			do
			{
				current = VisualTreeHelper.GetParent(current);
				if (current == default)
				{
					return default;
				}

			} while (!(current is T));

			return (T)current;
		}
	}
}
