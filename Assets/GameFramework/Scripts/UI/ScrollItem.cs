using UnityEngine;
using System.Collections;
using System;

namespace GameFramework
{   
    public class ScrollItem : MonoBehaviour
    {
        public UIHandler UIHandler;
        
        void Start()
        {
            if(null == UIHandler)
            {
                UIHandler = GetComponent<UIHandler>();
            }
        }

        void OnDestroy()
        {
            
        }

        public void SetData(ScrollItemData scrollItemData)
        {
            if(null != UIHandler)
            {
                //test
                UIHandler.SetTextString(0, scrollItemData.Info);

                //TODO:实现根据传入的data修改UI组件
            }
        }
    }
}