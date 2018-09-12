using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace GameFramework
{
    using SelectorData = List<UIItemData>;

    //TODO:使用AnimationCurve支持Item位置变换
    public class ScrollSelector : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RectTransform Viewport;
        public RectTransform Content;
        [HideInInspector]
        public int ShowNum = 5;
        [HideInInspector]
        public GameObject ItemPrefab;
        public Vector2 ItemSize = Vector2.zero;
        public float CenterScale = 1.1f;
        public float OtherScale = 1f;
        public UnityAction<int> OnItemSelected;
        public Ease AnimEase = Ease.Linear;
        public int ItemNumPerStep = 1;
        public SelectorToggles Toggles;
        public float DragRate = 1f;

        private ObjPool<ScrollItem> mItemPool;
        private List<ScrollItem> mCurItems;
        private SelectorData mData;

        private Vector3[] mItemPos;
        private Vector3 mItemOffset;
        private int mCurIndex = -1;
        private LuaFunction mOnItemSelectedLua;
        private Tween mMoveTween = null;
        private int mShowStart;
        private int mShowEnd;
        private Coroutine mUpCoroutine;
        private int mTargetIndex = 0;

        public void SetData(SelectorData data)
        {
            mData = data;
            int count = data.Count;
            //if(count < ShowNum)
            //{
            //    setShowNum(count);
            //}
            calculateItemOffset();
            recoverAll();
            //if (null == mItemPos || mItemPos.Length != count)
            calulateItemPos(count);
            checkNeedUpdate(true);
            if(null != Toggles)
            {
                Toggles.SetTotalNum(data.Count);
            }
            SetCurIndex(0);
            updateDrag (new Vector2(5, 0));
        }

        public void SetData(LuaTable luaTable)
        {
            List<UIItemData> data = Tools.GenUIIemDataList(luaTable);
            SetData(data);
        }

        public void SetDataAndIndex(SelectorData data, int index)
        {
            SetData(data);
            SetCurIndex(index);
        }

        public void SetCurIndex(int index)
        {
            if(null != mData)
            {
                if (null != mUpCoroutine)
                {
                    mTargetIndex = index;
                }
                else
                {
                    mTargetIndex = -1;
                    tweenToIndex(index);
                }
            }
        }


        public void SetOnSelectCallbackLua(LuaFunction call)
        {
            if (null != mOnItemSelectedLua)
            {
                mOnItemSelectedLua.Dispose();
                mOnItemSelectedLua = null;
            }
            mOnItemSelectedLua = call;
        }

        /// <summary>
        /// 本函数在编辑器使用
        /// </summary>
        /// <param name="showNum">Show number.</param>
        public void ShowNumFix(int showNum)
        {
            if (showNum > 1)
            {
                int v = showNum / 2 * 2 + 1;
                if (v != showNum)
                {
                    ShowNum = v;
                }
            }
        }

        #region UIBehaviour
        protected override void Awake()
        {
            base.Awake();
            if (null == Content)
            {
                // LogFile.Error("Selector Content is null");
                return;
            }
            mItemPool = new ObjPool<ScrollItem>(OnGetItemDelegate, OnItemRecoverDelegate, OnItemDisposeDelegate);
            mCurItems = new List<ScrollItem>();
            //Debug.Log(Content.anchoredPosition);
        }

        protected override void Start()
        {
            base.Start();
            calculateItemOffset();
            if(null != Toggles)
            {
                Toggles.SetOnIndexChange(SetCurIndex);
            }
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                StartCoroutine(setTestData());
            }
#endif
        }

        void LateUpdate()
        {
            if (null != mMoveTween)
            {
                checkNeedUpdate();
            }
        }

        protected override void OnDestroy()
        {
            if (null != mOnItemSelectedLua)
            {
                mOnItemSelectedLua.Dispose();
                mOnItemSelectedLua = null;
            }
            if (null != mData)
            {
                for (int i = 0; i < mData.Count; i++)
                {
                    mData[i].Dispose();
                }
                mData.Clear();
            }
            if (null != mItemPool)
            {
                mItemPool.Dispose();
                mItemPool = null;
            }
            base.OnDestroy();
        }
        #endregion UIBehaviour

        #region  Drag相关
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            //TODO:
            killMoveTween();
            checkNeedUpdate();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            //TODO:
            killMoveTween();
            checkNeedUpdate();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            if (!IsActive())
            {
                return;
            }
            updateDrag(eventData.delta * DragRate);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            //TODO:
            //checkCurItemOnEnd(eventData.delta);
            updateDragEnd();
        }
        #endregion  Drag相关

        #region ObjPool 回调
        bool OnGetItemDelegate(ref ScrollItem obj)
        {
            if (null == obj)
            {
                GameObject gobj = Instantiate(ItemPrefab, Content, false);
                gobj.name = "item" + mItemPool.TotalObjCount;
                //// LogFile.Warn(gobj.name);
                obj = gobj.GetComponent<ScrollItem>();
                if (null == obj)
                {
                    // LogFile.Error("ItemPrefab：{0} prefab没有添加ScrollItem组件", ItemPrefab.name);
                    return false;
                }
                obj.OnItemClicked = onItemClicked;
            }
            obj.transform.SetSiblingIndex(0);
            obj.gameObject.SetActive(true);
            return true;
        }

        bool OnItemRecoverDelegate(ScrollItem obj)
        {
            obj.gameObject.SetActive(false);
            obj.Index = mData.Count;
            return true;
        }

        bool OnItemDisposeDelegate(ref ScrollItem obj)
        {

            Destroy(obj.gameObject);
            obj = null;
            return true;
        }
        #endregion ObjPool 回调

        #region 私有方法
        private void setShowNum(int value)
        {
            if (value > 0)
            {
                //Debug.Log(value);
                int v = value / 2 * 2 + 1;
                if (!v.Equals(ShowNum) && v > 1)
                {
                    ShowNum = v;
                    calculateItemOffset();
                }
                //Debug.LogWarning(ShowNum);
            }
        }

        private void calculateItemOffset()
        {
            if (!ItemSize.Equals(Vector2.zero))
            {
                Vector2 contentSize = Content.rect.size;
                float realSize = 0;
                //if (ScrollType == ScrollViewType.Horizontal)
                {
                    realSize = contentSize.x - ItemSize.x * OtherScale;
                    mItemOffset.x = realSize / (ShowNum - 1);
                    //mCenter = new Vector2(contentSize.x / 2, 0);
                }
                //else
                //{
                //    realSize = contentSize.y - ItemSize.y * OtherScale * 2;
                //    mItemOffset.y = realSize / (ShowNum - 2);
                //}
                // LogFile.Log("contentSize{0}, mItemOffset:{1}, ItemSize:{2}, realSize:{3}", contentSize, mItemOffset, ItemSize, realSize);
            }
        }

        private void calulateItemPos(int count)
        {
            mItemPos = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                mItemPos[i] = mItemOffset * i;
            }
        }

        private void recoverItem(ScrollItem item)
        {
            mItemPool.Recover(item);
        }

        private ScrollItem getItem()
        {
            return mItemPool.Get();
        }

        private void recoverAll()
        {
            for (int i = mCurItems.Count - 1; i > -1; --i)
            {
                ScrollItem item = mCurItems[i];
                recoverItem(item);
                mCurItems.RemoveAt(i);
            }
        }

        private void onItemClicked(int index)
        {
            //Debug.Log("onItemClicked:" + index);
            tweenToIndex(index);
        }

        private int getCurIndex()
        {
            int index = -1;
            float min = float.MaxValue;
            float posX = -Content.localPosition.x;
            for (int i = 0; i < mItemPos.Length; i++)
            {
                float distance = Math.Abs(posX - mItemPos[i].x);
                if (distance < min)
                {
                    index = i;
                    min = distance;
                }
                else if (!min.Equals(float.MaxValue))
                {
                    return index;
                }
            }
            return index;
        }

        private void sortItems(bool force = false)
        {
            int curIndex = getCurIndex();
            if (mCurIndex != curIndex || force)
            {
                int count = mCurItems.Count;
                //Debug.LogWarning("curIndex:" + curIndex);
                mCurIndex = curIndex;
                if(null != Toggles)
                {
                    Toggles.SetCurIndex(mCurIndex);
                }
                List<ItemSortData> siblings = new List<ItemSortData>();
                for (int i = 0; i < count; i++)
                {
                    siblings.Add(new ItemSortData(i, mCurItems[i].Index));
                    //Debug.Log(siblings[i]);
                }
                siblings.Sort((ItemSortData left, ItemSortData right) =>
                {
                    int dl = Math.Abs(left.Index - mCurIndex);
                    int dr = Math.Abs(right.Index - mCurIndex);
                    if (dl > dr)
                    {
                        return -1;
                    }
                    if (dl < dr)
                    {
                        return 1;
                    }
                    return 0;
                });
                //string log = "";
                for (int i = 0; i < siblings.Count; i++)
                {
                    ScrollItem item = mCurItems[siblings[i].ListIndex];
                    item.transform.SetSiblingIndex(i);
                    item.transform.localScale = Vector3.one * (mCurIndex == item.Index ? CenterScale : OtherScale);
                    //log = log + "idx:" + item.Index + ",sib:" + i + ";";
                }
                //Debug.Log(log);
            }
        }

        private void updateDrag(Vector2 delta)
        {
            float posX = Content.localPosition.x;
            float rate = 1f;
            float outOfRange = 0f;
            if (posX <= -mItemPos[mItemPos.Length - 1].x)
            {
                outOfRange = -(posX + mItemPos[mItemPos.Length - 1].x);
                if (delta.x < 0)
                {
                    rate = -1f;
                }
            }
            else if (posX >= 0)
            {
                outOfRange = posX;
                if (delta.x > 0)
                {
                    rate = -1f;
                }
            }
            if (rate.Equals(-1f))
            {
                if (outOfRange > mItemOffset.x)
                {
                    rate = 0;
                }
                else
                {
                    rate = (mItemOffset.x - outOfRange) / mItemOffset.x;
                }
            }
            Content.Translate(new Vector3(delta.x * rate, 0, 0));
            checkNeedUpdate();
        }

        private void updateDragEnd()
        {
            tweenToIndex(mCurIndex);
        }

        private void tweenToIndex(int index)
        {
            if (index >= 0 && index < mData.Count)
            {
                float posX = Content.localPosition.x;
                float targetPosX = -mItemPos[index].x;
                if (!posX.Equals(targetPosX))
                {
                    Vector3 pos = new Vector3(targetPosX, 0, 0);
                    float dt = Math.Min(1f, Math.Abs(targetPosX - posX) / ItemSize.x / 8);
                    killMoveTween();
                    mMoveTween = DOTween.To(() => Content.localPosition, (Vector3 v) => Content.localPosition = v, pos, dt).SetEase(AnimEase).OnComplete(() => 
                    { 
                        noticeIndexChange(index);
                        checkNeedUpdate(true);
                    });
                }
            }
        }

        private ScrollItem getItemByIndex(int index)
        {
            ScrollItem item = null;
            //test
            //TODO:根据content position 计算index对应的Item，没有在显示区域的返回bull
            if (index >= mShowStart && index <= mShowEnd)
            { item = mCurItems[index - mShowStart]; }
            return item;
        }

        private void killMoveTween()
        {
            if (null != mMoveTween)
            {
                mMoveTween.Kill();
                mMoveTween = null;
            }
            checkNeedUpdate();
        }

        private void noticeIndexChange(int index)
        {
            if (null != OnItemSelected)
            {
                OnItemSelected(index);
            }
            if (null != mOnItemSelectedLua)
            {
                mOnItemSelectedLua.Call(index);
            }
        }
        #endregion 私有方法

        #region 刷新相关
        private Vector3 getItemPosByIndex(int i)
        {
            if (i >= 0 && i < mData.Count)
            {
                return mItemPos[i];
            }
            // LogFile.Error("ScrollSelector getItemPosByIndex error => wrong index");
            return Vector3.zero;
        }

        private bool canItemShow(int index)
        {
            return canItemShow(getItemPosByIndex(index));
        }

        private bool canItemShow(Vector2 pos)
        {
            float posX = pos.x + Content.localPosition.x + Viewport.rect.width / 2;
            return posX > -ItemSize.x / 2 && posX < Viewport.rect.width + ItemSize.x / 2;
        }

        private void checkNeedUpdate(bool forceUpdate = false)
        {
            bool start = false;
            int startIndex = -1;
            int endIndex = -1;
            for (int i = 0; i < mData.Count; i++)
            {
                bool canShow = canItemShow(i);
                if (!start)
                {
                    if (canShow)
                    {
                        startIndex = i;
                        start = true;
                    }
                }
                if (canShow && i > endIndex)
                {
                    endIndex = i;
                }
            }
            if (-1 == startIndex || -1 == endIndex)
            {
                return;
            }
            //当显示行数有变化或者需要强制刷新时刷新所有UI
            if (forceUpdate || startIndex != mShowStart || endIndex != mShowEnd)
            {
                if (null != mUpCoroutine)
                {
                    StopCoroutine(mUpCoroutine);
                    mUpCoroutine = null;
                }
                mUpCoroutine = StartCoroutine(updateAllItem(startIndex, endIndex, forceUpdate));
            }

            sortItems(forceUpdate);

        }

        private IEnumerator updateAllItem(int startIndex, int endIndex, bool forceUpdate = false)
        {
            if (mShowStart != startIndex || mShowEnd != endIndex || forceUpdate)
            {
                //只有首尾的行变动，只处理相应的行,只在一帧处理完
                if (Math.Abs(mShowStart - startIndex) <= 1 && Math.Abs(endIndex - mShowEnd) <= 1 && startIndex != endIndex)
                {
                    //减少Item
                    //如果开始的行数比当前的大1，证明现在在开头少显示了一行
                    if (startIndex - mShowStart == 1)
                    {
                        recoverStartItem(mShowStart);
                    }
                    //如果结束的行数比当前的小1，证明现在在末尾少显示了一行
                    if (mShowEnd - endIndex == 1)
                    {
                        recoverEndItem(mShowEnd);
                    }

                    ///添加新的Item
                    //如果开始的行数比当前的小1，证明现在在开头多显示了一行
                    if (mShowStart - startIndex == 1)
                    {
                        addStartItem(startIndex);
                    }
                    //如果结束的行数比当前的大1，证明现在在末尾多显示了一行
                    if (endIndex - mShowEnd == 1)
                    {
                        addEndItem(endIndex);
                    }

                    mShowStart = startIndex;
                    mShowEnd = endIndex;
                }
                else
                {
                    //有多行变动，在协程中处理所有刷新，有新变动停止协程重新处理
                    //// LogFile.Log("updateAllItem 滑动多行了，直接全部刷新 start:{0}, end{1}", startLine, endLine);
                    //滑动多行了，直接全部刷新
                    mShowStart = startIndex;
                    mShowEnd = endIndex;
                    int startIdx = Mathf.Clamp(startIndex, 0, mData.Count - 1);
                    int endIdx = Mathf.Clamp(endIndex, 0, mData.Count - 1);
                    int count = endIdx - startIdx + 1;

                    //回收所有Item，在之后的协程中刷新
                    recoverAll();

                    for (int i = 0; i < count; i++)
                    {
                        ScrollItem item = getItem();
                        mCurItems.Add(item);
                        setItemDataByIndex(item, startIdx + i);
                        if ((count + 1) % ItemNumPerStep == 0)
                        {
                            yield return null;
                        }
                    }
                }
                

                if (mTargetIndex != -1)
                {
                    tweenToIndex(mTargetIndex);
                    mTargetIndex = -1;
                }
            }
            if (null != mUpCoroutine)
            {
                StopCoroutine(mUpCoroutine);
                mUpCoroutine = null;
            }
        }

        private void addStartItem(int index)
        {
            ScrollItem item = getItem();
            mCurItems.Insert(0, item);
            setItemDataByIndex(item, index);
        }

        private void setItemDataByIndex(ScrollItem item, int index)
        {
            UIItemData data = mData[index];
            item.Index = index;
            item.SetData(data);
            item.transform.localPosition = getItemPosByIndex(index);
        }

        private void addEndItem(int index)
        {
            ScrollItem item = getItem();
            mCurItems.Add(item);
            setItemDataByIndex(item, index);
        }

        private void recoverStartItem(int index)
        {
            if (index < 0 || index > mData.Count - 1)
            {
                return;
            }
            if (index < mData.Count)
            {
                ScrollItem item = mCurItems[0];
                recoverItem(item);
                mCurItems.RemoveAt(0);
            }
        }

        private void recoverEndItem(int index)
        {
            if (index < 0 || index > mData.Count - 1)
            {
                return;
            }
            if (index < mData.Count)
            {
                int idx = mCurItems.Count - 1;
                ScrollItem item = mCurItems[idx];
                recoverItem(item);
                mCurItems.RemoveAt(idx);
            }
        }
        #endregion 刷新相关

#if UNITY_EDITOR
        IEnumerator setTestData()
        {
            yield return new WaitForSeconds(3);
            if (mData.Count == 0)
            {
                OnItemSelected = (int index) =>
                {
                    Debug.Log("Callbcak index:" + index);
                };
                var data = new List<UIItemData>();
                for (int i = 0; i < 10; i++)
                {
                    List<UIHandlerData> _data = new List<UIHandlerData>();
                    _data.Add(new UIHandlerData("SetTextString", 0, "Button" + i));
                    _data.Add(new UIHandlerData("setUIName", 1, "Button" + i));
                    data.Add(new UIItemData(_data));
                }
                SetData(data);
                yield return new WaitForSeconds(3);
                SetCurIndex(2);
                //mCurIndex = 3;
            }
        }
#endif

        struct ItemSortData
        {
            public int ListIndex;
            public int Index;

            public ItemSortData(int listIndex, int index)
            {
                ListIndex = listIndex;
                Index = index;
            }
        }
    }

}
