using System;
using UnityEngine;

namespace GameFramework
{
#if UNITY_ANDROID
    public class PlatformAnd : PlatformBase
    {
        //private AndroidJavaClass mClass;
        private AndroidJavaObject mPluginObj;

        public PlatformAnd()
        {
            AndroidJavaClass _class = new AndroidJavaClass("com.dekiven.gf_plugin.GF_PluginAndroid");
            if(null != _class)
            {
                _class.CallStatic("start");
                mPluginObj = _class.GetStatic<AndroidJavaObject>("instance");
            }
            if (null == mPluginObj)
            {
                LogFile.Error("GameFramework Android插件加载失败,请检查arr文件是否存在。");
            }
        }

        public override void TakeAlbum()
        {
            LogFile.Log("TakeAlbum");
            if(null != mPluginObj)
            {
                LogFile.Log("TakeAlbum 1 ");
                mPluginObj.Call("takeFromAlbum");
            }
        }

        public override void TakePhoto()
        {
            LogFile.Log("TakePhoto");
            if (null != mPluginObj)
            {
                LogFile.Log("TakePhoto 1 ");
                mPluginObj.Call("takeFromPhoto");
            }
        }
    }
#else
    #region 空实现
    public class PlatformAnd : PlatformBase
    {
        
    }
    #endregion
#endif
}
