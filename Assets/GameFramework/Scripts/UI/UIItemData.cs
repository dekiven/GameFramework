using System;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

namespace GameFramework
{
    public class UIItemData : IDisposable
    {
        //public string Info;
        public List<UIHandlerData> DataList { get { return mList; }}
        private List<UIHandlerData> mList;

        public UIItemData()
        {
            mList = new List<UIHandlerData>();
        }

        public UIItemData(LuaTable luaTable)
        {
            mList = new List<UIHandlerData>();
            convertLuaTable2Data(luaTable);
        }

        public UIItemData(List<UIHandlerData> data)
        {
            mList = data;
        }

        public void Dispose()
        {
            for (int i = 0; i < mList.Count; i++)
            {
                UIHandlerData data = mList[i];
                data.Dispose();
            }
            mList.Clear();
        }

        public void ClearAsyncRst()
        {
            if (null != mList)
            {
                for (int i = 0; i < mList.Count; ++i)
                {
                    UIHandlerDataAsync ds = mList[i] as UIHandlerDataAsync;
                    if (null != ds)
                    {
                        ds.OnAsyncRst = null;
                    }
                }
            }
        }

        private void convertLuaTable2Data(LuaTable luaTable)
        {
            if(null != luaTable)
            {
                int count = luaTable.RawGet<string, int>("count");
                if(count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        mList.Add(UIHandlerData.GetData(luaTable.RawGetIndex<LuaTable>(i+1)));
                    }
                }
                luaTable.Dispose();
            }
        }
    }
}
