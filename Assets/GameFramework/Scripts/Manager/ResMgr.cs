using System;
using LuaInterface;
using UnityEngine;

using UObj = UnityEngine.Object;

namespace GameFramework
{
    /// <summary>
    /// 管理 Material、TextAsset 等资源
    /// </summary>
    public class ResMgr : Singleton<ResMgr> , IResHandler<UObj>
    {
        #region private 属性
        private GameResHandler<UObj> mObjDict;
        #endregion

        /// <summary>
        /// 请勿直接调用构造函数，请使用 Instance 方法获取单例
        /// </summary>
        public ResMgr()
        {
            mObjDict = new GameResHandler<UObj>("common");
            //mObjDict.Suffix = ".spriteatlas";
            mObjDict.OnReleaseCallback = (ref UObj s) =>
            {
                Resources.UnloadAsset(s);
                s = null;
            };
            mObjDict.OnLoadCallbcak = (UObj obj, AsbInfo info) =>
            {
                //TODO:EventManager 通知
            };
        }

        public void GetAsync<T>(string asbName, string prefab, Action<T> action=null, LuaFunction lua=null) where T : UObj
        {
            Action<UObj> _a = null;
            if (null != action)
            {
                _a = (obj) => 
                {
                    if (null != action)
                    {
                        action(obj as T);
                    }
                };
            }
            mObjDict.GetAsync(asbName, prefab, _a, lua);
        }

        public void GetBytesAsync(string asbName, string prefab, Action<byte[]> action=null, LuaFunction lua=null)
        {
            mObjDict.GetAsync(asbName, prefab, (UObj obj) => 
            {
                TextAsset textAsset = obj as TextAsset;
                byte[] bytes;
                LuaByteBuffer buffer;
                if(null != textAsset)
                {
                    bytes = textAsset.bytes;
                }
                else
                {
                    bytes = new byte[]{};
                }
                buffer = new LuaByteBuffer(bytes);
                if(null != action)
                {
                    action(bytes);
                }
                if(null != lua)
                {
                    lua.Call(buffer);
                    lua.Dispose();
                    lua = null;
                }
            }, null);
        }

        public void GetStrAsync(string asbName, string prefab, Action<string> action=null, LuaFunction lua=null)
        {
            mObjDict.GetAsync(asbName, prefab, (UObj obj) =>
            {
                TextAsset textAsset = obj as TextAsset;
                string data = null;//string.Empty;
                if (null != textAsset)
                {
                    data = textAsset.text;
                }
                if (null != action)
                {
                    action(data);
                }
                if (null != lua)
                {
                    lua.Call(data);
                    lua.Dispose();
                    lua = null;
                }
            }, null);
        }

        #region IResHandler
        public void ClearGroup(string group)
        {
            mObjDict.ClearGroup(group);
        }

        public UObj Get(string asbName, string resName)
        {
            return mObjDict.Get(asbName, resName);
        }

        public void Load(string asbName, string resName)
        {
            mObjDict.Load(asbName, resName);
        }

        public void Load(string asbName, string[] resNames)
        {
            mObjDict.Load(asbName, resNames);
        }

        public void SetCurGroup(string group)
        {
            mObjDict.CurGroup = group;
        }
        #endregion IResHandler

    }
}
