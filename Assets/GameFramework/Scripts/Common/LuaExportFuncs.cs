using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
//using UnityEngine.SceneManagement;

using UObj = UnityEngine.Object;

namespace GameFramework
{
    /// <summary>
    /// 此类用来将单例组件的方法导出到lua，避免生成多余代码,减少lua注册的函数
    /// </summary>
    public class LuaExportFuncs
    {
        #region GameResManager
        public static void LoadGameObj(string abName, string name, LuaFunction luaFunc)
        {
            GameResManager.Instance.LoadRes<GameObject>(abName, name, null, luaFunc);
        }

        public static void LoadGameObj(string abName, string[] names, LuaFunction luaFunc)
        {
            GameResManager.Instance.LoadRes<GameObject>(abName, names, null, luaFunc);
        }

        public static void LoadTextAsset(string abName, string name, LuaFunction luaFunc)
        {
            GameResManager.Instance.LoadRes<TextAsset>(abName, name, null, luaFunc);
        }

        public static void LoadTextAsset(string abName, string[] names, LuaFunction luaFunc)
        {
            GameResManager.Instance.LoadRes<TextAsset>(abName, names, null, luaFunc);
        }

        public static void LoadTextAssetBytes(string abName, string name, LuaFunction luaFunc)
        {
            GameResManager.Instance.LoadTextAssetBytes(abName, new string[] { name, }, luaFunc);
        }

        public static void LoadTextAssetBytes(string abName, string[] names, LuaFunction luaFunc)
        {
            GameResManager.Instance.LoadTextAssetBytes(abName, names, luaFunc);
        }

        public static void LoadScene(string abName, string scenenName, bool sync, bool add, LuaFunction luaFunction)
        {
            GameResManager.Instance.LoadScene(abName, scenenName, sync, add, null, luaFunction);
        }

        public static void CountAsbGroup(string asbName, string group)
        {
            GameResManager.Instance.CountAsbGroup(asbName, group);
        }

        public static void UnloadAsbGroup(string group)
        {
            GameResManager.Instance.UnloadAsbGroup(group);
        }
        #endregion

        #region GameLuaManager

        public static void AddLuaBundle(string name)
        {
            GameLuaManager.Instance.AddLuaBundle(name);
        }

        public static void AddLuaBundles(string[] names)
        {
            GameLuaManager.Instance.AddLuaBundles(names);
        }

        #endregion

        #region GameUIManager
        public static void ShowView(string asbName, string viewName, LuaTable table)
        {
            GameUIManager.Instance.ShowView(asbName, viewName, table);
        }

        public static void PopView()
        {
            GameUIManager.Instance.PopView();
        }
        #endregion GameUIManager

        #region GameSpriteAtlasManager
        public static void GetAtlasSync(string asbName, string atlasName, LuaFunction luaCall)
        {
            GameSpriteAtlasManager.Instance.GetAtlasSync(asbName, atlasName, null, luaCall);
        }

        public static void GetSpriteSync(string asbName, string atlasName, string spriteName, LuaFunction luaCall)
        {
            GameSpriteAtlasManager.Instance.GetSpriteSync(asbName, atlasName, spriteName, null, luaCall);
        }

        #endregion GameSpriteAtlasManager

        #region Res 释放相关
        public static void SetCurGroup(EnumResGroup e, string group)
        {
            switch(e)
            {
                case EnumResGroup.UI:
                    GameUIManager.Instance.SetCurGroup(group);
                    break;
                case EnumResGroup.Audio:
                    GameSoundManager.Instance.SetCurGroup(group);
                    break;
                case EnumResGroup.SpriteAtlas:
                    GameSoundManager.Instance.SetCurGroup(group);
                    break;
            }
        }

        public static void ClearGroup(EnumResGroup e, string group)
        {
            switch (e)
            {
                case EnumResGroup.UI:
                    GameUIManager.Instance.ClearGroup(group);
                    break;
                case EnumResGroup.Audio:
                    GameSoundManager.Instance.ClearGroup(group);
                    break;
                case EnumResGroup.SpriteAtlas:
                    GameSoundManager.Instance.ClearGroup(group);
                    break;
            }
        }
        #endregion Res 释放相关

        #region TimeOutWWW 相关
        public static void DownloadFile(string noticeKey, LuaTable info, float timeoutSec=1f) 
        {
            TimeOutWWW t = GameManager.Instance.gameObject.AddComponent<TimeOutWWW>();
            t.DownloadFile(noticeKey, new WWWInfo(info), timeoutSec, GameManager.Instance.HandleWWWRstDel, null);
        }
        public static void DownloadFiles(string noticeKey, LuaTable infos, float timeoutSec= 1f) 
        {
            TimeOutWWW t = GameManager.Instance.gameObject.AddComponent<TimeOutWWW>();
            t.DownloadFiles(noticeKey, WWWInfo.GetListByLua(infos), timeoutSec, GameManager.Instance.HandleWWWRstDel, null);
        }
        public static void UploadFile(string noticeKey, LuaTable info, float timeoutSec= 1f) 
        {
            TimeOutWWW t = GameManager.Instance.gameObject.AddComponent<TimeOutWWW>();
            t.UploadFile(noticeKey, new WWWInfo(info), timeoutSec, GameManager.Instance.HandleWWWRstDel, null);
        }
        public static void UploadFiles(string noticeKey, LuaTable infos, float timeoutSec= 1f) 
        {
            TimeOutWWW t = GameManager.Instance.gameObject.AddComponent<TimeOutWWW>();
            t.UploadFiles(noticeKey, WWWUploadInfo.GetListByLua(infos), timeoutSec, GameManager.Instance.HandleWWWRstDel, null);
        }
        public static void RequestUrl(string url, LuaFunction lua, float timeoutSec= 1f) 
        {
            TimeOutWWW t = GameManager.Instance.gameObject.AddComponent<TimeOutWWW>();
            t.RequestUrl(url, timeoutSec, null, lua);
        }
        #endregion TimeOutWWW 相关

        #region Test
        public static void TestDelegate(System.Action<float> action)
        {
            action(100);
        }
        #endregion
    }

    public enum EnumResGroup
    {
        UI,
        Audio,
        SpriteAtlas,
    }

    //正则
    //public static (\S+) (\S+)\((.*)\) 
    //-- \1 \2 (\3)\n\2 = luaExp.\2
}
