using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace GameFramework
{   
    using DelScrollItemClicked = Action<int>;
    using DelBtnClickedStr = Action<int, string>;
    using DelBtnClickedIndex = Action<int, int>;

    public class ScrollItem : MonoBehaviour
    {
        public Button ItemClickBg;
        /// <summary>
        /// item被选中显示的效果
        /// </summary>
        public RectTransform selectedEffect;
        /// <summary>
        /// item没有被选中显示的效果
        /// </summary>
        public RectTransform unselectedEffect;
        public UIHandler UIHandler;
        [HideInInspector]
        public int Index;
        public DelScrollItemClicked OnItemClicked;
        public DelBtnClickedStr OnBtnClickedStr;
        public DelBtnClickedIndex OnBtnClickedIndex;
        public RectTransform rectTransform { get { return transform as RectTransform; } }

        UIItemData mItemData = null;

        [SerializeField]
        private bool mIsSelected = false;
        public bool IsSelected { 
            get { return mIsSelected; } 
            set 
            {
                mIsSelected = value; 
                if(null != selectedEffect)
                {
                    selectedEffect.gameObject.SetActive(value);
                }
                if (null != unselectedEffect)
                {
                    unselectedEffect.gameObject.SetActive(!value);
                }
            } 
        }



        #region MonoBehaviour
        void Awake()
        {
            if (null == UIHandler)
            {
                UIHandler = GetComponent<UIHandler>();
            }
        }

        void Start()
        {
            if (null != ItemClickBg)
            {
                ItemClickBg.onClick.AddListener(() =>
                {
                    if (null != OnItemClicked)
                    {
                        OnItemClicked(Index);
                    }
                });
            }
            if (null != UIHandler && UIHandler.Count > 0)
            {
                for (int i = 0; i < UIHandler.Count; i++)
                {
                    Button btn = UIHandler.UIArray[i] as Button;
                    if (null != btn && !btn.Equals(ItemClickBg))
                    {
                        btn.onClick.AddListener(() =>
                        {
                            if (null != OnBtnClickedStr)
                            {
                                OnBtnClickedStr(Index, btn.name);
                            }
                            if (null != OnBtnClickedIndex)
                            {
                                OnBtnClickedIndex(Index, UIHandler.GetUIIndex(btn));
                            }
                        });
                    }
                }
            }
        }

        void OnDestroy()
        {
            if (null != ItemClickBg)
            {
                ItemClickBg.onClick.RemoveAllListeners();
            }
        }
        #endregion MonoBehaviour

        /// <summary>
        /// 根据设置的data修改Item内容
        /// </summary>
        /// <param name="scrollItemData">Scroll item data.</param>
        public void SetData(UIItemData scrollItemData)
        {
            if (null != UIHandler)
            {
                if (null != mItemData)
                {
                    mItemData.ClearAsyncRst();
                }
                for (int i = 0; i < scrollItemData.DataList.Count; i++)
                {
                    UIHandler.ChangeUI(scrollItemData.DataList[i]);
                }
                mItemData = scrollItemData;
            }
        }
    }
}