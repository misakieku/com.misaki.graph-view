using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public struct SearchContextElement
    {
        public object Target
        {
            get;
        }
        public string Title
        {
            get;
        }

        public SearchContextElement(object target, string title)
        {
            Target = target;
            Title = title;
        }
    }

    public class NodeSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        //public VisualElement nodeCreationView;
        private readonly List<SearchContextElement> _searchContextElements = new();
        private GraphView _owner;

        public void SetOwner(GraphView owner)
        {
            _owner = owner;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            if (_owner == null)
            {
                return null;
            }

            var tree = new List<SearchTreeEntry>
            {
                // The first entry is the main group, which will be shown as title of the search window
                new SearchTreeGroupEntry(new GUIContent("Create Node"))
            };

            _searchContextElements.Clear();

            var types = string.IsNullOrEmpty(_owner.GraphViewConfig.searchNamespace) ?
                TypeCache.GetTypesDerivedFrom<ExecutableNode>() :
                TypeCache.GetTypesDerivedFrom<ExecutableNode>().Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith(_owner.GraphViewConfig.searchNamespace));

            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                if (type.CustomAttributes.Any())
                {
                    var nodeInfo = type.GetCustomAttribute<NodeInfoAttribute>();
                    if (!string.IsNullOrEmpty(nodeInfo.Category))
                    {
                        var title = $"{nodeInfo.Category}/{nodeInfo.Name}";
                        _searchContextElements.Add(new SearchContextElement(type, title));
                    }
                }
            }

            foreach (var property in _owner.GraphViewConfig.graphObject.ExposedProperties)
            {
                var title = $"Exposed Properties/{property.propertyName}";
                _searchContextElements.Add(new SearchContextElement(property, title));
            }

            // Sort by name
            _searchContextElements.Sort((a, b) =>
            {
                var splits1 = a.Title.Split('/');
                var splits2 = b.Title.Split('/');
                for (var i = 0; i < splits1.Length; i++)
                {
                    if (splits2.Length <= i)
                        return 1;

                    var compare = string.CompareOrdinal(splits1[i], splits2[i]);
                    if (compare != 0)
                    {
                        if (splits1.Length != splits2.Length && (i == splits1.Length - 1 || i == splits2.Length - 1))
                            return splits1.Length > splits2.Length ? 1 : -1;

                        return compare;
                    }
                }

                return 0;
            });

            // Build the tree
            foreach (var element in _searchContextElements)
            {
                var entryTitle = element.Title.Split('/');
                for (var i = 0; i < entryTitle.Length - 1; i++)
                {
                    var group = tree.FindLast(e => e is SearchTreeGroupEntry && e.name == entryTitle[i] && e.level == i + 1);
                    if (group == null)
                    {
                        group = new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1);
                        tree.Add(group);
                    }
                }

                var entry = new SearchTreeEntry(new GUIContent(entryTitle[^1]))
                {
                    level = entryTitle.Length,
                    userData = new SearchContextElement(element.Target, element.Title)
                };

                tree.Add(entry);
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowMousePosition = _owner.ChangeCoordinatesTo(_owner.contentContainer,
                context.screenMousePosition - _owner.EditorWindow.position.position);
            var graphMousePosition = _owner.contentViewContainer.WorldToLocal(windowMousePosition);

            var element = (SearchContextElement)searchTreeEntry.userData;

            ExecutableNode node = null;
            if (element.Target is ExposedProperty property)
            {
                node = new PropertyInput(property);
            }
            else if (element.Target is Type nodeType)
            {
                node = Activator.CreateInstance(nodeType) as ExecutableNode;
            }

            if (node == null)
            {
                return false;
            }

            node.nodePosition = new Rect(graphMousePosition, Vector2.zero);
            _owner.AddNode(node);
            _owner.GraphViewConfig.serializedObject.Update();

            return true;
        }
    }
}