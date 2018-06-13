using System.Collections.Generic;
using UnityEngine;
using FullSerializer;

namespace ActionGraph
{
    public class Graph : MonoBehaviour
    {
        // -------------------------------------------------------------------------------

        public string           SerialisedGraph = "";
        public List<Object>     UnityObjectReferences = new List<Object>();
        public GraphData        GraphData = null;

        // -------------------------------------------------------------------------------

        public bool IsTransitioning { get { return mCurrentNode is TransitionNode; } }
        public Node CurrentNode     { get { return mCurrentNode; } }

        // -------------------------------------------------------------------------------

        private fsSerializer    mSerialiser = null;
        private Node            mCurrentNode = null;

        // -------------------------------------------------------------------------------

        public void Start()
        {
            Initialise();
            Load();

            if (GraphData.Nodes != null && GraphData.Nodes.Count > 0)
            {
                mCurrentNode = GraphData.Nodes[GraphData.StartNodeIndex];
                mCurrentNode.OnEnter();
            }
            else
            {
                Debug.Log("Graph doesn't have any nodes - disabling!", this);
                gameObject.SetActive(false);
            }

        }

        // -------------------------------------------------------------------------------

        public void Initialise()
        {
            mSerialiser = new fsSerializer();
            mSerialiser.AddConverter(new UnityObjectConverter());
            mSerialiser.Context.Set(UnityObjectReferences);
        }

        // -------------------------------------------------------------------------------

        public void Update()
        {
            if (mCurrentNode != null)
            {
                mCurrentNode.OnUpdate();

                // Check transitions when we're ready (normally when all actions are finished, unless told otherwise)
                if (mCurrentNode.FinishedAllActions || !mCurrentNode.MustFinishAllActions)
                {
                    var newNode = mCurrentNode.CheckConnections();
                    if (newNode != mCurrentNode)
                    {
                        mCurrentNode.OnExit();
                        newNode.OnEnter();

                        mCurrentNode = newNode;
                    }
                }
            }
        }

        // -------------------------------------------------------------------------------

        public void Save()
        {
            // We want to fill in a fresh list of references so clear the existing one
            // in case it contains any stale/unused references
            UnityObjectReferences.Clear();

            // Serialise the graph and save it to a string
            fsData data;
            mSerialiser.TrySerialize(typeof(GraphData), GraphData, out data).AssertSuccessWithoutWarnings();
            SerialisedGraph = fsJsonPrinter.CompressedJson(data);
        }

        // -------------------------------------------------------------------------------

        public void Load()
        {
            if (GraphData == null)
            {
                if (SerialisedGraph != "")
                {
                    fsData parsedData = fsJsonParser.Parse(SerialisedGraph);
                    object deserializedGraph = null;
                    mSerialiser.TryDeserialize(parsedData, typeof(GraphData), ref deserializedGraph).AssertSuccessWithoutWarnings();
                    GraphData = (GraphData)deserializedGraph;
                }
                else
                {
                    GraphData = new GraphData();
                }
            }
        }

        // -------------------------------------------------------------------------------
    }
}