using System;
using UnityEngine;

namespace GameFramework
{
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
