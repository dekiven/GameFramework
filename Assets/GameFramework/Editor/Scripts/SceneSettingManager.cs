using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    public class SceneSettingManager
    {
        [MenuItem("GameFramework/Add res scenes to setting")]
        public static void SetAllSceneToBuildSetting()
        {
            // 设置场景 *.unity 路径
            string mainPath = Tools.RelativeTo(Tools.GetFrameworkPath(), Application.dataPath, true);
            string resPath = Tools.RelativeTo(Tools.GetResPath(), Application.dataPath, true);

            SetScenesByPaths(new string[] { mainPath, resPath });

        }

        [MenuItem("GameFramework/Set release res scenes to setting")]
        public static void SetReleaseSceneToBuildSetting()
        {
            // 设置场景 *.unity 路径
            string mainPath = Tools.RelativeTo(Tools.GetFrameworkPath(), Application.dataPath, true);
            //string resPath = Tools.RelativeTo(Tools.GetResPath(), Application.dataPath, true);

            SetScenesByPaths(new string[] { mainPath });

        }

        /// <summary>
        /// path 是以Assets/开头的路径
        /// </summary>
        /// <param name="paths">Paths.</param>
        public static void SetScenesByPaths(string[] paths)
        {
            // 遍历获取目录下所有 .unity 文件
            List<string> files = new List<string>();
            foreach(var p in paths)
            {
                files.AddRange(Directory.GetFiles(p, "*.unity", SearchOption.AllDirectories));
                //files.AddRange(Directory.GetFiles(p, "*.unity", SearchOption.AllDirectories));
            }

            // 定义 场景数组
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[files.Count];
            for (int i = 0; i < files.Count; ++i)
            {
                string scenePath = files[i];
                // 通过scene路径初始化
                scenes[i] = new EditorBuildSettingsScene(scenePath, true);
            }

            // 设置 scene 数组
            EditorBuildSettings.scenes = scenes;
        }
    }
}
