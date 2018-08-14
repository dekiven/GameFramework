using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenTouchHandler : MonoBehaviour
{

    #region 代理
    public delegate bool DelegateSingleTouch(Vector2 screenPos);
    public delegate bool DelegateSingleMove(Vector2 moveDelta, Vector2 sceenPos);
    public delegate bool DelegateScale(float scale, Vector2 centerPos);
    public delegate bool DelegateStationary(int count, Vector2 screenPos);
    #endregion

    #region 公共属性
    /// <summary>
    /// 鼠标滚轮滚动大小的缩放比例,用以保证双指缩放和鼠标滚轮使用同一个回调的一致性
    /// </summary>
    public float WheelScrollRate = 0.05f;

    /// <summary>
    /// 认为鼠标或者手指移动的阈值(的平方，便于计算)，小于这个值的认为没有移动
    /// </summary>
    public float MoveThresholdSqr = 4f;

    /// <summary>
    /// 长按触发时间（秒）
    /// </summary>
    public float LongTouchStartSec = 1.5f;

    /// <summary>
    /// 双击最大间隔
    /// </summary>
    public float DoubleIntervalMax = 0.6f;

    /// <summary>
    /// 按住的回调间隔
    /// </summary>
    public float StationaryInterval = 0.3f;

    /// <summary>
    /// 是否在刚点击的时候调用按住的回调
    /// </summary>
    public bool CallStationaryOnStart = false;

    //单指或鼠标左键相关回调
    /// <summary>
    /// 一次点击(按下后抬起)的回调，同时又双击和单击回调时会造成单击回调延时（双击最大间隔）
    /// </summary>
    public DelegateSingleTouch OnClick;
    /// <summary>
    /// 双击的回调，同时又双击和单击回调时会造成单击回调延时（双击最大间隔）
    /// </summary>
    public DelegateSingleTouch OnDoubleClick;
    /// <summary>
    /// 长按一定时间后回调一次，注意：该回调和一直按住的回调（OnTouchStationary）是互斥的，会被该回调屏蔽
    /// </summary>
    public DelegateSingleTouch OnLongClickStart;
    /// <summary>
    /// 一直按住的回调，会在间隔一定时间回调一次，与长按回调（OnLongClickStart）互斥，会屏蔽长按回调
    /// </summary>
    public DelegateStationary OnTouchStationary;
    /// <summary>
    /// 按住移动的回调
    /// </summary>
    public DelegateSingleMove OnTouchMove;

    //双指或滚轮滚动相关回调
    /// <summary>
    /// 多指或滚轮缩放回调, 回调的Scale值代表相对于上次的缩放比例，使用的时候直接*=就可以了，示例如下：
    /// float oriScale = 1f; //需要使用的Scale
    /// 
    /// handler.OnTouchScale = delegate(float scale, Vector2 centerPos)
    /// {
    ///     oriScale *= scale; //将回调的scale乘以之前的scale
    /// }
    /// </summary>
    public DelegateScale OnTouchScale;

    ///// <summary>
    ///// 设置是否允许多点触控,会改变整个游戏的多点触控设置
    ///// </summary>
    ///// <value><c>true</c> if enable multi touch; otherwise, <c>false</c>.</value>
    //public bool EnableMultiTouch { 
    //    get 
    //    {
    //        return mEnableMultiTouch;
    //    } 
    //    set
    //    {
    //        mEnableMultiTouch = value;
    //        Input.multiTouchEnabled = value;
    //    }
    //}
    #endregion

    #region 私有属性
    /// <summary>
    /// 是否是单点触摸
    /// </summary>
    private bool mIsSingleTouch;
    /// <summary>
    /// 点击开始位置
    /// </summary>
    private Vector2[] mTouchsBeginPos = { Vector2.zero, Vector2.zero };

    /// <summary>
    /// The m touchs begin position.
    /// </summary>
    private Vector2[] mTouchsLastPos = {Vector2.zero, Vector2.zero};


    /// <summary>
    /// 点击开始时间
    /// </summary>
    private float mTouchStartTime = -1f;
    /// <summary>
    /// 上个点击时间
    /// </summary>
    private float mLastTouchTime  = -1f;
    /// <summary>
    /// 是否移动过
    /// </summary>
    private bool mTouchMoved = false;
    /// <summary>
    /// 连续点击次数
    /// </summary>
    private int mClickCount = 0;
    /// <summary>
    /// 上次单击的位置
    /// </summary>
    private Vector2 mLastTouchPos;
    /// <summary>
    /// 有长按回调(到时间回调一次)或者一直按住的回调后取消单击和双击 
    /// </summary>
    private bool mSingleCancled = false;

    /// <summary>
    /// 按住不放回调次数
    /// </summary>
    private int mStationaryCount = 0;
    /// <summary>
    /// 是否已经多点触摸且始终有手指在屏幕上
    /// </summary>
    private bool mHasMultiTouched = false;

    /// <summary>
    /// 上次按住不放的回调时间
    /// </summary>
    private float mLastStaionaryTime = -1;
    /// <summary>
    /// 上次鼠标所在位置
    /// </summary>
    private Vector2 mLastMousePos = Vector2.zero;
    ///// <summary>
    ///// 是否允许多点触控
    ///// </summary>
    //private bool mEnableMultiTouch = Input.multiTouchEnabled;

    #endregion

    // Use this for initialization
    void Start () {
        //TODO:初始化
	}
	
	// Update is called once per frame
	void Update () {
        
        bool checkTouch = true;
        if (Input.mousePresent)
        {
            //鼠标没有操作才检测touch
            checkTouch = !updateMouse();
        }

        if(checkTouch)
        {
            if (Input.touchCount == 1)
            {
                //单指触摸
                updateSingle();
            }
            else if (Input.touchCount > 1)
            {
                //多指触摸
                updateMultiple();
            }
            else
            {
                updateClick();
                mHasMultiTouched = false;
                mSingleCancled = false;
            }
        }

	}

    private void updateClick()
    {
        if(1 == mClickCount 
           && !mSingleCancled
           && Time.time - mLastTouchTime >= DoubleIntervalMax 
           && mLastTouchTime > 0
           && !Equals(mLastTouchPos, Vector2.zero)
           && null != OnClick)
        {
            OnClick(mLastTouchPos);
            mTouchStartTime = -1f;
            mLastTouchTime = -1;
            mClickCount = 0;
            mLastTouchPos = Vector2.zero;
        }
    }

    private void updateSingle()
    {
        //单指触摸 
        mIsSingleTouch = true;

        Touch touch = Input.GetTouch(0);
        TouchPhase phase = touch.phase;
        float nowTime = Time.time;
        Vector2 pos = touch.position;
        switch (phase)
        {
            case TouchPhase.Began:
                onTouchBegin(nowTime, pos);
                break;
            case TouchPhase.Canceled:
                onTouchCancle();
                break;
            case TouchPhase.Ended:
                onTouchEnd(nowTime, pos);
                break;
            case TouchPhase.Moved:
                Vector2 delta = touch.deltaPosition;
                onTouchMove(pos, delta);
                break;
            case TouchPhase.Stationary:
                onStaionary(nowTime, pos);
                break;
        }
    }


    private void updateMultiple()
    {
        //多点触控是用来回调缩放（OnTouchScale）的，没有就跳过
        if(null == OnTouchScale)
        {
            return;
        }
        if(mIsSingleTouch)
        {
            mTouchsBeginPos[0] = Vector2.zero;
            mTouchsBeginPos[1] =  Vector2.zero;
        }
        mIsSingleTouch = false;
        mHasMultiTouched = true;
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);
        Vector2[] posArr = { touch1.position, touch2.position };

        if(touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            bool skip = false;
            for (int i = 0; i < 2; ++i)
            {
                Touch touch = Input.GetTouch(i);
                Vector2 pos = touch.position;
                Vector2 begin = mTouchsBeginPos[i];
                if (Equals(begin, Vector2.zero))
                {
                    mTouchsBeginPos[i] = pos;
                    skip = true;
                }
                if(Equals(mTouchsLastPos[i], Vector2.zero))
                {
                    skip = true;
                }
            }
            if(!skip)
            {
                float disNow = Vector2.Distance(posArr[0], posArr[1]);
                float disLast = Vector2.Distance(mTouchsLastPos[0], mTouchsLastPos[1]);
                float disBegin = Vector2.Distance(mTouchsBeginPos[0], mTouchsBeginPos[1]);
                if(!Equals(disBegin, 0f) && !Equals(disLast, 0f) && null != OnTouchScale)
                {
                    Vector2 center = (posArr[0] + posArr[1]) / 2;
                    OnTouchScale((disNow / disBegin) / (disLast / disBegin), center);
                }
            }
            mTouchsLastPos = posArr;
        }

    }

    /// <summary>
    /// 鼠标左键按下事件或者滚轮事件返回true
    /// </summary>
    /// <returns><c>true</c>, if mouse was updated, <c>false</c> otherwise.</returns>
    private bool updateMouse()
    {
        bool useMouse = false;
        Vector2 pos = Input.mousePosition;
        float nowTime = Time.time;
        //在没有触摸屏幕的情况下检测鼠标
        float wheelScroll = Input.GetAxis("Mouse ScrollWheel");
        if (!Equals(0f, wheelScroll))
        {
            //鼠标滚轮滑动
            if(null != OnTouchScale)
            {
                OnTouchScale(1f + wheelScroll * WheelScrollRate, pos);
            }
            useMouse = true;
        }
        if (Input.GetMouseButtonDown(0))
        {
            //鼠标左键按下时
            onTouchBegin(nowTime, pos);
            mLastMousePos = pos;
            useMouse = true;
        }
        if (Input.GetMouseButton(0))
        {
            //鼠标左键按住时
            onStaionary(nowTime, pos);
            if(!Equals(mLastMousePos, Vector2.zero))
            {
                onTouchMove(pos, pos - mLastMousePos);
            }
            mLastMousePos = pos;
            useMouse = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            //鼠标左键抬起时
            onTouchEnd(nowTime, pos);
            mLastMousePos = Vector2.zero;
            useMouse = true;
        }
        return useMouse;
    }

    private void onTouchBegin(float nowTime, Vector2 pos)
    {
        mIsSingleTouch = true;
        //TODO:以后可以做双击长按等的区域限制
        mTouchsBeginPos[0] = pos;
        mTouchStartTime = nowTime;
        mTouchMoved = false;
        mLastStaionaryTime = CallStationaryOnStart ? 0f : nowTime;
        mStationaryCount = 0;
        mHasMultiTouched = false;
        mSingleCancled = false;
    }

    private void onTouchMove(Vector2 pos, Vector2 delta)
    {
        if (!mTouchMoved && delta.sqrMagnitude >= MoveThresholdSqr)
        {
            mTouchMoved = true;
        }
        else if(!mTouchMoved)
        {
            //如果一直以来移动的距离都很小当做固定按住某个位置处理
            onStaionary(Time.time, pos);
        }
        if (mTouchMoved && null != OnTouchMove)
        {
            OnTouchMove(delta, pos);
        }
    }


    private void onStaionary(float nowTime, Vector2 pos)
    {
        if (null != OnTouchStationary)
        {
            if (nowTime - mLastStaionaryTime >= StationaryInterval)
            {
                OnTouchStationary(++mStationaryCount, pos);
                mLastStaionaryTime = nowTime;
                mSingleCancled = true;
            }
        }

        if(null != OnLongClickStart && !mSingleCancled)
        {
            if(nowTime - mTouchStartTime >= LongTouchStartSec)
            {
                OnLongClickStart(pos);
                mSingleCancled = true;
            }
        }
    }

    private void onTouchEnd(float nowTime, Vector2 pos)
    {
        if (mIsSingleTouch && !mTouchMoved && !mHasMultiTouched && !mSingleCancled)
        {
            //有双击回调优先检测双击，再在update中回调超时的单击
            if (null != OnDoubleClick)
            {
                if (mClickCount > 0)
                {
                    if (nowTime - mLastTouchTime <= DoubleIntervalMax)
                    {
                        OnDoubleClick(pos);
                        mTouchStartTime = -1f;
                        mClickCount = 0;
                        mLastTouchPos = Vector2.zero;
                        return;
                    }
                }

                mLastTouchTime = nowTime;
                mClickCount = 1;
                mLastTouchPos = pos;
            }
            else if (null != OnClick)
            {
                OnClick(pos);
                mTouchStartTime = -1f;
                mLastTouchTime = -1;
                mClickCount = 0;
                mLastTouchPos = Vector2.zero;
                return;
            }
        }
    }

    private void onTouchCancle()
    {
        mIsSingleTouch = true;
        mTouchsBeginPos[0] = Vector2.zero;
        mTouchsBeginPos[1] = Vector2.zero;
        mTouchStartTime = -1f;
        mTouchMoved = false;
        mHasMultiTouched = false;
        mLastMousePos = Vector2.zero;
    }
}
