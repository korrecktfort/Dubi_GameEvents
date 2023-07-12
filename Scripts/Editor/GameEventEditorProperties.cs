using Dubi.GameEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class GameEventEditorProperties
{
    [MenuItem("Dubi/Create Scriptable Object From Selected")]
    public static void CrateGameEventSO()
    {
        if(Selection.activeObject is MonoScript mono)
        {
            Type type = mono.GetClass();
            if(type.IsSubclassOf(typeof(ScriptableObject)))
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if(path != null)
                {
                    var so = ScriptableObject.CreateInstance(type);
                    so.name = "New " + type.Name;
                    path = path[..(path.LastIndexOf("/") + 1)];
                    AssetDatabase.CreateAsset(so, path + so.name + ".asset");
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }

}
