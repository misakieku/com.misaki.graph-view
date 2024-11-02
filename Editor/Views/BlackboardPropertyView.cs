using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public class BlackboardPropertyView : BlackboardField, IInspectable
    {
        private readonly ExposedPropertyEditor _editor;

        public Action<IInspectable> OnItemSelected { get; set; }
        public string InspectorName => text;
        
        public BlackboardPropertyView(ExposedPropertyEditor editor)
        {
            _editor = editor;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnItemSelected?.Invoke(this);
        }
        
        public override void OnUnselected()
        {
            base.OnUnselected();
            OnItemSelected?.Invoke(null);
        }
        
        public virtual EditorNodeView CreateNodeView()
        {
            return null;
        }

        public virtual VisualElement CreateInspector()
        {
            return _editor.CreateInspector();
        }
    }
}