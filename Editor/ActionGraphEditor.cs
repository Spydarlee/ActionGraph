using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace ActionGraph
{
    public class ActionGraphEditor : EditorWindow
    {
        // -------------------------------------------------------------------------------

        public static Graph     CurrentGraph = null;
        public static Action    CopiedAction = null;

        // -------------------------------------------------------------------------------

        private Vector2     mPan = Vector2.zero;
        private Texture2D   mHandleTexture = null;
        private Texture2D   mArrowTexture = null;

        private Vector2     mHandleRectSize = new Vector2(20, 20);
        private Node        mNewConnectionStartNode = null;
        private Rect        mNewConnectionStartRect;
        private bool        mStartedNewConnectionThisFrame = false;
        private bool        mInitialised = false;
        private Color       mDefaultColor = Color.grey;

        private static ActionGraphEditor mInstance = null;

        // -------------------------------------------------------------------------------

        [MenuItem("Window/ActionGraph Editor")]
        public static void ShowEditor()
        {
            mInstance = EditorWindow.GetWindow<ActionGraphEditor>();
            mInstance.Init();
        }

        // -------------------------------------------------------------------------------

        public void Init()
        {
            mHandleTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Source/Textures/EditorCircle.png", typeof(Texture2D));
            mArrowTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Source/Textures/EditorArrow.png", typeof(Texture2D));

            titleContent = new GUIContent("ActionGraph");

            if (CurrentGraph == null)
            {
                CurrentGraph = GameObject.FindObjectOfType<Graph>();
            }

            if (CurrentGraph != null)
            {
                CurrentGraph.Initialise();
                CurrentGraph.Load();
                mInitialised = true;
                mDefaultColor = GUI.backgroundColor;
            }
        }

        // -------------------------------------------------------------------------------

        void OnGUI()
        {
            if (!mInitialised)
            {
                Init();
            }

            mStartedNewConnectionThisFrame = false;

            if (CurrentGraph == null)
            {
                EditorGUILayout.LabelField("Please select a Graph from the scene view!");
                return;
            }

            if (CurrentGraph.GraphData == null)
            {
                CurrentGraph.Initialise();
                CurrentGraph.Load();
            }

            DrawMenu();

            GUI.BeginGroup(new Rect(mPan.x, mPan.y, 100000, 100000));

            // Draw all the nodes!
            BeginWindows();
            for (int i = 0; i < CurrentGraph.GraphData.Nodes.Count; i++)
            {
                var node = CurrentGraph.GraphData.Nodes[i];
                var windowRect = node.GraphWindowRect;
                windowRect.height = 0; // Let the GUILayout.Window function assign the correct height

                if ((CurrentGraph.CurrentNode == CurrentGraph.GraphData.Nodes[i]))
                {
                    GUI.backgroundColor = new Color(0.63f, 0.83f, 0.56f);
                }
                else if (i == CurrentGraph.GraphData.StartNodeIndex)
                {
                    GUI.backgroundColor = new Color(.6f, .85f, 0.91f);
                }
                else
                {
                    GUI.backgroundColor = mDefaultColor;
                }

                var windowName = (CurrentGraph.GraphData.StartNodeIndex == i) ? "START NODE: " + node.Name : node.Name;
                CurrentGraph.GraphData.Nodes[i].GraphWindowRect = GUILayout.Window(i, windowRect, DrawNodeWindow, windowName);

                UpdateNodeResize(node);
                DrawHandles(node);
            }
            EndWindows();

            // Draw connections between nodes
            foreach (var node in CurrentGraph.GraphData.Nodes)
            {
                foreach (var connection in node.OutgoingConnections)
                {
                    DrawConnection(connection);
                }
            }

            GUI.EndGroup();

            if (mNewConnectionStartNode != null && !mStartedNewConnectionThisFrame)
            {
                // Editor view doesn't update just because the mouse moved so repaint manually in this state
                Repaint();

                var startPos = new Vector2(mNewConnectionStartRect.x + mPan.x, mNewConnectionStartRect.y + mPan.y);
                var startRect = new Rect(startPos, mNewConnectionStartRect.size);
                var mouseRect = new Rect(Event.current.mousePosition, new Vector2(10, 10));
                DrawBezierCurve(startRect.center, mouseRect.center);

                // Cancel the new connection if the user clicks anywhere on the window background
                if (Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
                {
                    mNewConnectionStartNode = null;
                }
            }
            else
            {
                // Context menu for creating new nodes
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                {
                    var spawnPosition = Event.current.mousePosition - mPan;

                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Standard Node"), false, () => { CreateStandardNode(spawnPosition); });
                    menu.AddItem(new GUIContent("Transition Node"), false, () => { CreateTransitionNode(spawnPosition); });
                    menu.ShowAsContext();
                }

                // Grab and pan all the visual elements of the graph
                if (Event.current.button == 0 )
                {
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        mPan.x += Event.current.delta.x;
                        mPan.y += Event.current.delta.y;
                        Repaint();
                    }
                    else if (Event.current.type == EventType.MouseUp)
                    {
                        Selection.SetActiveObjectWithContext(CurrentGraph.gameObject, null);
                    }
                }
            }
        }

        // -------------------------------------------------------------------------------

        void DrawNodeWindow(int id)
        {
            var node = CurrentGraph.GraphData.Nodes[id];

            // Is the user trying to create a new connection, possibly with this node?
            if (mNewConnectionStartNode != null && mNewConnectionStartNode != node)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    // Assuming we're not already connected to the given node.
                    if (mNewConnectionStartNode.GetOutgoingConnectionToNode(node) == null)
                    {
                        // Create a new conneciton between the two nodes!
                        var newConnection = new Connection { StartNode = mNewConnectionStartNode, EndNode = node };
                        newConnection.StartNode.OutgoingConnections.Add(newConnection);
                        newConnection.EndNode.IncomingConnections.Add(newConnection);
                        mNewConnectionStartNode = null;
                        ShowConnectionInspector(newConnection);
                    }
                }
            }
            else
            {
                // Has this node been clicked on? Save change in position
                Event currentEvent = Event.current;
                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                {
                    SaveGraph();
                }

                // Context menu for modifying existing node
                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Delete"), false, () => { DeleteNode(id); });
                    menu.AddItem(new GUIContent("Make Start Node"), false, () => { SetStartNode(id); });
                    menu.ShowAsContext();
                }
            }

            // TODO: This is not very extensible! OnGUI logic for Nodes lives in extension methods in the
            // editor project, but calling directly on the base node reference in our list will always
            // call the default implementation.. haven't find a nice fix for this yet!
            var transitionNode = (node as TransitionNode);
            if (transitionNode != null)
            {
                transitionNode.OnGUI();
            }
            else
            {
                node.OnGUI();
            }
            
            GUI.DragWindow();
        }

        // -------------------------------------------------------------------------------

        void DrawConnection(Connection connection)
        {
            Node startnode = connection.StartNode;
            Node endNode = connection.EndNode;

            Vector2 startPos = Vector2.zero;
            Vector2 endPos = Vector2.zero;
            float minSqrDistance = float.MaxValue;

            // Find the shortest path between the two nodes
            foreach (var startNodeHandle in startnode.ConnectionHandles)
            {
                foreach (var endNodeHandle in endNode.ConnectionHandles)
                {
                    var sqrDistance = Vector2.SqrMagnitude(startNodeHandle.Position - endNodeHandle.Position);
                    if (sqrDistance < minSqrDistance)
                    {
                        minSqrDistance = sqrDistance;
                        startPos = startNodeHandle.DisplayRect.center;
                        endPos = endNodeHandle.DisplayRect.center;
                    }
                }
            }

            DrawBezierCurve(startPos, endPos);

            var curveMidPoint = startPos + ((endPos - startPos) * 0.5f);
            var curveMidPointRect = new Rect(curveMidPoint, new Vector2(32, 32));

            // Draw connection icon used to display connection in the editor
            GUI.DrawTexture(curveMidPointRect, mHandleTexture);
            EditorGUIUtility.AddCursorRect(curveMidPointRect, MouseCursor.Text);

            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown &&
                curveMidPointRect.Contains(Event.current.mousePosition))
            {
                ShowConnectionInspector(connection);
            }

            // Draw directional arrow
            {
                Matrix4x4 matrixBackup = GUI.matrix;
                float angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * 180 / Mathf.PI;
                GUIUtility.RotateAroundPivot(angle, curveMidPoint);
                GUI.DrawTexture(curveMidPointRect, mArrowTexture);
                GUI.matrix = matrixBackup;
            }
        }

        // -------------------------------------------------------------------------------

        void DrawBezierCurve(Vector2 startPos, Vector2 endPos)
        {
            Vector2 startToEnd = (endPos - startPos);
            float directionFactor = Mathf.Clamp(startToEnd.magnitude, 20f, 80f);
            Vector2 projection = Vector3.Project(startToEnd.normalized, Vector3.right);

            Vector2 startTan = startPos + projection * directionFactor;
            Vector2 endTan = endPos - projection * directionFactor;

            // Draw a shadow
            Color shadowCol = new Color(0, 0, 0, 0.06f);
            for (int i = 0; i < 3; i++)
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
        }

        // -------------------------------------------------------------------------------

        void DrawHandles(Node node)
        {
            var windowRect = node.GraphWindowRect;
            var handleHalfSize = mHandleRectSize * 0.5f;
            var leftAndRightHandleYPos = (windowRect.y + windowRect.height * 0.5f) - handleHalfSize.y;
            var topAndBottomHandleXPos = (windowRect.x + windowRect.width * 0.5f) - handleHalfSize.x;
            
            // Initialise this node's list of connection handles if we haven't already done so
            if (node.ConnectionHandles == null || node.ConnectionHandles.Count != 4)
            {
                node.ConnectionHandles = new List<Node.ConnectionHandle>{new Node.ConnectionHandle(),
                    new Node.ConnectionHandle(), new Node.ConnectionHandle(), new Node.ConnectionHandle() };
            }

            // Calculate positions for all connection handles
            node.ConnectionHandles[0].Position = (new Vector2(windowRect.x - handleHalfSize.x, leftAndRightHandleYPos));  // Left
            node.ConnectionHandles[1].Position = (new Vector2(windowRect.x + windowRect.width - handleHalfSize.x, leftAndRightHandleYPos));   // Right
            node.ConnectionHandles[2].Position = (new Vector2(topAndBottomHandleXPos, windowRect.y - handleHalfSize.y));    // Top
            node.ConnectionHandles[3].Position = (new Vector2(topAndBottomHandleXPos, windowRect.y + windowRect.height - handleHalfSize.y));   // Bottom

            // Create an interactive rect area and texture for each handle
            for (int i = 0; i < 4; i++)
            {
                var handleRect = new Rect(node.ConnectionHandles[i].Position, mHandleRectSize);
                node.ConnectionHandles[i].DisplayRect = handleRect;
                GUI.DrawTexture(handleRect, mHandleTexture);
                EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ArrowPlus);
            }

            // Check for a user clicking on a handle
            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
            {
                foreach (var connectionHandle in node.ConnectionHandles)
                {
                    if (connectionHandle.DisplayRect.Contains(Event.current.mousePosition))
                    {
                        mNewConnectionStartNode = node;
                        mNewConnectionStartRect = connectionHandle.DisplayRect;
                        mStartedNewConnectionThisFrame = true;
                        break;
                    }
                }
            }
        }

        // -------------------------------------------------------------------------------

        void DrawMenu()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                SaveGraph();
            }
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Settings", EditorStyles.toolbarDropDown))
            {
                GenericMenu settingsMenu = new GenericMenu();
                settingsMenu.AddItem(new GUIContent("Load"), false, LoadGraph);
                settingsMenu.DropDown(new Rect(Screen.width - 200 - 40, 0, 0, 16));
            }

            GUILayout.EndHorizontal();
        }

        // -------------------------------------------------------------------------------

        void UpdateNodeResize(Node node)
        {
            Rect resizeAreaRect = node.GraphWindowRect;
            resizeAreaRect.x += resizeAreaRect.width * 0.9f;
            resizeAreaRect.y += resizeAreaRect.height * 0.9f;
            resizeAreaRect.width *= 0.1f;
            resizeAreaRect.height *= 0.1f;

            EditorGUIUtility.AddCursorRect(resizeAreaRect, MouseCursor.ResizeUpLeft);

            // KB TODO: RESIZE ON DRAG
            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                //MyGraph.GraphData.Nodes[i].GraphWindowRect.width += 0.1f;
            }
        }

        // -------------------------------------------------------------------------------

        private void CreateStandardNode(Vector2 spawnPosition)
        {
            var newStandardNode = new Node();
            newStandardNode.GraphWindowRect.position = spawnPosition;
            CurrentGraph.GraphData.Nodes.Add(newStandardNode);
            MarkAsDirty();
        }

        // -------------------------------------------------------------------------------

        private void CreateTransitionNode(Vector2 spawnPosition)
        {
            var newTransitionNode = new TransitionNode();
            newTransitionNode.GraphWindowRect.position = spawnPosition;
            CurrentGraph.GraphData.Nodes.Add(newTransitionNode);
            MarkAsDirty();
        }

        // -------------------------------------------------------------------------------

        private void DeleteNode(int id)
        {
            ActionGraphInspector.Clear();
            CurrentGraph.GraphData.Nodes[id].OnDelete();
            CurrentGraph.GraphData.Nodes.RemoveAt(id);
            MarkAsDirty();

            if (CurrentGraph.GraphData.StartNodeIndex == id)
            {
                Debug.Log("Just deleted graph's start node, reverting to default start node (id 0)");
                SetStartNode(0);
            }
        }

        // -------------------------------------------------------------------------------

        private void SetStartNode(int id)
        {
            CurrentGraph.GraphData.StartNodeIndex = id;
        }

        // -------------------------------------------------------------------------------

        private void SaveGraph()
        {
            if (!Application.isPlaying)
            {
                CurrentGraph.Initialise();
                CurrentGraph.Save();
                MarkAsDirty();
            }
        }

        // -------------------------------------------------------------------------------

        private void LoadGraph()
        {
            if (!Application.isPlaying && CurrentGraph != null)
            {
                CurrentGraph.Load();
            }
        }

        // -------------------------------------------------------------------------------

        public static void MarkAsDirty(bool repaint = false)
        {
            if (!Application.isPlaying && CurrentGraph != null)
            {
                EditorUtility.SetDirty(CurrentGraph);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

                if (repaint && mInstance != null)
                {
                    mInstance.Repaint();
                }
            }
        }

        // -------------------------------------------------------------------------------

        private void ShowConnectionInspector(Connection connection)
        {
            ActionGraphInspector.CurrentGraph = ActionGraphEditor.CurrentGraph;
            ActionGraphInspector.SetCurrentConnection(connection);
            ActionGraphInspector.ShowEditor();
        }

        // -------------------------------------------------------------------------------
    }
}
