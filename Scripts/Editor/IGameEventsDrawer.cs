using Dubi.GameEvents;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

[CustomEditor(typeof(Object),true)]
public class IGameEventsDrawer : Editor
{
    int arraySize = -1;

    public override void OnInspectorGUI()
    {    
        if (base.serializedObject.targetObject is IGameEvents iGameEventsPre)
            this.arraySize = iGameEventsPre.GameEventValues.Length;

        base.OnInspectorGUI();

        if (base.serializedObject.targetObject is IGameEvents iGameEventsPost)
        {
            CheckArray(iGameEventsPost);
            DrawButtons();
        }
    }

    void CheckArray(IGameEvents iGameEvents)
    {
        if(this.arraySize > -1 && this.arraySize < iGameEvents.GameEventValues.Length)
        {
            SerializedProperty gameEventValues = base.serializedObject.FindProperty("gameEventValues");
            SerializedProperty item = gameEventValues.GetArrayElementAtIndex(gameEventValues.arraySize - 1);
            item.FindPropertyRelative("muted").boolValue = false;
            item.FindPropertyRelative("gameEvent").objectReferenceValue = null;
            item.serializedObject.ApplyModifiedProperties();
        }

        if(this.arraySize > -1 && this.arraySize > iGameEvents.GameEventValues.Length)
        {
            CleanupSubs();
        }
    }

    void DrawButtons()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(Path() == "" || EditorApplication.isPlaying);
        if (GUILayout.Button("Cleanup Sub Assets")) CleanupSubs();
        EditorGUI.EndChangeCheck();

        EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
        if (GUILayout.Button("Call Events")) CallEvents();
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
    }

    void CleanupSubs()
    {
        List<GameEvent> allEvents = ReferencedGameEvents();
        List<GameEvent> toDelete = new List<GameEvent>();

        if(allEvents != null)
            foreach(GameEvent e in SubAssets())
                if(!allEvents.Contains(e))
                    toDelete.Add(e);

        foreach (GameEvent subAsset in toDelete)
            AssetDatabase.RemoveObjectFromAsset(subAsset);

        Object asset = AssetDatabase.LoadAssetAtPath(Path(), typeof(Object));

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssetIfDirty(asset);   
    }

    void CallEvents()
    {
        if (base.serializedObject.targetObject is IGameEvents iGameEvents)
            iGameEvents.CallEvents();        
    }

    List<GameEvent> ReferencedGameEvents()
    {
        if(base.serializedObject.targetObject is IGameEvents iGameEvents)
        {
            switch(iGameEvents)
            {
                case GameEventStack:
                    return iGameEvents.GameEvents.ToList();

                case Component component:
                    List<GameEvent> allEvents = new List<GameEvent>();

                    Transform TopTransform(Transform transform)
                    {
                        if (transform.parent == null)
                            return transform;

                        return TopTransform(transform.parent);
                    }

                    IGameEvents[] children = TopTransform(component.transform).GetComponentsInChildren<IGameEvents>();

                    foreach (IGameEvents iGameEventChild in children)
                        foreach(GameEvent gameEvent in iGameEventChild.GameEvents)
                            if(!allEvents.Contains(gameEvent))
                                allEvents.Add(gameEvent);

                    return allEvents;
            }            
        }

        return null;
    }
         
    string Path()
    {
        string path = AssetDatabase.GetAssetPath(base.serializedObject.targetObject);

        /// non valid path && do we have a stage and are we working on the prefab or its contents that are placed on the stage?
        if (path == "" && base.serializedObject.targetObject is Component component && PrefabStageUtility.GetCurrentPrefabStage() != null
            && PrefabStageUtility.GetCurrentPrefabStage().IsPartOfPrefabContents(component.gameObject))
            path = PrefabStageUtility.GetCurrentPrefabStage().assetPath;

        return path;
    }

    GameEvent[] SubAssets()
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(Path());
        List<GameEvent> list = new List<GameEvent>();
        foreach (UnityEngine.Object asset in assets)
            if (asset is GameEvent gameEvent && !list.Contains(gameEvent))
                list.Add(gameEvent);

        return list.ToArray();
    }    
}
