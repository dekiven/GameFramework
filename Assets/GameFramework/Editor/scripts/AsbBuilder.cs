using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GameFramework
{
    public static class AsbBuilder
    {
        public static string sTempLuaDir = Tools.PathCombine(Application.dataPath, "Lua");
        public static string sResDir = Tools.GetResPath();

        public static BuilderConfig mConfig;

        public static void BuildAsb(string path, BuildAssetBundleOptions opt = BuildAssetBundleOptions.None, BuildTarget target = BuildTarget.StandaloneWindows)
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
            BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, target);

            renameFiles();

            AssetDatabase.Refresh();
        }

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

            renameFiles();

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
                    if (!BuilderConfig.SET_SKIP_EXTS.Contains(ext) && f != ".DS_Store")
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
                case BuildTarget.StandaloneOSX :
                    return "mac";
            }
            return "pc";
        }

        /// <summary>
        /// 按平台打包所有资源代码
        /// </summary>
        /// <param name="t"></param>
        public static void BuildAllRes(BuilderConfig config)
        {
            mConfig = config;
            GenResBuild(config);
            BuildAbsByConfig(config);
        }

        /// <summary>
        /// 按平台打包所有lua文件
        /// </summary>
        /// <param name="config"></param>
        public static void BuildAllLua(BuilderConfig config)
        {
            mConfig = config;
            //根据选择平台打包所有lua文件
            GenLuaBuild(config);
            BuildAbsByConfig(config);

            ////删除生成的Lua文件夹
            //if (Directory.Exists(sTempLuaDir))
            //{
            //  Directory.Delete(sTempLuaDir, true);
            //}
        }

        /// <summary>
        /// 按平台打包所有lua文件和资源
        /// </summary>
        /// <param name="config"></param>
        public static void BuildAll(BuilderConfig config)
        {
            mConfig = config;
            // 配置lua打包信息
            GenLuaBuild(config);
            //配置所有资源打包信息
            GenResBuild(config);
            //根据选择平台打包
            BuildAbsByConfig(config);

            ////删除生成的Lua文件夹
            //if (Directory.Exists(sTempLuaDir))
            //{
            //  Directory.Delete(sTempLuaDir, true);
            //}
        }

        /// <summary>
        /// 获取游戏中所有资源的assetbundle 信息，用来打包
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static void GenResBuild(BuilderConfig config)
        {
            // List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();
            string path = config.LoadPath;
            GenResBuildByDir(path, "*.*");
        }

        /// <summary>
        /// 获取游戏中所有Lua文件的assetbundle 信息，用来打包
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static void GenLuaBuild(BuilderConfig config)
        {
            //获取所有lua文件的bundle信息，所有lua文件需以.bytes为后缀名，否则不能打进assetbundle
            // 清空临时lua文件夹，正常情况下是不会有该文件夹的
            if (Directory.Exists(sTempLuaDir))
            {
                Directory.Delete(sTempLuaDir, true);
            }
            Directory.CreateDirectory(sTempLuaDir);
            //拷贝（编译）lua文件到临时目录
            string[] srcDirs = {
                CustomSettings.lfuLuaDir
                , CustomSettings.baseLuaDir
                , Tools.GetLuaSrcPath()
            };
            foreach (var dir in srcDirs)
            {
                if (GameConfig.encodeLua)
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
                        EncodeLuaFile(files[j], dest, config);
                    }
                }
                else
                {
                    ToLuaMenu.CopyLuaBytesFiles(dir, sTempLuaDir);
                }
            }
            //通过GenLuaBuildByDir配置AssetBundle name
            foreach (var luaDir in Directory.GetDirectories(sTempLuaDir, "*", SearchOption.AllDirectories))
            {
                GenLuaBuildByDir(luaDir, "*.bytes");
                //Debug.LogWarning("lua Add luaDir:" + luaDir);
            }
            GenLuaBuildByDir(sTempLuaDir, "*.bytes");
            //Debug.LogWarning("lua Add luaDir: lua/lua");
        }

        public static void BuildAbsByConfig(BuilderConfig config)
        {
            if (BuildTarget.NoTarget == config.target)
            {
                BuildAsb(config.ExportPath, config.options, BuildTarget.Android);
                BuildAsb(config.ExportPath, config.options, BuildTarget.iOS);
                BuildAsb(config.ExportPath, config.options, BuildTarget.StandaloneWindows);
            }
            else
            {
                BuildAsb(config.ExportPath, config.options, config.target);
            }
        }

        public static void EncodeLuaFile(string srcFile, string outFile, BuilderConfig config)
        {
            if (!srcFile.ToLower().EndsWith(".lua"))
            {
                File.Copy(srcFile, outFile, true);
                return;
            }
            bool isWin = true;
            string luaexe = string.Empty;
            string args = string.Empty;
            string exedir = string.Empty;
            string currDir = Directory.GetCurrentDirectory();
            string platStr = "/";
            if(BuildTarget.Android == config.target )
            {
                platStr = "_32/";
                //Debug.LogError(platStr);
            }
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                isWin = true;
                luaexe = "luajit.exe";
                args = "-b -g " + srcFile + " " + outFile;
                exedir = Application.dataPath.Replace("Assets", "") + "LuaEncoder/luajit" + platStr;
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                isWin = false;
                luaexe = "./luajit";
                args = "-b -g " + srcFile + " " + outFile;
                exedir = Application.dataPath.Replace("Assets", "") + "LuaEncoder/luajit_mac" + platStr;
            }
            Directory.SetCurrentDirectory(exedir);
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.FileName = luaexe;
            info.Arguments = args;
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            info.UseShellExecute = isWin;
            info.ErrorDialog = true;
            LogFile.Log(info.FileName + " " + info.Arguments);

            System.Diagnostics.Process pro = System.Diagnostics.Process.Start(info);
            pro.WaitForExit();
            Directory.SetCurrentDirectory(currDir);
        }


        public static void GenLuaBuildByDir(string dir, string pattern)
        {

            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                string relative2 = sTempLuaDir;
                string[] files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
                string asbName = "lua/" + Tools.GetAsbName(Tools.RelativeTo(dir, relative2, true)).Replace("/", "_");

                for (int i = 0; i < files.Length; i++)
                {
                    string f = files[i];
                    if (BuilderConfig.IsResFile(f))
                    {
                        AsbNameSetting.SetBundleName(Tools.RelativeTo(files[i], Application.dataPath, true), asbName, true);
                    }
                }
            }
        }

        public static void GenResBuildByDir(string dir, string pattern)
        {
            string relative2 = sResDir;
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                //查看该文件夹下是否有文件，有文件将该文件夹作为一个bundle
                string[] files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
                int count = 0;
                foreach (var f in files)
                {
                    if (BuilderConfig.IsResFile(f))
                    {
                        count += AsbNameSetting.GetHasAsbName(f) ? 0 : 1;
                        break;
                    }
                }
                if (count > 0)
                {
                    files = Directory.GetFiles(dir, pattern, SearchOption.AllDirectories);
                    string asbName = Tools.GetAsbName(Tools.RelativeTo(dir, relative2));
                    // List<string> list = new List<string>();
                    for (int i = 0; i < files.Length; i++)
                    {
                        string f = files[i];
                        if (BuilderConfig.IsResFile(f))
                        {
                            string resPath = Tools.RelativeTo(f, Application.dataPath, true);
                            AsbNameSetting.SetBundleName(resPath, asbName, true);
                        }
                    }
                }
                else
                {
                    //如果文件夹下没有文件，遍历其子文件夹
                    string[] dirs = Directory.GetDirectories(dir);
                    if (dirs.Length > 0)
                    {
                        foreach (var d in dirs)
                        {
                            GenResBuildByDir(d, pattern);
                        }
                    }
                }
            }
        }

        private static void renameFiles()
        {
            Debug.Log("renameFiles");
            string[] plats = { "pc","and","ios","mac", };
            foreach (var p in plats)
            {
                string oriPath = Tools.PathCombine(mConfig.ExportPath, p, p);
                string newPath = oriPath + GameConfig.STR_ASB_EXT;
                Tools.RenameFile(oriPath, newPath);
                oriPath = oriPath + ".manifest";
                newPath = newPath + ".manifest";
                Tools.RenameFile(oriPath, newPath);
            }
        }
    }



}