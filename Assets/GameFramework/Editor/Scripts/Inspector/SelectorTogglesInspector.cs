using System;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    [CustomEditor(typeof(SelectorToggles))]
    public class SelectorTogglesInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty dynamically = serializedObject.FindProperty("Dynamically");
            EditorGUILayout.PropertyField(dynamically, new GUIContent("Dynamically", "是否动态创建Togl\n如果是，需要TogglePrefab，不需要则要事先创建好toggle"));
            if(dynamically.boolValue)
            {
                SerializedProperty prefab = serializedObject.FindProperty("TogglePrefab");
                EditorGUILayout.PropertyField(prefab, new GUIContent("Toogle Preafab", "如果不同位置Toggle有变化，需要在Toggle上加上UIHandler,通过SetData的方式改变，当前仅支持初始化时改变,\n如果没有变化直接SetTotalNum"));
            }
            else
            {
                SerializedProperty toggles = serializedObject.FindProperty("mToggls");
                CustomListInspector.Show(toggles);
                if(toggles.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("请手动创建所有Toggle", MessageType.Warning);
                }
            }
            SerializedProperty callbackOnSet = serializedObject.FindProperty("CallbackOnSet");
            EditorGUILayout.PropertyField(callbackOnSet, new GUIContent("Callback On Set", "通过SetCurIndex设置的当前选中toggle时是否回调通知"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Group"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
