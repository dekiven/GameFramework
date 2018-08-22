using System;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

namespace GameFramework
{
    public class ScrollItemData : IDisposable
    {
        //public string Info;
        public List<UIHandlerData> DataList { get { return mList; }}
        private List<UIHandlerData> mList;

        public ScrollItemData()
        {
            mList = new List<UIHandlerData>();
        }

        public ScrollItemData(LuaTable luaTable)
        {
            mList = new List<UIHandlerData>();
            convertLuaTable2Data(luaTable);
        }

        public ScrollItemData(List<UIHandlerData> data)
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
        }

        private void convertLuaTable2Data(LuaTable luaTable)
        {
            int count = luaTable.RawGet<string, int>("count");
            if(count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    mList.Add(new UIHandlerData(luaTable.RawGetIndex<LuaTable>(i+1)));
                }
            }
        }
    }
}
