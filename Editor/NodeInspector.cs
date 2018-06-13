using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace ActionGraph
{
    public static class NodeInspector
    {
        // -------------------------------------------------------------------------------

        public static void OnGUI(this Node node)
        {
            node.Name = EditorGUILayout.TextField("Name", node.Name);
            node.MustFinishAllActions = EditorGUILayout.Toggle("Must Finish All Actions?", node.MustFinishAllActions);

            EditorGUILayout.LabelField("ACTIONS", EditorStyles.boldLabel);

            if (GUILayout.Button("Add New Action"))
            {
                // Get a list of all possible Actions via reflection
                Type baseActionType = typeof(Action);
                Assembly assembly = Assembly.GetAssembly(baseActionType);
                List<Type> allActionTypes = assembly.GetTypes().Where(type => type != baseActionType && baseActionType.IsAssignableFrom(type)).ToList();

                // Create a menu item for each type so we can create new instances from the GUI
                var menu = new GenericMenu();

                // Also, if we have an action copied to the clipboard, add an option to paste that in instead
                if (ActionGraphEditor.CopiedAction != null)
                {
                    menu.AddItem(new GUIContent("PASTE COPIED ACTION"), false, () =>
                    {
                        var newAction = ActionGraphEditor.CopiedAction.Clone();
                        node.Actions.Add(newAction);
                        ShowActionGraphInspector(newAction);
                    });
                }

                foreach (var actionType in allActionTypes)
                {
                    menu.AddItem(new GUIContent(actionType.Name), false, () =>
                    {
                        var newAction = Activator.CreateInstance(actionType) as Action; 
                        node.Actions.Add(newAction);
                        ShowActionGraphInspector(newAction);
                    });
                }
                menu.ShowAsContext();
            }

            Action actionToMoveUp = null;
            Action actionToMoveDown = null;

            foreach (var action in node.Actions)
            {
                var labelStyle = EditorStyles.label;

                // Highlight the currently executing action, if applicable
                if ((ActionGraphEditor.CurrentGraph.CurrentNode != null && 
                    ActionGraphEditor.CurrentGraph.CurrentNode.CurrentAction == action))
                {
                    labelStyle = EditorStyles.boldLabel;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(action.DisplayName, labelStyle);

                if (GUILayout.Button("↑"))
                {
                    actionToMoveUp = action;
                }

                if (GUILayout.Button("↓"))
                {
                    actionToMoveDown = action;
                }

                if (GUILayout.Button("Edit"))
                {
                    ActionGraphEditor.MarkAsDirty();
                    ShowActionGraphInspector(action);
                }

                if (GUILayout.Button("Delete"))
                {
                    node.Actions.Remove(action);
                    ActionGraphInspector.Clear();
                    break;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            // Re-order actions list if requested!
            if (actionToMoveUp != null)
            {
                var actionIndex = node.Actions.IndexOf(actionToMoveUp);
                if (actionIndex > 0)
                {
                    node.Actions.Swap(actionIndex, actionIndex-1);
                }
            }
            else if (actionToMoveDown != null)
            {
                var actionIndex = node.Actions.IndexOf(actionToMoveDown);
                if (actionIndex < node.Actions.Count - 1)
                {
                    node.Actions.Swap(actionIndex, actionIndex+1);
                }
            }

            EditorGUILayout.Space();
        }

        // -------------------------------------------------------------------------------

        private static void ShowActionGraphInspector(Action action)
        {
            ActionGraphInspector.CurrentGraph = ActionGraphEditor.CurrentGraph;
            ActionGraphInspector.SetCurrentAction(action);
            ActionGraphInspector.ShowEditor();
        }

        // -------------------------------------------------------------------------------
    }
}
