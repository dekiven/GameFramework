using UnityEditor;
using UnityEngine;

namespace GameFramework
{ 
    [CustomEditor(typeof(UIWorld))]
    public class UIWorldInspector : UIBaseInspector
    {
        ////进入时刷新部分参数
        //void OnEnable()
        //{
        //    mTarget = target as UIWorld;
        //    if (null == mTarget)
        //    {
        //        return;
        //    }
        //}

        //Editor 展示
        public override void OnInspectorGUI()
        {
            if (null == mTarget)
            {
                return;
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("IsBillboard"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("UITarget"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UIOffset"));

            GUILayout.Space(5);
            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
