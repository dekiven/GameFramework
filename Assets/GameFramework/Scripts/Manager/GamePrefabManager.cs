using System;
using LuaInterface;
using UnityEngine;

using UObj = UnityEngine.Object;

namespace GameFramework
{
    public class GamePrefabManager : Singleton<GamePrefabManager>, IResHandler<GameObject>
    {
        #region private 属性
        private GameResHandler<GameObject> mObjDict;
        #endregion

        /// <summary>
        /// 请勿直接调用构造函数，请使用 Instance 方法获取单例
        /// </summary>
        public GamePrefabManager()
        {
            mObjDict = new GameResHandler<GameObject>("common");
            mObjDict.Suffix = ".prefab";
            mObjDict.OnReleaseCallback = (ref GameObject s) =>
            {
                //Resources.UnloadAsset(s);
                //TODO:是否是直接 destroy 待验证
                UObj.Destroy(s);
                s = null;
            };

            mObjDict.OnLoadCallbcak = (GameObject obj, AsbInfo info) =>
            {
                //TODO:EventManager 通知
            };
        }

        public void GetAsync(string asbName, string prefab, Action<GameObject> action, LuaFunction lua)
        {
            mObjDict.GetAsync(asbName, prefab, action, lua);
        }

        #region IResHandler
        public void ClearGroup(string group)
        {
            mObjDict.ClearGroup(group);
        }

        public GameObject Get(string asbName, string resName)
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
