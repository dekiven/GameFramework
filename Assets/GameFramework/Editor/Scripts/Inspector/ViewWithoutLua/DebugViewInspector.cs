using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{

    [CustomEditor(typeof(DebugView))]
    public class DebugViewInspector : UIViewInspector
    {
        //Editor 展示
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DebugPL"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MainBtn"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LogFreshInterval"), new GUIContent("日志刷新间隔"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxLineNum"), new GUIContent("最大日志条数"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LogSvIdx"), new GUIContent("日志Sv index"));
            CustomListInspector.Show(serializedObject.FindProperty("PagesIdx"));
            //EditorGUILayout.PropertyField();
            GUILayout.Space(5);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }

}