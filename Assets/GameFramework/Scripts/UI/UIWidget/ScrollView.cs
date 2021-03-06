﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using LuaInterface;
using DG.Tweening;
using UnityEngine.EventSystems;


//参考资料：「Unity3D」(10)自定义属性面板Inspector详解
//URL:https://www.jianshu.com/p/497fcbad2ad0 

namespace GameFramework
{
    using DelScrollItemClicked = Action<int>;
    using DelBtnClickedStr = Action<int, string>;
    using DelBtnClickedIndex = Action<int, int>;
    using DelSelectChange = Action<int[]>;

    [RequireComponent(typeof(ScrollRect))]
    public class ScrollView : ScrollRect
    {
        [HideInInspector]
        public ScrollViewType ScrollType;

        [HideInInspector]
        public Vector2 ItemSize = Vector2.one;

        [HideInInspector]
        public GameObject ItemPrefab;

        [HideInInspector]
        public float PaddingLeft;
        [HideInInspector]
        public float PaddingRight;
        [HideInInspector]
        public float PaddingTop;
        [HideInInspector]
        public float PaddingBottom;
        [HideInInspector]
        public int ItemNumPerStep = 1;
        [HideInInspector]
        public bool ShowItemIntegers = true;
        [HideInInspector]
        public float LineOffset = 2f;

        //TODO:目前仅支持拖动 Item 后对齐,鼠标滚轮和 scrollbar 拖动后不能对齐
        [HideInInspector]
        public bool AlignLines = false;

        [HideInInspector]
        public bool EnableMuiltSelect = false;
        [HideInInspector]
        public bool SwitchItemOnClick;
        [HideInInspector]
        public float TweenRate = 0.001f;
        [HideInInspector]
        public float TweenMaxTime = 0.8f;

        #region 私有属性
        private ObjPool<ScrollItem> mItemPool;
        private List<ScrollItem> mCurItems;
        private List<UIItemData> mItemDatas;
        private int mNumPerLine;
        private int mLinePerPage;
        private int mTotalLines;
        private int mShowStart = -1;
        private int mShowEnd = -1;
        private Coroutine mUpCoroutine = null;
        /// <summary>
        /// Item 之间的间隔
        /// </summary>
        private Vector2 mContentPadding = new Vector2();
        private int mTargetIndex = -1;
        private float mTargetPos = -1f;
        private Tween mMoveTween = null;
        private Vector2 mRealItemSize = Vector2.one;


        //Item 点击回调
        private DelScrollItemClicked mOnItemClicked;
        private LuaFunction mOnItemClickLua;

        // Item 上按钮点击回调（BG除外）
        private DelBtnClickedStr mOnBtnClickedS;
        private DelBtnClickedIndex mOnBtnClickedI;
        private LuaFunction mOnBtnClickLua;

        /// <summary>
        /// ScrollItem 上的btn被点击时是否传递btn名，否则传UIHandler的index
        /// </summary>
        private bool mBtnClickPassStr = false;

        private List<bool> mItemSelectStatus;
        private DelSelectChange mOnSelectChange;
        private LuaFunction mOnSelectChangeLua;
        /// <summary>
        /// 当Item多选关闭的情况下，记录当前选中的 Item
        /// </summary>
        private int mCurSelectIndex = -1;
        private Vector3 mLastLocalPos;
        /// <summary>
        /// 是否改变了 content size，如果是，下一次 onValueChanged 刷新
        /// </summary>
        private bool mIsSizeChanged = false;

        #endregion 私有属性

        /// <summary>
        /// SetData 会将所有 item 选中状态置为未选中
        /// </summary>
        /// <param name="data">Data.</param>
        public void SetData(List<UIItemData> data)
        {
            mItemDatas = data;
            mItemSelectStatus.Clear();
            mItemSelectStatus.AddRange(new bool[data.Count]);
            _calculateAndUpdateContent();
        }

        /// <summary>
        /// SetData 会将所有 item 选中状态置为未选中
        /// </summary>
        /// <param name="table">Table.</param>
        public void SetData(LuaTable table)
        {
            List<UIItemData> data = Tools.GenUIIemDataList(table);
            SetData(data);
        }

        public void UpdateData(int index, UIItemData data)
        {
            if (index >= 0 && index < mItemDatas.Count)
            {
                mItemDatas[index] = data;
            }
            //TODO:更新某数据只需要刷新该数据即可
            _checkNeedUpdate(true);
        }

        /// <summary>
        /// 更新某位置的数据,为了方便UIhandler使用LuaTable包含index和数据
        /// </summary>
        /// <param name="table">包含index和UIItemData的teble，/n格式：{index=0, data={{"xxx",0,"xxx"},...},...,count=x}</param>
        public void UpdateData(LuaTable table)
        {
            int index = table.RawGet<string, int>("index");
            UIItemData data = new UIItemData(table.RawGet<string, LuaTable>("data"));
            UpdateData(index, data);
        }

        public void AddData(UIItemData data)
        {
            mItemDatas.Add(data);
            mItemSelectStatus.Add(false);
            _calculateAndUpdateContent();
        }

        public void AddData(LuaTable table)
        {
            UIItemData data = new UIItemData(table);
            AddData(data);
        }

        public void InsertData(UIItemData data, int index)
        {
            mItemDatas.Insert(index, data);
            mItemSelectStatus.Insert(index, false);
            _calculateAndUpdateContent();
        }

        /// <summary>
        /// 在某位置插入数据,为了方便UIhandler使用LuaTable包含index和数据
        /// </summary>
        /// <param name="table">包含index和UIItemData的teble，/n格式：{index=0, data={{"xxx",0,"xxx"},...},...,count=x}</param>
        public void InsertData(LuaTable table)
        {
            int index = table.RawGet<string, int>("index");
            UIItemData data = new UIItemData(table.RawGet<string, LuaTable>("data"));
            InsertData(data, index);
        }

        public void RemoveData(UIItemData data)
        {
            if (mItemDatas.Contains(data))
            {
                data.Dispose();
                int index = mItemDatas.IndexOf(data);
                mItemDatas.RemoveAt(index);
                mItemSelectStatus.RemoveAt(index);

                _calculateAndUpdateContent();
            }
        }

        public void RemoveDataAt(int index)
        {
            if (mItemDatas.Count > index && index >= 0)
            {
                mItemDatas[index].Dispose();
                mItemDatas.RemoveAt(index);
                mItemSelectStatus.RemoveAt(index);

                _calculateAndUpdateContent();
            }
        }

        public void AddDatas(List<UIItemData> datas)
        {
            mItemDatas.AddRange(datas);
            mItemSelectStatus.AddRange(new bool[datas.Count]);
            _calculateAndUpdateContent();
        }

        public void AddDatas(LuaTable lua)
        {
            AddDatas(Tools.GenUIIemDataList(lua));
        }

        public void RemoveDatasAt(int[] idxs)
        {
            Array.Reverse(idxs);
            for (int i = 0; i < idxs.Length; i++)
            {
                int idx = idxs[i];
                mItemDatas.RemoveAt(idx);
                mItemSelectStatus.RemoveAt(idx);
            }
            _calculateAndUpdateContent();
        }

        public void RemoveDatasAt(string intArr)
        {
            RemoveDatasAt(Tools.GetIntArry(intArr));
        }


        public void Tween2Index(int index)
        {
            if(null != mUpCoroutine)
            {
                mTargetIndex = index;
            }
            else
            {
                _tweenToIndex(index);
            }
        }

        /// <summary>
        /// Tween 到 固定位置0f~1f，0表示 显示列表头，1表示显示列表末尾
        /// </summary>
        /// <param name="pos">Position 取值:0f~1f</param>
        public void Tween2Pos(float pos)
        {
            if (null != mUpCoroutine)
            {
                mTargetPos = pos;
            }
            else
            {
                _tweenToPos(pos);
            }
        }

        public void CalculateContentSize()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }
            if (null == mItemDatas || null == ItemPrefab)
            {
                LogFile.Warn("ScrollView calculateContentSize Error: null == mItemDatas || null == ItemPrefab");
                return;
            }
                
            Rect rect = content.rect;
            Vector2 rectSize = rect.size;
            Vector2 viewSize = viewport.rect.size;
            Vector2 useSize = Vector2.zero;
            mRealItemSize = ItemSize;

            int dataCount = mItemDatas.Count;
            if (ScrollViewType.Vertical == ScrollType)
            {
                useSize.x = rectSize.x - PaddingLeft - PaddingRight;
                useSize.y = viewSize.y - PaddingTop - PaddingBottom;
                if (ItemSize.x.Equals(-1f))
                {
                    mRealItemSize.x = useSize.x;
                }
                if (ItemSize.y.Equals(-1f))
                {
                    mRealItemSize.y = useSize.y;
                }
                mNumPerLine = Mathf.FloorToInt(useSize.x / mRealItemSize.x);
                mLinePerPage = Mathf.FloorToInt(useSize.y / mRealItemSize.y);
                if (mNumPerLine < 1 || mLinePerPage < 1)
                {
                    LogFile.Error("ScrollView calculateContentSize Error:  mNumPerLine < 1 || mLinePerPage < 1, mRealItemSize:{0}, useSize:{1}", mRealItemSize, useSize);
#if UNITY_EDITOR
                    if(UnityEditor.EditorUtility.DisplayDialog("提示", "ScrollItem宽高超出可显示区域，请修改 ScrollItem 宽高或者 ScrollView宽高、 padding", "退出运行并修改"))
                    {
                        UnityEditor.EditorApplication.isPlaying = false;
                    }
#endif
                }
                float space = useSize.x - mRealItemSize.x * mNumPerLine;
                if (mNumPerLine > 1)
                {
                    mContentPadding.x = space / (mNumPerLine - 1);
                }
                else if (mNumPerLine == 1)
                {
                    mContentPadding.x = 0f;
                    PaddingLeft += space / 2;
                    PaddingRight += space / 2;
                }
                space = useSize.y - mLinePerPage * mRealItemSize.y;
                if(ShowItemIntegers)
                {
                    if (mLinePerPage > 1)
                    {
                        mContentPadding.y = space / (mLinePerPage - 1);
                    }
                    else if (mLinePerPage == 1)
                    {
                        mContentPadding.y = mContentPadding.x;
                    }
                }
                else
                {
                    mContentPadding.y = LineOffset;
                    ////TODO:mLinePerPage 根据 lineOffset 来算
                    //if (useSize.y - ItemSize.y * mLinePerPage - LineOffset * (mLinePerPage - 1) - PaddingBottom > 0.5)
                    //{
                    //    mLinePerPage += 1;
                    //}
                }

                mTotalLines = Mathf.CeilToInt(dataCount / (float)mNumPerLine);
                float height = PaddingTop + PaddingBottom + mTotalLines * mRealItemSize.y + (mTotalLines - 1) * mContentPadding.y;
                //content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, viewSize.y > height ? viewSize.y : height);
                //LogFile.Log("PaddingBottom:{0}, mTotalLines:{1},hieght:{2}, space:{3}", PaddingBottom, mTotalLines, rect.height, space);
            }
            else
            {
                useSize.x = viewSize.x - PaddingLeft - PaddingRight;
                useSize.y = rectSize.y - PaddingTop - PaddingBottom;
                if (ItemSize.x.Equals(-1f))
                {
                    mRealItemSize.x = useSize.x;
                }
                if (ItemSize.y.Equals(-1f))
                {
                    mRealItemSize.y = useSize.y;
                }
                mNumPerLine = Mathf.FloorToInt(useSize.y / mRealItemSize.y);
                mLinePerPage = Mathf.FloorToInt(useSize.x / mRealItemSize.x);

                float space = useSize.y - mRealItemSize.y * mNumPerLine;
                if (mNumPerLine > 1)
                {
                    mContentPadding.y = space / (mNumPerLine - 1);
                }
                else if (mNumPerLine == 1)
                {
                    mContentPadding.y = 0f;
                    PaddingTop += space / 2;
                    PaddingBottom += space / 2;
                }
                space = useSize.x - mLinePerPage * mRealItemSize.x;
                if(ShowItemIntegers)
                {
                    if (mLinePerPage > 1)
                    {
                        mContentPadding.x = space / (mLinePerPage - 1);
                    }
                    else if (mLinePerPage == 1)
                    {
                        mContentPadding.x = mContentPadding.y;
                    }
                }
                else
                {
                    mContentPadding.x = LineOffset;
                    ////TODO:mLinePerPage 根据 lineOffset 来算
                    //if(useSize.x - ItemSize.x * mLinePerPage - LineOffset*(mLinePerPage - 1) - PaddingRight > 0.5)
                    //{
                    //    mLinePerPage += 1;
                    //}
                }


                mTotalLines = Mathf.CeilToInt(dataCount / (float)mNumPerLine);
                float width = PaddingLeft + PaddingRight + mTotalLines * mRealItemSize.x + (mTotalLines - 1) * mContentPadding.x;
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewSize.x > width ? viewSize.x : width);
                //content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            mIsSizeChanged = true;
        }

        public void SetOnItemClickDelegate(DelScrollItemClicked del)
        {
            mOnItemClicked = del;
        }

        public void SetOnItemClickLua(LuaFunction call)
        {
            if (null != mOnItemClickLua)
            {
                mOnItemClickLua.Dispose();
                mOnItemClickLua = null;
            }
            mOnItemClickLua = call;
        }

        /// <summary>
        /// 设置Scrolview Item上按钮回调按钮名
        /// </summary>
        /// <param name="del"></param>
        public void SetOnBtnClick_S(DelBtnClickedStr del)
        {
            mBtnClickPassStr = true;
            mOnBtnClickedS = del;
        }

        /// <summary>
        /// 设置Scrolview Item上按钮回调按钮 Index
        /// </summary>
        /// <param name="del"></param>
        public void SetOnBtnClick_I(DelBtnClickedIndex del)
        {
            mBtnClickPassStr = false;
            mOnBtnClickedI = del;
        }

        /// <summary>
        /// 设置Scrolview Item上按钮回调按钮名
        /// </summary>
        /// <param name="call"></param>
        public void SetOnBtnClickLua_S(LuaFunction call)
        {
            _setOnBtnClickLua(call, true);
        }

        /// <summary>
        /// 设置Scrolview Item上按钮回调按钮 Index
        /// </summary>
        /// <param name="call"></param>
        public void SetOnBtnClickLua_I(LuaFunction call)
        {
            _setOnBtnClickLua(call, false);
        }


        public void SelectItem(int index)
        {
            if (!EnableMuiltSelect)
            {
                if(mCurSelectIndex >= 0 && mCurSelectIndex < mItemDatas.Count)
                {
                    _setItemSelected(mCurSelectIndex, false);
                    mItemSelectStatus[mCurSelectIndex] = false;
                }
                mCurSelectIndex = index;
            }
            _setItemSelected(index, true);
            _onSelectStatusChanges();
        }

        public void SelectItems(int[] indexArr)
        {
            if(!EnableMuiltSelect)
            {
                LogFile.Warn("当前ScrollView为单选模式，请打开开关EnableMuiltSelect");
                return;
            }
            for (int i = 0; i < indexArr.Length; i++)
            {
                _setItemSelected(indexArr[i], true);
            }
            _onSelectStatusChanges();
        }

        public void SelectItems(string indexArr)
        {
            SelectItems(Tools.GetIntArry(indexArr));
        }

        public void UnselectItem(int index)
        {
            if (!EnableMuiltSelect && index == mCurSelectIndex)
            {
                //mItemSelectStatus[murSelectIndex] = false;
                mCurSelectIndex = -1;
            }
            _setItemSelected(index, false);
            //mItemSelectStatus[index] = false;
            _onSelectStatusChanges();
        }

        public void UnselectItems(int[] indexArr)
        {
            if (!EnableMuiltSelect)
            {
                LogFile.Warn("当前ScrollView为单选模式，请打开开关EnableMuiltSelect");
                return;
            }
            for (int i = 0; i < indexArr.Length; i++)
            {
                _setItemSelected(indexArr[i], false);
            }
            _onSelectStatusChanges();
        }

        public void UnselectItems(string indexArr)
        {
            UnselectItems(Tools.GetIntArry(indexArr));
        }

        public void SwitchItem(int index)
        {
            if (!EnableMuiltSelect)
            {
                if (mCurSelectIndex >= 0 && mCurSelectIndex < mItemDatas.Count)
                {
                    _setItemSelected(mCurSelectIndex, false);
                } 
                if(index == mCurSelectIndex)
                {
                    mItemSelectStatus[mCurSelectIndex] = false;
                    mCurSelectIndex = -1;
                    return;
                }
                mCurSelectIndex = index;
            }
            _switchItem(index);
            _onSelectStatusChanges();
        }

        public void SwitchItems(int[] indexArr)
        {
            if (!EnableMuiltSelect)
            {
                LogFile.Warn("当前ScrollView为单选模式，请打开开关EnableMuiltSelect");
                return;
            }
            for (int i = 0; i < indexArr.Length; i++)
            {
                _switchItem(indexArr[i]);
            }
            _onSelectStatusChanges();
        }

        public void SwitchItems(string indexArr)
        {
            SwitchItems(Tools.GetIntArry(indexArr));
        }

        public void SelectAll()
        {
            if (!EnableMuiltSelect)
            {
                LogFile.Warn("当前ScrollView为单选模式，请打开开关EnableMuiltSelect");
                return;
            }
            //已经通知修改，不需要像上面的再写一次
            changeSelectAll(true);
        }

        public void UnselectAll()
        {
            if (!EnableMuiltSelect)
            {
                LogFile.Warn("当前ScrollView为单选模式，请打开开关EnableMuiltSelect");
                return;
            }
            //已经通知修改，不需要像上面的再写一次
            changeSelectAll(false);
        }

        public void SetOnSelectChangeCall(DelSelectChange del)
        {
            mOnSelectChange = del;
        }

        public void SetOnSelectChangeCall(LuaFunction lua)
        {
            if(null != mOnSelectChangeLua)
            {
                mOnSelectChangeLua.Dispose();
                mOnSelectChangeLua = null;
            }
            mOnSelectChangeLua = lua;
        }

        #region MonoBehaviour
        protected override void Awake()
        {
            base.Awake();
            mItemPool = new ObjPool<ScrollItem>(onPoolGetDelegate, onPoolRecoverDelegate, onPoolDisposeDelegate);
            mCurItems = new List<ScrollItem>();
            mItemSelectStatus = ObjPools.GetListBool();
            //// 开启回弹
            //movementType = MovementType.Elastic;
            //elasticity = 0.05f;
            //// 关闭移动结束后回弹惯性
            //inertia = false;
        }

        protected override void Start()
        {
            base.Start();
            if (null == ItemPrefab)
            {
                LogFile.Error("ScrollView error：ItemPrefab 为空。");
                return;
            }

            bool isVertical = ScrollViewType.Vertical == ScrollType;
            vertical = isVertical;
            horizontal = !isVertical;

            this.onValueChanged.AddListener(onSrollViewValueChanged);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (null != mItemDatas && null != ItemPrefab)
            {                
                _calculateAndUpdateContent();
            }
        }

        protected override void OnDestroy()
        {
            _recoverAll();
            if (null != mCurItems)
            {
                mCurItems.Clear();
            }
            if (null != mItemPool)
            {
                mItemPool.Dispose();
            }
            if (null != mItemDatas)
            {
                for (int i = 0; i < mItemDatas.Count; i++)
                {
                    mItemDatas[i].Dispose();
                }
                mItemDatas.Clear();
            }
            if (null != mOnItemClickLua)
            {
                mOnItemClickLua.Dispose();
                mOnItemClickLua = null;
            }
            if(null != mItemSelectStatus)
            {
                ObjPools.Recover(mItemSelectStatus);
            }
            if(null != mOnSelectChangeLua)
            {
                mOnSelectChangeLua.Dispose();
                mOnSelectChangeLua = null;
            }
            base.OnDestroy();
        }
        #endregion MonoBehaviour

        #region 私有方法
        private ScrollItem _getItem()
        {
            return mItemPool.Get();
        }

        private void _recoverItem(ScrollItem item)
        {
            mItemPool.Recover(item);
        }

        private Vector3 _getItemPosByIndex(int i)
        {
            int row = 0;
            int colume = 0;
            if (ScrollViewType.Vertical == ScrollType)
            {
                colume = i % mNumPerLine;
                row = i / mNumPerLine;
                return new Vector3(PaddingLeft + colume * mRealItemSize.x + mRealItemSize.x / 2 + mContentPadding.x * colume - viewport.rect.width / 2, -(PaddingTop + row * mRealItemSize.y + mRealItemSize.y / 2 + mContentPadding.y * row));
            }
            else
            {
                row = i % mNumPerLine;
                colume = i / mNumPerLine;
                return new Vector3(PaddingLeft + colume * mRealItemSize.x + mRealItemSize.x / 2 + mContentPadding.x * colume, viewport.rect.height / 2 - (PaddingTop + row * mRealItemSize.y + mRealItemSize.y / 2 + mContentPadding.y * row));
            }
        }

        private bool _canItemShow(int index)
        {
            return _canItemShow(_getItemPosByIndex(index));
        }

        private bool _canItemShow(Vector2 pos)
        {
            if (ScrollViewType.Vertical == ScrollType)
            {
                float posY = -pos.y - content.localPosition.y;
                return posY > -mRealItemSize.y / 2 && posY < viewport.rect.height + mRealItemSize.y / 2;
            }
            else
            {
                float posX = pos.x + content.localPosition.x;
                return posX > -mRealItemSize.x / 2 && posX < viewport.rect.width + mRealItemSize.x / 2;
            }
        }

        private bool canLineShow(int line)
        {
            return _canItemShow(line * mNumPerLine);
        }

        private void _checkNeedUpdate(bool forceUpdate = false)
        {
            if(!gameObject.activeSelf)
            {
                return;
            }
            if (null == mItemDatas || mItemDatas.Count == 0)
            {
                _recoverAll ();
                return;
            }
            bool start = false;
            int startLine = -1;
            int endLine = -1;
            for (int i = 0; i < mTotalLines; i++)
            {
                bool canShow = canLineShow(i);
                if (!start)
                {
                    if (canShow)
                    {
                        startLine = i;
                        start = true;
                    }
                }
                if (canShow && i > endLine)
                {
                    endLine = i;
                }
            }
            if (-1 == startLine || -1 == endLine)
            {
                return;
            }
            //当显示行数有变化或者需要强制刷新时刷新所有UI
            if (forceUpdate || startLine != mShowStart || endLine != mShowEnd)
            {
                if (null != mUpCoroutine)
                {
                    //if(!forceUpdate && )
                    StopCoroutine(mUpCoroutine);
                    mUpCoroutine = null;
                }
                mUpCoroutine = StartCoroutine(_updateAllItem(startLine, endLine, forceUpdate));
            }

        }

        private IEnumerator _updateAllItem(int startLine, int endLine, bool forceUpdate = false)
        {
            //修复异步加载图片可能失败的问题
            yield return null;
            if (mShowStart != startLine || mShowEnd != endLine || forceUpdate)
            {
                //只有首尾的行变动，只处理相应的行,只在一帧处理完
                if (!forceUpdate && Math.Abs(mShowStart - startLine) <= 1 && Math.Abs(endLine - mShowEnd) <= 1)
                {
                    //减少Item
                    //如果开始的行数比当前的大1，证明现在在开头少显示了一行
                    if (startLine - mShowStart == 1)
                    {
                        _recoverStartLine(mShowStart);
                    }
                    //如果结束的行数比当前的小1，证明现在在末尾少显示了一行
                    if (mShowEnd - endLine == 1)
                    {
                        _recoverEndLine(mShowEnd);
                    }

                    ///添加新的Item
                    //如果开始的行数比当前的小1，证明现在在开头多显示了一行
                    if (mShowStart - startLine == 1)
                    {
                        _addStartLine(startLine);
                    }
                    //如果结束的行数比当前的大1，证明现在在末尾多显示了一行
                    if (endLine - mShowEnd == 1)
                    {
                        _addEndLine(endLine);
                    }

                    mShowStart = startLine;
                    mShowEnd = endLine;
                }
                else
                {
                    //有多行变动，在协程中处理所有刷新，有新变动停止协程重新处理
                    //LogFile.Log("updateAllItem 滑动多行了，直接全部刷新 start:{0}, end{1}", startLine, endLine);
                    //滑动多行了，直接全部刷新
                    int startIndex = Mathf.Clamp(startLine * mNumPerLine, 0, mItemDatas.Count - 1);
                    int endIndex = Mathf.Clamp((endLine + 1) * mNumPerLine - 1, 0, mItemDatas.Count - 1);
                    int count = Mathf.Clamp(endIndex - startIndex + 1, 0, mItemDatas.Count);

                    //回收所有Item，在之后的协程中刷新
                    _recoverAll();

                    for (int i = 0; i < count; i++)
                    {
                        ScrollItem item = _getItem();
                        mCurItems.Add(item);
                        _setItemDataByIndex(item, startIndex + i);
                        if ((count + 1) % ItemNumPerStep == 0)
                        {
                            yield return null;
                        }
                    }
                    if(mTargetIndex != -1)
                    {
                        _tweenToIndex(mTargetIndex);
                        mTargetIndex = -1;
                        mTargetPos = -1f;
                    }
                    else if( !mTargetPos.Equals(-1f) )
                    {
                        _tweenToPos(mTargetPos);
                        mTargetPos = -1f;
                    }
                }
                mShowStart = startLine;
                mShowEnd = endLine;
            }
            if (null != mUpCoroutine)
            {
                StopCoroutine(mUpCoroutine);
                mUpCoroutine = null;
            }
        }

        private void _addStartLine(int line)
        {
            for (int i = 0; i < mNumPerLine; i++)
            {
                int index = line * mNumPerLine + i;
                if (index < mItemDatas.Count)
                {
                    ScrollItem item = _getItem();
                    mCurItems.Insert(0, item);
                    _setItemDataByIndex(item, index);
                }
            }
        }

        private void _setItemDataByIndex(ScrollItem item, int index)
        {
            UIItemData data = mItemDatas[index];
            item.Index = index;
            item.SetData(data);
            item.IsSelected = mItemSelectStatus[index];
            RectTransform rect = item.rectTransform;
            Vector3 offset = Vector3.zero;
            if (!rect.pivot.Equals(Vector2.one * 0.5f))
            {
                offset.x = rect.rect.width * (rect.pivot.x - 0.5f);
                offset.y = rect.rect.height * (rect.pivot.y - 0.5f);
            }
            item.transform.localPosition = _getItemPosByIndex(index) + offset;
        }

        private void _addEndLine(int line)
        {
            for (int i = 0; i < mNumPerLine; i++)
            {
                int index = line * mNumPerLine + i;
                if (index < mItemDatas.Count)
                {
                    ScrollItem item = _getItem();
                    mCurItems.Add(item);
                    _setItemDataByIndex(item, index);
                }
            }
        }

        private void _recoverStartLine(int line)
        {
            if (line < 0 || line > mTotalLines - 1)
            {
                return;
            }
            for (int i = 0; i < mNumPerLine; i++)
            {
                int index = line * mNumPerLine + i;
                if (index < mItemDatas.Count)
                {
                    ScrollItem item = mCurItems[0];
                    _recoverItem(item);
                    mCurItems.RemoveAt(0);
                }
            }
        }

        private void _recoverEndLine(int line)
        {
            if (line < 0 || line > mTotalLines - 1)
            {
                return;
            }
            int count = mCurItems.Count;
            for (int i = 0; i < mNumPerLine; i++)
            {
                int index = line * mNumPerLine + i;
                if (index < mItemDatas.Count)
                {
                    int idx = count - 1 - i;
                    ScrollItem item = mCurItems[idx];
                    _recoverItem(item);
                    mCurItems.RemoveAt(idx);
                }
            }
        }

        private void _recoverAll()
        {
            if(null == mCurItems)
            {
                return;
            }
            //LogFile.Log("recoverAll 1 mCurItems.Count:{0}, objPool.count:{1}", mCurItems.Count, mItemPool.Count);
            for (int i = mCurItems.Count - 1; i > -1; --i)
            {
                ScrollItem item = mCurItems[i];
                _recoverItem(item);
                mCurItems.RemoveAt(i);
            }

            mShowStart = -1;
            mShowEnd = -1;
            //LogFile.Warn("recoverAll 2 mCurItems.Count:{0}, objPool.count:{1}", mCurItems.Count, mItemPool.Count);
        }

        private void _calculateAndUpdateContent()
        {
            CalculateContentSize();
            _checkNeedUpdate(true);
        }

        private void _onItemClicked(int index)
        {
            if (null != mOnItemClicked)
            {
                mOnItemClicked(index);
            }

            if (null != mOnItemClickLua)
            {
                mOnItemClickLua.Call(index);
            }
            if(SwitchItemOnClick)
            {                
                SwitchItem(index);
            }
        }

        private void _onItemBtnClickI(int index, int btnIndex)
        {
            //Debug.LogWarningFormat("onItemBtnClickI({0}, {1})", index, btnIndex);
            if(!mBtnClickPassStr)
            {
                if (null != mOnBtnClickedI)
                {
                    mOnBtnClickedI(index, btnIndex);
                }
                if (null != mOnBtnClickLua)
                {
                    mOnBtnClickLua.Call(index, btnIndex);
                }
            }
        }

        private void _onItemBtnClickS(int index, string btnName)
        {
            //Debug.LogWarningFormat("onItemBtnClickS({0}, {1})", index, btnName);
            if (mBtnClickPassStr)
            {
                if (null != mOnBtnClickedI)
                {
                    mOnBtnClickedS(index, btnName);
                }
                if (null != mOnBtnClickLua)
                {
                    mOnBtnClickLua.Call(index, btnName);
                }
            }
        }

        private void _tweenToIndex(int index)
        {
            if(null != mMoveTween)
            {
                mMoveTween.Kill();
                mMoveTween = null;
            }

            Vector3 localPos = _getConetntPosByIdx(index);
            localPos.z = content.localPosition.z;
            float dt = _getTweenTimeByDis(Vector3.Distance(content.localPosition, localPos));
            mMoveTween = DOTween.To(() => content.localPosition, (Vector2 v) => content.localPosition = v, localPos, dt).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                mMoveTween = null;
            });
        }

        private void _tweenToPos(float pos)
        {
            if (null != mMoveTween)
            {
                mMoveTween.Kill();
                mMoveTween = null;
            }
            pos = Mathf.Clamp01(pos);
            Vector2 tarPos = Vector2.zero;
            float dis = 0f;
            if (ScrollViewType.Vertical == ScrollType)
            {
                //normalizedPosition 以左下角为原点，详见normalizedPosition说明
                pos = 1 - pos;
                dis = Math.Abs(normalizedPosition.y - pos) * content.rect.size.y;
                tarPos.y = pos;
            }
            else
            {
                dis = Math.Abs(normalizedPosition.x - pos) * content.rect.size.x;
                tarPos.x = pos;
            }
            float dt = _getTweenTimeByDis(dis);
            mMoveTween = DOTween.To(() => normalizedPosition, (Vector2 v) => normalizedPosition = v, tarPos, dt).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                mMoveTween = null;
            });
        }

        private float _getTweenTimeByDis(float dis)
        {
            if (dis <= 0 )
            {
                return 0f;
            }
            return Math.Min(TweenMaxTime, dis * TweenRate);
        }

        private void _setOnBtnClickLua(LuaFunction call, bool passStr)
        {
            mBtnClickPassStr = passStr;
            if (null != mOnBtnClickLua)
            {
                mOnBtnClickLua.Dispose();
                mOnBtnClickLua = null;
            }
            mOnBtnClickLua = call;
        }

        private Vector2 _getNormalizedPosByIndex(int idx)
        {
            Vector2 pos = Vector2.zero;
            int max = Math.Max(0, mTotalLines - mLinePerPage);
            if (0 < max)
            {
                float value = idx / mNumPerLine;
                value = Mathf.Clamp(value, 0, max);
                if (ScrollType == ScrollViewType.Vertical)
                {
                    value = (PaddingTop + value * (mRealItemSize.y + mContentPadding.y)) / (PaddingTop + max * (mRealItemSize.y + mContentPadding.y));
                    pos.y = 1 - value;
                }
                else
                {
                    value = (PaddingLeft + value * (mRealItemSize.x + mContentPadding.x)) / (PaddingTop + max * (mRealItemSize.x + mContentPadding.x));
                    pos.x = value;
                }
            }
            else
            {
                pos = new Vector2(0, 1);
            }
            return pos;
        }

        /// <summary>
        /// 根据传入的 line index 确定 content 首行/列的 line index
        /// </summary>
        /// <returns>The valid line.</returns>
        /// <param name="idx">Index.</param>
        private int _getFirstLineIdx(int idx)
        {
            //int ret = 0;
            //int min = 0;
            idx = idx / mNumPerLine;
            int max = Math.Max(0, mTotalLines - mLinePerPage);
            //ret = Mathf.Clamp(idx, 0, max);
            return Mathf.Clamp(idx, 0, max);
        }

        private Vector2 _getConetntPosByIdx(int idx)
        {
            Vector2 ret = Vector2.zero;
            int line = -1;
            line = _getFirstLineIdx(idx);
            if (ScrollViewType.Vertical == ScrollType)
            {
                ret = new Vector2(content.transform.localPosition.x, (0 == line ? 0 : PaddingTop) + (mRealItemSize.y + mContentPadding.y) * line);
            }
            else
            {
                ret = new Vector2((0 == line ? 0 : -PaddingLeft) - (mRealItemSize.x + mContentPadding.x) * line, content.transform.localPosition.y);
            }
            return ret;
        }

        private void _alignNearestLine()
        {
            if (AlignLines && null == mMoveTween)
            //if (null == mMoveTween)
            {
                if (mLinePerPage >= mTotalLines)
                {
                    _tweenToIndex(0);
                }
                else
                {
                    Vector3 pos = content.localPosition;
                    Vector3 pos1 = _getConetntPosByIdx( mShowStart*mNumPerLine );
                    int i = 0;
                    if (ScrollViewType.Vertical == ScrollType)
                    {
                        if (pos.y - pos1.y > (mRealItemSize.y + mContentPadding.y) / 2)
                        {
                            i = 1;
                        }
                    }
                    else if (ScrollViewType.Horizontal == ScrollType)
                    {
                        if (pos1.x - pos.x > (mRealItemSize.x  + mContentPadding.x ) / 2)
                        {
                            i = 1;
                        }
                    }
                    _tweenToIndex((mShowStart + i) * mNumPerLine);
                }
            }
        }

        private void _setItemSelected(int index, bool selected)
        {
            if (index >= 0 && index < mItemDatas.Count)
            {
                mItemSelectStatus[index] = selected;
                for (int i = 0; i < mCurItems.Count; i++)
                {
                    ScrollItem item = mCurItems[i];
                    if (index == item.Index)
                    {
                        item.IsSelected = selected;
                        return;
                    }
                }
            }
        }

        private void _switchItem(int index)
        {
            if (index >= 0 && index < mItemDatas.Count)
            {
                _setItemSelected(index, !mItemSelectStatus[index]);
            }
        }

        private void changeSelectAll(bool selected)
        {
            for (int i = 0; i < mItemDatas.Count; i++)
            {
                mItemSelectStatus[i] = selected;
            }
            for (int i = 0; i < mCurItems.Count; i++)
            {
                ScrollItem item = mCurItems[i];
                item.IsSelected = selected;
            }
            _onSelectStatusChanges();
        }

        private void _onSelectStatusChanges()
        {
            List<int> l = ObjPools.GetListInt();
            for (int i = 0; i < mItemSelectStatus.Count; i++)
            {
                if(mItemSelectStatus[i])
                {
                    l.Add(i);
                }
            }

            int[] arr = l.ToArray();
            ObjPools.Recover(l);

            if(null != mOnSelectChange)
            {
                mOnSelectChange(arr);
            }
            if(null != mOnSelectChangeLua)
            {
                mOnSelectChangeLua.Call<int[]>(arr);
            }

        }
        #endregion 私有方法

        #region ObjPool回调
        bool onPoolGetDelegate(ref ScrollItem obj)
        {
            if (null == obj)
            {
                GameObject gobj = Instantiate(ItemPrefab, this.content, false);
                gobj.name = "item" + mItemPool.TotalObjCount;
                //LogFile.Warn(gobj.name);
                obj = gobj.GetComponent<ScrollItem>();
                if (null == obj)
                {
                    LogFile.Error("ItemPrefab：{0} prefab没有添加ScrollItem组件", ItemPrefab.name);
                    return false;
                }
                obj.OnItemClicked = _onItemClicked;
                obj.OnBtnClickedIndex = _onItemBtnClickI;
                obj.OnBtnClickedStr = _onItemBtnClickS;
                if (ItemSize.x.Equals(-1f))
                {
                    //修改锚点
                    Vector2 vector = obj.rectTransform.anchorMax;
                    vector.x = 0.5f;
                    obj.rectTransform.anchorMax = vector;
                    vector = obj.rectTransform.anchorMin;
                    vector.x = 0.5f;
                    obj.rectTransform.anchorMin = vector;
                    obj.rectTransform.pivot = vector;
                    //修改 item 宽高
                    vector.x = mRealItemSize.x;
                    vector.y = obj.rectTransform.rect.height;
                    obj.rectTransform.sizeDelta = vector;
                }
                if (ItemSize.y.Equals(-1f))
                {
                    //修改锚点
                    Vector2 vector = obj.rectTransform.anchorMax;
                    vector.y = 0.5f;
                    obj.rectTransform.anchorMax = vector;
                    vector = obj.rectTransform.anchorMin;
                    vector.y = 0.5f;
                    obj.rectTransform.anchorMin = vector;
                    obj.rectTransform.pivot = vector;
                    //修改 item 宽高
                    vector.x = obj.rectTransform.rect.width;
                    vector.y = mRealItemSize.y;
                    obj.rectTransform.sizeDelta = vector;
                }
            }
            obj.gameObject.SetActive(true);
            return true;
        }

        bool onPoolRecoverDelegate(ScrollItem obj)
        {
            obj.gameObject.SetActive(false);
            return true;
        }

        bool onPoolDisposeDelegate(ref ScrollItem obj)
        {
            Destroy(obj.gameObject);
            obj = null;
            return true;
        }
        #endregion ObjPool回调

        #region ScrollRect 显示区域改变回调
        void onSrollViewValueChanged(Vector2 value)
        {
            if (mIsSizeChanged)
            {
                mIsSizeChanged = false;
                return;
            }

            Vector2 pos = content.localPosition;
            if (Math.Abs(mLastLocalPos.x - pos.x) < mRealItemSize.x / 100 && Math.Abs(mLastLocalPos.y - pos.y) < mRealItemSize.y / 100)
            {
                return;
            }
            mLastLocalPos = pos;
            _checkNeedUpdate();
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            _alignNearestLine();
        }
        #endregion ScrollRect 显示区域改变回调
    }

    #region 辅助类或枚举
    public enum ScrollViewType
    {
        Horizontal,
        Vertical,
    }
    #endregion 辅助类或枚举
}