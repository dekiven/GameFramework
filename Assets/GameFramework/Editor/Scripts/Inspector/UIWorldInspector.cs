using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    
    [CustomEditor(typeof(UIWorld))]
    public class UIWorldInspector : Editor
    {
        private UIWorld mTarget;

        //进入时刷新部分参数
        void OnEnable()
        {
            mTarget = target as UIWorld;
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

            //GUILayout.Space(5);
            //mTarget.IsBillboard = EditorGUILayout.Toggle(new GUIContent("IsBillboard"), mTarget.IsBillboard);
            //GUILayout.Space(5);
            //mTarget.UIOffset = EditorGUILayout.Vector3Field("UIOffset", mTarget.UIOffset);
            //GUILayout.Space(5);

            base.OnInspectorGUI();

            mTarget.Handler = EditorGUILayout.ObjectField("UIHandler", mTarget.Handler, typeof(UIHandler), false) as UIHandler;
            GUILayout.BeginHorizontal();
            GUI.enabled = null == mTarget.Handler;
            //if (GUILayout.Button(new GUIContent("添加UIHandler"), GUILayout.ExpandWidth(false)))
            if (GUILayout.Button(new GUIContent("添加UIHandler", "推荐使用UIHandler管理UIBehaviour")))
            {
                mTarget.Handler = mTarget.gameObject.AddComponent<UIHandler>();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}
