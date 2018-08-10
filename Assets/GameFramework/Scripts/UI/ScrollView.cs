using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;


//参考资料：「Unity3D」(10)自定义属性面板Inspector详解
//URL:https://www.jianshu.com/p/497fcbad2ad0 

namespace GameFramework
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollView : ScrollRect
    {
        public Action<int> OnClickItem;

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

        private ObjPool<ScrollItem> mItemPool;
        private List<ScrollItem> mCurItems;
        private List<ScrollItemData> mItemDatas;
        private int mNumPerLine;
        private int mLinePerPage;
        private int mTotalLines;
        private int mShowStart = 0;
        private int mShowEnd = 0;

        private Vector2 mContntSpace = new Vector2();

        protected override void Awake()
        {
            base.Awake();
            mItemPool = new ObjPool<ScrollItem>(onPoolGetDelegate, onPoolRecoverDelegate, onPoolDisposeDelegate);
            mCurItems = new List<ScrollItem>();
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

            //TODO:优化，直接在OnDragxxxx上处理
            this.onValueChanged.AddListener(onSrollViewValueChanged);
        }

        private ScrollItem getItem()
        {
            return mItemPool.Get();
        }

        private void recoverItem(ScrollItem item)
        {
            mItemPool.Recover(item);
        }

        public void SetDatas(List<ScrollItemData> data)
        {
            mItemDatas = data;
            CalculateContentSize();

            //normalizedPosition = Vector2.zero;
            checkNeedUpdate();
        }

        public void UpdateData(ScrollItemData data)
        {
            //TODO:
        }

        public void UpdateData(int indext)
        {
            //TODO:
        }

        public void AddData(ScrollItemData data)
        {
            mItemDatas.Add(data);
        }

        public void Insert(ScrollItemData data, int index)
        {
            mItemDatas.Insert(index, data);
        }

        public void RemoveData(ScrollItemData data)
        {
            if (mItemDatas.Contains(data))
            {
                mItemDatas.Remove(data);
            }
        }

        public void RemoveDataAt(int index)
        {
            if(mItemDatas.Count > index && index >=0 )
            {
                mItemDatas.RemoveAt(index);
            }
        }

        public void CalculateContentSize()
        {
#if UNITY_EDITOR
            if(null == mItemDatas)
            {
                mItemDatas = new List<ScrollItemData>();
                for (int i = 0; i < 100; i++)
                {
                    mItemDatas.Add(new ScrollItemData());
                }
            }
#endif
            if(null == mItemDatas || null == ItemPrefab)
            {
                LogFile.Warn("ScrollView calculateContentSize Error: null == mItemDatas || null == ItemPrefab");
                return;
            }
            Rect rect = content.rect; 
            Vector2 rectSize = rect.size;
            Vector2 viewSize = viewport.rect.size;
            Vector2 useSize = Vector2.zero;
            int dataCount = mItemDatas.Count;
            if(ScrollViewType.Vertical == ScrollType)
            {
                useSize.x = rectSize.x - PaddingLeft - PaddingBottom;
                useSize.y = viewSize.y - PaddingTop - PaddingBottom;
                mNumPerLine =  Mathf.FloorToInt(useSize.x / ItemSize.x);
                mLinePerPage = Mathf.FloorToInt(useSize.y / ItemSize.y);
                if(mNumPerLine < 1 || mLinePerPage < 1)
                {
                    LogFile.Error("ScrollView calculateContentSize Error:  mNumPerLine < 1 || mLinePerPage < 1");
                }
                //TODO:计算space
                float space = useSize.x - ItemSize.x * mNumPerLine;
                if(mNumPerLine > 1)
                {
                    mContntSpace.x = space / (mNumPerLine - 1);
                }
                else if(mNumPerLine == 1)
                {
                    mContntSpace.x = 0f;
                    PaddingLeft += space / 2;
                    PaddingBottom += space / 2;
                }
                space = useSize.y - mLinePerPage * ItemSize.y;
                if (mLinePerPage > 1)
                {
                    mContntSpace.y = space / (mLinePerPage - 1);
                }
                else if (mLinePerPage == 1)
                {
                    mContntSpace.y = mContntSpace.x <= 5f ? mContntSpace.x : 5f;
                    //PadingTop += space / 2;
                    //PadingBottom += space / 2;
                }
                mTotalLines = Mathf.CeilToInt(dataCount / (float)mNumPerLine);
                rect.height = PaddingTop + PaddingBottom + mTotalLines * ItemSize.y + (mTotalLines - 1) * mContntSpace.y;
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
            }
            else
            {
                useSize.x = viewSize.x - PaddingLeft - PaddingBottom;
                useSize.y = rectSize.y - PaddingTop - PaddingBottom;
                mNumPerLine = Mathf.FloorToInt( useSize.y / ItemSize.y);
                mLinePerPage = Mathf.FloorToInt(useSize.x / ItemSize.x);
                //TODO:计算space

                float space = useSize.y - ItemSize.y * mNumPerLine;
                if (mNumPerLine > 1)
                {
                    mContntSpace.y = space / (mNumPerLine - 1);
                }
                else if (mNumPerLine == 1)
                {
                    mContntSpace.y = 0f;
                    PaddingTop += space / 2;
                    PaddingBottom += space / 2;
                }
                space = useSize.x - mLinePerPage * ItemSize.x;
                if (mLinePerPage > 1)
                {
                    mContntSpace.x = space / (mLinePerPage - 1);
                }
                else if (mLinePerPage == 1)
                {
                    mContntSpace.x = mContntSpace.y <= 5f ? mContntSpace.y : 5f;
                    //PadingLeft += space / 2;
                    //PadingBottom += space / 2;
                }

                mTotalLines = Mathf.CeilToInt(dataCount / (float)mNumPerLine);
                rect.width = PaddingLeft + PaddingBottom + mTotalLines * ItemSize.x + (mTotalLines - 1) * mContntSpace.x; 
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);

            }
            //content. = rect;
            //LogFile.Warn("numPerLine:{0}, linePerPage;{1}, padding:{2}, space{3}, spaceSc{4}, useSize:{5}", mNumPerLine, mLinePerPage, ContentPadding, mContntSpace, verticalScrollbarSpacing, useSize);
        }

        private Vector3 getItemPosByIndex(int i)
        {
            int row = 0;
            int colume = 0;
            if (ScrollViewType.Vertical == ScrollType)
            {
                colume = i % mNumPerLine;
                row = i / mNumPerLine;
            }
            else
            {
                row = i % mNumPerLine;
                colume = i / mNumPerLine;
            }
            var pos =  new Vector3(PaddingLeft + colume * ItemSize.x + ItemSize.x/2 + mContntSpace.x * colume, -(PaddingTop + row * ItemSize.y + ItemSize.y / 2 + mContntSpace.y * row));
            return pos;
            //return new Vector3(PadingLeft + colume * mItemSize.x + mItemSize.x/2 + mContntSpace.x * colume, PadingTop + row * mItemSize.y + mItemSize.y/2 + mContntSpace.y * row);
        }

        private bool canItemShow(int index)
        {
            return canItemShow(getItemPosByIndex(index));
        }

        private bool canItemShow(Vector2 pos)
        {
            if(ScrollViewType.Vertical == ScrollType)
            {
                float posY = -pos.y - content.localPosition.y;
                //Debug.LogFormat("PosY:{0}, halfY:{1}, maxHeight:{2}", posY, mItemSize.y / 2, viewport.rect.height - mItemSize.y / 2);
                return posY > -ItemSize.y / 2 && posY < viewport.rect.height + ItemSize.y / 2;
            }else
            {
                float posX = pos.x - content.localPosition.x;
                return posX > -ItemSize.x / 2 && posX < viewport.rect.width + ItemSize.x / 2;
            }
        }

        private bool canLineShow(int line)
        {
            return canItemShow(line * mNumPerLine);
        }

        private void checkNeedUpdate()
        {
            bool start = false;
            int startLine = -1;
            int endLine = -1;
            for (int i = 0; i < mTotalLines; i++)
            {
                //Debug.Log("chack line:" + i);
                bool canShow = canLineShow(i);
                if(!start)
                {
                    if(canShow)
                    {
                        startLine = i;
                        start = true;
                    }
                }
                if(canShow && i > endLine) 
                {
                    endLine = i;
                }
            }
            updateAllItem(startLine, endLine);

        }

        private void updateAllItem(int startLine, int endLine)
        {
            if(startLine > endLine || 0 == endLine)
            {
                LogFile.Warn("startLine > endLine || 0 == endLine");       
                return;
            }
            if (mShowStart != startLine || mShowEnd != endLine)
            {
                //只有首尾的行变动，只处理相应的行
                if(Math.Abs(mShowStart - startLine) <= 1 && Math.Abs(endLine - mShowEnd) <= 1)
                {

                    ///添加新的Item
                    //如果开始的行数比当前的小1，证明现在在开头多显示了一行
                    if (mShowStart - startLine == 1)
                    {
                        addStartLine(startLine);
                    }
                    //如果结束的行数比当前的大1，证明现在在开头多显示了一行
                    if (endLine - mShowEnd == 1)
                    {
                        addEndLine(endLine);
                    }

                    //减少Item
                    if (startLine - mShowStart == 1)
                    {
                        recoverStartLine(mShowStart);
                    }
                    if(mShowEnd - endLine == 1)
                    {
                        recoverEndLine(mShowEnd);
                    }
                    mShowStart = startLine;
                    mShowEnd = endLine;
                }
                else
                {
                    //滑动多行了，直接全部刷新
                    int startIndex = Mathf.Clamp(startLine * mNumPerLine, 0, mItemDatas.Count);
                    int endIndex = Mathf.Clamp((endLine + 1) * mNumPerLine - 1, 0, mItemDatas.Count);
                    int count = endIndex - startIndex + 1;
                    if(count > mCurItems.Count)
                    {
                        for (int i = 0; i < count; ++i)
                        {
                            mCurItems.Add(getItem());
                        }
                    }
                    else if(count < mCurItems.Count)
                    {
                        for (int i = 0; i < mCurItems.Count - count; ++i)
                        {
                            ScrollItem item = mCurItems[0];
                            recoverItem(item);
                            mCurItems.RemoveAt(0);
                        }
                    }
                    for (int i = 0; i < count; i++)
                    {
                        ScrollItem item = mCurItems[i];
                        item.SetData(mItemDatas[startIndex + i]);
                        item.transform.localPosition = getItemPosByIndex(startIndex + i);
                    }
                }
                mShowStart = startLine;
                mShowEnd = endLine;
            }

        }

        private void addStartLine(int line)
        {
            for (int i = 0; i < mNumPerLine; i++)
            {
                int index = line * mNumPerLine + i;
                if (index < mItemDatas.Count)
                {
                    ScrollItem item = getItem();
                    mCurItems.Insert(0, item);
                    item.SetData(mItemDatas[index]);
                    item.transform.localPosition = getItemPosByIndex(index);
                }
            }
        }

        private void addEndLine(int line)
        {
            for (int i = 0; i < mNumPerLine; i++)
            {
                int index = line * mNumPerLine + i;
                if (index < mItemDatas.Count)
                {
                    ScrollItem item = getItem();
                    mCurItems.Add(item);
                    item.SetData(mItemDatas[index]);
                    item.transform.localPosition = getItemPosByIndex(index);
                }
            }
        }

        private void recoverStartLine(int line)
        {
            for (int i = 0; i < mNumPerLine; i++)
            {
                int index = line * mNumPerLine + i;
                if (index < mItemDatas.Count)
                {
                    ScrollItem item = mCurItems[0];
                    recoverItem(item);
                    mCurItems.RemoveAt(0);
                }
            }
        }

        private void recoverEndLine(int line)
        {
            int count = mCurItems.Count;
            for (int i = 0; i < mNumPerLine; i++)
            {
                int index = line * mNumPerLine + i;
                if (index < mItemDatas.Count)
                {
                    int idx = count - 1 - i;
                    ScrollItem item = mCurItems[idx];
                    recoverItem(item);
                    mCurItems.RemoveAt(idx);
                }
            }
        }

        #region ObjPool回调
        bool onPoolGetDelegate(ref ScrollItem obj)
        {
            if (null == obj)
            {
                GameObject gobj = Instantiate(ItemPrefab, this.content, false);
                gobj.name = "item" + mItemPool.TotalObjCount;
                obj = gobj.GetComponent<ScrollItem>();
                if (null == obj)
                {
                    LogFile.Error("ItemPrefab：{0} prefab没有添加ScrollItem组件", ItemPrefab.name);
                    return false;
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
            checkNeedUpdate();
        }
        #endregion ScrollRect 显示区域改变回调

    }

    public enum ScrollViewType
    {
        Horizontal,
        Vertical,
    }
}