#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace ActionGraph
{
    public static class EditorGUIHelper
    {
        // -------------------------------------------------------------------------------

        public static object ShowInspectorForType(System.Type type, object value, string name)
        {
            if (type == typeof(bool))
            {
                return EditorGUILayout.Toggle(name, (bool)value);
            }
            else if (type == typeof(string))
            {
                return EditorGUILayout.TextField(name, (string)value);
            }
            else if (type == typeof(int))
            {
                return EditorGUILayout.IntField(name, (int)value);
            }
            else if (type == typeof(float))
            {
                return EditorGUILayout.FloatField(name, (float)value);
            }
            else if (type == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(name, (Vector2)value);
            }
            else if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(name, (Vector3)value);
            }
            else if (typeof(Enum).IsAssignableFrom(type))
            {
                return EditorGUILayout.EnumPopup(name, (Enum)value);
            }
            else if (typeof(IList<>).IsAssignableFrom(type))
            {
                EditorGUILayout.LabelField("TODO: LIST SUPPORT");
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return EditorGUILayout.ObjectField(name, (UnityEngine.Object)value, type,
                    typeof(Component).IsAssignableFrom(type) || type == typeof(GameObject) || type == typeof(UnityEngine.Object));
            }
            else if (type.IsGenericType && typeof(Variable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                EditorGUILayout.BeginHorizontal();

                if (value == null)
                {
                    value = System.Activator.CreateInstance(type);
                }

                var variableBoundMemberNameField = GetPrivateFieldInfo(value, "mBoundMemberName");
                var variableBoundMemberName = (string)variableBoundMemberNameField.GetValue(value);

                bool isBound = (variableBoundMemberName != null);
                if (!isBound)
                {
                    var variableValueField = GetPrivateFieldInfo(value, "mValue");
                    var variableValue = variableValueField.GetValue(value);
                    var variableValuetype = variableValueField.FieldType;

                    variableValueField.SetValue(value, ShowInspectorForType(variableValuetype, variableValue, name));
                }
                else
                {
                    var variableBoundOwnerField = GetPrivateFieldInfo(value, "mBoundOwner");
                    var variableBoundOwnerValue = (MonoBehaviour)variableBoundOwnerField.GetValue(value);

                    if (variableBoundOwnerValue != null)
                    {
                        string path = GetComponentPath(variableBoundOwnerValue);
                        EditorGUILayout.LabelField(name + ": " + path + "/" + variableBoundMemberName);
                    }
                    else
                    {
                        Debug.LogWarning("Bound value couldn't be found, the object may have been deleted? Unbinding...");
                        UnbindVariable(value);
                    }
                }

                if (GUILayout.Button("@", GUILayout.Width(20), GUILayout.Height(16)))
                {
                    var menu = new GenericMenu();
                    if (isBound)
                    {
                        menu.AddItem(new GUIContent("Unbind"), false, () => { UnbindVariable(value); });
                    }
                    else
                    {
                        // Grab a list of all the GameObjects at the root of the scene
                        List<GameObject> sceneGameObjects = new List<GameObject>();
                        SceneManager.GetActiveScene().GetRootGameObjects(sceneGameObjects);

                        var genericVariableType = type.GetGenericArguments()[0];

                        // Iterate over all the GameObjects at the root of the scene
                        foreach (var gameObject in sceneGameObjects)
                        {
                            // For each scene object, iterate over all components (and children's components)
                            // and look for any properties or fields that match our Variable's generic type
                            foreach (var component in gameObject.GetComponentsInChildren<Component>())
                            {
                                // GameObjects are special type because we can get at them from components..
                                var showComponentsForGameObjects = (genericVariableType == typeof(GameObject));

                                foreach (PropertyInfo componentPropertyInfo in component.GetType().GetProperties())
                                {
                                    var propertyType = componentPropertyInfo.PropertyType;

                                    // Do we actually want to bind to the gameObject property on a component, rather than the component itself?
                                    var bindGameObjectFromMonoBehaviour = (showComponentsForGameObjects && typeof(MonoBehaviour).IsAssignableFrom(propertyType));

                                    if (bindGameObjectFromMonoBehaviour || propertyType == genericVariableType || genericVariableType.IsAssignableFrom(propertyType))
                                    {
                                        var menuPath = "Properties/" + GetComponentPath(component) + "/" + componentPropertyInfo.Name;
                                        menu.AddItem(new GUIContent(menuPath), false, () =>
                                        {
                                            BindVariable(value, component, componentPropertyInfo);
                                        });
                                    }
                                }

                                foreach (FieldInfo componentFieldInfo in component.GetType().GetFields())
                                {
                                    var fieldType = componentFieldInfo.FieldType;
                                    var bindGameObjectFromComponent = (showComponentsForGameObjects && typeof(MonoBehaviour).IsAssignableFrom(fieldType));

                                    if (bindGameObjectFromComponent || fieldType == genericVariableType || genericVariableType.IsAssignableFrom(fieldType))
                                    {
                                        var menuPath = "Fields/" + GetComponentPath(component) + "/" + componentFieldInfo.Name;
                                        menu.AddItem(new GUIContent(menuPath), false, () =>
                                        {
                                            MemberInfo memberToBind = componentFieldInfo;

                                            // Do we actually want to bind to the gameObject property on a component, rather than the component itself?
                                            if (bindGameObjectFromComponent)
                                            {
                                                memberToBind = component.GetType().GetProperty("gameObject");
                                            }

                                            BindVariable(value, component, memberToBind);
                                        });
                                    }
                                }
                            }
                        }
                    }
                    menu.ShowAsContext();
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Couldn't display inspector for '" + name + "' of type: " + type.ToString());
            }

            return value;
        }

        // -------------------------------------------------------------------------------

        public static string GetComponentPath(Component component)
        {
            Transform transform = component.transform;
            string path = transform.name;

            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        // -------------------------------------------------------------------------------

        public static FieldInfo GetPrivateFieldInfo(object owner, string fieldName)
        {
            return owner.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        // -------------------------------------------------------------------------------

        public static void CreateConditionButton(Action<Condition> callback, string buttonText = "Add Condition")
        {
            Condition newCondition = null;

            if (GUILayout.Button(buttonText))
            {
                // Get a list of all possible Conditions via reflection
                Type baseConditionType = typeof(Condition);
                Assembly assembly = Assembly.GetAssembly(baseConditionType);
                List<Type> allConditionTypes = assembly.GetTypes().Where(type => type != baseConditionType && baseConditionType.IsAssignableFrom(type)).ToList();

                // Create a menu item for each type so we can create new instances from the GUI
                var menu = new GenericMenu();
                foreach (var conditionType in allConditionTypes)
                {
                    menu.AddItem(new GUIContent(conditionType.Name), false, () =>
                    {
                        newCondition = (Activator.CreateInstance(conditionType) as Condition);
                        callback(newCondition as Condition);
                    });
                }
                menu.ShowAsContext();
            }
        }

        // -------------------------------------------------------------------------------

        public static void ShowConditionInspector(Condition condition)
        {
            // Show inspector GUI for all public fields on the Condition
            foreach (FieldInfo fieldInfo in condition.GetType().GetFields())
            {
                if (fieldInfo != null && !fieldInfo.IsStatic)
                {
                    var value = fieldInfo.GetValue(condition);
                    value = ShowInspectorForType(fieldInfo.FieldType, value, fieldInfo.Name);
                    fieldInfo.SetValue(condition, value);
                }
            }
        }

        // -------------------------------------------------------------------------------

        private static void BindVariable(object variableObject, Component owner, MemberInfo memberInfo)
        {
            var variableBoundMemberNameField = GetPrivateFieldInfo(variableObject, "mBoundMemberName");
            var variableBoundOwnerField = GetPrivateFieldInfo(variableObject, "mBoundOwner");
            var variableBoundOwnerTypeField = GetPrivateFieldInfo(variableObject, "mBoundOwnerType");
            var variableValueField = GetPrivateFieldInfo(variableObject, "mValue");

            variableBoundMemberNameField.SetValue(variableObject, memberInfo.Name);
            variableBoundOwnerField.SetValue(variableObject, owner);
            variableBoundOwnerTypeField.SetValue(variableObject, owner.GetType());
            variableValueField.SetValue(variableObject, null);
        }

        // -------------------------------------------------------------------------------

        private static void UnbindVariable(object variableObject)
        {
            var variableBoundMemberNameField = GetPrivateFieldInfo(variableObject, "mBoundMemberName");
            var variableBoundOwnerField = GetPrivateFieldInfo(variableObject, "mBoundOwner");
            var variableBoundOwnerTypeField = GetPrivateFieldInfo(variableObject, "mBoundOwnerType");

            variableBoundMemberNameField.SetValue(variableObject, null);
            variableBoundOwnerField.SetValue(variableObject, null);
            variableBoundOwnerTypeField.SetValue(variableObject, null);
        }

        // -------------------------------------------------------------------------------
    }
}

#endif
