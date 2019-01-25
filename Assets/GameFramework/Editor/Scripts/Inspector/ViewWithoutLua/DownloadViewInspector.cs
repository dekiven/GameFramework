using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{

    [CustomEditor(typeof(DownloadView))]
    public class DownloadViewInspector : UIViewInspector
    {
        //Editor 展示
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("STR_NOTIFY_FUNC"), new GUIContent("通知事件名"));
            serializedObject.ApplyModifiedProperties();
        }
    }

}