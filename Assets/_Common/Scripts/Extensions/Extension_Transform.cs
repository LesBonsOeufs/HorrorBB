using System.Collections.Generic;
using UnityEngine;

public static class Extension_Transform
{
    public static Transform FindRecursive(this Transform self, string n)
    {
        List<Transform> lSearchList = new () { self };
        List<Transform> lNextSearchList = new ();
        Transform lReturnedValue = null;

        while (lSearchList.Count > 0)
        {
            foreach (Transform lTransform in lSearchList)
            {
                lReturnedValue = lTransform.Find(n);

                if (lReturnedValue != null)
                    return lReturnedValue;
                
                for (int i = 0; i < lTransform.childCount; i++)
                    lNextSearchList.Add(lTransform.GetChild(i));
            }

            lSearchList = new(lNextSearchList);
            lNextSearchList.Clear();
        }

        return null;
    }
}