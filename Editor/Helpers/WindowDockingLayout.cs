using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public enum DockingPosition
    {
        Left,
        Right,
        Top,
        Bottom
    }
    
    internal static class WindowDockingLayout
    {
        public static void DockToParent(this VisualElement ve, Rect parentLayout, DockingPosition dockingPosition, bool changeSize)
        {
            var layout = ve.layout;
            switch (dockingPosition)
            {
                case DockingPosition.Left:
                    ve.style.left = parentLayout.x;
                    ve.style.top = parentLayout.y;
                    ve.style.width = layout.width;
                    ve.style.height = changeSize ?  parentLayout.height : layout.height;
                    break;
                case DockingPosition.Right:
                    ve.style.left = parentLayout.x + parentLayout.width - layout.width;
                    ve.style.top = parentLayout.y;
                    ve.style.width = layout.width;
                    ve.style.height = changeSize ?  parentLayout.height : layout.height;
                    break;
                case DockingPosition.Top:
                    ve.style.left = parentLayout.x;
                    ve.style.top = parentLayout.y;
                    ve.style.width = changeSize ?  parentLayout.width : layout.width;
                    ve.style.height = layout.height;
                    break;
                case DockingPosition.Bottom:
                    ve.style.left = parentLayout.x;
                    ve.style.top = parentLayout.y + parentLayout.height - layout.height;
                    ve.style.width = changeSize ?  parentLayout.width : layout.width;
                    ve.style.height = layout.height;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}