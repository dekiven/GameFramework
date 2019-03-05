using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LuaInterface;
using UnityEngine;

namespace GameFramework
{
    public class LanguageManager
    {
        public const string LANGUAGE_CHINESE = "cn";
        public const string LANGUAGE_ENGLISH = "en";
        public const string LANGUAGE_CHINESE_TRAD = "cnt";
        public const string LANGUAGE_JAPANESE = "jp";
        public const string LANGUAGE_FRENCH = "fr";
        public const string LANGUAGE_GERMAN = "ge";
        public const string LANGUAGE_ITALY = "it";
        public const string LANGUAGE_KOREA = "kr";
        public const string LANGUAGE_RUSSIA = "ru";
        public const string LANGUAGE_SPANISH = "sp";

        private static Dictionary<string, string> sDic = new Dictionary<string, string>();
        private static List<string> sLanguages = new List<string>();
        /// <summary>
        /// 配置文件分隔符
        /// </summary>
        public static string sStrSplit = GameDefine.STR_SPLIT_STR;
        private static string STR_BASE_ASB_PATH = "BaseLanguage";

        public static void Init(Action<bool> action)
        {
            GameResManager.Instance.GetStrAsync(STR_BASE_ASB_PATH, "config.bytes", (text) =>
            {
                // TextAsset text = obj as TextAsset;
                if (string.IsNullOrEmpty(text) && null != action)
                {
                    action(false);
                    LogFile.Error("语言配置文件不存在");
                    return;
                }
                SetValidLanguages(text);
                SetLanguage(GetCurLanguage(), action, null);
            });

        }

        /// <summary>
        /// 获取 c#处定义的多语言文字
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="key">Key.</param>
        public static string GetStr(string key)
        {
            key.Replace("\n", "\\n");
            string ret;
            if(sDic.TryGetValue(key, out ret))
            {
                return ret;
            }
            return key.Replace("\\n", "\n");
        }

        //public static string SetLanguage(int language, Action<bool> action, LuaFunction function)
        //{
        //    return SetLanguage((SystemLanguage)language, action, function);
        //}

        public static void SetLanguage(string language, Action<bool> action, LuaFunction function)
        {
            if(sLanguages.Contains(language))
            {
                GameConfig.SetStr(GameDefine.STR_CUR_LANGUAGE, language);
                GameResManager.Instance.GetStrAsync(STR_BASE_ASB_PATH, language + ".bytes", (text) =>
                {
                    // TextAsset text = obj as TextAsset;
                    bool ret = text != null;
                    if (ret)
                    {
                        resetLanguageData(text);
                    }
                    else
                    {
                        LogFile.Error("基础语言配置不存在：" + STR_BASE_ASB_PATH + "/" + language + ".bytes");
                    }

                    if (null != action)
                    {
                        action(ret);
                    }

                    if (null != function)
                    {
                        function.Call(ret);
                        function.Dispose();
                        function = null;
                    }

                    ResManager.Instance.UnloadAssetBundle(STR_BASE_ASB_PATH);
                });
            }
            else
            {
                LogFile.Warn("设置语言不存在：" + language + ",请初始化支持的语言");
                if (null != action)
                {
                    action(false);
                }

                if (null != function)
                {
                    function.Call(false);
                    function.Dispose();
                    function = null;
                }
            }

        }

        public static void SetValidLanguages(List<string> languages)
        {
            sLanguages = languages;
        }

        public static void SetValidLanguages(string languages)
        {
            sLanguages = new List<string>(languages.Split(','));
        }

        public static string GetValidLanguages()
        {
            return string.Join(",", sLanguages.ToArray());
        }

        public static string GetCurLanguage()
        {
            return GameConfig.GetStr(GameDefine.STR_CUR_LANGUAGE, getValidLanguageCode((int)Application.systemLanguage));
        }
        #region 私有方法
        private static string getLanguageCode(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Afrikaans:
                case SystemLanguage.Arabic:
                case SystemLanguage.Basque:
                case SystemLanguage.Belarusian:
                case SystemLanguage.Bulgarian:
                case SystemLanguage.Catalan:
                    return LANGUAGE_ENGLISH;
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    return LANGUAGE_CHINESE;
                case SystemLanguage.ChineseTraditional:
                    return LANGUAGE_CHINESE_TRAD;
                case SystemLanguage.Czech:
                case SystemLanguage.Danish:
                case SystemLanguage.Dutch:
                case SystemLanguage.English:
                case SystemLanguage.Estonian:
                case SystemLanguage.Faroese:
                case SystemLanguage.Finnish:
                    return LANGUAGE_ENGLISH;
                case SystemLanguage.French:
                    return LANGUAGE_FRENCH;
                case SystemLanguage.German:
                    return LANGUAGE_GERMAN;
                case SystemLanguage.Greek:
                case SystemLanguage.Hebrew:
                case SystemLanguage.Icelandic:
                case SystemLanguage.Indonesian:
                    return LANGUAGE_ENGLISH;
                case SystemLanguage.Italian:
                    return LANGUAGE_ITALY;
                case SystemLanguage.Japanese:
                    return LANGUAGE_JAPANESE;
                case SystemLanguage.Korean:
                    return LANGUAGE_KOREA;
                case SystemLanguage.Latvian:
                case SystemLanguage.Lithuanian:
                case SystemLanguage.Norwegian:
                case SystemLanguage.Polish:
                case SystemLanguage.Portuguese:
                case SystemLanguage.Romanian:
                    return LANGUAGE_ENGLISH;
                case SystemLanguage.Russian:
                    return LANGUAGE_RUSSIA;
                case SystemLanguage.SerboCroatian:
                case SystemLanguage.Slovak:
                case SystemLanguage.Slovenian:
                    return LANGUAGE_ENGLISH;
                case SystemLanguage.Spanish:
                    return LANGUAGE_SPANISH;
                case SystemLanguage.Swedish:
                case SystemLanguage.Thai:
                case SystemLanguage.Turkish:
                case SystemLanguage.Ukrainian:
                case SystemLanguage.Vietnamese:
                case SystemLanguage.Unknown:
                    return LANGUAGE_ENGLISH;
            }
            return LANGUAGE_CHINESE;
        }

        private static void resetLanguageData(string text)
        {
            sDic.Clear();
            //TODO:
            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string[] kv = Regex.Split(lines[i], sStrSplit, RegexOptions.IgnoreCase);
                if (kv.Length == 2)
                {
                    sDic[kv[0].Replace("\n", "\\n")] = kv[1].Replace("\\n", "\n");
                }
                else
                {
                    LogFile.Warn("resetLanguageData error ----> line {0}: {1}", i, lines[i]);
                }
            }
        }

        private static string getValidLanguageCode(int language)
        {
            return getValidLanguageCode((SystemLanguage)language);
        }

        private static string getValidLanguageCode(SystemLanguage language)
        {
            string code = getLanguageCode(language);
            if (sLanguages.Contains(code))
            {
                return code;
            }
            return LANGUAGE_CHINESE;
        }
        #endregion 私有方法
    }
}
