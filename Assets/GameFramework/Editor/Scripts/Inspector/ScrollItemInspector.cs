using UnityEngine;
using UnityEditor;

namespace GameFramework
{
    [CustomEditor(typeof(ScrollItem))]
    public class ScrollItemInspector : Editor
    {
        private ScrollItem mTarget;

        //进入时刷新部分参数
        void OnEnable()
        {
            mTarget = target as ScrollItem;
            if (null == mTarget)
            {
                return;
            }
            //mPrefab = mTarget.ItemPrefab;
            //Debug.Log(mPrefab);
            if(null == mTarget.UIHandler)
            {
                mTarget.UIHandler = mTarget.GetComponent<UIHandler>();
            }
        }

        //Editor 展示
        public override void OnInspectorGUI()
        {
            if (null == mTarget)
            {
                return;
            }

            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            GUI.enabled = null == mTarget.UIHandler;
            if(GUILayout.Button("添加Handler"))
            {
                mTarget.UIHandler = mTarget.gameObject.AddComponent<UIHandler>();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}