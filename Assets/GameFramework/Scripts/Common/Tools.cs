using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GameFramework
{
    public class Tools
    {
        #region GameResManger等资源路径相关
        public static string GetWriteableDataPath(string subPath = "")
        {
            string root = string.Empty;

#if UNITY_EDITOR
            root = Application.streamingAssetsPath;
#elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
        root = Application.streamingAssetsPath;
#else
        root = Application.persistentDataPath;        
#endif

            if (string.IsNullOrEmpty(subPath))
            {
                return root;
            }
            else
            {
                return PathCombine(root, subPath);
            }
        }

        /// <summary>
        /// 获取GameFramework的路径，仅在编辑器使用
        /// </summary>
        /// <returns>The framework path.</returns>
        public static string GetFrameworkPath()
        {
            return PathCombine(Application.dataPath, "GameFramework");
        }

        public static string GetReadOnlyPath(string suPath = "")
        {
            return Application.streamingAssetsPath;
        }

        public static string PathCombine(string root, string file)
        {
            return FormatPathStr(Path.Combine(root, file));
        }

        public static string PathCombine(string root, string subRoot, string file)
        {
            return PathCombine(PathCombine(root, subRoot), file);
        }

        public static string FormatPathStr(string path)
        {
            return path.Replace('\\', '/');
        }

        public static bool CheckDirExists(string path, bool createIfNot = false)
        {
            bool exists = Directory.Exists(path);
            if (!exists && createIfNot)
            {
                Directory.CreateDirectory(path);
                exists = true;
            }
            return exists;
        }

        public static bool CheckFileExists(string path, bool createIfNot = false)
        {
            bool exists = File.Exists(path);
            if (!exists && createIfNot)
            {
                if (CheckDirExists(Directory.GetParent(path).FullName, true))
                {
                    File.Create(path).Close();
                    exists = true;
                }
            }
            return exists;
        }

        public static void RenameFile(string oriPath, string newPath)
        {
            if (File.Exists(oriPath))
            {
                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }
                else
                {
                    CheckDirExists(Directory.GetParent(newPath).FullName, true);
                }
                FileInfo fi = new FileInfo(oriPath);
                fi.MoveTo(newPath);
                Debug.LogFormat("Rename file: {0} ---> {1}", oriPath, newPath);
            }
        }

        public static string RelativeTo(string fullPath, string relative2, bool startWith = false)
        {
            fullPath = FormatPathStr(fullPath);
            relative2 = FormatPathStr(relative2);
            if (startWith)
            {
                relative2 = FormatPathStr(Directory.GetParent(relative2).FullName);
            }
            if (!relative2.EndsWith("/"))
            {
                relative2 = relative2 + '/';
            }

            if (Path.IsPathRooted(fullPath) && fullPath.StartsWith(relative2))
            {
                fullPath = fullPath.Replace(relative2, "");

            }
            return fullPath;
        }

        /// <summary>
        /// 获取Assetbundle文件的路径
        /// </summary>
        /// <returns>The asb name.</returns>
        /// <param name="path">三层的文件夹路径（相对于BundleRes),如：conf/common/res,文件夹中不能有空格 </param>
        public static string GetAsbName(string path, bool getParent = false)
        {
            path = FormatPathStr(path).Trim('/');
            if (getParent)
            {
                path = path.Substring(0, path.LastIndexOf("/"));
            }
            //如果以asb后缀结尾，认为是正确的路径，直接返回
            if (path.EndsWith(GameConfig.STR_ASB_EXT))
            {
                return path;
            }
            //Regex regex = new Regex("([\\w_]+/[\\w_]+/)([\\w]+)");
            //MatchCollection macths = regex.Matches(path);
            //if(macths.Count == 1)
            //{
            //    var groups = macths[0].Groups;
            //    path = string.Format("{0}{1}{2}", groups[1].Value, groups[2].Value.ToLower(), GameConfig.STR_ASB_EXT);
            //}
            //return path;
            return path.ToLower() + GameConfig.STR_ASB_EXT;
        }

        /// <summary>
        /// 返回从persistentDataPath读取文件的url
        /// </summary>
        /// <returns>The URL path pars.</returns>
        /// <param name="path">Path.</param>
        /// <param name="subPath">Sub path.</param>
        public static string GetUrlPathWritebble(string path, string subPath = "")
        {
            if (!string.IsNullOrEmpty(subPath))
            {
                path = PathCombine(path, subPath);
            }

            if (Application.isEditor)
            {
                return "file://" + path;
            }
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
            {
                return "file:///" + path;
            }
            else // For standalone player.
            {
                return "file://" + path;
            }
        }

        /// <summary>
        /// 返回从streamingAssetsPath读取文件的url
        /// </summary>
        /// <returns>The URL path stream.</returns>
        /// <param name="path">Path.</param>
        /// <param name="subPath">Sub path.</param>
        public static string GetUrlPathStream(string path, string subPath = "")
        {
            if (!string.IsNullOrEmpty(subPath))
            {
                path = PathCombine(path, subPath);
            }

            string pre = "file://";
#if UNITY_ANDROID
            if (Application.isMobilePlatform)
            {
                pre = "";
            }
#endif
            return pre + path;
        }

        /// <summary>
        /// 获取运行时的assetbundle所在根目录
        /// </summary>
        /// <returns></returns>
        public static string GetAsbPath()
        {
            return GetWriteableDataPath(GameConfig.STR_ASB_MANIFIST);
        }
        /// <summary>
        /// 获取某assetbundle的url
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetAsbUrl(string name)
        {
            return GetUrlPathWritebble(GetAsbPath(), name);
        }
        /// <summary>
        /// 获取原始资源的根目录，在Assets/BundleRes,一般情况下只有编辑器会使用本函数
        /// </summary>
        /// <returns></returns>
        public static string GetResPath(string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                return PathCombine(Application.dataPath, GameConfig.STR_RES_FOLDER);
            }
            else
            {
                return PathCombine(PathCombine(Application.dataPath, GameConfig.STR_RES_FOLDER), path);
            }

        }

        /// <summary>
        /// 获取某原始资源的url
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetResUrl(string name)
        {
            return GetUrlPathWritebble(GetResPath(), name);
        }

        //test
        public static string GetLuaSrcPath()
        {
            return PathCombine(Application.dataPath, GameConfig.STR_LUA_FOLDER);
        }


        public static string GetLuaAsbPath(string bundleName)
        {
            return PathCombine(GetAsbPath(), "lua/" + bundleName.ToLower());
        }


        /// <summary>
        /// 获取在BundleRes下资源的以Assets/开头的路径，常用于获取AssetBundle资源名
        /// </summary>
        /// <returns>The res in assets name.</returns>
        /// <param name="asbPath">Asb path.</param>
        /// <param name="resPath">Res path.</param>
        public static string GetResInAssetsName(string asbPath, string resPath)
        {
            if (resPath[0] == '.')
            {
                return PathCombine("Assets/" + GameConfig.STR_RES_FOLDER, asbPath + resPath);
            }
            return PathCombine("Assets/" + GameConfig.STR_RES_FOLDER, asbPath, resPath);
        }
        /// <summary>
        /// 根据传入的路径获取正式加载资源时的asbPath和filePath
        /// 在地图编辑器会用到
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="asbPath"></param>
        /// <param name="filePath"></param>
        public static void SplitResPath(string fullPath, out string asbPath, out string filePath, bool useFullPath = false)
        {
            asbPath = null;
            filePath = null;
            if (string.IsNullOrEmpty(fullPath))
            {
                return;
            }
            string relative = Tools.RelativeTo(fullPath, Tools.GetResPath());
            if (!useFullPath || fullPath != relative)
            {
                int index = 0;
                for (int i = 0; i < 3; ++i)
                {
                    if (index < relative.Length)
                    {
                        index = relative.IndexOf('/', index + 1);
                    }
                }
                if (-1 != index)
                {
                    asbPath = relative.Substring(0, index);
                    filePath = relative.Substring(index + 1);
                }
            }

        }

        /// <summary>
        /// 获取Transform的名字，如果传入了root则返回相对于root的名字
        /// </summary>
        /// <returns>The transform name.</returns>
        /// <param name="transform">Transform.</param>
        /// <param name="root">Root.</param>
        public static string GetTransformName(Transform transform, Transform root)
        {
            if (null != transform)
            {
                if (null == root)
                {
                    return transform.name;
                }
                else
                {
                    string name = transform.name;
                    Transform parent = transform.parent;
                    while (null != parent)
                    {
                        if (parent.Equals(root))
                        {
                            break;
                        }
                        name = string.Format("{0}/{1}", parent.name, name);
                        parent = parent.parent;
                    }
                    return name;
                }
            }
            return string.Empty;
        }

        public static string GetTransformName(Transform transform)
        {
            return GetTransformName(transform, null);
        }
        #endregion GameResManger等资源路径相关

        #region U3D常用类型转换相关
        public static Rect GenRect(float[] array)
        {
            Rect rect = new Rect();
            rect.x = array[0];
            rect.y = array[1];
            rect.width = array[2];
            rect.height = array[3];
            return rect;
        }
        #endregion U3D常用类型转换相关
    }
}

