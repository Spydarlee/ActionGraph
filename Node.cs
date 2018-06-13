using System.Collections.Generic;
using UnityEngine;

namespace ActionGraph
{
    public partial class Node
    {
        // -------------------------------------------------------------------------------

        public string           Name = "Node";
        public Rect             GraphWindowRect = new Rect(10, 10, 300, 75);
        public bool             MustFinishAllActions = true;
        public List<Action>     Actions = new List<Action>();
        public List<Connection> OutgoingConnections = new List<Connection>();
        public List<Connection> IncomingConnections = new List<Connection>();
        private int             mCurrentActionIndex = 0;

#if UNITY_EDITOR
        public class ConnectionHandle
        {
            public Vector2 Position;
            public Rect DisplayRect;
        }

        [System.NonSerialized]
        public List<ConnectionHandle> ConnectionHandles = new List<ConnectionHandle>();
#endif

        // -------------------------------------------------------------------------------

        public bool FinishedAllActions { get; set; }
        public Action CurrentAction { get { return (mCurrentActionIndex < Actions.Count) ? Actions[mCurrentActionIndex] : null; } }

        // -------------------------------------------------------------------------------

        public virtual void OnEnter()
        {
            mCurrentActionIndex = 0;

            if (Actions.Count > 0)
            {
                FinishedAllActions = false;
                Actions[mCurrentActionIndex].Start();
            }
            else
            {
                FinishedAllActions = true;
            }
        }

        // -------------------------------------------------------------------------------

        public virtual void OnUpdate()
        {
            if (FinishedAllActions || mCurrentActionIndex >= Actions.Count)
            {
                return;
            }

            // Update the current action until it says it's done
            var currentAction = Actions[mCurrentActionIndex];
            currentAction.Update();
            var actionComplete = (currentAction.Status == ActionStatus.Finished);   // KB TODO: SUPPORT THIS ON START TOO

            if (actionComplete)
            {
                currentAction.Finish();
                mCurrentActionIndex++;

                // Have we finished running all of our actions?
                if (mCurrentActionIndex >= Actions.Count)
                {
                    FinishedAllActions = true;
                }
                else
                {
                    Actions[mCurrentActionIndex].Start();
                }
            }
        }

        // -------------------------------------------------------------------------------

        public virtual void OnExit()
        {
            // If we're leaving this node before we've finished every action, make sure we
            // give our current action the chance to clean up after itself!
            if (!FinishedAllActions && mCurrentActionIndex < Actions.Count)
            {
                Actions[mCurrentActionIndex].Finish();
            }
        }

        // -------------------------------------------------------------------------------

        public virtual void OnDelete()
        {
            // Make sure we clean up any connection we have to other nodes!
            foreach (var incomingConnection in IncomingConnections)
            {
                incomingConnection.StartNode.OutgoingConnections.Remove(incomingConnection);
            }
            foreach (var outgoingConnection in OutgoingConnections)
            {
                outgoingConnection.EndNode.IncomingConnections.Remove(outgoingConnection);
            }
        }

        // -------------------------------------------------------------------------------

        public Connection GetOutgoingConnectionToNode(Node node)
        {
            foreach (var connection in OutgoingConnections)
            {
                if (connection.EndNode == node)
                {
                    return connection;
                }
            }

            return null;
        }

        // -------------------------------------------------------------------------------

        public virtual Node CheckConnections()
        {
            // Check all of our outgoing connections
            foreach (var outgoingConnection in OutgoingConnections)
            {
                // If we pass all the necessary conditions, return the EndNode for a transition!
                if (outgoingConnection.CheckConditions())
                {
                    return outgoingConnection.EndNode;
                }
            }

            return this;
        }

        // -------------------------------------------------------------------------------
    }
}