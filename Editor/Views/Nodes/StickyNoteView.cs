using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public class StickyNoteView : StickyNote
    {
        private readonly StickyNoteData _data;
        
        public StickyNoteView(StickyNoteData data)
        {
            _data = data;
            
            userData = data;
            title = data.title;
            contents = data.contents;
            theme = data.theme;
            fontSize = data.fontSize;
            
            this.Q<TextField>("title-field").RegisterCallback<ChangeEvent<string>>(e => {
                _data.title = e.newValue;
            });
            this.Q<TextField>("contents-field").RegisterCallback<ChangeEvent<string>>(e => {
                _data.contents = e.newValue;
            });
        }
        
        public override void SetPosition(Rect rect)
        {
            base.SetPosition(rect);

            if (_data == null)
            {
                return;
            }
            _data.position = rect;
        }

        public override void OnResized()
        {
            base.OnResized();
            
            if (_data == null)
            {
                return;
            }
            _data.position = layout;
        }
    }
}