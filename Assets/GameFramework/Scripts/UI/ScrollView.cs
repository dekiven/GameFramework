using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GameFramework
{
    public class ScrollView : ScrollRect
    {
        public GameObject ItemPrefab;
        public ObjPool<ScrollItem> ItemPool;
        public Action<int> OnClickItem;

        private List<ScrollItemData> mData;

        public void SetDatas(List<ScrollItemData> data)
        {
            mData = data;
        }
    }
}