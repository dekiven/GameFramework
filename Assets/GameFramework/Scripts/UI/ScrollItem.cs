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
        public RectTransform rectTransform { get { return transform as RectTransform; }}

        #region MonoBehaviour
        void Start()
        {
            if(null == UIHandler)
            {
                UIHandler = GetComponent<UIHandler>();
            }
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
                //test
                //UIHandler.SetTextString(0, scrollItemData.Info);
                for (int i = 0; i < scrollItemData.DataList.Count; i++)
                {
                    UIHandler.ChangeUI(scrollItemData.DataList[i]);
                }
            }
        }
    }

    public delegate void DelScrollItemClicked(int index);
}