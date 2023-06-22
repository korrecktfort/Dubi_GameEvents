using Dubi.GameEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class GameEventEditorProperties
{
    [MenuItem("Dubi/Game Events/Create Scriptable Object")]
    public static void CrateGameEventSO()
    {
        if(Selection.activeObject is MonoScript mono)
        {
            Type type = mono.GetClass();
            if(type.IsSubclassOf(typeof(GameEvent)))
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if(path != null)
                {
                    var gameEvent = ScriptableObject.CreateInstance(type);
                    gameEvent.name = "New " + type.Name;
                    path = path[..(path.LastIndexOf("/") + 1)];
                    AssetDatabase.CreateAsset(gameEvent, path + gameEvent.name + ".asset");
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }

}
