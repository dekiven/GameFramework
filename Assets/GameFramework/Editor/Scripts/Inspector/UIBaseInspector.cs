﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    [CustomEditor(typeof(UIBase), true)]
    public class UIBaseInspector : Editor
    {
        protected UIBase mTarget;

        //进入时刷新部分参数
        void OnEnable()
        {
            mTarget = target as UIBase;
            if (null == mTarget)
            {
                return;
            }
            if (null == mTarget.Handler)
            {
                mTarget.Handler = mTarget.gameObject.GetComponent<UIHandler>();
                //serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnInspectorGUI()
        {
            if (null == mTarget)
            {
                return;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimType"));
            if(ViewAnimType.none != mTarget.AnimType)
            {
                if (ViewAnimType.zoom != mTarget.AnimType)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimDisType"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimOppor"));   
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimEase"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimTime"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimValue"));
            }           

            mTarget.Handler = EditorGUILayout.ObjectField("UIHandler", mTarget.Handler, typeof(UIHandler), false) as UIHandler;
            if (null == mTarget.Handler)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("添加UIHandler", "推荐使用UIHandler管理UIBehaviour")))
                {
                    mTarget.Handler = mTarget.gameObject.AddComponent<UIHandler>();
                }
                GUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}