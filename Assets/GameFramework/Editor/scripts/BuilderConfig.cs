using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    public class BuilderConfig
    {
        //public static string STR_ASB_EXT = ".asb";
        public static HashSet<string> SET_SKIP_EXTS = new HashSet<string>() { ".meta", ".DS_Store", ".cs"};
        const string configPath = "Assets/GameFramework/Editor/conifg.cfg";

        IDictionary<string, string> mConfDic = new Dictionary<string, string>();

        public BuilderConfig()
        {
            LoadConfig();
        }

        public string LoadPath
        {
            get
            {
                string v = GetConf("LoadPath");
                if (string.IsNullOrEmpty(v))
                {
                    return Tools.GetResPath();
                }
                return v;
            }

            set
            {
                setConfig("LoadPath", value);
                saveConfig();
            }
        }

        public string ExportPath
        {
            get
            {
                string v = GetConf("ExportPath");
                if (string.IsNullOrEmpty(v))
                {
                    return Application.dataPath.Replace("Assets", "") + "AssetBundles";
                }
                return v;
            }

            set
            {
                setConfig("ExportPath", value);
                saveConfig();
            }
        }

        public BuildAssetBundleOptions options
        {
            get
            {
                BuildAssetBundleOptions bbo = BuildAssetBundleOptions.None;
                string v = GetConf("BuildAssetBundleOptions");
                if (!string.IsNullOrEmpty(v))
                {
                    bbo = (BuildAssetBundleOptions)Enum.Parse(typeof(BuildAssetBundleOptions), v);
                }
                return bbo;
            }

            set
            {
                setConfig("BuildAssetBundleOptions", value.ToString());
                saveConfig();
            }
        }

        public BuildTarget target
        {
            get
            {
                BuildTarget t = BuildTarget.StandaloneWindows;
                string v = GetConf("BuildTarget");
                if (!string.IsNullOrEmpty(v))
                {
                    //return Application.dataPath.Replace("Assets", "") + "Bundles";
                    t = (BuildTarget)Enum.Parse(typeof(BuildTarget), v);
                }
                return t;
            }

            set
            {
                setConfig("BuildTarget", value.ToString());
                saveConfig();
            }
        }

        public void LoadConfig()
        {
            //TODO:dekiven
            if (!File.Exists(configPath))
            {
                File.Create(configPath).Close();
            }
            if (File.Exists(configPath))
            {
                var lines = File.ReadAllLines(configPath);
                var regex = new Regex("\"(.*)\":\"(.*)\"");
                foreach (var l in lines)
                {
                    var m = regex.Match(l.Trim());
                    if (null != m)
                    {
                        setConfig(m.Groups[1].ToString(), m.Groups[2].ToString());
                    }
                }
            }
        }

        private void saveConfig()
        {
            List<string> lines = new List<string>();
            foreach (var item in mConfDic)
            {
                lines.Add(string.Format("\"{0}\":\"{1}\"", item.Key, item.Value));
            }

            File.WriteAllLines(configPath, lines.ToArray());
        }

        private void setConfig(string key, string value)
        {
            mConfDic[key] = value;
        }

        public string GetConf(string key)
        {
            string value = string.Empty;
            mConfDic.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// 判断是否是可以打包到assetbundle的资源
        /// </summary>
        /// <returns><c>true</c>, if res file was ised, <c>false</c> otherwise.</returns>
        /// <param name="fileName">File name.</param>
        public static bool IsResFile(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            if (!BuilderConfig.SET_SKIP_EXTS.Contains(ext) && fileName != ".DS_Store")
            {
                return true;
            }
            return false;
        }
    }

}