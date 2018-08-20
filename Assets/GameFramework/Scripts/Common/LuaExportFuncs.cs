﻿using System.Collections;
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

        public void AddLuaBundle(string name)
        {
            GameLuaManager.Instance.AddLuaBundle(name);
        }

        public void AddLuaBundles(string[] names)
        {
            GameLuaManager.Instance.AddLuaBundles(names);
        }

        #endregion

        #region GameUIManager
        public static void ShowView(string asbName, string viewName)
        {
            GameUIManager.Instance.ShowView(asbName, viewName);
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
        #endregion GameSpriteAtlasManager

        #region Res 释放相关
        public void SetCurGroup(EnumResGroup e, string group)
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

        public void ClearGroup(EnumResGroup e, string group)
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
}