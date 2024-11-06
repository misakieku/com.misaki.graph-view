using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public partial class GraphView
    {
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            var mousePosition = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);

            if (evt.target is GraphView)
            {
                evt.menu.InsertAction(1, "Create Sticky Note", e =>
                {
                    var stickyNote = new StickyNoteData
                    {
                        title = "Sticky Note",
                        contents = "Contents",
                        theme = StickyNoteTheme.Classic,
                        fontSize = StickyNoteFontSize.Medium,
                        position = new Rect(mousePosition, new Vector2(200, 200))
                    };

                    AddStickyNote(stickyNote);
                }, DropdownMenuAction.AlwaysEnabled);
            }

            if (evt.target is Edge edge)
            {
                evt.menu.AppendAction("Create Relay Node", e =>
                {
                    var relayNode = new RelayNode
                    {
                        position = new Rect(mousePosition, Vector2.zero)
                    };

                    AddRelayNode(relayNode, edge);
                }, DropdownMenuAction.AlwaysEnabled);
            }
        }
    }
}
