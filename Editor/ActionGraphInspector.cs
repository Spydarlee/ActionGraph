using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

namespace ActionGraph
{
    public class ActionGraphInspector : EditorWindow
    {
        // -------------------------------------------------------------------------------

        public static Graph                 CurrentGraph = null;
        public static ActionGraphInspector  Instance = null;

        private static Action               mCurrentAction = null;
        private static Connection           mCurrentConnection = null;

        // -------------------------------------------------------------------------------

        [MenuItem("Window/ActionGraph Inspector")]
        public static void ShowEditor()
        {
            Instance = EditorWindow.GetWindow<ActionGraphInspector>();
            Instance.Init();
        }

        // -------------------------------------------------------------------------------

        public static void Clear()
        {
            if (Instance != null)
            {
                CurrentGraph = null;
                mCurrentAction = null;
                mCurrentConnection = null;
                Instance.Repaint();
            }
        }

        // -------------------------------------------------------------------------------

        public static void SetCurrentAction(Action newCurrentAction)
        {
            mCurrentAction = newCurrentAction;
            mCurrentConnection = null;
        }

        // -------------------------------------------------------------------------------

        public static void SetCurrentConnection(Connection newCurrentConnection)
        {
            mCurrentAction = null;
            mCurrentConnection = newCurrentConnection;
        }

        // -------------------------------------------------------------------------------

        void Init()
        {
            titleContent = new GUIContent("AG Inspector");
        }

        // -------------------------------------------------------------------------------

        void OnGUI()
        {
            if (mCurrentAction != null)
            {
                ShowActionInspector();
            }
            else if (mCurrentConnection != null)
            {
                ShowConnectionInspector(mCurrentConnection);

                // Do we have a two-way connection between these two nodes?
                var otherConnection = mCurrentConnection.EndNode.GetOutgoingConnectionToNode(mCurrentConnection.StartNode);                
                if (otherConnection != null)
                {
                    // If so, draw the *other* conneciton as well
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    EditorGUILayout.Space();

                    ShowConnectionInspector(otherConnection);
                }
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("No Action or Connection selected from the ActionGraph Editor!");
                EditorGUILayout.Space();
            }

            // Make sure the main editor window shows up-to-date changes we make
            ActionGraphEditor.MarkAsDirty(true);
        }

        // -------------------------------------------------------------------------------

        void ShowActionInspector()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(mCurrentAction.GetType().Name, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Actions can control their own GUI, or we can just do a generic version ourselves
            if (!mCurrentAction.OnGUI())
            {
                // Show inspector GUI for all public fields on the CurrentAction
                foreach (FieldInfo fieldInfo in mCurrentAction.GetType().GetFields())
                {
                    if (fieldInfo != null && !fieldInfo.IsStatic)
                    {
                        var value = fieldInfo.GetValue(mCurrentAction);
                        value = EditorGUIHelper.ShowInspectorForType(fieldInfo.FieldType, value, fieldInfo.Name);
                        fieldInfo.SetValue(mCurrentAction, value);
                    }
                }
            }

            if (GUILayout.Button("Copy Action"))
            {
                ActionGraphEditor.CopiedAction = mCurrentAction.Clone();
            }
        }

        // -------------------------------------------------------------------------------

        void ShowConnectionInspector(Connection connection)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(connection.StartNode.Name + " -> " + connection.EndNode.Name, EditorStyles.boldLabel);

            EditorGUILayout.Space();
            connection.ConditionRequirement = (Connection.ConditionRequirements)EditorGUILayout.EnumPopup("Condition Requirements", connection.ConditionRequirement);
            EditorGUILayout.Space();

            EditorGUIHelper.CreateConditionButton((condition) =>
            {
                connection.Conditions.Add(condition as Condition);
            });

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("CONDITIONS", EditorStyles.boldLabel);

            if (connection.Conditions == null || connection.Conditions.Count == 0)
            {
                EditorGUILayout.LabelField("None! This connection will transition automatically.");
            }
            else
            {
                Condition conditionToDelete = null;
                foreach (var condition in connection.Conditions)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(condition.ToString(), EditorStyles.boldLabel);
                    if (GUILayout.Button("Delete"))
                    {
                        conditionToDelete = condition;
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    // Show inspector GUI for all public fields on the Condition
                    EditorGUIHelper.ShowConditionInspector(condition);
                }

                if (conditionToDelete != null)
                {
                    connection.Conditions.Remove(conditionToDelete);
                    conditionToDelete = null;
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("*DELETE CONNECTION*"))
            {
                connection.StartNode.OutgoingConnections.Remove(connection);
                connection.EndNode.IncomingConnections.Remove(connection);
                if (connection == mCurrentConnection)
                {
                    mCurrentConnection = null;
                }
                return;
            }
        }

        // -------------------------------------------------------------------------------
    }
}
