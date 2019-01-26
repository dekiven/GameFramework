using System;
using System.Collections;
using LuaInterface;
using UnityEngine;
using DG.Tweening;

//参考：http://blog.csdn.net/u011484013/article/details/52182997
namespace GameFramework
{
    public class UIBase : MonoBehaviour, ICanvasRaycastFilter
    {
        #region delegate
        public delegate void UIAnimResult(bool ret);
        #endregion

        [HideInInspector]
        public bool IsBillboard;
        [HideInInspector]
        public UIHandler Handler;
        /// <summary>
        /// 是否加入UI栈
        /// </summary>
        [HideInInspector]
        public bool IsInStack = true;
        /// <summary>
        /// 加入栈中的UI是否隐藏当前栈顶
        /// </summary>
        [HideInInspector]
        public bool HideBefor = true;
        /// <summary>
        /// 是否是静态的UI(全局多次使用，close不释放)
        /// </summary>
        [HideInInspector]
        public bool IsStatic = false;
        /// <summary>
        /// show和hide的动画时间
        /// </summary>
        [HideInInspector]
        public float AnimTime = 0.5f;
        /// <summary>
        /// show和hide的动画值，可能是缩放大小、某个轴的移动距离等
        /// </summary>
        [HideInInspector]
        public float AnimValue = 1f;
        [HideInInspector]
        public ViewAnimType AnimType = ViewAnimType.none;
        public UIAnimResult OnInitCallbcak;
        public UIAnimResult OnAnimCallbcak;
        /// <summary>
        /// 动画缓动效果，默认无效果
        /// </summary>
        [HideInInspector]
        public Ease AnimEase = Ease.Linear;
        [HideInInspector]
        public bool HasDarkMask = true;
        [HideInInspector]
        public RenderMode RenderMode; // { get { return mRenderMode; } set { mRenderMode = value; }}
        //public RectTransform rectTransform { get { return mRectTransform; }}

        //protected RenderMode RenderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
        //private Action<ViewStatus> mStatusChangeCall;

        private Tween mAnimTween = null;
        private LuaTable mLuaFuncs;
        private RectTransform mRectTransform;
        //private RenderMode mRenderMode ;
        private bool mIsPlayinngAni = false;

        public void SetLuaStatusListeners(LuaTable table)
        {
            mLuaFuncs = table;
        }


        /// <summary>
        /// 通过GameUImanager显示View
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void Show(UIAnimResult callback)
        {
            GameUIManager.Instance.ShowViewObj(this, callback);
        }

        /// <summary>
        /// 通过GameUImanager关闭View
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void Hide(UIAnimResult callback)
        {
            GameUIManager.Instance.HideView(this, callback);
        }

        /// <summary>
        /// 准备显示的处理，可以在这里做打开动画,同时有lua和c#代码时执行lua
        /// </summary>
        public void ShowAnim(UIAnimResult callback)
        {
            onStartAnim(ViewStatus.onShowBegin, callback);
        }

        /// <summary>
        /// 准备关闭的处理，可以在这里做关闭动画
        /// </summary>
        public void HideAnim(UIAnimResult callback)
        {
            onStartAnim(ViewStatus.onHideBegin, callback);
        }

        public void Close()
        {
            GameUIManager.Instance.CloseView(this);
        }

        /// <summary>
        /// 在Lua ViewBase(或其子类）的onInit方法中调
        /// </summary>
        /// <param name="rst">If set to <c>true</c> lua初始化成功.</param>
        public void OnLuaInitResult(bool rst)
        {
            if (null != OnInitCallbcak)
            {
                OnInitCallbcak(rst);
            }
        }

        /// <summary>
        /// 在Lua ViewBase(或其子类）需要重载C#的show或hide动画时，在动画完成后调用
        /// </summary>
        /// <param name="rst">If set to <c>true</c> 动画播放成功.</param>
        public void OnLuaAnimResult(bool rst)
        {
            if(null != OnAnimCallbcak)
            {
                OnAnimCallbcak(rst);
            }
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

        protected virtual void dispose()
        {
            if (null != mLuaFuncs)
            {
                mLuaFuncs.Dispose();
            }
        }

        /// <summary>
        /// 准备显示的处理，可以在这里做打开动画,同时有lua和c#代码时执行lua
        /// </summary>
        protected virtual void onShow(UIAnimResult callback)
        {
            gameObject.SetActive(true);
            runAnimTween(AnimType, true, callback);
            //callback(true);
        }


        /// <summary>
        /// 准备关闭的处理，可以在这里做关闭动画
        /// </summary>
        protected virtual void onHide(UIAnimResult callback)
        {
            runAnimTween(AnimType, false, callback);
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
                        func.Call<UIBase, UIHandler>(this, Handler);
                    }
                    else
                    {
                        func.Call();
                    }
                    return true;
                }
            }
            return false;
        }

        private void onStartAnim(ViewStatus status, UIAnimResult callback)
        {
            mIsPlayinngAni = true;
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

                mIsPlayinngAni = false;

                //Debug.LogWarningFormat("status:{0}", status.ToString());
                if (null != callback)
                {
                    callback(ret);
                }
            };
            if (null != mLuaFuncs)
            {
                LuaFunction func = getLuaFunc(status.ToString());
                if (null != func)
                {
                    OnAnimCallbcak = _callback;
                    func.Call();
                    return;
                }
            }

            //不在lua上修改动画直接使用C#定义的
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
        protected void runAnimTween(ViewAnimType animType, bool revert, UIAnimResult result)
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
            if(null == Handler)
            {
                Handler = GetComponent<UIHandler>();
            }
            IsBillboard = false;
#if UNITY_EDITOR 
            if (GameUIManager.HasInstance())
            {
#endif
                //进入初始化之后直接隐藏UI，修复在UI切换的时候会显示该UI，等待上个UI隐藏动画完成后再播放动画修复的bug
                gameObject.SetActive(false);
#if UNITY_EDITOR                
            }
#endif
            
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
            dispose();
        }

        /// <summary>
        /// 实现ICanvasRaycastFilter接口，播放动画时屏蔽UI及子节点点击事件
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <param name="eventCamera"></param>
        /// <returns></returns>
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            return !mIsPlayinngAni;
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