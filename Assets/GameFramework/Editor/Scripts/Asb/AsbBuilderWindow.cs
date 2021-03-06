﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace GameFramework
{
    class AsbBuilderWindow : EditorWindow
    {
        private BuilderConfig _config;

        //% (ctrl on Windows, cmd on macOS), # (shift), & (alt).
        //https://docs.unity3d.com/ScriptReference/MenuItem.html
        [MenuItem("GameFramework/Show AssetBundle Builder #%g")]
        public static void Open()
        {
            AsbBuilderWindow _instance = (AsbBuilderWindow)EditorWindow.GetWindow(typeof(AsbBuilderWindow), false, "GameFramework", true);
            _instance.Init();
            _instance.Show();
        }

        private void Init()
        {
            //titleContent = new GUIContent("Builder");
            minSize = new Vector2(300f, 400f);
            /**
             * 空合并运算符(??)：
用于定义可空类型和引用类型的默认值。如果此运算符的左操作数不为null，则此运算符将返回左操作数，否则返回右操作数。
例如：a??b 当a为null时则返回b，a不为null时则返回a本身。
空合并运算符为右结合运算符，即操作时从右向左进行组合的。如，“a??b??c”的形式按“a??(b??c)”计算
            **/
            _config = _config ?? new BuilderConfig();
            GameConfig.Load();
        }

        void OnGUI()
        {
            if (this._config == null)
                Init();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            _config.LoadPath = EditorGUILayout.TextField("Load Path", _config.LoadPath);
            if (GUILayout.Button("Select"))
            {
                var path = EditorUtility.OpenFolderPanel("Load", _config.LoadPath, "");
                _config.LoadPath = string.IsNullOrEmpty(path) ? _config.LoadPath : path;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            _config.ExportPath = EditorGUILayout.TextField("Export Path", _config.ExportPath);
            if (GUILayout.Button("Select"))
            {
                var path = EditorUtility.OpenFolderPanel("Load", _config.ExportPath, "");
                _config.ExportPath = string.IsNullOrEmpty(path) ? _config.ExportPath : path;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BuildAssetBundleOptions:");
            if (GUILayout.Button(_config.options.ToString()))
            {
                ShowTypeNamesMenu(
                     _config.options.ToString(), optionsList,
                    (string selectedTypeStr) =>
                    {
                        _config.options = _getEnumByString<BuildAssetBundleOptions>(selectedTypeStr);
                    }
                );
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BuildTarget:");
            if (GUILayout.Button(_config.target.ToString()))
            {
                ShowTypeNamesMenu(
                     _config.target.ToString(), targetList,
                    (string selectedTypeStr) =>
                    {

                        _config.target = _getEnumByString<BuildTarget>(selectedTypeStr);
                    }
                );
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("重置资源导出路径"))
            {
                _config.DeletConfig();
                _config = new BuilderConfig();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("LuaBuildTest"))
            {
                //List<AssetBundleBuild> list = new List<AssetBundleBuild>();

                ////测试常规打包载入
                //list.Add(AsbBuilder.GenBuildByDir(_config.LoadPath, Tools.GetResPath(), "*.*"));

                ////更新相关测试 begin ------------------------------------------------
                ////测试资源依赖
                ////测试资源依赖：1.   多个文件依赖同一个文件
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t1/common.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T1/cmat.mat", "Assets/BundleRes/DepTest/T1/img.png", } });
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t1/sphere.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T1/Sphere.prefab", } });
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t1/cube.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T1/Cube.prefab", } });
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t1/capsule.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T1/Capsule.prefab", } });

                ////测试资源依赖：2.   一个文件依赖多个文件
                ////list = new List<AssetBundleBuild>();
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t2/obj.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T2/Obj.prefab", } });
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t2/sphere.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T2/Sphere.prefab", } });
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t2/cube.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T2/Cube.prefab", } });
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t2/capsule.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T2/Capsule.prefab", } });
                ////测试资源依赖：3.   一列依赖对象中有变化
                ////list = new List<AssetBundleBuild>();
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t3/cube.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T3/Cube.prefab", } });
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t3/img2.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T3/img2.png", } });
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t3/mat_img2.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T3/mat_img2.mat", } });
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t3/obj2.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T3/Obj2.prefab", } });
                ////测试资源依赖：4.对有c#脚本的绑定的依赖
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t4/sphere.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T4/Sphere.prefab", } });
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t4/cube.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T4/Cube.prefab", } });
                //list.Add(new AssetBundleBuild() { assetBundleName = "deptest/t4/capsule.asb", assetNames = new string[] { "Assets/BundleRes/DepTest/T4/Capsule.prefab", } });
                ////更新相关测试 end ==================================================

                //if (BuildTarget.NoTarget == _config.target)
                //{
                //    AsbBuilder.BuildAsb(_config.ExportPath, list.ToArray(), _config.options, BuildTarget.Android);
                //    AsbBuilder.BuildAsb(_config.ExportPath, list.ToArray(), _config.options, BuildTarget.iOS);
                //    AsbBuilder.BuildAsb(_config.ExportPath, list.ToArray(), _config.options, BuildTarget.StandaloneWindows);
                //}
                //else
                //{
                //    AsbBuilder.BuildAsb(_config.ExportPath, list.ToArray(), _config.options, _config.target);
                //}
                AsbBuilder.BuildAllLua(_config);
                Close();
            }
            if (GUILayout.Button("ResBuildTest"))
            {
                AsbBuilder.BuildAllRes(_config);
                Close();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("BuildAll"))
            {
                AsbBuilder.BuildAll(_config);
                Close();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30);
            bool v = GUILayout.Toggle(GameConfig.HasDebugView, "是否显示DebugView");
            if (v != GameConfig.HasDebugView)
            {
                GameConfig.HasDebugView = v;
                GameConfig.Save();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("设置清理不打包的 Scenens "))
            {
                SceneSettingManager.SetReleaseSceneToBuildSetting();
            }
            if (GUILayout.Button("显示 BuildSettings"))
            {
                EditorApplication.ExecuteMenuItem("File/Build Settings...");
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }

        private List<string> optionsList = new List<string>()
        {
            BuildAssetBundleOptions.None.ToString(),
            BuildAssetBundleOptions.DeterministicAssetBundle.ToString(),
            BuildAssetBundleOptions.IgnoreTypeTreeChanges.ToString(),
            //BuildAssetBundleOptions.OmitClassVersions.ToString(),
            BuildAssetBundleOptions.UncompressedAssetBundle.ToString(),
        };

        private List<string> targetList = new List<string>()
        {
            BuildTarget.StandaloneWindows.ToString(),
            BuildTarget.StandaloneOSX.ToString(),
            BuildTarget.Android.ToString(),
            BuildTarget.iOS.ToString(),
            //表示打包所有平台（IOS和Android), 有bug暂时分开打包
            //BuildTarget.NoTarget.ToString(),
        };


        private T _getEnumByString<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        void ShowTypeNamesMenu(string current, List<string> contents, Action<string> ExistSelected)
        {
            var menu = new GenericMenu();

            for (var i = 0; i < contents.Count; i++)
            {
                var type = contents[i];
                var selected = false;
                if (type == current) selected = true;

                menu.AddItem(
                    new GUIContent(type),
                    selected,
                    () =>
                    {
                        ExistSelected(type);
                    }
                );
            }
            menu.ShowAsContext();
        }
    }

}