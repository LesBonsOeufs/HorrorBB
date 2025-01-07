using System;
using System.Reflection;
using UnityEngine;

public static class Extension_Component
{
    /// <returns>Created component of target object</returns>
    public static T Duplicate<T>(this T self, GameObject target) where T : Component
    {
        if (self == null)
            return null;

        T lTargetComponent = target.GetComponent<T>();

        if (lTargetComponent == null)
            lTargetComponent = target.AddComponent<T>();

        Type lType = typeof(T);
        BindingFlags lFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] lProperties = lType.GetProperties(lFlags);
        FieldInfo[] lFields = lType.GetFields(lFlags);

        foreach (PropertyInfo lProperty in lProperties)
        {
            if (lProperty.CanWrite)
            {
                try
                {
                    lProperty.SetValue(lTargetComponent, lProperty.GetValue(self, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }

        foreach (FieldInfo lField in lFields)
            lField.SetValue(lTargetComponent, lField.GetValue(self));

        return lTargetComponent;
    }
}