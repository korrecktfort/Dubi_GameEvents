using Dubi.GameEvents;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[CustomPropertyDrawer(typeof(GameEventValue))]
public class GameEventDrawer : PropertyDrawer
{
    float headerHeight = 22.0f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = this.headerHeight;
        Rect rect = new Rect(position);
        SerializedProperty eventProperty = property.FindPropertyRelative("gameEvent");

        void DrawEntryHeader()
        {
            SerializedProperty muted = property.FindPropertyRelative("muted");
            Texture2D tex = new Texture2D(1, 1);
            float greyScale = 0.18f;
            tex.SetPixel(0, 0, new Color(greyScale, greyScale, greyScale, 1.0f));
            tex.Apply();

            GUIStyle box = new GUIStyle(GUI.skin.box);
            box.normal.background = tex;   
            box.alignment = TextAnchor.MiddleLeft;
            box.stretchHeight = false;


            box.normal.textColor = muted.boolValue ? Color.gray : Color.white;

            string TypeSuffix()
            {
                bool isMainAsset = AssetDatabase.IsMainAsset(eventProperty.objectReferenceValue);
                bool isSubAsset = AssetDatabase.IsSubAsset(eventProperty.objectReferenceValue);

                if (!isMainAsset && !isSubAsset)
                    return ""; /// There will be a (Clone) string thanks to instancing on game start

                if (eventProperty.IsNoSubAsset() || isMainAsset)
                    return " (Asset)";

                return " (Sub Asset)";
            }
                        
            EditorGUI.LabelField(position, eventProperty.objectReferenceValue.name + TypeSuffix(), box);

            #region Buttons
            Vector2 buttonSize = Vector2.one * 18.0f;
            float yOffset = Mathf.Ceil((position.height - buttonSize.y) * 0.5f);
            position.x += position.width - buttonSize.x - yOffset;
            position.y += yOffset;
            position.width = buttonSize.x;
            position.height = buttonSize.y;

            /// Delete Button
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.padding = new RectOffset(0, 0, 1, 0);
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            Vector2 iconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(buttonSize - Vector2.one * 1.0f);
            if (GUI.Button(position, new GUIContent(EditorGUIUtility.IconContent("d_clear", "Remove Event")), buttonStyle))
            {      
                eventProperty.DeleteSubAsset(eventProperty.objectReferenceValue as ScriptableObject);
                eventProperty.objectReferenceValue = null;
                eventProperty.isExpanded = false;
                eventProperty.serializedObject.ApplyModifiedProperties();

            }
            EditorGUIUtility.SetIconSize(iconSize);
                        
            /// Highlight Button
            position.x -= position.width + 2.0f;
            buttonStyle.alignment = TextAnchor.UpperCenter;
            buttonStyle.padding = new RectOffset(2, 0, 2, 0);
            if (GUI.Button(position, new GUIContent("h", "Highlight Event Asset"), buttonStyle))
            {
                EditorGUIUtility.PingObject(eventProperty.objectReferenceValue);             
            }

            /// Muted Button
            position.x -= position.width + 2.0f;
            buttonStyle.alignment = TextAnchor.UpperCenter;
            buttonStyle.padding = new RectOffset(1, 0, 2, 0);
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            muted.boolValue = GUI.Toggle(position, muted.boolValue, new GUIContent("m", "If toggled, the instantiated event will be muted."), buttonStyle);
            EditorGUI.EndDisabledGroup();
            if(EditorGUI.EndChangeCheck())                
                property.serializedObject.ApplyModifiedProperties();
            #endregion

            position = rect;
        }

        void DrawEmptyEntryHeader()
        {     
            position.height = EditorGUIUtility.singleLineHeight;
            float x = position.x;
            float width = position.width;
            float buttonWidth = 100.0f;
            float space = 3.0f;           
            position.width -= buttonWidth + space;
                           
            EditorGUI.BeginChangeCheck();            
            EditorGUI.ObjectField(position, eventProperty, GUIContent.none);
            if (EditorGUI.EndChangeCheck())            
                eventProperty.serializedObject.ApplyModifiedProperties();                
                        

            #region Add Sub Asset
            position.x += position.width + space;
            position.width = buttonWidth;
            string ButtonText()
            {   
                return "Create Sub";
            }

            if (GUI.Button(position, ButtonText(), GUI.skin.FindStyle("Button")))
            {
                Type[] types = typeof(GameEvent).GetAllSubTypes(eventProperty);

                GenericMenu genericMenu = new GenericMenu();

                void OnEntrySelected(object obj)
                {
                    Type type = obj as Type;
                    var scriptableObject = ScriptableObject.CreateInstance(type);

                    string name = type.Name.ToString();
                    name = string.Concat(name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
                    scriptableObject.name = name;

                    if (eventProperty.IsPrefabAsset())
                    {
                        UnityEngine.Object prefab = eventProperty.GetPrefabAsset();

                        if (prefab == null)
                            prefab = eventProperty.serializedObject.targetObject;

                        AssetDatabase.AddObjectToAsset(scriptableObject, prefab);
                        AssetDatabase.SaveAssetIfDirty(prefab);
                    }

                    eventProperty.objectReferenceValue = scriptableObject;
                    eventProperty.isExpanded = true;
                    eventProperty.serializedObject.ApplyModifiedProperties();
                }

                foreach (Type type in types)
                {
                    string menuPath = type.ToString();
                    if (menuPath.Contains("."))
                    {
                        int lastIndex = menuPath.LastIndexOf('.');
                        menuPath = menuPath[..lastIndex] + "/" + menuPath[(lastIndex + 1)..];
                    }

                    genericMenu.AddItem(new GUIContent(menuPath), false, OnEntrySelected, type);
                }

                genericMenu.ShowAsContext();
            }
            #endregion
        }

        void DrawFoldout()
        {
            if (!eventProperty.DisplayFoldout())
                return;

            EditorGUI.BeginChangeCheck();
            eventProperty.isExpanded = EditorGUI.Foldout(rect, eventProperty.isExpanded, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
                eventProperty.serializedObject.ApplyModifiedProperties();
        }

        void DrawFoldoutProperties()
        {
            if (!eventProperty.DisplayFoldout())
                return;

            if (!eventProperty.isExpanded)
                return;

            SerializedObject serializedEvent = new SerializedObject(eventProperty.objectReferenceValue);
            SerializedProperty lockValuesOnSubAsset = serializedEvent.FindProperty("lockValuesOnSubAsset");
            EditorGUI.indentLevel++;
            float lastPropHeight = position.height;
            position.y += EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.BeginDisabledGroup(lockValuesOnSubAsset.boolValue);
            switch(serializedEvent.targetObject)
            {
                case CallObjectEvent:
                    string[] options = eventProperty.GetObjectEventNames();
                    position.y += lastPropHeight;

                    if (options.Length == 0)
                    {
                        float oeWidth = position.width;
                        position.width = 175.0f;
                        EditorGUI.LabelField(position, "No Object Events available.");

                        position.x += position.width;
                        position.width = oeWidth - position.width;

                        if (!eventProperty.HasObjectEventComponent())
                        {
                            if (GUI.Button(position, "Add Object Events"))
                            {
                                eventProperty.GetTransform().gameObject.AddComponent<ObjectEventsComponent>();
                            }
                        }
                        break;
                    }

                    /// Find and apply CallGameEvents reference
                    eventProperty.ReferenceInstantiatedObjectEventComponent(serializedEvent);

                    /// Display Game Events Names Selection
                    SerializedProperty selectedIndex = serializedEvent.FindProperty("selectedIndex");
                    EditorGUI.BeginChangeCheck();
                    selectedIndex.intValue = EditorGUI.Popup(position, selectedIndex.intValue, options);
                    if (EditorGUI.EndChangeCheck())
                        selectedIndex.serializedObject.ApplyModifiedProperties();
                    break;

                default:
                    foreach (string propertyName in eventProperty.GetValidPropertyNames())
                    {
                        position.y += lastPropHeight;
                        SerializedProperty currentProp = serializedEvent.FindProperty(propertyName);

                        EditorGUI.BeginChangeCheck();
                        EditorGUI.PropertyField(position, currentProp, new GUIContent(currentProp.displayName), true);
                        if (EditorGUI.EndChangeCheck())
                            serializedEvent.ApplyModifiedProperties();

                        lastPropHeight = EditorGUI.GetPropertyHeight(currentProp) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    break;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
        }

        void DisplayError(string message, string buttonText = "", Action onPressed = null)
        {
            EditorGUI.indentLevel++;
            position.y += headerHeight + EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUIUtility.singleLineHeight * 2.0f;
            float buttonWidth = buttonText == "" ? 0.0f : 100.0f;
            float space = 3.0f;
            position.width -= buttonWidth + space;

            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new Color(.9f, .2f, .2f, 0.5f));
            tex.Apply();
            GUIStyle gUIStyle = new GUIStyle(GUI.skin.box);
            gUIStyle.normal.background = tex;
            gUIStyle.wordWrap = true;
            gUIStyle.alignment = TextAnchor.MiddleLeft;
            gUIStyle.normal.textColor = Color.white;

            EditorGUI.LabelField(position, message, gUIStyle);
            position.x += position.width + space;
            position.width = buttonWidth;
                        
            if (buttonText != "" && GUI.Button(position, buttonText))
            {
                onPressed?.Invoke();
            }
            
            EditorGUI.indentLevel--;
        }

        bool DrawError()
        {
            Type type = null;

            if(eventProperty.ObjectComponentMissing(out type))
            {
                DisplayError("Missing component of type " + type.Name, "Add Missing", () => { eventProperty.GetTransform().gameObject.AddComponent(type); });  
                return true;               
            }           

            if(eventProperty.GameEventStackLoop())
            {
                DisplayError("A referenced GameEventStack is creating a reference loop. Take care of this or you will cause a stack overflow on game start!");
                return true;
            }

            return false;
        }

        EditorGUI.BeginProperty(position, GUIContent.none, property);

        DrawFoldout();

        if (eventProperty.objectReferenceValue != null)
            DrawEntryHeader();
        else
            DrawEmptyEntryHeader();

        if(!DrawError())
            DrawFoldoutProperties();

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty eventProperty = property.FindPropertyRelative("gameEvent");
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (eventProperty.objectReferenceValue != null)
            height = this.headerHeight + EditorGUIUtility.standardVerticalSpacing;

        if (eventProperty.ObjectComponentMissing(out Type type) || eventProperty.GameEventStackLoop())
            height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2.0f;

        if (eventProperty.DisplayFoldout() && eventProperty.isExpanded)
        {            
            SerializedObject serializedEntry = new SerializedObject(eventProperty.objectReferenceValue);

            switch (serializedEntry.targetObject)
            {
                case CallObjectEvent:
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    break;

                default: 
                    foreach (string propertyName in eventProperty.GetValidPropertyNames())
                    {
                        height += EditorGUI.GetPropertyHeight(serializedEntry.FindProperty(propertyName)) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    break;
            }
        }

        return height;
    }
}

public static class GameEventDrawerExtensions
{   
    public static bool GameEventStackLoop(this SerializedProperty property)
    {
        bool ContainsSameType(IGameEvents iGameEvents, Type type)
        {
            foreach (GameEventValue gameEventValue in iGameEvents.GameEventValues)
                if (gameEventValue.GameEvent is IGameEvents subStack)
                    if (subStack.GetType() == type)
                        return true;
                    else return ContainsSameType(subStack, type);

            return false;
        }


        return property.objectReferenceValue is IGameEvents iGameEvents && ContainsSameType(iGameEvents, iGameEvents.GetType());
    }

    public static bool ObjectComponentMissing(this SerializedProperty property, out Type type)
    {
        type = null;

        if (property.objectReferenceValue == null)
            return false;

        RequireObjectComponent requireAttr = property.objectReferenceValue.GetType().GetCustomAttribute<RequireObjectComponent>(false);

        if (requireAttr == null || !requireAttr.type.IsSubclassOf(typeof(Component)))
            return false;

        type = requireAttr.type;
        Transform transform = property.GetTransform();
        if (transform.GetComponent(type) != null)
            return false;

        return true;
    }

    public static bool DisplayFoldout(this SerializedProperty property)
    {
        if(property.objectReferenceValue == null) 
            return false;

        SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
        switch(serializedObject.targetObject)
        {
            case CallObjectEvent:
                return property.GetObjectEventNames().Length > 0;               

            default:
                return property.GetValidPropertyNames().Length > 0;
        }
    }   

    public static void ReferenceInstantiatedObjectEventComponent(this SerializedProperty property, SerializedObject serializedEvent)
    {
        if(property.isInstantiatedPrefab && property.objectReferenceValue != null && property.objectReferenceValue is CallObjectEvent objectEvent)
        {            
            SerializedProperty objectEvents = serializedEvent.FindProperty("objectEvents");
            ObjectEventsComponent objectEventsComponent = property.GetCallObjectEventsComponent();

            if(objectEvents.objectReferenceValue != objectEventsComponent)
            {
                objectEvents.objectReferenceValue = objectEventsComponent;
                objectEvents.serializedObject.ApplyModifiedProperties();
            }
        }
    }

    public static ObjectEventsComponent GetCallObjectEventsComponent(this SerializedProperty property)
    {
        if (property.serializedObject.targetObject is Component component)
        {
            component.TryGetComponent(out ObjectEventsComponent oeComponent);
            return oeComponent;
        }

        return null;
    }

    public static bool HasObjectEventComponent(this SerializedProperty property)
    {
        if(property.serializedObject.targetObject is Component component)
        {
            return component.TryGetComponent(out ObjectEventsComponent oeComponent);
        }

        return false;
    }

    public static string[] GetObjectEventNames(this SerializedProperty property)
    {
        if(property.serializedObject.targetObject is Component component)
        {
            if (component.gameObject.TryGetComponent(out ObjectEventsComponent objectEvents))
                return objectEvents.ObjectEvents.EventNames;
        }

        return new string[0];
    }  

    public static void DeleteSubAsset(this SerializedProperty property, ScriptableObject scriptableObject)
    {
        if (!property.IsPrefabAsset())
            return;

        GameObject prefab = property.GetPrefabAsset();
        foreach(UnityEngine.Object subAsset in AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(prefab)))
        {
            if(subAsset != null && AssetDatabase.IsSubAsset(subAsset) && subAsset.GetInstanceID() == scriptableObject.GetInstanceID())
            {
                AssetDatabase.RemoveObjectFromAsset(subAsset);
                AssetDatabase.SaveAssetIfDirty(prefab);
                // AssetDatabase.SaveAssets();
            }    
        }
    }

    public static GameObject GetPrefabAsset(this SerializedProperty property)
    {
        Transform current = property.GetTopMostTransform();       

        if (current == null)
            return null;

        /// casual prefab path when working on the prefab in the project foldder
        string path = AssetDatabase.GetAssetPath(property.serializedObject.targetObject);
        
        /// non valid path && do we have a stage and are we working on the prefab or its contents that are placed on the stage?
        if(path == "" && PrefabStageUtility.GetCurrentPrefabStage() != null 
            && PrefabStageUtility.GetCurrentPrefabStage().IsPartOfPrefabContents(current.gameObject))
        {
            path = PrefabStageUtility.GetCurrentPrefabStage().assetPath;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return prefab;
    }

    public static Transform GetTransform(this SerializedProperty property)
    {
        if(property.serializedObject.targetObject is Component component)
        {
            return component.transform;
        }

        return null;
    }

    public static Transform GetTopMostTransform(this SerializedProperty property)
    {
        if(property.serializedObject.targetObject is Component component)
        {
            Transform current = component.transform;

            if (current.parent == null)
                return current;

            do
            {
                current = current.parent;
            } while (current.parent != null);

            return current;
        }

        return null;
    }

    public static bool IsPrefabAsset(this SerializedProperty property)
    {
        if(EditorApplication.isPlaying)
            return false;

        return !property.isInstantiatedPrefab;       
    }

    public static bool IsNoSubAsset(this SerializedProperty property)
    {
        return !property.isInstantiatedPrefab && !AssetDatabase.IsSubAsset(property.objectReferenceValue);        
    }
    
    public static Type[] GetAllSubTypes(this Type type, SerializedProperty property)
    {       
        List<Type> types = new List<Type>();

        void AddTypes(Assembly assembly)
        {
            Type[] subTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(type)).ToArray();
            foreach (Type subType in subTypes)
            {
                if (types.Contains(subType))
                    continue;

                if (subType.GetCustomAttribute<DenyAsSubAssetAttribute>(false) != null)
                    continue;

                /// We are trying to add events that need a component to a non gameobject asset
                if (property.serializedObject.targetObject is not Component
                    && subType.GetCustomAttribute<RequireObjectComponent>(false) != null)
                    continue;

                types.Add(subType);
            }
        }

        string assemblyName = type.Assembly.GetName().Name;
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
                if (reference.Name == assemblyName)
                    AddTypes(assembly);

            if (assembly == type.Assembly)
                AddTypes(assembly);
        }

        return types.ToArray();        
    }

    public static string[] GetValidPropertyNames(this SerializedProperty property)
    {
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
        GameEvent target = property.objectReferenceValue as GameEvent;

        if(target == null) 
            return new string[0];

        if (target.GetType().GetCustomAttribute<DenyAsSubAssetAttribute>(false) != null)
            return new string[0];


        List<string> propertyNames = new List<string>();
        foreach (FieldInfo fieldInfo in target.GetType().GetFields(flags))
        {
            if (fieldInfo.IsPrivate && fieldInfo.GetCustomAttribute<SerializeField>() == null)
                continue;

            if (fieldInfo.GetCustomAttribute<HideAttribute>() != null)
                continue;

            if (fieldInfo.GetCustomAttribute<HideInInspector>() != null)
                continue;

            if (fieldInfo.GetCustomAttribute<ShowOnSubAsset>() != null && property.IsNoSubAsset() && !target.LockValuesOnSubAsset)
                continue;

            propertyNames.Add(fieldInfo.Name);
        }

        return propertyNames.ToArray();
    }
}
