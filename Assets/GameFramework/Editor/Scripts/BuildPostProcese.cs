using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;

//xcode导出项目修改参考：
//参考：Unity iOS 插件开发与SDK接入
//https://www.yangzhenlin.com/unity-ios-plugin/

#region 更详细参考
//Unity 自动化打包XCode工程
//https://blog.csdn.net/RadiusCLL/article/details/81219862
//Unity ios一键出包
//https://blog.csdn.net/xiao756757373xiao/article/details/78286293
#endregion 更详细参考

namespace GameFramework
{
    public static class BuildPostProcese
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (BuildTarget.iOS != target)
            {
                return;
            }
            //修改 PBXProject
            ModifyPBXProject(pathToBuiltProject);
            //修改 Plist
            ModifyPlist(pathToBuiltProject);
            Debug.Log("Xcode修改完毕");
        }

        private static void ModifyPBXProject(string path)
        {
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();

            proj.ReadFromString(File.ReadAllText(projPath));
            string target = proj.TargetGuidByName("Unity-iPhone");

            //执行修改操作

            ////修改 SEARCH_PATHS
            //proj.SetBuildProperty(target, "LIBRARY_SEARCH_PATHS", "$(inherited)");
            //proj.AddBuildProperty(target, "LIBRARY_SEARCH_PATHS", "$(SRCROOT)");
            //proj.AddBuildProperty(target, "LIBRARY_SEARCH_PATHS", "$(PROJECT_DIR)/Libraries");

            //添加.framework /.tbd
            //bool 参数 true 表示框架是 optional，false 表示框架是 required。
            ////苹果内购
            //proj.AddFrameworkToProject(target, "StoreKit.framework", false);

            ////添加 OTHER_LDFLAGS
            //proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");

            File.WriteAllText(projPath, proj.WriteToString());
        }

        private static void ModifyPlist(string path)
        {
            //Info.plist
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            //ROOT
            PlistElementDict rootDict = plist.root;

            //执行修改操作
            //设置使用简体中文
            rootDict.SetString("CFBundleDevelopmentRegion", "zh_CN");

            //iOS 10 设置使用权限说明
            //摄像机权限
            rootDict.SetString("NSCameraUsageDescription", "App需要您的同意,才能使用相机");
            //摄像机权限
            rootDict.SetString("NSPhotoLibraryUsageDescription", "App需要您的同意,才能访问相册");
            ////定位权限
            //rootDict.SetString("NSLocationWhenInUseUsageDescription", "LBS");
            ////录音权限
            //rootDict.SetString("NSMicrophoneUsageDescription", "VoiceChat");

            ////添加第三方应用的 URL Scheme 到白名单
            //PlistElementArray LSApplicationQueriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
            ////微信
            //LSApplicationQueriesSchemes.AddString("weixin");

            //PlistElementArray urlTypes = rootDict.CreateArray("CFBundleURLTypes");

            ////添加自己应用的 URL Scheme
            ////网页唤起
            //PlistElementDict webUrl = urlTypes.AddDict();
            //webUrl.SetString("CFBundleTypeRole", "Editor");
            //webUrl.SetString("CFBundleURLName", "web");
            //PlistElementArray webUrlScheme = webUrl.CreateArray("CFBundleURLSchemes");
            //webUrlScheme.AddString("productname"); //换成自己的产品名

            //写入
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif