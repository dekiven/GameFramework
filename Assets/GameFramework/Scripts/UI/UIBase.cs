using System;
using System.Collections;
using LuaInterface;
using UnityEngine;

//参考：http://blog.csdn.net/u011484013/article/details/52182997
namespace GameFramework
{
    public class UIBase : MonoBehaviour
    {
        #region delegate
        public delegate void UIAnimResult(bool ret);
        #endregion

        public bool IsBillboard;
        protected RenderMode mRenderMode = RenderMode.ScreenSpaceOverlay;
        public UIHandler UIObjs;
        public bool IsInStack;

        //private Action<ViewStatus> mStatusChangeCall;
        private LuaDictTable mLuaFuncs;

        public bool HideBefor = true;

        public void SetLuaStatusListeners(LuaDictTable table)
        {
            mLuaFuncs = table;
        }

        public RenderMode GetUIMode()
        {
            return mRenderMode;
        }

        /// <summary>
        /// 准备显示的处理，可以在这里做打开动画,同时有lua和c#代码时执行lua
        /// </summary>
        public void Show(UIAnimResult callback)
        {
            onStartAnim(ViewStatus.onShow, callback);
        }

        /// <summary>
        /// 准备关闭的处理，可以在这里做关闭动画
        /// </summary>
        public void Hide(UIAnimResult callback)
        {
            onStartAnim(ViewStatus.onHide, callback);
        }

        public void Close()
        {
            GameUIManager.Instance.CloseView(this);
        }

        #region public virtual 方法


        #endregion

        #region protected virtual 方法
        protected virtual void init()
        {
            onStatusChange(ViewStatus.onInit);
        }

        protected virtual void update()
        {

        }

        /// <summary>
        /// 准备显示的处理，可以在这里做打开动画,同时有lua和c#代码时执行lua
        /// </summary>
        protected virtual void onShow(UIAnimResult callback)
        {
            //TODO:实现打开动画，可选择类型
            callback(true);
        }


        /// <summary>
        /// 准备关闭的处理，可以在这里做关闭动画
        /// </summary>
        protected virtual void onHide(UIAnimResult callback)
        {
            //TODO:实现关闭动画，可选择类型
            callback(true);
        }
        #endregion

        #region private 方法
        /// <summary>
        /// 转换UI角度，使之正对摄像头，实现billboard效果
        /// </summary>
        private void CalcBillboard()
        {
            transform.rotation = Camera.main.transform.rotation;
        }

        private void onStatusChange(ViewStatus status)
        {
            if(null != mLuaFuncs)
            {
                LuaFunction func = getLuaFunc(status.ToString());
                if(null != func)
                {
                    if (ViewStatus.onInit == status)
                    {
                        func.Call<UIHandler>(UIObjs);
                    }
                    else
                    {
                        func.Call();
                    }
                }
            }
        }

        private void onStartAnim(ViewStatus status, UIAnimResult callback)
        {
            UIAnimResult _callback = (bool ret) =>
            {
                switch (status)
                {
                    case ViewStatus.onHide:
                        gameObject.SetActive(false);
                        break;
                    case ViewStatus.onShow:
                        gameObject.SetActive(true);
                        break;
                }
                if(null != callback)
                {
                    callback(ret);
                }
            };
            if (null != mLuaFuncs)
            {
                LuaFunction func = getLuaFunc(status.ToString());
                if (null != func)
                {
                    func.Call<UIAnimResult>(_callback);
                }
            }
            else
            {
                switch(status)
                {
                    case ViewStatus.onHide :
                        onHide(_callback);
                        break;
                    case ViewStatus.onShow :
                        onShow(_callback);
                        gameObject.SetActive(true);
                        break;
                }
            }
        }

        private void Dispose()
        {
            if (null != mLuaFuncs)
            {
                mLuaFuncs.Dispose();
            }
        }

        private LuaFunction getLuaFunc(string funcName)
        {
            if(null == mLuaFuncs)
            {
                return null;
            }
            return mLuaFuncs[funcName] as LuaFunction;
        }
        #endregion

        #region MonoBehaviour
        void Start()
        {
            IsBillboard = false;
            init();

            //test
            //TODO:
            StartCoroutine(destroyAtTime());
        }

        IEnumerator destroyAtTime()
        {
            yield return new WaitForSeconds(10);
            Close();
        }

        void Update()
        {
            if (IsBillboard)
            {
                CalcBillboard();
            }
            update();
        }

        void OnDestroy()
        {
            onStatusChange(ViewStatus.onDestroy);
            Dispose();
        }
        #endregion
    }

    public enum ViewStatus
    {
        /// <summary>
        /// 当View Start时调用
        /// </summary>
        onInit = 0,
        //onEnable,
        //onDisable,
        onDestroy,
        onShow,
        //onShowEnd,
        onHide,
        //onHideEnd,
    }
}