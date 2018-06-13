using System;
using System.Collections.Generic;
using FullSerializer;

namespace ActionGraph
{
    /// <summary>
    /// This converter serialises a UnityObject (or subclass such as MonoBehaviour or ScriptableObject)
    /// as an integer index into a list maintained by a parent class which handles the *actual* serialisation.
    /// This allows us to leverage Unity's built-in serialisation of UnityObjects while also having our own 
    /// serialisation logic. The external list is accessed during de/serialisation via Serializer.Context.
    /// This works with both scene references and asset references.
    /// /// </summary>
    public class UnityObjectConverter : fsConverter
    {
        // -------------------------------------------------------------------------------

        public override bool CanProcess(Type type)
        {
            // This converter applies to any UnityEngine.Object or subclass e.g. MonoBehaviour, ScriptableObject
            var unityObjectType = typeof(UnityEngine.Object);
            return (type == unityObjectType) || (type.IsSubclassOf(unityObjectType));
        }

        // -------------------------------------------------------------------------------

        public override bool RequestCycleSupport(Type storageType)
        {
            return false;
        }

        // -------------------------------------------------------------------------------

        public override bool RequestInheritanceSupport(Type storageType)
        {
            return false;
        }

        // -------------------------------------------------------------------------------

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
        {
            // Grab the list of serialised Unity objects from the Context object
            var serialisedUnityObjects = Serializer.Context.Get<List<UnityEngine.Object>>();
            var unityObjectInstance = instance as UnityEngine.Object;

            // If the Unity Object reference is null we don't need a valid index
            // into the list of serialisedUnityObjects, so just store -1 and return
            if (unityObjectInstance == null)
            {
                serialized = new fsData(-1);
                return fsResult.Success;
            }

            // Otherwise, search our existing list of object references for a match
            var serialisedUnityObjectsIndex = -1;
            for (var i = 0; i < serialisedUnityObjects.Count; i++)
            {
                if (serialisedUnityObjects[i] == unityObjectInstance)
                {
                    serialisedUnityObjectsIndex = i;
                    break;
                }
            }

            // If this is a new object reference that we don't already have in our list
            // add it now and serialize the new object's index in the list
            if (serialisedUnityObjectsIndex < 0)
            {
                serialisedUnityObjectsIndex = serialisedUnityObjects.Count;
                serialisedUnityObjects.Add(unityObjectInstance);
            }

            serialized = new fsData(serialisedUnityObjectsIndex);
            return fsResult.Success;
        }

        // -------------------------------------------------------------------------------

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
        {
            // Grab the list of serialised Unity objects from the Context object
            var serialisedUnityObjects = Serializer.Context.Get<List<UnityEngine.Object>>();
            var serialisedUnityObjectsIndex = (int)data.AsInt64;

            // Sanity check the index we've just deserialised against the Context list
            if (serialisedUnityObjectsIndex < 0 || serialisedUnityObjectsIndex >= serialisedUnityObjects.Count)
            {
                return fsResult.Warn("Invalid Unity Object reference index, could not deserialise!");
            }

            instance = serialisedUnityObjects[serialisedUnityObjectsIndex];
            return fsResult.Success;
        }

        // -------------------------------------------------------------------------------

        public override object CreateInstance(fsData data, Type storageType)
        {
            // We never want the serialiser to create Unity objects, null references are fine!
            return null;
        }

        // -------------------------------------------------------------------------------
    }
}