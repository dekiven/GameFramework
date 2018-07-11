﻿using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GameFramework
{
    public class GameConfig
    {

        //静态常量 begin------------------------------------------------------------
        public const string STR_RES_FOLDER = "BundleRes";
        public const string STR_LUA_FOLDER = "BundleSrc";
        /// <summary>
        /// assetbundle文件后缀名，请确保为全小写，
        /// 在获取assetbundle的文件名必须为小写，后缀大写可能造成不能读取资源
        /// </summary>
        public const string STR_ASB_EXT = ".unity3d";
        public const string STR_ASB_MANIFIST =
#if UNITY_EDITOR
        "pc";
#elif UNITY_IOS
        "ios";
#elif UNITY_ANDROID
        "and";
#elif UNITY_STANDALONE_OSX
        "mac";
#elif UNITY_STANDALONE_WIN
        "pc";
#else
        "pc";
#endif
        //静态常量 end==============================================================

        //单例模式 begin------------------------------------------------------------
        private static GameConfig sGameConfig = null;
        private static object sSyncObj = new object();

        private GameConfig()
        {
            //LoadConfig();
        }

        public static GameConfig Instance
        {
            get
            {
                if (sGameConfig == null)
                {
                    lock (sSyncObj)
                    {
                        if (sGameConfig == null)
                        {
                            sGameConfig = new GameConfig();
                        }
                    }
                }
                return sGameConfig;
            }
        }
        //单例模式 end================================================================


        //业务逻辑 begin--------------------------------------------------------------
        
        //配置是否在Assetbundle情况下使用luajit编译lua代码再打包
        public bool encodeLua = false;
#if UNITY_EDITOR
        //配置是否使用Assetbundle
        public bool useAsb = false;
#else
    // 注意，非编辑器模式下 useAsb只能为true，请勿修改，要在编辑器模式下不使用asb请修改上面的useAsb值
    public bool useAsb = true;
#endif



        private IDictionary<string, string> mConfDic = new Dictionary<string, string>();
        private string mConfigPath = Tools.PathCombine(Application.streamingAssetsPath, "gameConfig");

        public void LoadConfig()
        {
            Tools.CheckFileExists(mConfigPath, true);
            var config = Resources.Load<TextAsset>(mConfigPath);
            if (null != config)
            {
                var text = config.text;
                var lines = text.Split('\n');
                var regex = new Regex("\"(.*)\":\"(.*)\"");
                foreach (var item in lines)
                {
                    var m = regex.Match(item.Trim());
                    if (null != m)
                    {
                        mConfDic[m.Groups[0].ToString()] = m.Groups[1].ToString();
                    }

                }
            }
        }

        public string GetConf(string key)
        {
            string value = string.Empty;
            mConfDic.TryGetValue(key, out value);
            return value;
        }

        //业务逻辑 end================================================================


    }

}