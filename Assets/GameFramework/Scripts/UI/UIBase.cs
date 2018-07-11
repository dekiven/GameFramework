using System;
using UnityEngine;

//参考：http://blog.csdn.net/u011484013/article/details/52182997
namespace GameFramework
{
    public class UIBase : MonoBehaviour
    {
        //public GameObject UIPrefab;
        public bool IsBillboard;
        //public bool IsSingle;
        protected RenderMode mRenderMode = RenderMode.ScreenSpaceOverlay;

        public RenderMode GetUIMode()
        {
            return mRenderMode;
        }

        // Use this for initialization
        void Start()
        {
            IsBillboard = false;
            init();
        }

        // Update is called once per frame
        void Update()
        {
            if (IsBillboard)
            {
                CalcBillboard();
            }
            update();
        }


        /// <summary>
        /// 转换UI角度，使之正对摄像头，实现billboard效果
        /// </summary>
        private void CalcBillboard()
        {
            transform.rotation = Camera.main.transform.rotation;
        }


        protected virtual void init()
        {

        }

        protected virtual void update()
        {

        }
    }
}