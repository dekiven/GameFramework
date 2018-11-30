using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

namespace GameFramework
{
    public class UIHandlerDataSync : UIHandlerData
    {
        public System.Object ContentBefor = null;

        Action<System.Object> mOnSyncRst;
        public Action<System.Object> OnSyncRst
        {
            get {
                return OnSyncRst;
            }
            set {
                if((null == Content && null != ContentBefor) || null == value)
                {
                    mOnSyncRst = value;
                }
            }
        }

        public UIHandlerDataSync(string funcStr, string uiName, System.Object content)
            :base(funcStr, uiName, null)
        {
            ContentBefor = content;
            startSync();
        }

        public UIHandlerDataSync(string funcStr, int uiIndex, System.Object content)
            : base(funcStr, uiIndex, null)
        {
            ContentBefor = content;
            startSync();
        }

        public UIHandlerDataSync(LuaTable luaTable)
            :base(luaTable)
        {
            ContentBefor = Content;
            Content = null;
            startSync();
        } 

        public UIHandlerDataSync(UIHandlerData data)
        {
            FuncStr = data.FuncStr;
            UIName = data.UIName;
            UIIndex = data.UIIndex;
            ContentBefor = data.Content;

            startSync();

            data.Dispose();
        }

        private void startSync()
        {
            if (FuncStr.EndsWith("sprite", StringComparison.Ordinal))
            {
                string spriteStr = ContentBefor as String;
                string[] _params = spriteStr.Split(',');
                if (_params.Length == 3)
                {
                    GameSpriteAtlasManager.Instance.GetSpriteSync(_params[0], _params[1], _params[2], (Sprite s) =>
                    {
                        Content = s;
                        if (null != mOnSyncRst)
                        {
                            mOnSyncRst(s);
                            mOnSyncRst = null;
                        }
                    });
                }
                else
                {
                    LogFile.Warn("UIHandlerDataSync error => can't get sprite:" + spriteStr);
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
                        if (null != mOnSyncRst)
                        {
                            mOnSyncRst(material);
                            mOnSyncRst = null;
                        }
                    });
                }
                else
                {
                    LogFile.Warn("UIHandlerDataSync error => can't get Material:" + str);
                }
                return;
            } 
        }
    }
}
