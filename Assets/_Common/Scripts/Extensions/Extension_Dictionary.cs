using System;
using System.Collections.Generic;

public static class Extension_Dictionary
{
    public static bool ContainsKeyOfType<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Type typeToCheck)
    {
        foreach (KeyValuePair<TKey, TValue> KVPair in dictionary)
        {
            if (typeToCheck.IsInstanceOfType(KVPair.Key))
                return true;
        }

        return false;
    }
}