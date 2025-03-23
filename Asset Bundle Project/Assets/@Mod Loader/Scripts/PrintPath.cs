#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

public static class PrintPath
{
    [MenuItem("@Mod Loader/Print Path")]
    private static void Print()
    {
        var builder = new StringBuilder();

        var list = new List<string>();
        var parent = Selection.activeGameObject.transform;
        var maxCount = 100;
        var index = 0;
        while (parent != null)
        {
            list.Add(parent.name);
            parent = parent.transform.parent;
            index++;
            if (index >= maxCount)
            {
                Debug.LogError("Reached max count");
                return;
            }
        }

        for (int i = list.Count - 1; i >= 0; i--)
        {
            builder.Append(list[i] + "/");
        }
        Debug.Log(builder.ToString());
    }
}
#endif