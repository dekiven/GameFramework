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
            FuncStr = funcStr;
            UIName = uiName;
            UIIndex = -1;
            Content = content;
        }

        public UIHandlerData(string funcStr, int uiIndex, System.Object content)
        {
            FuncStr = funcStr;
            UIName = string.Empty;
            UIIndex = uiIndex;
            Content = content;
        }

        public UIHandlerData(LuaTable luaTable)
        {
            if(null == luaTable)
            {
                return;
            }
            int i = 0;
            FuncStr = luaTable.RawGetIndex<string>(++i).ToLower();
            UIName = string.Empty;
            UIIndex = luaTable.RawGetIndex<int>(++i);
            if (FuncStr.EndsWith("sprite"))
            {
                string spriteStr = luaTable.RawGetIndex<string>(++i);
                string[] _params = spriteStr.Split(',');
                if (_params.Length == 3)
                {
                    GameSpriteAtlasManager.Instance.GetSpriteSync(_params[0], _params[1], _params[2], (Sprite s) =>
                    {
                        Content = s;
                    });
                }
                else
                {

                    LogFile.Warn("UIHandlerData error => can't get sprite:" + spriteStr);
                }
            }
            else if (FuncStr.EndsWith("color"))
            {
                Content = Tools.GenColorByStr(luaTable.RawGetIndex<string>(++i));
            }
            else if (FuncStr.EndsWith("rect"))
            {
                Content = Tools.GenRectByStr(luaTable.RawGetIndex<string>(++i));
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
    }
}
