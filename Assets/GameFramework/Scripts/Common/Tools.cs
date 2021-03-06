﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LuaInterface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GameFramework
{
    public class Tools
    {
        #region GameResManger等资源路径相关
        public static string GetWriteableDataPath(string subPath = "")
        {
            string root = string.Empty;
#if UNITY_EDITOR
            //root = Application.streamingAssetsPath;
            root = PathCombine(Application.dataPath, "../RunTimeRes");
#elif UNITY_STANDALONE_WIN
            //root = Application.streamingAssetsPath;
            root = Path.GetFullPath(PathCombine(Application.streamingAssetsPath, "../Data"));
#else
            root = Application.persistentDataPath;        
#endif
            root = Path.GetFullPath(root);
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
        [NoToLua]
        public static string GetFrameworkPath()
        {
            return PathCombine(Application.dataPath, "GameFramework");
        }

        public static string GetReadOnlyPath(string suPath = "")
        {
            if (string.IsNullOrEmpty(suPath))
            {
                return Application.streamingAssetsPath;
            }
            else
            {
                return PathCombine(Application.streamingAssetsPath, suPath);
            }

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
                    File.Create(path).Dispose();
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
                //FileInfo fi = new FileInfo(oriPath);
                //fi.MoveTo(newPath);
                File.Move(oriPath, newPath);
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
            if (!relative2.EndsWith("/", StringComparison.Ordinal))
            {
                relative2 = relative2 + '/';
            }

            if (Path.IsPathRooted(fullPath) && fullPath.StartsWith(relative2, StringComparison.Ordinal))
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
                path = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal));
            }
            //如果以asb后缀结尾，认为是正确的路径，直接返回
            if (path.EndsWith(GameConfig.STR_ASB_EXT, StringComparison.Ordinal))
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
            return path.ToLower().Replace("/", "_") + GameConfig.STR_ASB_EXT;
        }

        /// <summary>
        /// 返回从persistentDataPath读取文件的url
        /// </summary>
        /// <returns>The URL path pars.</returns>
        /// <param name="path">Path.</param>
        /// <param name="subPath">Sub path.</param>
        public static string GetUrlPathWriteabble(string path, string subPath = "")
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
            if (Application.platform == RuntimePlatform.Android)
            {
                pre = "";
            }
            return pre + path;
        }

        /// <summary>
        /// 获取某assetbundle的url
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetAsbUrl(string name)
        {
            string url = string.Empty;
            if (GetAsbPath(name, out url))
            {
                url = GetUrlPathWriteabble(url);
            }
            else
            {
                url = GetUrlPathStream(url);
            }
            return url;
        }

        /// <summary>
        /// 获取运行时的assetbundle所在根目录, 
        /// 如果可读写文件夹存在返回可读写文件夹下路径，
        /// 不存在返回Streaming文件夹下路径(该路径下文件不一定存在)
        /// </summary>
        /// <returns><c>true</c>, if asb path was gotten, <c>false</c> otherwise.</returns>
        /// <param name="name">Name.</param>
        /// <param name="path">Path.</param>
        /// <param name="isLua">是否是luaAsb</param>
        public static bool GetAsbPath(string name, out string path, bool isLua = false)
        {
            bool ret = false;
            if (isLua)
            {
                name = GameConfig.STR_ASB_MANIFIST + "/lua/" + GetAsbName(name);
            }
            else
            {
                name = GameConfig.STR_ASB_MANIFIST + "/" + GetAsbName(name);
            }
            path = GetWriteableDataPath(name);
            ret = File.Exists(path);
            if (!ret)
            {
                path = PathCombine(Application.streamingAssetsPath, name);
            }

            return ret;
        }

        /// <summary>
        /// 获取文件在Stream或Writeable Path 的路径，Writeable路径优先
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileUrl(string path)
        {
            string url = string.Empty;
            if (GetFilePath(path, out url))
            {
                url = GetUrlPathWriteabble(url);
            }
            else
            {
                url = GetUrlPathStream(url);
            }
            return url;
        }

        /// <summary>
        /// 获取资源路径，可能在下载路径或者包内
        /// </summary>
        /// <returns><c>true</c>, 表示在下载(Writeable)路径, <c>false</c> 表示可能存在包内(Stream),(android 不能校验是否在包内).</returns>
        /// <param name="path">资源相对 Assets(或平台文件夹) 文件夹的路径</param>
        /// <param name="realPath">资源路径</param>
        public static bool GetFilePath(string path, out string realPath)
        {
            bool ret = false;
            realPath = GetWriteableDataPath(path);
            ret = File.Exists(realPath);
            if (!ret)
            {
                realPath = PathCombine(Application.streamingAssetsPath, path);
            }
//#if !UNITY_ANDROID
//            if(!File.Exists(realPath))
//            {
//                realPath = string.Empty;
//            }
//#endif
            return ret;
        }

        /// <summary>
        /// 获取原始资源的根目录，在Assets/BundleRes,一般情况下只有编辑器会使用本函数
        /// </summary>
        /// <returns></returns>
        [NoToLua]
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
            return GetUrlPathWriteabble(GetResPath(), name);
        }

        public static string GetLuaSrcPath()
        {
            return PathCombine(Application.dataPath, GameConfig.STR_LUA_FOLDER);
        }

#if UNITY_EDITOR
        /// <summary>
        /// GameFramework框架的Lua路径
        /// </summary>
        /// <returns></returns>
        public static string GetGFLuaPath()
        {
            return PathCombine(GetFrameworkPath(), "Lua");
        }
#endif


        public static string GetLuaAsbPath(string bundleName)
        {
            string path = string.Empty;
            GetAsbPath(bundleName, out path, true);
            return path;
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

        ///// <summary>
        ///// 根据传入的路径获取正式加载资源时的asbPath和filePath
        ///// 在地图编辑器会用到
        ///// </summary>
        ///// <param name="fullPath"></param>
        ///// <param name="asbPath"></param>
        ///// <param name="filePath"></param>
        //public static void SplitResPath(string fullPath, out string asbPath, out string filePath, bool useFullPath = false)
        //{
        //    asbPath = null;
        //    filePath = null;
        //    if (string.IsNullOrEmpty(fullPath))
        //    {
        //        return;
        //    }
        //    string relative = Tools.RelativeTo(fullPath, Tools.GetResPath());
        //    if (!useFullPath || fullPath != relative)
        //    {
        //        int index = 0;
        //        for (int i = 0; i < 3; ++i)
        //        {
        //            if (index < relative.Length)
        //            {
        //                index = relative.IndexOf('/', index + 1);
        //            }
        //        }
        //        if (-1 != index)
        //        {
        //            asbPath = relative.Substring(0, index);
        //            filePath = relative.Substring(index + 1);
        //        }
        //    }

        //}

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
            Rect rect = Rect.zero;
            if (array.Length == 4)
            {
                rect = new Rect(array[0], array[1], array[2], array[3]);
            }

            return rect;
        }

        public static Rect GenRectByStr(string rectStr)
        {
            Rect rect = Rect.zero;
            string[] array = rectStr.Split(',');
            if (array.Length == 4)
            {
                float[] _params = new float[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    if (!float.TryParse(array[i], out _params[i]))
                    {
                        LogFile.Warn("GenRectByStr error -> rectStr:" + rectStr);
                        return rect;
                    }
                }
                return GenRect(_params);
            }
            return rect;
        }

        public static Color GenColor(float[] array)
        {
            Color color = Color.white;
            if (array.Length == 3)
            {
                color = new Color(array[0], array[1], array[2]);
            }
            else if (array.Length == 4)
            {
                color = new Color(array[0], array[1], array[2], array[3]);
            }
            return color;
        }

        public static Color GenColorByStr(string colorStr)
        {
            Color color = Color.white;
            string[] array = colorStr.Split(',');
            if (array.Length >= 3)
            {
                float[] rgba = new float[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    if (!float.TryParse(array[i], out rgba[i]))
                    {
                        LogFile.Warn("GenColorByStr error -> colorStr:" + colorStr);
                        return color;
                    }
                }
                return GenColor(rgba);
            }
            switch (colorStr.ToLower())
            {
                case "black":
                    color = Color.black;
                    break;
                case "blue":
                    color = Color.blue;
                    break;
                case "cyan":
                    color = Color.cyan;
                    break;
                case "clear":
                    color = Color.clear;
                    break;
                case "gray":
                    color = Color.gray;
                    break;
                case "grey":
                    color = Color.grey;
                    break;
                case "green":
                    color = Color.green;
                    break;
                case "magenta":
                    color = Color.magenta;
                    break;
                case "red":
                    color = Color.red;
                    break;
                case "white":
                    color = Color.white;
                    break;
                case "yellow":
                    color = Color.yellow;
                    break;
            }
            return color;
        }

        public static Vector3 GenVector3(float[] array)
        {
            Vector3 pos = Vector3.zero;
            if (array.Length >= 3)
            {
                pos = new Vector3(array[0], array[1], array[2]);
            }
            else if (array.Length == 2)
            {
                pos = new Vector3(array[0], array[1]);
            }
            return pos;
        }

        public static Vector3 GenVector3ByStr(string vecStr)
        {
            Vector3 pos = Vector3.zero;
            string[] array = vecStr.Split(',');
            if (array.Length >= 2)
            {
                float[] _array = new float[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    if (!float.TryParse(array[i], out _array[i]))
                    {
                        LogFile.Warn("GenVector3ByStr error -> vecStr:" + vecStr);
                        return pos;
                    }
                }
                return GenVector3(_array);
            }

            switch (vecStr.ToLower())
            {
                case "back":
                    pos = Vector3.back;
                    break;
                case "down":
                    pos = Vector3.down;
                    break;
                case "forward":
                    pos = Vector3.forward;
                    break;
                case "left":
                    pos = Vector3.left;
                    break;
                case "one":
                    pos = Vector3.one;
                    break;
                case "right":
                    pos = Vector3.right;
                    break;
                case "up":
                    pos = Vector3.up;
                    break;
                case "zero":
                    pos = Vector3.zero;
                    break;
            }
            return pos;
        }

        public static Vector2 GenVector2(float[] array)
        {
            Vector2 pos = Vector2.zero;
            if (array.Length >= 2)
            {
                pos = new Vector2(array[0], array[1]);
            }
            return pos;
        }

        public static Vector2 GenVector2ByStr(string vecStr)
        {
            Vector3 pos = Vector3.zero;
            string[] array = vecStr.Split(',');
            if (array.Length >= 2)
            {
                float[] _array = new float[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    if (!float.TryParse(array[i], out _array[i]))
                    {
                        LogFile.Warn("GenVector3ByStr error -> vecStr:" + vecStr);
                        return pos;
                    }
                }
                return GenVector2(_array);
            }

            switch (vecStr.ToLower())
            {
                //case "back":
                //pos = Vector2.back;
                //break;
                case "down":
                    pos = Vector2.down;
                    break;
                //case "forward":
                //pos = Vector2.forward;
                //break;
                case "left":
                    pos = Vector2.left;
                    break;
                case "one":
                    pos = Vector2.one;
                    break;
                case "right":
                    pos = Vector2.right;
                    break;
                case "up":
                    pos = Vector2.up;
                    break;
                case "zero":
                    pos = Vector2.zero;
                    break;
            }
            return pos;
        }

        public static List<UIItemData> GenUIIemDataList(LuaTable table)
        {
            List<UIItemData> list = new List<UIItemData>();
            if (null != table)
            {
                int count = table.RawGet<string, int>("count");
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(new UIItemData(table.RawGetIndex<LuaTable>(i + 1)));
                    }
                }
                table.Dispose();
            }
            return list;
        }

        public static void ModifyRectTransform(RectTransform rectTransform, Dictionary<string, System.Object> dict)
        {
            if (null != rectTransform && null != dict)
            {
#region ModifyRectTransform实现
                //rectTransform.anchoredPosition = Vector2.zero;
                //rectTransform.anchoredPosition3D = Vector3.zero;
                //rectTransform.anchorMax = Vector2.zero;
                //rectTransform.anchorMin = Vector2.zero;
                //rectTransform.localEulerAngles = Vector3.zero;
                //rectTransform.localScale = Vector3.zero;
                //rectTransform.localPosition = Vector3.zero;
                ////rectTransform.localRotation = Vector3.zero; 
                //rectTransform.offsetMax = Vector2.zero;
                //rectTransform.offsetMin = Vector2.zero;
                //rectTransform.pivot = Vector2.zero;
                //rectTransform.position = Vector3.zero;
                //rectTransform.sizeDelta = Vector2.zero;
                ////rectTransform.rect
                System.Object obj = null;
                foreach (var item in dict)
                {
                    Debug.LogFormat("key:{0}, value:{1}", item.Key, item.Value);
                }

                if (dict.TryGetValue("anchoredPosition", out obj))
                {
                    Vector2 value = rectTransform.anchoredPosition;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector2ByStr(str);
                    }
                    else
                    {
                        // value = (Vector2)obj;
                        LogFile.Warn("修改{0}的rectTransform.anchoredPosition失败", rectTransform.name);
                    }
                    rectTransform.anchoredPosition = value;
                }

                if (dict.TryGetValue("anchoredPosition3D", out obj))
                {
                    Vector3 value = rectTransform.anchoredPosition3D;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector3ByStr(str);
                    }
                    else
                    {
                        // value = (Vector3)obj;
                        LogFile.Warn("修改{0}的rectTransform.anchoredPosition3D失败", rectTransform.name);
                    }
                    rectTransform.anchoredPosition3D = value;
                }

                if (dict.TryGetValue("anchorMax", out obj))
                {
                    Vector2 value = rectTransform.anchorMax;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector2ByStr(str);
                    }
                    else
                    {
                        // value = (Vector2)obj;
                        LogFile.Warn("修改{0}的rectTransform.anchorMax失败", rectTransform.name);
                    }
                    rectTransform.anchorMax = value;
                }

                if (dict.TryGetValue("anchorMin", out obj))
                {
                    Vector2 value = rectTransform.anchorMin;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector2ByStr(str);
                    }
                    else
                    {
                        // value = (Vector2)obj;
                        LogFile.Warn("修改{0}的rectTransform.anchorMin失败", rectTransform.name);
                    }
                    rectTransform.anchorMin = value;
                }

                if (dict.TryGetValue("localEulerAngles", out obj))
                {
                    Vector3 value = rectTransform.localEulerAngles;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector3ByStr(str);
                    }
                    else
                    {
                        // value = (Vector3)obj;
                        LogFile.Warn("修改{0}的rectTransform.localEulerAngles失败", rectTransform.name);
                    }
                    rectTransform.localEulerAngles = value;
                }

                if (dict.TryGetValue("localScale", out obj))
                {
                    Vector3 value = rectTransform.localScale;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector3ByStr(str);
                    }
                    else
                    {
                        // value = (Vector3)obj;
                        LogFile.Warn("修改{0}的rectTransform.localScale失败", rectTransform.name);
                    }
                    rectTransform.localScale = value;
                }

                if (dict.TryGetValue("localPosition", out obj))
                {
                    Vector3 value = rectTransform.localPosition;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector3ByStr(str);
                    }
                    else
                    {
                        // value = (Vector3)obj;
                        LogFile.Warn("修改{0}的rectTransform.localPosition失败", rectTransform.name);
                    }
                    rectTransform.localPosition = value;
                }

                if (dict.TryGetValue("offsetMax", out obj))
                {
                    Vector2 value = rectTransform.offsetMax;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector2ByStr(str);
                    }
                    else
                    {
                        // value = (Vector2)obj;
                        LogFile.Warn("修改{0}的rectTransform.offsetMax失败", rectTransform.name);
                    }
                    rectTransform.offsetMax = value;
                }

                if (dict.TryGetValue("offsetMin", out obj))
                {
                    Vector2 value = rectTransform.offsetMin;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector2ByStr(str);
                    }
                    else
                    {
                        // value = (Vector2)obj;
                        LogFile.Warn("修改{0}的rectTransform.offsetMin失败", rectTransform.name);
                    }
                    rectTransform.offsetMin = value;
                }

                if (dict.TryGetValue("pivot", out obj))
                {
                    Vector2 value = rectTransform.pivot;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector2ByStr(str);
                    }
                    else
                    {
                        // value = (Vector2)obj;
                        LogFile.Warn("修改{0}的rectTransform.pivot失败", rectTransform.name);
                    }
                    rectTransform.pivot = value;
                }

                if (dict.TryGetValue("position", out obj))
                {
                    Vector3 value = rectTransform.position;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector3ByStr(str);
                    }
                    else
                    {
                        // value = (Vector3)obj;
                        LogFile.Warn("修改{0}的rectTransform.position失败", rectTransform.name);
                    }
                    rectTransform.position = value;
                }

                if (dict.TryGetValue("sizeDelta", out obj))
                {
                    Vector2 value = rectTransform.sizeDelta;
                    string str = obj as string;
                    if (null != str)
                    {
                        value = GenVector2ByStr(str);
                    }
                    else
                    {
                        // value = (Vector2)obj;
                        LogFile.Warn("修改{0}的rectTransform.sizeDelta失败", rectTransform.name);
                    }
                    rectTransform.sizeDelta = value;
                }
#endregion
            }
        }

        public static void AddEventTrigger(GameObject obj, List<EventTrigger.Entry> entries)
        {
            if(null != obj && null != entries)
            {
                EventTrigger trigger = obj.GetComponent<EventTrigger>();
                if(null == trigger)
                {
                    trigger = obj.AddComponent<EventTrigger>();
                }
                trigger.triggers = entries;
            }
        }

        public static void AddEventTrigger(GameObject obj, LuaTable table)
        {
            if (null != obj && null != table)
            {
                LuaEventTrigger trigger = obj.GetComponent<LuaEventTrigger>();
                if (null == trigger)
                {
                    trigger = obj.AddComponent<LuaEventTrigger>();
                }
                trigger.SetTriggers(table);
            }
        }

        public static void RemoveEventTrigger(GameObject obj)
        {
            if(null != obj)
            {
                EventTrigger[] triggers = obj.GetComponents<EventTrigger>();
                for (int i = triggers.Length-1; i >= 0; --i)
                {
                    UnityEngine.Object.Destroy(triggers[i]);
                }
            }
        }
#endregion U3D常用类型转换相关

#region Lua相关
        [NoToLua]
        public static Dictionary<string, System.Object> LuaTable2Dict(LuaTable table)
        {
            if (null == table)
            {
                return null;
            }
            Dictionary<string, System.Object> dictionary = table.ToDictTable<string, System.Object>().ToDictionary();
            table.Dispose();
            table = null;
            return dictionary;

        }

        /// <summary>
        /// 将以英文逗号','隔开的 整数字符串分割成整数数组
        /// </summary>
        /// <returns>The int arry.</returns>
        /// <param name="content">Content.</param>
        [NoToLua]
        public static int[] GetIntArry(string content)
        {
            string[] array = content.Split(',');
            List<int> l = ObjPools.GetListInt();
            for (int i = 0; i < array.Length; i++)
            {
                int v;
                if (!int.TryParse(array[i], out v))
                {
                    LogFile.Warn("GetIntArry error -> colorStr:" + content);
                }
                else
                {
                    l.Add(v);
                }
            }
            int[] ret = l.ToArray();
            ObjPools.Recover(l);
            return ret;
        }
#endregion Lua相关

#region 基本的工具函数
        public static string FormatMeroySize(long size)
        {
            string[] unitStrs = { "B", "K", "M", "G" };
            long unit = 1;
            for (int i = 0; i < unitStrs.Length; i++)
            {
                long last = unit;
                unit *= 1024;
                if (size < unit)
                {
                    return (size / (double)last).ToString(i==0? "F0" : "F2") + unitStrs[i];
                }
            }

            return size.ToString();
        }

        [NoToLua]
        public static Dictionary<string, string> SplitStr2Dic(string content, string pairSplit="\n", string valueSplit="|")
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var item in content.Split(pairSplit.ToCharArray()))
            {
                string[] kv = item.Split(valueSplit.ToCharArray());
                if(kv.Length == 2)
                {
                    dic[kv[0]] = kv[1];
                }
                else if(kv.Length ==1)
                {
                    dic[kv[0]] = string.Empty;
                }
            }
            return dic;
        }

        [NoToLua]
        public static bool GetBoolValue(Dictionary<string, string> dic, string key, bool def=false)
        {
            bool ret = def;
            if (null != dic)
            {
                string value;
                if(dic.TryGetValue(key, out value))
                {
                    ret = bool.Parse(value);
                }
            }
            return ret;
        }

        [NoToLua]
        public static string GetStringValue(Dictionary<string, string> dic, string key, string def = "")
        {
            string ret = def;
            if (null != dic)
            {
                string value;
                if (dic.TryGetValue(key, out value))
                {
                    ret = value;
                }
            }
            return ret;
        }

        [NoToLua]
        public static int GetIntValue(Dictionary<string, string> dic, string key, int def = 0)
        {
            int ret = def;
            if (null != dic)
            {
                string value;
                if (dic.TryGetValue(key, out value))
                {
                    ret = int.Parse(value);
                }
            }
            return ret;
        }

        [NoToLua]
        public static float GetFloatValue(Dictionary<string, string> dic, string key, float def = 0)
        {
            float ret = def;
            if (null != dic)
            {
                string value;
                if (dic.TryGetValue(key, out value))
                {
                    ret = float.Parse(value);
                }
            }
            return ret;
        }

        /// <summary>
        /// 仅支持包含数字和小数点的版本号对比,返回1 表示 v1 大于 v2，返回0表示相等
        /// </summary>
        /// <returns>The version.</returns>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        public static int CompareVersion(string v1, string v2)
        {
            int ret = 0;
            if(v1.Equals(v2))
            {
                return 0;
            }

            if(string.IsNullOrEmpty(v1))
            {
                v1 = "0.0.0";
            }

            if (string.IsNullOrEmpty(v2))
            {
                v2 = "0.0.0";
            }

            string[] v1a = v1.Split('.');
            string[] v2a = v2.Split('.');
            int l = Math.Min(v1a.Length, v2a.Length);
            for (int i = 0; i < l; i++)
            {
                int i1 = int.Parse(v1a[i]);
                int i2 = int.Parse(v2a[i]);
                if (i1 > i2)
                {
                    return 1;
                }
                if(i1 < i2)
                {
                    return -1;                    
                }

            }
            if(v1a.Length > v2a.Length)
            {
                ret = 1;
            }
            else if(v1a.Length < v2a.Length)
            {
                ret = -1;
            }
            return ret;
        }

        [NoToLua]
        public static bool Equals(double a, double b)
        {
            return Math.Abs(a - b) < 0.00000000001d;
        }

        [NoToLua]
        public static bool Equals(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }
#endregion 基本的工具函数
    }
}

