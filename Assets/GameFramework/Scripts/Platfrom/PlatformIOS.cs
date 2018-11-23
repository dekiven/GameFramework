using System;
using System.Runtime.InteropServices;

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
            GFTakePhoto();
        }

        public override void TakeImagePhoto()
        {
            GFTakeAlbum();
        }

        //public override void Restart(float delaySec)
        //{
        //    //TODO:
        //}

        public override void StartPurchase(string pid, string externalData)
        {
            GFStartPurchase(pid, externalData);
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
