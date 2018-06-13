using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ActionGraph
{
    [CustomEditor(typeof(Graph))]
    public class GraphEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Graph graph = (Graph)target;

            if (GUILayout.Button("Open Editor"))
            {
                ActionGraphEditor.CurrentGraph = graph;
                ActionGraphEditor.ShowEditor();
            }

            DrawDefaultInspector();
        }            
    }
}
