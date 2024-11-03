using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Misaki.GraphView
{
    [Serializable]
    public class StickyNoteData
    {
        [SerializeField]
        private string _id = Guid.NewGuid().ToString();
        
        public string title;
        public string contents;
        public Rect position;
        
        public StickyNoteTheme theme;
        public StickyNoteFontSize fontSize;
        
        public string Id => _id;
    }
}