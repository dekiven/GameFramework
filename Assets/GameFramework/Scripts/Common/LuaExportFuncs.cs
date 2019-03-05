using System;
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
        public static void LoadPrefab(string abName, string name, LuaFunction luaFunc)
        {
            GamePrefabManager.Instance.GetAsync(abName, name, null, luaFunc);
        }

        //public static void LoadPrefab(string abName, string[] names, LuaFunction luaFunc)
        //{
        //    ResManager.Instance.LoadRes<GameObject>(abName, names, null, luaFunc);
        //}

        public static void LoadString(string abName, string name, LuaFunction luaFunc)
        {
            GameResManager.Instance.GetStrAsync(abName, name, null, luaFunc);
        }

        //public static void LoadString(string abName, string[] names, LuaFunction luaFunc)
        //{
        //    ResManager.Instance.LoadRes<TextAsset>(abName, names, null, luaFunc);
        //}

        public static void LoadBytes(string abName, string name, LuaFunction luaFunc)
        {
            GameResManager.Instance.GetBytesAsync(abName, name, null, luaFunc);
        }

        //public static void LoadBytes(string abName, string[] names, LuaFunction luaFunc)
        //{
        //    ResManager.Instance.LoadBytes(abName, names, luaFunc);
        //}

        public static void LoadScene(string abName, string scenenName, bool sync, bool add, LuaFunction luaFunction)
        {
            GameSceneManager.Instance.LoadScene(abName, scenenName, sync, add, null, luaFunction);
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
        public static void GetAtlasAsync(string asbName, string atlasName, LuaFunction luaCall)
        {
            GameSpriteAtlasManager.Instance.GetAtlasAsync(asbName, atlasName, null, luaCall);
        }

        public static void GetSpriteAsync(string asbName, string atlasName, string spriteName, LuaFunction luaCall)
        {
            GameSpriteAtlasManager.Instance.GetSpriteAsync(asbName, atlasName, spriteName, null, luaCall);
        }

        #endregion GameSpriteAtlasManager

        #region GameSoundManager
        public static void LoadAudios(string asbName, string names)
        {
            GameSoundManager.Instance.LoadAudios(asbName, names.Split(','));
        }

        public static void PlayBgm(string asbName, string audioName, float fadeOutTime = 0f)
        {
            GameSoundManager.Instance.PlayBgm(asbName, audioName, fadeOutTime);
        }

        public static void StopBgm(float fadeOutTime = 0f)
        {
            GameSoundManager.Instance.StopBgm(fadeOutTime);
        }

        public static void PauseBgm()
        {
            GameSoundManager.Instance.PauseBgm();
        }

        public static void ResumeBgm()
        {
            GameSoundManager.Instance.ResumeBgm();
        }

        public static void PlaySound(string asbName, string audioName)
        {
            GameSoundManager.Instance.PlaySound(asbName, audioName);
        }

        public static void StopAllSound()
        {
            GameSoundManager.Instance.StopAllSound();
        }
        #endregion GameSoundManager


        #region Res 释放相关
        public static void SetCurGroup(string group, EnumResGroup e=EnumResGroup.All)
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
                case EnumResGroup.All :
                    GameUIManager.Instance.SetCurGroup(group);
                    GameSoundManager.Instance.SetCurGroup(group);
                    GameSoundManager.Instance.SetCurGroup(group);
                    break;
            }
        }

        public static void ClearGroup(string group, EnumResGroup e=EnumResGroup.All)
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
                case EnumResGroup.All:
                    GameUIManager.Instance.ClearGroup(group);
                    GameSoundManager.Instance.ClearGroup(group);
                    GameSoundManager.Instance.ClearGroup(group);
                    break;
            }
        }
        #endregion Res 释放相关

        #region TimeOutWWW 相关
        public static void DownloadFile(string noticeKey, LuaTable info, float timeoutSec=1f) 
        {
            TimeOutWWW t = GameManager.Instance.gameObject.AddComponent<TimeOutWWW>();
            t.DownloadFile(noticeKey, new WWWInfo(info), timeoutSec, GameManager.Instance.OnLuaWWWRst, null);
        }
        public static void DownloadFiles(string noticeKey, LuaTable infos, float timeoutSec= 1f) 
        {
            TimeOutWWW t = GameManager.Instance.gameObject.AddComponent<TimeOutWWW>();
            t.DownloadFiles(noticeKey, WWWInfo.GetListByLua(infos), timeoutSec, GameManager.Instance.OnLuaWWWRst, null);
        }
        public static void UploadFile(string noticeKey, LuaTable info, float timeoutSec= 1f) 
        {
            TimeOutWWW t = GameManager.Instance.gameObject.AddComponent<TimeOutWWW>();
            t.UploadFile(noticeKey, new WWWInfo(info), timeoutSec, GameManager.Instance.OnLuaWWWRst, null);
        }
        public static void UploadFiles(string noticeKey, LuaTable infos, float timeoutSec= 1f) 
        {
            TimeOutWWW t = GameManager.Instance.gameObject.AddComponent<TimeOutWWW>();
            t.UploadFiles(noticeKey, WWWUploadInfo.GetListByLua(infos), timeoutSec, GameManager.Instance.OnLuaWWWRst, null);
        }
        public static void RequestUrl(string url, LuaFunction lua, float timeoutSec= 1f) 
        {
            TimeOutWWW t = GameManager.Instance.gameObject.AddComponent<TimeOutWWW>();
            t.RequestUrl(url, timeoutSec, null, lua);
        }
        #endregion TimeOutWWW 相关

        #region LanguageManager 相关
        public static string GetStr(string key)
        {
            return LanguageManager.GetStr(key);
        }

        public static string GetLanguage()
        {
            return LanguageManager.GetCurLanguage();
        }

        public static void SetLanguage(string language, LuaFunction function)
        {
            LanguageManager.SetLanguage(language, null, function);
        }

        public static string GetValidLanguages()
        {
            return LanguageManager.GetValidLanguages();
        }
        #endregion LanguageManager 相关

        #region Test
        public static void TestDelegate(System.Action<float> action)
        {
            action(100);
        }
        #endregion
    }

    public enum EnumResGroup
    {
        All,
        UI,
        Audio,
        SpriteAtlas,
    }

    //正则
    //public static (\S+) (\S+)\((.*)\) 
    //-- \1 \2 (\3)\n\2 = luaExp.\2
}
