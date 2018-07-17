using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GameFramework
{
    public class AsbNameSetting
    {

        [MenuItem("Assets/设置Asb名字")]
        public static void SetAsbName()
        {
            string[] selections = Selection.assetGUIDs;
            foreach (var item in selections)
            {
                string resPath = AssetDatabase.GUIDToAssetPath(item);
                //Debug.Log (resPath);
                SetBundleNameByPath(resPath);
            }
        }

        [MenuItem("Assets/清理Asb名字")]
        public static void ClearAsbName()
        {
            string[] selections = Selection.assetGUIDs;
            foreach (var item in selections)
            {
                string resPath = AssetDatabase.GUIDToAssetPath(item);
                //Debug.Log (resPath);
                clearBundleNameByPath(resPath);
            }
        }


        //[MenuItem("Assets/打包测试")]
        //public static void BuildAll()
        //{
        //    string outDir = Application.dataPath.Replace("Assets", "AssetBundles/test");
        //    Debug.Log("打包路径：" + outDir);
        //    Tools.CheckDirExists(outDir, true);
        //    BuildPipeline.BuildAssetBundles(outDir, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        //}

        private static void SetBundleNameByPath(string path)
        {
            string fullPath = path;
            if (!Path.IsPathRooted(path) && !path.StartsWith(Application.dataPath))
            {
                fullPath = Application.dataPath.Replace("Assets", path);
            }
            else
            {
                path = Tools.RelativeTo(path, Tools.GetResPath(), true);
            }

            if (File.Exists(fullPath))
            {
                if (BuilderConfig.IsResFile(path))
                {
                    string asbName = fullPath.Substring(0, fullPath.Length - Path.GetExtension(fullPath).Length);
                    asbName = Tools.RelativeTo(asbName, Tools.GetResPath()) + GameConfig.STR_ASB_EXT;
                    SetBundleName(path, asbName.ToLower());
                }
            }
            else if (Directory.Exists(fullPath))
            {
                string[] files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);
                string asbName = Tools.RelativeTo(fullPath, Tools.GetResPath()) + GameConfig.STR_ASB_EXT;

                foreach (var f in files)
                {
                    if (BuilderConfig.IsResFile(f))
                    {
                        string assetPath = Tools.RelativeTo(f, Application.dataPath, true);
                        SetBundleName(assetPath, asbName.ToLower());
                    }
                }
            }
        }

        /// <summary>
        ///设置资源的AssetBundle Name 
        /// </summary>
        /// <param name="resPath">以Assets/开头的相对文件路径</param>
        /// <param name="bundleName">AssetBundle Name.</param>
        /// <param name="skipIfHas">If set to <c>true</c> 跳过已经设置AssetBundle name的</param>
        public static void SetBundleName(string resPath, string bundleName, bool skipIfHas = false)
        {
            AssetImporter importer = AssetImporter.GetAtPath(resPath);
            //test

            if (bundleName.EndsWith(".unity3d.unity3d", System.StringComparison.Ordinal))
            {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(new System.Diagnostics.StackFrame(true));
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                Debug.LogError(sf.ToString());
                string s = bundleName;
                Debug.LogError(bundleName);
            }
            if (importer && importer.assetBundleName != bundleName)
            {
                if (skipIfHas && !string.IsNullOrEmpty(importer.assetBundleName))
                {
                    //已经有Assetbundle name， 跳过
                    return;
                }
                importer.assetBundleName = bundleName;
                Debug.LogFormat("设置 Assetbundle 名称：{0} -----> {1}", resPath, bundleName);
            }
        }


        private static void clearBundleNameByPath(string path)
        {
            string fullPath = path;
            if (!Path.IsPathRooted(path) && !path.StartsWith(Application.dataPath))
            {
                fullPath = Application.dataPath.Replace("Assets", path);
            }
            else
            {
                path = Tools.RelativeTo(path, Tools.GetResPath(), true);
            }

            if (File.Exists(fullPath))
            {
                if (!path.EndsWith(".meta"))
                {
                    SetBundleName(path, null);
                }
            }
            else if (Directory.Exists(fullPath))
            {
                string[] files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    if (!path.EndsWith(".meta"))
                    {
                        string assetPath = Tools.RelativeTo(f, Application.dataPath, true);
                        SetBundleName(assetPath, null);
                    }
                }
            }
        }

    }
}