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
            bundleName = bundleName.Replace(GameConfig.STR_ASB_EXT, "");

            if(HasBundle(bundleName))
            {
                return;
            }

            if (File.Exists(url))
            {
                //var bytes = File.ReadAllBytes(url);
                AssetBundle bundle = AssetBundle.LoadFromFile(url);
                if (bundle != null)
                {
                    base.AddSearchBundle(bundleName.ToLower(), bundle);
                }
            }
            else
            {
                LogFile.Error("{0} do not exists", url);
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