using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    using Lm = LanguageManager;
#if UNITY_ANDROID
    public class PlatformAnd : PlatformBase
    {
        //private AndroidJavaClass mClass;
        private readonly AndroidJavaObject mPluginObj;

        public PlatformAnd()
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                LogFile.Warn("PlatformAnd 开始");
                AndroidJavaClass _class = new AndroidJavaClass("com.dekiven.gameframework.GF_PluginAndroid");
                if(null != _class)
                {
                    //_class.CallStatic("start");
                    mPluginObj = _class.CallStatic<AndroidJavaObject>("getInstance");
                }
                if (null == mPluginObj)
                {
                    LogFile.Error("GameFramework Android插件加载失败,请检查arr文件是否存在。");
                }
            }
        }

        public override void SetNoticeObFunc(string gameobjName, string funcName)
        {
            if (null != mPluginObj)
            {
                mPluginObj.Call("setNoticeObFunc", gameobjName, funcName);
            }
        }

        public override void SetNotifySplitStr(string s)
        {
            if (null != mPluginObj)
            {
                mPluginObj.Call("setNoticeSplitStr", s);
            }
        }

        public override void TakeImageAlbum()
        {
            if(null != mPluginObj)
            {
                mPluginObj.Call("takeFromAlbum");
            }
        }

        public override void TakeImagePhoto()
        {
            if (null != mPluginObj)
            {
                mPluginObj.Call("takeFromPhoto");
            }
        }

        public override void StartPurchase(string pid, string externalData)
        {
            if (null != mPluginObj)
            {
                mPluginObj.Call("startPurchase", pid, externalData);
            }
        }

        public override void CheckAppVer(Action<bool> callback)
        {
            //base.CheckAppVer(callback);
            string version = Tools.GetStringValue(GameUpManager.Instance.ServConf, Application.identifier, "0.0.0");
            string curVersion = Application.version;
            if(Tools.CompareVersion(version, curVersion) > 0)
            {
                string apkName = Application.identifier + "_v" + version + ".apk";
                //更新 apk
                List<string> urls = new List<string>();
                foreach (var item in GameUpManager.Instance.ResServList)
                {
                    urls.Add(Tools.PathCombine(item.path, apkName));
                }
                LargeFileDownloader downloader = new LargeFileDownloader();
                string savePath = Tools.GetWriteableDataPath(apkName);
                downloader.DownloadFile(urls, savePath, (double arg1, string arg2) => 
                {
                    EventManager.notifyMain("UpdateDownloadView", "", Lm.GetStr("下载新版本 apk (v") + version + Lm.GetStr(")..."), (float)arg1);
                    if(arg1.Equals(1d) && arg2.Equals(LargeFileDownloader.STR_SUCCEEDED))
                    {
                        InstallNewApp(savePath);
                        callback(false);
                    }
                    if(arg1.Equals(1d))
                    {
                        downloader.Dispose();
                    }
                });
            }
            else
            {
                callback(true);
            }
        }

        public override bool HasAngentExitDialog()
        {
            if (null != mPluginObj)
            {
                return mPluginObj.Call<bool>("hasAngentExitDialog");
            }
            return false;
        }

        public override void Restart(float delaySec)
        {
            if (null != mPluginObj)
            {
                if(delaySec < 1)
                {
                    delaySec = 1;
                }
                mPluginObj.Call("restart", delaySec);
                Application.Quit();
            }
        }

        public override void InstallNewApp(string path)
        {
            if (null != mPluginObj)
            {
                mPluginObj.Call("installApk", Tools.GetWriteableDataPath(path));
            }
        }

        public override void Copy2Clipboard(string content)
        {
            if (null != mPluginObj)
            {
                mPluginObj.Call("copy2Clipboard", content);
            }
        }

        public override string GetFirstClipboard()
        {
            if (null != mPluginObj)
            {
                return mPluginObj.Call<string>("getFirstClipboard");
            }
            return string.Empty;
        }
        //=====================================test--------------------------------------
        public override void test1()
        {
            if (null != mPluginObj)
            {
                mPluginObj.Call("test1");
            }
        }

        public override void test2()
        {
            if (null != mPluginObj)
            {
                mPluginObj.Call("test2");
            }
        }
        //--------------------------------------test=====================================
    }
#else
    #region 空实现
    public class PlatformAnd : PlatformBase
    {
        
    }
    #endregion
#endif
}
