using System;
using UnityEngine;
using LuaInterface;

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
            int i = 0;
            FuncStr = luaTable.RawGetIndex<string>(++i);
            UIName = string.Empty;
            UIIndex = luaTable.RawGetIndex<int>(++i);
            if (FuncStr.EndsWith("Sprite"))
            {
                string spriteStr = luaTable.RawGetIndex<string>(++i);
                string[] _params = spriteStr.Split(',');
                if(_params.Length == 3)
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
            else if (FuncStr.EndsWith("Color"))
            {
                Content = Tools.GenColorByStr(luaTable.RawGetIndex<string>(++i));
            }
            else if (FuncStr.EndsWith("Rect"))
            {
                Content = Tools.GenRectByStr(luaTable.RawGetIndex<string>(++i));
            }
            else
            {
                Content = luaTable.RawGetIndex<System.Object>(++i);
            }
        }

        public UIHandlerData()
        {
        }

        public void Dispose()
        {
            LuaFunction func = Content as LuaFunction;
            if (null != func)
            {
                func.Dispose();
            }
        }
    }
}
