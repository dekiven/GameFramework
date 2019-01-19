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
        private int showCount = 30;
        private ScrollView mTarget;
        private AnimBool fadeGroup;
        private ScrollViewType mScrollType;
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
        #endregion

        //进入时刷新部分参数
        void OnEnable()
        {
            mTarget = target as ScrollView;
            if (null == mTarget)
            {
                return;
            }
            if (null != mTarget.ItemPrefab)
            {
                resetItemSize();
            }

            //resetScrollType();
            mScrollType = mTarget.ScrollType;

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
            serializedObject.Update();

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
            if(null == Prefab)
            {
                EditorGUILayout.HelpBox("请设置 ScrollView ItemPrefab", MessageType.Error);

            }

            mTarget.ItemSize = EditorGUILayout.Vector2Field("ItemSize", mTarget.ItemSize);
            if(mTarget.ItemSize.x.Equals(-1f) || mTarget.ItemSize.y.Equals(-1f))
            {
                EditorGUILayout.HelpBox("ItemSize x或y 值为-1时，表示 item 根据 ScrollView 缩放，prefab的RectTransform中left、right、top、bottom 无效，请调整下方PaadingXXX 修改 Item 显示大小", MessageType.Warning);
            }

            GUILayout.Space(10);

            mTarget.ScrollType = (ScrollViewType)EditorGUILayout.EnumPopup("ScrollType", mTarget.ScrollType);
            if (mScrollType != mTarget.ScrollType)
            {
                resetScrollType();
                mScrollType = mTarget.ScrollType;
            }
            GUILayout.Space(10);

            mTarget.PaddingLeft = EditorGUILayout.FloatField("PaddingLeft", mTarget.PaddingLeft);
            mTarget.PaddingRight = EditorGUILayout.FloatField("PaddingRight", mTarget.PaddingRight);
            mTarget.PaddingTop = EditorGUILayout.FloatField("PaddingTop", mTarget.PaddingTop);
            mTarget.PaddingBottom = EditorGUILayout.FloatField("PaddingBottom", mTarget.PaddingBottom);
            GUILayout.Space(5);

            mTarget.ItemNumPerStep = EditorGUILayout.IntField("ItemNumPerSte", mTarget.ItemNumPerStep);

            string str = mTarget.ScrollType == ScrollViewType.Vertical ? "行" : "列";
            SerializedProperty showItemIntegersProperty = serializedObject.FindProperty("ShowItemIntegers");
            EditorGUILayout.PropertyField(showItemIntegersProperty, new GUIContent("显示整数" + str));
            if (!showItemIntegersProperty.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LineOffset"), new GUIContent(str + "间距"));
            }
            serializedObject.ApplyModifiedProperties();

            //测试相关，显示给定数量的item
            if(Application.isPlaying)
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


        private void OnDisable()
        {
            // 移除动画监听
            fadeGroup.valueChanged.RemoveListener(Repaint);
        }

        #region 私有方法
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
                if(!rectTransform.anchorMin.x.Equals(rectTransform.anchorMax.x))
                {
                    mTarget.ItemSize.x = -1;
                }
                if (!rectTransform.anchorMin.y.Equals(rectTransform.anchorMax.y))
                {
                    mTarget.ItemSize.y = -1;
                }
            }
            DestroyImmediate(obj);
        }

        /// <summary>
        /// 根据ScrollType设置Viewport和Content的RectTransform
        /// </summary>
        private void resetScrollType()
        {
            bool isVertical = mTarget.ScrollType == ScrollViewType.Vertical;

            mTarget.vertical = isVertical;
            mTarget.horizontal = !isVertical;
            RectTransform content = mTarget.content;
            RectTransform viewport = mTarget.viewport;

            if (isVertical)
            {
                viewport.offsetMin = Vector2.zero;
                viewport.offsetMax = Vector2.zero;
                if(null != mTarget.verticalScrollbar)
                {
                    viewport.offsetMax = new Vector2(-(mTarget.verticalScrollbar.transform as RectTransform).rect.width - mTarget.verticalScrollbarSpacing, 0);
                }
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
                viewport.offsetMin = Vector2.zero;
                if (null != mTarget.horizontalScrollbar)
                {
                    viewport.offsetMin = new Vector2(0, (mTarget.horizontalScrollbar.transform as RectTransform).rect.height + mTarget.horizontalScrollbarSpacing);
                }
                viewport.offsetMax = Vector2.zero;
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
        #endregion 私有方法
    }
}