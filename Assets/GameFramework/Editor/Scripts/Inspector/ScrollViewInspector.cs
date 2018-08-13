using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;

namespace GameFramework
{
    [CustomEditor(typeof(ScrollView))]
    [CanEditMultipleObjects]
    public class ScrollViewInspector : Editor
    {
        #region private 属性相关
        private ScrollView mTarget;
        private GameObject mPrefab;
        private SerializedObject serializedObj;
        private ScrollViewType mType;

        private AnimBool fadeGroup;

        private SerializedProperty mScrollType;
        private SerializedProperty mPLeft;
        private SerializedProperty mPRight;
        private SerializedProperty mPTop;
        private SerializedProperty mPBottom;
        private SerializedProperty mItemSize;
        public GameObject Prefab
        {
            get
            {
                return mPrefab;
            }

            set
            {
                if (null != value)
                {
                    //if(!Equals(value, mPrefab))
                    {
                        GameObject obj = Instantiate(value);
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
                            //mTarget.CalculateContentSize();
                        }
                        DestroyImmediate(obj);
                        mPrefab = value;
                        mTarget.ItemPrefab = value;
                    }
                }
            }
        }

        public ScrollViewType ScrollType
        {
            get
            {
                return mType;
            }

            set
            {
                mType = value;
                bool isVertical = ScrollViewType.Vertical == value;
                mTarget.vertical = isVertical;
                mTarget.horizontal = !isVertical;
            }
        }
        #endregion

        //进入时刷新部分参数
        void OnEnable()
        {
            mTarget = target as ScrollView;
            if (null == mTarget)
            {
                return;
            }
            mPrefab = mTarget.ItemPrefab;

            serializedObj = new SerializedObject(target);
            mScrollType = serializedObj.FindProperty("ScrollType");
            mPLeft = serializedObj.FindProperty("PaddingLeft");
            mPRight = serializedObj.FindProperty("PaddingRight");
            mPTop = serializedObj.FindProperty("PaddingTop");
            mPBottom = serializedObj.FindProperty("PaddingBottom");
            mItemSize = serializedObj.FindProperty("ItemSize");
            //Debug.Log(mPrefab);

            //super 属性折叠相关
            fadeGroup = new AnimBool(false);
            // 注册动画监听
            fadeGroup.valueChanged.AddListener(Repaint);
        }

        //Editor 展示
        public override void OnInspectorGUI()
        {
            if (null == mTarget)
            {
                return;
            }

            #region 显示ScrollRect的属性
            // target控制动画开始播放
            fadeGroup.target = EditorGUILayout.Foldout(fadeGroup.target, "ScrollRect", true);
            // 系统使用tween渐变faded数值
            if (EditorGUILayout.BeginFadeGroup(fadeGroup.faded))
            {
                base.OnInspectorGUI();
            }
            // begin - end 之间元素会进行动画
            EditorGUILayout.EndFadeGroup();
            // 又一种风格的空格
            GUILayout.Space(10);
            #endregion 显示ScrollRect的属性

            Prefab = EditorGUILayout.ObjectField("ItemPrefab", Prefab, typeof(GameObject), false) as GameObject;

            EditorGUILayout.PropertyField(mItemSize);

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(mScrollType);
            foreach (var item in Enum.GetValues(typeof(ScrollViewType)))
            {
                if (mScrollType.enumValueIndex == (int)item)
                {
                    if (mTarget.ScrollType != (ScrollViewType)item)
                    {
                        bool isVertical = ScrollViewType.Vertical == (ScrollViewType)item;
                        mTarget.ScrollType = (ScrollViewType)item;
                        mTarget.vertical = isVertical;
                        mTarget.horizontal = !isVertical;
                        RectTransform content = mTarget.content;

                        if (isVertical)
                        {
                            //设置与父节点等宽，顶端与父节点对齐，高度暂定与父节点一样
                            content.anchorMin = new Vector2(0, 1);
                            content.anchorMax = new Vector2(1, 1);
                            content.localPosition = Vector3.zero;
                            content.pivot = new Vector2(0.5f, 1f);
                            content.offsetMax = Vector2.zero;
                            content.offsetMin = Vector2.zero;
                            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mTarget.viewport.rect.size.y);
                        }
                        else
                        {
                            //设置与父节点等高，左侧与父节点对齐，宽度暂定与父节点一样
                            mTarget.content.anchorMin = new Vector2(0, 0);
                            mTarget.content.anchorMax = new Vector2(0, 1);
                            content.localPosition = Vector3.zero;
                            content.pivot = new Vector2(0f, 0.5f);
                            content.offsetMax = Vector2.zero;
                            content.offsetMin = Vector2.zero;
                            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mTarget.viewport.rect.size.x);
                        }
                    }
                    break;
                }
            }
            GUILayout.Space(10);

            EditorGUILayout.PropertyField(mPLeft);
            EditorGUILayout.PropertyField(mPRight);
            EditorGUILayout.PropertyField(mPTop);
            EditorGUILayout.PropertyField(mPBottom);
            GUILayout.Space(5);
        }


        private void OnDisable()
        {
            // 移除动画监听
            fadeGroup.valueChanged.RemoveListener(Repaint);
        }
    } 
}