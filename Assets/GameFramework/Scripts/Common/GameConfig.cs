using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        /// <summary>
        /// 该文件不会热更，用来定义一打包就定死的参数，如：是否开启日志按钮
        /// </summary>
        private const string FILE_NAME = "gc.bytes";
        //静态常量 end==============================================================

        //从配置文件加载的配置，没吃启动前从配置文件读取
        public static bool HasDebugView = true;

        //必须要的基础资源，如：DebugView、C#部分language等
        public const string BasicRes = "BasicRes";

        //public static 

        //业务逻辑 begin--------------------------------------------------------------
        public static bool IsPlayBgm { get { return GetBool(GameDefine.STR_CONF_KEY_IS_BGM_PLAY); } internal set { SetBool(GameDefine.STR_CONF_KEY_IS_BGM_PLAY, value); } }
        public static bool IsPlaySound { get { return GetBool(GameDefine.STR_CONF_KEY_IS_SOUND_PLAY); } internal set { SetBool(GameDefine.STR_CONF_KEY_IS_SOUND_PLAY, value); } }
        public static float BGMVolume { get { return GetFloat(GameDefine.STR_CONF_KEY_BGM_V); } internal set { SetFloat(GameDefine.STR_CONF_KEY_BGM_V, value); } }
        public static float SoundVolume { get { return GetFloat(GameDefine.STR_CONF_KEY_SOUND_V); } internal set { SetFloat(GameDefine.STR_CONF_KEY_SOUND_V, value); } }

        //配置是否在Assetbundle情况下使用luajit编译lua代码再打包
        public static bool EncodeLua = false;


        //是否开启线程处理EventManager线程上的消息
        public static bool UseThreadEvent = true;
#if UNITY_EDITOR
        //配置是否使用Assetbundle
        public static bool UseAsb = false;
        // 检查更新
        public static bool CheckUpdate = false;
#else
    // 注意，非编辑器模式下 useAsb只能为true，请勿修改，要在编辑器模式下使用asb请修改上面的useAsb值
    public static bool UseAsb = true;
    // 检查更新
    public static bool CheckUpdate = true;
#endif
        /// <summary>
        /// 检测更新失败是否直接进入游戏
        /// </summary>
        public static bool StartWhileCheckError = false;

        public static bool GetBool(string key, bool def = false)
        {
            return PlayerPrefs.GetInt(key, def ? 1 : 0) > 0;
        }

        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public static int GetInt(string key, int def = 0)
        {
            return PlayerPrefs.GetInt(key, def);
        }

        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static float GetFloat(string key, float def = 0f)
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

        public static void Load()
        {
            TextAsset text = Resources.Load<TextAsset>(FILE_NAME.Split('.')[0]);
            if (null == text)
            {
                return;
            }
            if(string.IsNullOrEmpty(text.text))
            {
                return;
            }
            string[] values = text.text.Split('|');
            for (int i = 0; i < values.Length; i++)
            {
                switch(i)
                {
                    case 0:
                        HasDebugView = values[i].ToLower().Equals("true"); 
                        break;
                }
            }
        }

        public static void Save()
        {
#if UNITY_EDITOR
            string p = Tools.PathCombine(Application.dataPath+"/Resources", FILE_NAME);
            Tools.CheckFileExists(p, true);
            FileStream fs = new FileStream(p, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            sw.Write(HasDebugView+"|");
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        //业务逻辑 end================================================================


        public const string STR_ASB_MANIFIST =
#if UNITY_EDITOR_OSX
        // mac editor使用pc asb
        "mac";
#elif UNITY_EDITOR_WIN
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