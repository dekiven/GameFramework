using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace GameFramework
{   
    public class ScrollItem : MonoBehaviour
    {
        public Button ItemClickBg;
        public UIHandler UIHandler;
        [HideInInspector]
        public int Index;
        public DelScrollItemClicked OnItemClicked;
        public DelBtnClickedStr OnBtnClickedStr;
        public DelBtnClickedIndex OnBtnClickedIndex;
        public RectTransform rectTransform { get { return transform as RectTransform; }}

        UIItemData mItemData = null;

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
            if(null != ItemClickBg)
            {
                ItemClickBg.onClick.AddListener(() => 
                {
                    if(null != OnItemClicked)
                    {
                        OnItemClicked(Index);
                    }
                });
            }
            if (UIHandler.Count > 0)
            {
                for (int i = 0; i < UIHandler.Count; i++)
                {
                    Button btn = UIHandler.UIArray[i] as Button;
                    if(null != btn && !btn.Equals(ItemClickBg))
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
            if(null != ItemClickBg)
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
            if(null != UIHandler)
            {
                if(null != mItemData)
                {
                    mItemData.ClearSyncRst();
                }
                for (int i = 0; i < scrollItemData.DataList.Count; i++)
                {
                    UIHandler.ChangeUI(scrollItemData.DataList[i]);
                }
                mItemData = scrollItemData;
            }
        }
    }

    public delegate void DelScrollItemClicked(int index);
    public delegate void DelBtnClickedStr(int index, string name);
    public delegate void DelBtnClickedIndex(int index, int btnIndex);
}