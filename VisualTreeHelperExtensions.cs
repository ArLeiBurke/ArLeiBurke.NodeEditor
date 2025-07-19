
using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;

namespace ArLeiBurke.NodeEditor
{

	public static class VisualTreeHelperExtensions
	{
		// 获取指定类型的所有子控件
		public static List<A> FindChildrenOfType<A>(DependencyObject parent) where A : DependencyObject
		{
			List<A> result = new List<A>();

			// 从父控件开始遍历
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				// 获取子控件
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);

				// 如果子控件是 A 类型，加入结果列表
				if (child is A aChild)
				{
					result.Add(aChild);
				}

				// 递归查找子控件的子控件
				result.AddRange(FindChildrenOfType<A>(child));
			}

			return result;
		}

		public static T FindParentOfType<T>(DependencyObject child) where T : DependencyObject
		{
			// 获取父控件
			DependencyObject parent = VisualTreeHelper.GetParent(child);

			// 如果父控件为空，则返回 null
			if (parent == null)
				return null;

			// 如果父控件的类型是 T，则返回该控件
			if (parent is T)
			{
				return (T)parent;
			}

			// 递归查找父控件的父控件
			return FindParentOfType<T>(parent);
		}

	}



}
