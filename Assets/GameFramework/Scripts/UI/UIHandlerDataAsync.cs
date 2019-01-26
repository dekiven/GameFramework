using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

namespace GameFramework
{
    public class UIHandlerDataAsync : UIHandlerData
    {
        public System.Object ContentBefor = null;

        Action<System.Object> mOnAsyncRst;
        public Action<System.Object> OnAsyncRst
        {
            get {
                return OnAsyncRst;
            }
            set {
                if((null == Content && null != ContentBefor) || null == value)
                {
                    mOnAsyncRst = value;
                }
            }
        }

        public UIHandlerDataAsync(string funcStr, string uiName, System.Object content)
            :base(funcStr, uiName, null)
        {
            ContentBefor = content;
            startAsync();
        }

        public UIHandlerDataAsync(string funcStr, int uiIndex, System.Object content)
            : base(funcStr, uiIndex, null)
        {
            ContentBefor = content;
            startAsync();
        }

        public UIHandlerDataAsync(LuaTable luaTable)
            :base(luaTable)
        {
            ContentBefor = Content;
            Content = null;
            startAsync();
        } 

        public UIHandlerDataAsync(UIHandlerData data)
        {
            FuncStr = data.FuncStr;
            UIName = data.UIName;
            UIIndex = data.UIIndex;
            ContentBefor = data.Content;

            startAsync();

            data.Dispose();
        }

        private void startAsync()
        {
            if (FuncStr.EndsWith("sprite", StringComparison.Ordinal))
            {
                string spriteStr = ContentBefor as String;
                string[] _params = spriteStr.Split(',');
                if (_params.Length == 3)
                {
                    GameSpriteAtlasManager.Instance.GetSpriteAsync(_params[0], _params[1], _params[2], (Sprite s) =>
                    {
                        Content = s;
                        if (null != mOnAsyncRst)
                        {
                            mOnAsyncRst(s);
                            mOnAsyncRst = null;
                        }
                        ContentBefor = null;
                    });
                }
                else
                {
                    LogFile.Warn("UIHandlerDataAsync error => can't get sprite:" + spriteStr);
                }
                return;
            }
            if (FuncStr.EndsWith("material", StringComparison.Ordinal))
            {
                string str = ContentBefor as String;
                string[] _params = str.Split(',');
                if (_params.Length == 2)
                {
                    GameResManager.Instance.LoadRes<Material>(_params[0], _params[1], (UnityEngine.Object material) =>
                    {
                        Content = material;
                        if (null != mOnAsyncRst)
                        {
                            mOnAsyncRst(material);
                            mOnAsyncRst = null;
                        }
                    });
                }
                else
                {
                    LogFile.Warn("UIHandlerDataAsync error => can't get Material:" + str);
                }
                return;
            } 
        }
    }
}
