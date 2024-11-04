using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public abstract class GraphSubWindow : GraphElement
    {
        private readonly VisualElement _contentContainer = new();
        
        private readonly Dragger _dragger = new();
        private readonly ResizableElement _resizableElement = new();

        protected GraphSubWindow()
        {
            style.position = Position.Absolute;
            style.marginBottom = 0;
            style.marginTop = 0;
            style.marginLeft = 0;
            style.marginRight = 0;
            style.paddingBottom = 0;
            style.paddingTop = 0;
            style.paddingLeft = 0;
            style.paddingRight = 0;
            
            _contentContainer.style.flexGrow = 1;
            _contentContainer.pickingMode = PickingMode.Ignore;
            
            capabilities = Capabilities.Movable | Capabilities.Resizable;
            
            _dragger.clampToParentEdges = true;

            hierarchy.Add(_contentContainer);
            hierarchy.Add(_resizableElement);
            
            this.AddManipulator(_dragger);
        }

        public override VisualElement contentContainer => _contentContainer;

        public override void SetPosition(Rect rect)
        {
            style.left = rect.x;
            style.top = rect.y;
            style.width = rect.width;
            style.height = rect.height;
        }
        
        public override Rect GetPosition()
        {
            return new Rect(resolvedStyle.left, resolvedStyle.top, resolvedStyle.width, resolvedStyle.height);
        }
    }
}