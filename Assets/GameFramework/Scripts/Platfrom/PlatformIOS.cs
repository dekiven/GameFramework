using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GameFramework
{
#if UNITY_IOS
    public class PlatformIOS : PlatformBase
    {
    #region 导入函数声明
        [DllImport("__Internal")]
        private static extern void GFTakePhoto();

        [DllImport("__Internal")]
        private static extern void GFTakeAlbum();

        [DllImport("__Internal")]
        private static extern void GFSetNoticeObFunc(string gameobjName, string funcName);

        [DllImport("__Internal")]
        private static extern void GFSetNotifySplitStr(string s);

        [DllImport("__Internal")]
        private static extern void GFStartPurchase(string pid, string externalData);
    #endregion

        public override void SetNoticeObFunc(string gameobjName, string funcName)
        {
            GFSetNoticeObFunc(gameobjName, funcName);
        }

        public override void SetNotifySplitStr(string s)
        {
            GFSetNotifySplitStr(s);
        }

        public override void TakeImageAlbum()
        {
            GFTakeAlbum();
        }

        public override void TakeImagePhoto()
        {
            GFTakePhoto();
        }

        //public override void Restart(float delaySec)
        //{
        //    //TODO:
        //}

        public override void StartPurchase(string pid, string externalData)
        {
            GFStartPurchase(pid, externalData);
        }

        public override void InstallNewApp(string path)
        {
            //path为appid，一般是一串数字
            var url = string.Format("itms-apps://itunes.apple.com/cn/app/id{0}?mt=8", path);
            Application.OpenURL(url);
        }
    }
#else
    #region 空实现
    public class PlatformIOS : PlatformBase
    {
        
    }
    #endregion 空实现
#endif
}
