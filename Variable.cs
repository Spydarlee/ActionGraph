using System;
using System.Reflection;
using UnityEngine;

namespace ActionGraph
{
    /// <summary>
    /// Can hold a value directly or be bound to a property/field on a MonoBehaviour.
    /// </summary>
    /// <typeparam name="T">Variable type</typeparam>
    public class Variable<T>
    {
        // -------------------------------------------------------------------------------

        [SerializeField] private T              mValue;
        [SerializeField] private string         mBoundMemberName = null;
        [SerializeField] private MonoBehaviour  mBoundOwner = null;
        [SerializeField] private Type           mBoundOwnerType = null;

        // -------------------------------------------------------------------------------

        public T Value
        {
            get
            {
                if (mBoundMemberName != null && mBoundOwner != null)
                {
                    var property = mBoundOwnerType.GetProperty(mBoundMemberName);
                    if (property != null && property.CanRead)
                    {
                        return getValue(property.GetGetMethod().Invoke(mBoundOwner, null));
                    }
                    else
                    {
                        var field = mBoundOwnerType.GetField(mBoundMemberName);
                        if (field != null)
                        {
                            return getValue(field.GetValue(mBoundOwner));
                        }
                        else
                        {
                            Debug.LogError("Failed to retreive either Property or Field for bound Variable with member name: " + mBoundMemberName);
                            return mValue;
                        }
                    }
                }
                else
                {
                    return mValue;
                }
            }
            set
            {
                if (mBoundMemberName != null)
                {
                    var property = mBoundOwnerType.GetProperty(mBoundMemberName);
                    if (property != null && property.CanWrite)
                    {
                        property.GetSetMethod().Invoke(mBoundOwner, new object[] { value });
                    }
                    else
                    {
                        var field = mBoundOwnerType.GetField(mBoundMemberName);
                        if (field != null)
                        {
                            field.SetValue(mBoundOwner, value);
                        }
                        else
                        {
                            Debug.LogError("Failed to retreive either Property or Field for bound Variable with member name: " + mBoundMemberName);
                        }
                    }
                }
                else
                {
                    mValue = value;
                }
            }
        }

        // -------------------------------------------------------------------------------

        public void Bind(MemberInfo member, MonoBehaviour owner)
        {
            if (member is FieldInfo || member is PropertyInfo)
            {
                mBoundOwner = owner;
                mBoundMemberName = member.Name;
                mBoundOwnerType = owner.GetType();
            }
        }

        // -------------------------------------------------------------------------------

        public void Unbind()
        {
            mBoundOwner = null;
            mBoundMemberName = null;
            mBoundOwnerType = null;
        }

        // -------------------------------------------------------------------------------

        public override string ToString()
        {
            if (mValue != null)
            {
                var valueAsUnityObject = (mValue as UnityEngine.Object);
                return (valueAsUnityObject != null) ? valueAsUnityObject.name : mValue.ToString();
            }
            else if (mBoundMemberName != null)
            {
                return mBoundOwner.name + "." + mBoundMemberName;
            }

            return "NULL";
        }

        // -------------------------------------------------------------------------------

        private T getValue(object value)
        {
            // Are we bound to a MonoBehaviour but actually want to return the GameObject it belongs to?
            if (typeof(T) == typeof(GameObject) && typeof(MonoBehaviour).IsAssignableFrom(value.GetType()))
            {
                return (T)((value as MonoBehaviour).gameObject as object);
            }

            // Otherwise just return the input value directly
            return (T)value;
        }

        // -------------------------------------------------------------------------------
    }
}