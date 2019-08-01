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
            PrefabMgr.Instance.GetAsync(abName, name, null, luaFunc);
        }

        //public static void LoadPrefab(string abName, string[] names, LuaFunction luaFunc)
        //{
        //    ResManager.Instance.LoadRes<GameObject>(abName, names, null, luaFunc);
        //}

        public static void LoadString(string abName, string name, LuaFunction luaFunc)
        {
            ResMgr.Instance.GetStrAsync(abName, name, null, luaFunc);
        }

        //public static void LoadString(string abName, string[] names, LuaFunction luaFunc)
        //{
        //    ResManager.Instance.LoadRes<TextAsset>(abName, names, null, luaFunc);
        //}

        public static void LoadBytes(string abName, string name, LuaFunction luaFunc)
        {
            ResMgr.Instance.GetBytesAsync(abName, name, null, luaFunc);
        }

        //public static void LoadBytes(string abName, string[] names, LuaFunction luaFunc)
        //{
        //    ResManager.Instance.LoadBytes(abName, names, luaFunc);
        //}

        public static void LoadScene(string abName, string scenenName, bool sync, bool add, LuaFunction luaFunction)
        {
            SceneMgr.Instance.LoadScene(abName, scenenName, sync, add, null, luaFunction);
        }
        #endregion

        #region GameLuaManager

        public static void AddLuaBundle(string name)
        {
            LuaMgr.Instance.AddLuaBundle(name);
        }

        public static void AddLuaBundles(string[] names)
        {
            LuaMgr.Instance.AddLuaBundles(names);
        }

        #endregion

        #region GameUIManager
        public static void ShowView(string asbName, string viewName, LuaTable table)
        {
            UIMgr.Instance.ShowView(asbName, viewName, table);
        }

        public static void PopView()
        {
            UIMgr.Instance.PopView();
        }
        #endregion GameUIManager

        #region GameSpriteAtlasManager
        public static void GetAtlasAsync(string asbName, string atlasName, LuaFunction luaCall)
        {
            SpriteAtlasMgr.Instance.GetAtlasAsync(asbName, atlasName, null, luaCall);
        }

        public static void GetSpriteAsync(string asbName, string atlasName, string spriteName, LuaFunction luaCall)
        {
            SpriteAtlasMgr.Instance.GetSpriteAsync(asbName, atlasName, spriteName, null, luaCall);
        }

        #endregion GameSpriteAtlasManager

        #region GameSoundManager
        public static void LoadAudios(string asbName, string names)
        {
            SoundMgr.Instance.LoadAudios(asbName, names.Split(','));
        }

        public static void PlayBgm(string asbName, string audioName, float fadeOutTime = 0f)
        {
            SoundMgr.Instance.PlayBgm(asbName, audioName, fadeOutTime);
        }

        public static void StopBgm(float fadeOutTime = 0f)
        {
            SoundMgr.Instance.StopBgm(fadeOutTime);
        }

        public static void PauseBgm()
        {
            SoundMgr.Instance.PauseBgm();
        }

        public static void ResumeBgm()
        {
            SoundMgr.Instance.ResumeBgm();
        }

        public static void PlaySound(string asbName, string audioName)
        {
            SoundMgr.Instance.PlaySound(asbName, audioName);
        }

        public static void StopAllSound()
        {
            SoundMgr.Instance.StopAllSound();
        }
        #endregion GameSoundManager


        #region Res 释放相关
        public static void SetCurGroup(string group, EnumResGroup e=EnumResGroup.All)
        {
            switch(e)
            {
                case EnumResGroup.UI:
                    UIMgr.Instance.SetCurGroup(group);
                    break;
                case EnumResGroup.Audio:
                    SoundMgr.Instance.SetCurGroup(group);
                    break;
                case EnumResGroup.SpriteAtlas:
                    SoundMgr.Instance.SetCurGroup(group);
                    break;
                case EnumResGroup.All :
                    UIMgr.Instance.SetCurGroup(group);
                    SoundMgr.Instance.SetCurGroup(group);
                    SoundMgr.Instance.SetCurGroup(group);
                    break;
            }
        }

        public static void ClearGroup(string group, EnumResGroup e=EnumResGroup.All)
        {
            switch (e)
            {
                case EnumResGroup.UI:
                    UIMgr.Instance.ClearGroup(group);
                    break;
                case EnumResGroup.Audio:
                    SoundMgr.Instance.ClearGroup(group);
                    break;
                case EnumResGroup.SpriteAtlas:
                    SoundMgr.Instance.ClearGroup(group);
                    break;
                case EnumResGroup.All:
                    UIMgr.Instance.ClearGroup(group);
                    SoundMgr.Instance.ClearGroup(group);
                    SoundMgr.Instance.ClearGroup(group);
                    break;
            }
        }
        #endregion Res 释放相关

        #region TimeOutWWW 相关
        public static void DownloadFile(LuaTable info, LuaFunction call, float timeoutSec, int retry=3) 
        {
            var www = WWWTO.DownloadFile(new WWWInfo(info), null, call);
            www.TimeoutSec = timeoutSec;
            www.Retry = retry;
            www.Start();
        }
        public static void DownloadFiles(LuaTable info, LuaFunction call, float timeoutSec=3f, int retry = 3)
        {
            var www = WWWTO.DownloadFiles(WWWInfo.GetListByLua(info), null, call);
            www.TimeoutSec = timeoutSec;
            www.Retry = retry;
            www.Start();
        }
        public static void UploadFile(LuaTable info, LuaFunction call, float timeoutSec=3f, int retry = 3)
        {
            var www = WWWTO.DownloadFiles(WWWInfo.GetListByLua(info), null, call);
            www.TimeoutSec = timeoutSec;
            www.Retry = retry;
            www.Start();
        }
        public static void UploadFiles(LuaTable info, LuaFunction call, float timeoutSec=3f, int retry = 3)
        {
            var www = WWWTO.UploadFiles(WWWInfo.GetListByLua(info), null, call);
            www.TimeoutSec = timeoutSec;
            www.Retry = retry;
            www.Start();
        }
        public static void RequestUrl(string url, LuaFunction call, float timeoutSec=3f, int retry = 3)
        {
            var www = WWWTO.RequestUrl(url, null, call);
            www.TimeoutSec = timeoutSec;
            www.Retry = retry;
            www.Start();
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
