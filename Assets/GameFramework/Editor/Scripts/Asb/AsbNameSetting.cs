using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace GameFramework
{
    public class AsbNameSetting
    {

        [MenuItem("Assets/GF/设置Asb名字", priority=20)]
        public static void SetAsbName()
        {
            string[] selections = Selection.assetGUIDs;
            foreach (var item in selections)
            {
                string resPath = AssetDatabase.GUIDToAssetPath(item);
                //Debug.Log (resPath);
                _setBundleNameByPath(resPath);
            }
        }

        [MenuItem("Assets/GF/清理Asb名字", priority=21)]
        public static void ClearAsbName()
        {
            string[] selections = Selection.assetGUIDs;
            foreach (var item in selections)
            {
                string resPath = AssetDatabase.GUIDToAssetPath(item);
                //Debug.Log (resPath);
                _clearBundleNameByPath(resPath);
            }
        }


        public static bool GetHasAsbName(string path)
        {
            path = Tools.RelativeTo(path, Application.dataPath, true);
            AssetImporter importer = AssetImporter.GetAtPath(path);
            return importer && !string.IsNullOrEmpty(importer.assetBundleName);
        }

        //[MenuItem("Assets/打包测试")]
        //public static void BuildAll()
        //{
        //    string outDir = Application.dataPath.Replace("Assets", "AssetBundles/test");
        //    Debug.Log("打包路径：" + outDir);
        //    Tools.CheckDirExists(outDir, true);
        //    BuildPipeline.BuildAssetBundles(outDir, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        //}

        private static void _setBundleNameByPath(string path)
        {
            string fullPath = path;
            //if (!Path.IsPathRooted(path) && !path.StartsWith(Application.dataPath, StringComparison.Ordinal))
            if (!path.StartsWith(Application.dataPath, StringComparison.Ordinal))
            {
                fullPath = Tools.PathCombine(Directory.GetParent(Application.dataPath).ToString(), path);
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
                    //asbName = Tools.RelativeTo(asbName, Tools.GetResPath()) + GameConfig.STR_ASB_EXT;
                    asbName = Tools.GetAsbName(Tools.RelativeTo(asbName, Tools.GetResPath()));
                    SetBundleName(path, asbName.ToLower());
                }
            }
            else if (Directory.Exists(fullPath))
            {
                string[] files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);
                //string asbName = Tools.RelativeTo(fullPath, Tools.GetResPath()) + GameConfig.STR_ASB_EXT;
                string asbName = Tools.GetAsbName(Tools.RelativeTo(fullPath, Tools.GetResPath()));

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
            if(BuilderConfig.IsResFile(resPath))
            {
                AssetImporter importer = AssetImporter.GetAtPath(resPath);

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
        }


        private static void _clearBundleNameByPath(string path)
        {
            string fullPath = path;
            if (!Path.IsPathRooted(path) && !path.StartsWith(Application.dataPath, StringComparison.Ordinal))
            {
                fullPath = Application.dataPath.Replace("Assets", path);
            }
            else
            {
                path = Tools.RelativeTo(path, Tools.GetResPath(), true);
            }

            if (File.Exists(fullPath))
            {
                if (!path.EndsWith(".meta", StringComparison.Ordinal))
                {
                    SetBundleName(path, null);
                }
            }
            else if (Directory.Exists(fullPath))
            {
                string[] files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    if (!path.EndsWith(".meta", StringComparison.Ordinal))
                    {
                        string assetPath = Tools.RelativeTo(f, Application.dataPath, true);
                        SetBundleName(assetPath, null);
                    }
                }
            }
        }

    }
}