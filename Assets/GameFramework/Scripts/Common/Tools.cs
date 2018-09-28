using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LuaInterface;
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
            //#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                pre = "";
            }
            //#endif
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
                #region
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
        #endregion U3D常用类型转换相关

        #region Lua相关
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
        #endregion Lua相关
    }
}

