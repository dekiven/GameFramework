using System;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    [CustomEditor(typeof(ScrollSelector))]
    public class ScroolSelectorInspector : Editor
    {
        ScrollSelector mTarget;
        private int showCount = 5;

        void OnEnable()
        {
            mTarget = target as ScrollSelector;
        }

        public override void OnInspectorGUI()
        {
            if(null == mTarget)
            {
                return;
            }
            Prefab = EditorGUILayout.ObjectField("ItemPrefab", Prefab, typeof(GameObject), false) as GameObject;
            //mTarget.ItemSize = EditorGUILayout.Vector2Field("ItemSize", mTarget.ItemSize);

            mTarget.ShowNum = EditorGUILayout.IntField(new GUIContent("Show Num", "Selector区域内显示Item的个数，必须是大于1的奇数"), mTarget.ShowNum);
            if(mTarget.ShowNum % 2 == 0)
            {
                mTarget.ShowNumFix(mTarget.ShowNum);
            }
            GUILayout.Space(5);
            base.OnInspectorGUI();

            //测试相关，显示给定数量的item
            if (Application.isPlaying)
            {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                showCount = EditorGUILayout.IntField("测试显示数量", showCount);
                if (GUILayout.Button("显示items"))
                {
                    System.Collections.Generic.List<UIItemData> data = new System.Collections.Generic.List<UIItemData>();
                    for (int i = 0; i < showCount; i++)
                    {
                        data.Add(new UIItemData());
                    }
                    mTarget.SetData(data);
                }
                GUILayout.EndHorizontal();
            }
        }

        public GameObject Prefab
        {
            get
            {
                return mTarget.ItemPrefab;
            }

            set
            {
                if (null != value)
                {
                    if (!Equals(value, mTarget.ItemPrefab))
                    {
                        mTarget.ItemPrefab = value;
                        resetItemSize();
                    }
                }
            }
        }

        /// <summary>
        /// 重新设置ScrollView的ItemSize
        /// </summary>
        private void resetItemSize()
        {
            GameObject obj = Instantiate(mTarget.ItemPrefab);
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            ScrollItem i = obj.GetComponent<ScrollItem>();
            if (null == i)
            {
                LogFile.Error("ScrollView 的 Item 必须添加ScrollView组件！");
                return;
            }
            if (null != rectTransform)
            {
                mTarget.ItemSize = rectTransform.rect.size;
            }
            DestroyImmediate(obj);
        }
    }
}
