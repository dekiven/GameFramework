using System;
namespace GameFramework
{
    public class GameDefine
    {
        public const string STR_SPLIT_STR = "__;__";

        public const string STR_CONF_KEY_RES_VER_I = "GameResVerUint";

        //Lua层接收c#Event的全局函数名
        public const string STR_LUA_EVENT_FUNC = "OnEvent";
        public const string STR_EVENT_LUA_WWW_RST = "OnLuaWWWRst";

        #region GameConfig key
        internal static string STR_CUR_LANGUAGE = "CurLanguage";
        #region 音乐音效相关
        public const string STR_CONF_KEY_IS_BGM_PLAY = "IsBgmPlay";
        public const string STR_CONF_KEY_IS_SOUND_PLAY = "IsSoundPlay";
        public const string STR_CONF_KEY_BGM_V = "BgmVolume";
        public const string STR_CONF_KEY_SOUND_V = "SoundVolume";
        #endregion 音乐音效相关
        #endregion GameConfig key

        #region 事件相关
        public const string STR_EVENT_APP_QUIT = "OnApplicationQuit";
        public const string STR_EVENT_APP_FOCUS = "OnApplicationFocus";
        public const string STR_EVENT_APP_PAUSE = "OnApplicationPause";
        public const string STR_EVENT_LOG_EVENT = "LogEvent";
        #endregion
    }
}
