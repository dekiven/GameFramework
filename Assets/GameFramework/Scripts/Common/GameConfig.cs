using System.Collections;
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

        #region 私有常量
        private const string STR_KEY_IS_BGM = "IsPlayBgm";
        private const string STR_KEY_IS_SOUND = "IsPlaySound";
        private const string STR_KEY_BGM_V = "BgmVolume";
        private const string STR_KEY_SOUND_V = "SoundVolume";
        #endregion
        //静态常量 end==============================================================

        //业务逻辑 begin--------------------------------------------------------------
        public static bool IsPlayBgm { get { return GetBool(STR_KEY_IS_BGM); } internal set {SetBool(STR_KEY_IS_BGM, value);} }
        public static bool IsPlaySound { get { return GetBool(STR_KEY_IS_SOUND); } internal set { SetBool(STR_KEY_IS_SOUND, value); } }
        public static float BGMVolume { get { return GetFloat(STR_KEY_BGM_V); } internal set { SetFloat(STR_KEY_BGM_V, value); } }
        public static float SoundVolume { get { return GetFloat(STR_KEY_SOUND_V); } internal set { SetFloat(STR_KEY_SOUND_V, value); } }


        //配置是否在Assetbundle情况下使用luajit编译lua代码再打包
        public static bool encodeLua = false;


        //是否开启线程处理EventManager线程上的消息,TODO:使用线程有bug，先不使用Thread 通知
        public static bool progressThreadEvent = false;
#if UNITY_EDITOR
        //配置是否使用Assetbundle
        public static bool useAsb = false;
        // 检查更新
        public static bool checkUpdate = false;
#else
    // 注意，非编辑器模式下 useAsb只能为true，请勿修改，要在编辑器模式下不使用asb请修改上面的useAsb值
    public static bool useAsb = true;
    // 检查更新
    public static bool checkUpdate = true;
#endif

        public static bool GetBool(string key, bool def=false)
        {
            return PlayerPrefs.GetInt(key, def?1:0) > 0; 
        }

        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
        
        public static int GetInt(string key, int def=0)
        {
            return PlayerPrefs.GetInt(key, def);
        }

        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static float GetFloat(string key, float def=0f)
        {
            return PlayerPrefs.GetFloat(key, def);
        }

        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public static string GetStr(string key, string def = "")
        {
            return PlayerPrefs.GetString(key, def);
        }

        public static void SetStr(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }
        //业务逻辑 end================================================================


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
    }
}