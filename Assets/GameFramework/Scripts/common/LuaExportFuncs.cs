using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

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

        public static void LoadAsb(string abName, LuaFunction luaFunc)
        {
            GameResManager.Instance.LoadAsb(abName, null, luaFunc);
        }
        #endregion
        public static void TestDelegate(System.Action<float> action)
        {
            action(100);
        }
    }
}
