using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameFramework
{
    public class GameLuaLoader : LuaFileUtils
    {
        // Use this for initialization
        public GameLuaLoader()
        {
            instance = this;
            beZip = GameConfig.useAsb;
        }

        /// <summary>
        /// 添加打入Lua代码的AssetBundle
        /// </summary>
        /// <param name="bundle"></param>
        public void AddBundle(string bundleName)
        {
            string url = Tools.GetLuaAsbPath(bundleName);
#if UNITY_ANDROID
            //这是个大坑，要注意，没有.Replace("!/", "!")是读不出来的，开始我以为是雨松手滑写错
            url = url.Replace("jar:file://", "").Replace("!/", "!");
#endif
            bundleName = bundleName.Replace(GameConfig.STR_ASB_EXT, "");

            if(HasBundle(bundleName))
            {
                return;
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(url);
            if (bundle != null)
            {
                base.AddSearchBundle(bundleName.ToLower(), bundle);
            }
            else
            {
                LogFile.Error("AddBundle: error [" + url + "] do not exists");
            }
        }

        /// <summary>
        /// 当LuaVM加载Lua文件的时候，这里就会被调用，
        /// 用户可以自定义加载行为，只要返回byte[]即可。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public override byte[] ReadFile(string fileName)
        {
            return base.ReadFile(fileName);
        }
    }

}