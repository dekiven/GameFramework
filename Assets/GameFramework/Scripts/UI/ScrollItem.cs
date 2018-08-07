using UnityEngine;
using System.Collections;
using System;

namespace GameFramework
{   
    public class ScrollItem : MonoBehaviour, IDisposable
    {
        private UIHandler mHandler;
        public ObjPool<ScrollItem> ItemPool;
        
        void Start()
        {
            if(null == mHandler)
            {
                mHandler = GetComponent<UIHandler>();
            }
        }

        public void Dispose()
        {
            
        }
    }
}