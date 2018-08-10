using System;
using System.Collections;
using LuaInterface;
using UnityEngine;
using DG.Tweening;

//参考：http://blog.csdn.net/u011484013/article/details/52182997
namespace GameFramework
{
    public class UIBase : MonoBehaviour
    {
        #region delegate
        public delegate void UIAnimResult(bool ret);
        #endregion

        public bool IsBillboard;
        public UIHandler UIObjs;
        public bool IsInStack;
        public bool HideBefor = true;
        /// <summary>
        /// show和hide的动画时间
        /// </summary>
        public float AnimTime = 0.5f;
        /// <summary>
        /// show和hide的动画值，可能是缩放大小、某个轴的移动距离等
        /// </summary>
        public float AnimValue = 1f;
        public ViewAnimType AnimType = ViewAnimType.none;
        public UIAnimResult OnInitCallbcak;
        /// <summary>
        /// 动画缓动效果，默认无效果
        /// </summary>
        public Ease AnimEase = Ease.Linear;
        public bool HasDarkMask = true;

        protected RenderMode mRenderMode = RenderMode.ScreenSpaceOverlay;
        //private Action<ViewStatus> mStatusChangeCall;

        private Tween mAnimTween = null;
        private LuaDictTable mLuaFuncs;
        private RectTransform mRectTransform;


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
            onStartAnim(ViewStatus.onShowBegin, callback);
        }

        /// <summary>
        /// 准备关闭的处理，可以在这里做关闭动画
        /// </summary>
        public void Hide(UIAnimResult callback)
        {
            onStartAnim(ViewStatus.onHideBegin, callback);
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
            if(!onLuaStatusChange(ViewStatus.onInit))
            {
                if(null != OnInitCallbcak)
                {
                    OnInitCallbcak(true);
                }
            }
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
            gameObject.SetActive(true);
            getAnimTween(AnimType, true, callback);
            //callback(true);
        }


        /// <summary>
        /// 准备关闭的处理，可以在这里做关闭动画
        /// </summary>
        protected virtual void onHide(UIAnimResult callback)
        {
            //TODO:实现关闭动画，可选择类型
            getAnimTween(AnimType, false, callback);
            //callback(true);
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

        private bool onLuaStatusChange(ViewStatus status)
        {
            if(null != mLuaFuncs)
            {
                LuaFunction func = getLuaFunc(status.ToString());
                if(null != func)
                {
                    if (ViewStatus.onInit == status)
                    {
                        func.Call<UIHandler, UIAnimResult>(UIObjs, OnInitCallbcak);
                    }
                    else
                    {
                        func.Call();
                    }
                }
                return true;
            }
            return false;
        }

        private void onStartAnim(ViewStatus status, UIAnimResult callback)
        {
            UIAnimResult _callback = (bool ret) =>
            {
                switch (status)
                {
                    case ViewStatus.onHideBegin:
                        onLuaStatusChange(ViewStatus.onDisable);
                        gameObject.SetActive(false);
                        break;
                    case ViewStatus.onShowBegin:
                        onLuaStatusChange(ViewStatus.onEnable);
                        //gameObject.SetActive(true);
                        break;
                }
                //test
                Debug.LogWarningFormat("status:{0}", status.ToString());
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
                    case ViewStatus.onHideBegin :
                        onHide(_callback);
                        break;
                    case ViewStatus.onShowBegin :
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

        #region protected
        protected void getAnimTween(ViewAnimType animType, bool revert, UIAnimResult result)
        {
            if (null != mAnimTween && mAnimTween.IsActive())
            {
                mAnimTween.Kill();
                mAnimTween = null;
            }
            switch(animType)
            {
                case ViewAnimType.none :
                    result(true);
                    break;
                case ViewAnimType.moveUp :
                    if (!revert)
                    {
                        float posY = mRectTransform.localPosition.y;
                        mAnimTween = mRectTransform.DOLocalMoveY(posY-AnimValue, AnimTime).SetEase(AnimEase).OnComplete(() => result(true));
                    }else
                    {
                        mAnimTween = mRectTransform.DOLocalMoveY(-AnimValue, AnimTime).From(true).SetEase(AnimEase).OnComplete(() => result(true));
                    }
                    break;
                case ViewAnimType.moveDown :
                    if (!revert)
                    {
                        float posY = mRectTransform.localPosition.y;
                        mAnimTween = mRectTransform.DOLocalMoveY(posY + AnimValue, AnimTime).SetEase(AnimEase).OnComplete(() => result(true));
                    }
                    else
                    {
                        mAnimTween = mRectTransform.DOLocalMoveY(AnimValue, AnimTime).From(true).SetEase(AnimEase).OnComplete(() => result(true));
                    }
                    break;
                case ViewAnimType.move2Left :
                    if (!revert)
                    {
                        float posX = mRectTransform.localPosition.x;
                        mAnimTween = mRectTransform.DOLocalMoveX(posX+AnimValue, AnimTime).OnComplete(() => result(true));
                    }
                    else
                    {
                        mAnimTween = mRectTransform.DOLocalMoveX(AnimValue, AnimTime).From(true).OnComplete(() => result(true));
                    }
                    break;
                case ViewAnimType.move2Right :
                    if (!revert)
                    {
                        float posX = mRectTransform.localPosition.x;
                        mAnimTween = mRectTransform.DOLocalMoveX(posX - AnimValue, AnimTime).OnComplete(() => result(true));
                    }
                    else
                    {
                        mAnimTween = mRectTransform.DOLocalMoveX(-AnimValue, AnimTime).From(true).OnComplete(() => result(true));
                    }
                    break;
                case ViewAnimType.zoom :
                    if (!revert)
                    {
                        mAnimTween = transform.DOScale(0f, AnimTime).OnComplete(() => result(true));
                    }
                    else
                    {
                        transform.localScale = Vector3.zero;
                        mAnimTween = transform.DOScale(AnimValue, AnimTime).OnComplete(() => result(true));
                    }
                    break;
            }
        }
        #endregion

        #region MonoBehaviour
        void Start()
        {
            mRectTransform = GetComponent<RectTransform>();
            if(null == mRectTransform)
            {
                LogFile.Error("mRectTransform is null");
                return;
            }
            IsBillboard = false;
            //进入初始化之后直接隐藏UI，修复在UI切换的时候会显示该UI，等待上个UI隐藏动画完成后再播放动画修复的bug
            gameObject.SetActive(false);
            init();
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
            onLuaStatusChange(ViewStatus.onDestroy);
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
        onEnable,
        onDisable,
        onDestroy,
        onShowBegin,
        //onShowEnd,
        onHideBegin,
        //onHideEnd,
    }

    public enum ViewAnimType
    {
        none = 0,
        moveUp,
        moveDown,
        move2Left,
        move2Right,
        zoom,
    }
}