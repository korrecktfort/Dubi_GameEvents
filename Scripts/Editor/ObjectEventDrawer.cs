using Dubi.GameEvents;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ObjectEvent))]
public class ObjectEventDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position.height = EditorGUIUtility.singleLineHeight;
        float width = position.width;
        float x = position.x;
        float space = 5.0f;

        position.width = width - space;
        position.x = x + space;
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.BeginChangeCheck();        
        EditorGUI.PropertyField(position, property.FindPropertyRelative("name"), GUIContent.none);
        if (EditorGUI.EndChangeCheck())
            property.serializedObject.ApplyModifiedProperties();

        position.x = x;
        position.width = 0.0f;
        EditorGUI.BeginChangeCheck();
        property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(position, property.isExpanded, GUIContent.none);
        if(EditorGUI.EndChangeCheck())
            property.serializedObject.ApplyModifiedProperties();

        if (property.isExpanded)
        {
            position.width = width;
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property.FindPropertyRelative("events"));
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndFoldoutHeaderGroup();

        EditorGUI.EndProperty();        
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        if (property.isExpanded)
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("events"));

        return height;
    }
}

[CustomPropertyDrawer(typeof(ObjectEvents))]
public class ObjectEventsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {        
        EditorGUI.BeginProperty(position, GUIContent.none, property);

        EditorGUI.PropertyField(position, property.FindPropertyRelative("objectEvents"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("objectEvents"));
    }
}