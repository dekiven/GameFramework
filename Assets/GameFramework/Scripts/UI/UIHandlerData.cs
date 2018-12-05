using System;
using UnityEngine;
using LuaInterface;
using System.Collections.Generic;

namespace GameFramework
{
    public class UIHandlerData : IDisposable
    {
        public string FuncStr;
        public string UIName;
        public int UIIndex;
        public System.Object Content;

        public UIHandlerData(string funcStr, string uiName, System.Object content)
        {
            FuncStr = funcStr.ToLower();
            UIName = uiName;
            UIIndex = -1;
            Content = content;
        }

        public UIHandlerData(string funcStr, int uiIndex, System.Object content)
        {
            FuncStr = funcStr.ToLower();
            UIName = string.Empty;
            UIIndex = uiIndex;
            Content = content;
        }

        public UIHandlerData(LuaTable luaTable)
        {
            if (null == luaTable)
            {
                return;
            }
            int i = 0;
            FuncStr = luaTable.RawGetIndex<string>(++i).ToLower();
            UIName = string.Empty;
            UIIndex = luaTable.RawGetIndex<int>(++i);

            //else
            if (FuncStr.EndsWith("color", StringComparison.Ordinal))
            {
                Content = Tools.GenColorByStr(luaTable.RawGetIndex<string>(++i));
            }
            else if (FuncStr.EndsWith("rect", StringComparison.Ordinal))
            {
                Content = Tools.GenRectByStr(luaTable.RawGetIndex<string>(++i));
            }
            else if (FuncStr.Equals("changesubhandlerui"))
            {
                LuaTable lua = luaTable.RawGetIndex<LuaTable>(++i);
                Content = new UIHandlerData(lua);
            }
            else if (FuncStr.Equals("changesubhandleritem"))
            {
                LuaTable lua = luaTable.RawGetIndex<LuaTable>(++i);
                Content = new UIItemData(lua);
            }
            else
            {
                Content = luaTable.RawGetIndex<System.Object>(++i);
            }
            luaTable.Dispose();
            luaTable = null;
        }

        public UIHandlerData()
        {
        }

        public void Dispose()
        {
            LuaBaseRef lua = Content as LuaBaseRef;
            if (null != lua)
            {
                lua.Dispose();
                Content = null;
            }
        }

        #region 静态方法
        public static UIHandlerData GetData(string funcStr, string uiName, System.Object content)
        {
            UIHandlerData data = new UIHandlerData(funcStr, uiName, content);
            checkAsyncData(ref data);
            return data;
        }

        public static UIHandlerData GetData(string funcStr, int uiIndex, System.Object content)
        {
            UIHandlerData data = new UIHandlerData(funcStr, uiIndex, content);
            checkAsyncData(ref data);
            return data;
        }

        public static UIHandlerData GetData(LuaTable luaTable)
        {
            UIHandlerData data = new UIHandlerData(luaTable);
            checkAsyncData(ref data);
            return data;
        }

        static void checkAsyncData(ref UIHandlerData data)
        {
            UnityEngine.Object obj = null;
            bool needAsync = false;
            if (data.FuncStr.EndsWith("sprite", StringComparison.Ordinal))
            {
                obj = data.Content as Sprite;
                needAsync = false;
            }
            if (data.FuncStr.EndsWith("material", StringComparison.Ordinal))
            {
                obj = data.Content as Material;
                needAsync = false;
            }
            if(null == obj && needAsync)
            {
                string spriteStr = data.Content as string;
                if (!string.IsNullOrEmpty(spriteStr))
                {
                    data = new UIHandlerDataAsync(data);
                }
            }
        }
        #endregion
    }
}
