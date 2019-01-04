using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    [CustomEditor(typeof(UIView))]
    public class UIViewInspector : UIBaseInspector
    {
        //进入时刷新部分参数
        void OnEnable()
        {
            mTarget = target as UIBase;
            if (null == mTarget)
            {
                return;
            }
        }

        //Editor 展示
        public override void OnInspectorGUI()
        {
            if (null == mTarget)
            {
                return;
            }

            //mTarget.IsStatic
            //mTarget.IsInStack
            //mTarget.HasDarkMask
            //mTarget.HideBefor
            //mTarget.AnimTime
            //mTarget.AnimType
            //mTarget.AnimEase
            //mTarget.AnimValue
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("IsInStack"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HasDarkMask"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HideBefor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IsStatic"));

            GUILayout.Space(10);
            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
