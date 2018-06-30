using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using LuaFramework;

namespace GameFramework
{
    public class AsbBuilder
    {
        public static string sTempLuaDir = Tools.PathCombine(Application.dataPath, "Lua");

        public static void BuildAsb(string path, AssetBundleBuild[] builds, BuildAssetBundleOptions opt = BuildAssetBundleOptions.None, BuildTarget target = BuildTarget.StandaloneWindows)
        {
            string absFolder = GetAsbFolderByTarget(target);
            if (!path.EndsWith("/" + absFolder))
            {
                path = Tools.PathCombine(path, absFolder);
            }
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarningFormat("[{0}] dose not exists!", path);
                return;
            }
            Tools.CheckDirExists(path, true);
            BuildPipeline.BuildAssetBundles(path, builds, BuildAssetBundleOptions.None, target);
            AssetDatabase.Refresh();
        }

        public static AssetBundleBuild GenBundleBuild(string assetName, string[] files)
        {
            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleName = assetName;
            abb.assetNames = files;
            return abb;
        }

        public static AssetBundleBuild GenBuildByDir(string dir, string relative2, string pattern, bool searchAll = true)
        {
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                string[] files = Directory.GetFiles(dir, pattern, searchAll ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                List<string> list = new List<string>();
                for (int i = 0; i < files.Length; i++)
                {
                    string f = files[i];
                    string ext = Path.GetExtension(f);
                    if (!BuilderConfig.SET_SKIP_EXTS.Contains(ext))
                    {
                        list.Add(Tools.RelativeTo(files[i], Directory.GetParent(Application.dataPath).FullName));
                    }
                }
                return GenBundleBuild(Tools.GetAsbName(Tools.RelativeTo(dir, relative2)), list.ToArray());
            }
            return new AssetBundleBuild();
        }

        public static string GetAsbFolderByTarget(BuildTarget t)
        {
            switch (t)
            {
                case BuildTarget.Android:
                    return "and";
                case BuildTarget.iOS:
                    return "ios";
                case BuildTarget.StandaloneWindows:
                    return "pc";
            }
            return "pc";
        }

        /// <summary>
        /// 按平台打包所有资源代码
        /// </summary>
        /// <param name="t"></param>
        public static void BuildAllRes(BuilderConfig config)
        {
            //获取所有资源打包信息
            List<AssetBundleBuild> bundles = GenResBuild(config);
            BuildAbsByConfig(bundles, config);
        }

        /// <summary>
        /// 按平台打包所有lua文件
        /// </summary>
        /// <param name="config"></param>
        public static void BuildAllLua(BuilderConfig config)
        {
            //根据选择平台打包所有lua文件
            List<AssetBundleBuild> bundles = GenLuaBuild(config);
            BuildAbsByConfig(bundles, config);

            ////删除生成的Lua文件夹
            //if (Directory.Exists(sTempLuaDir))
            //{
            //    Directory.Delete(sTempLuaDir, true);
            //}
        }

        /// <summary>
        /// 按平台打包所有lua文件和资源
        /// </summary>
        /// <param name="config"></param>
        public static void BuildAll(BuilderConfig config)
        {
            //获取所有资源打包信息
            List<AssetBundleBuild> bundles = GenResBuild(config);
            bundles.AddRange(GenLuaBuild(config));
            //根据选择平台打包
            BuildAbsByConfig(bundles, config);

            ////删除生成的Lua文件夹
            //if (Directory.Exists(sTempLuaDir))
            //{
            //    Directory.Delete(sTempLuaDir, true);
            //}
        }

        /// <summary>
        /// 获取游戏中所有资源的assetbundle 信息，用来打包
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static List<AssetBundleBuild> GenResBuild(BuilderConfig config)
        {
            List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();
            string path = config.LoadPath;
            foreach (var d in Directory.GetDirectories(path))
            {
                foreach (var d2 in Directory.GetDirectories(d))
                {
                    foreach (var d3 in Directory.GetDirectories(d2))
                    {
                        string relativeDir = Tools.RelativeTo(d3, Application.dataPath);
                        Debug.LogWarning(relativeDir);
                        bundles.Add(GenBuildByDir(d3, Tools.GetResPath(), "*.*"));
                    }
                }
            }
            return bundles;
        }

        /// <summary>
        /// 获取游戏中所有Lua文件的assetbundle 信息，用来打包
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static List<AssetBundleBuild> GenLuaBuild(BuilderConfig config)
        {
            List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();
            //TODO:获取所有lua文件的bundle信息，所有lua文件需以.bytes为后缀名，否则不能打进assetbundle
            // 清空临时lua文件夹，正常情况下是不会有该文件夹的
            if (Directory.Exists(sTempLuaDir))
            {
                Directory.Delete(sTempLuaDir, true);
            }
            Directory.CreateDirectory(sTempLuaDir);
            //拷贝（编译）lua文件到临时目录
            string[] srcDirs = {
            CustomSettings.luaDir
            , CustomSettings.FrameworkPath + "/ToLua/Lua"
            , Tools.PathCombine(Application.dataPath, GameConfig.STR_LUA_FOLDER)
        };
            foreach (var dir in srcDirs)
            {
                if (GameConfig.Instance.encodeLua)
                {
                    string sourceDir = dir;
                    string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
                    int len = sourceDir.Length;

                    if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
                    {
                        --len;
                    }
                    for (int j = 0; j < files.Length; j++)
                    {
                        string str = files[j].Remove(0, len);
                        string dest = sTempLuaDir + str + ".bytes";
                        string _dir = Path.GetDirectoryName(dest);
                        Directory.CreateDirectory(_dir);
                        Packager.EncodeLuaFile(files[j], dest);
                    }
                }
                else
                {
                    ToLuaMenu.CopyLuaBytesFiles(dir, sTempLuaDir);
                }
            }
            foreach (var luaDir in Directory.GetDirectories(sTempLuaDir, "*", SearchOption.AllDirectories))
            {
                AssetBundleBuild bundle = GenBuildByDir(luaDir, sTempLuaDir, "*.bytes", false);
                bundle.assetBundleName = "lua/lua_" + bundle.assetBundleName.Replace('/', '_').ToLower();
                bundles.Add(bundle);
                Debug.LogWarning("lua Add luaDir:" + luaDir);
            }
            AssetBundleBuild lua = GenBuildByDir(sTempLuaDir, sTempLuaDir, "*.bytes", false);
            lua.assetBundleName = "lua/lua" + GameConfig.STR_ASB_EXT;
            bundles.Add(lua);
            Debug.LogWarning("lua Add luaDir: lua/lua");

            return bundles;
        }

        public static void BuildAbsByConfig(List<AssetBundleBuild> bundles, BuilderConfig config)
        {
            if (BuildTarget.NoTarget == config.target)
            {
                BuildAsb(config.ExportPath, bundles.ToArray(), config.options, BuildTarget.Android);
                BuildAsb(config.ExportPath, bundles.ToArray(), config.options, BuildTarget.iOS);
                BuildAsb(config.ExportPath, bundles.ToArray(), config.options, BuildTarget.StandaloneWindows);
            }
            else
            {
                BuildAsb(config.ExportPath, bundles.ToArray(), config.options, config.target);
            }
        }
    }

}