using System;
namespace GameFramework
{
    public static class Platform
    {
        #region OnMessage 通知事件回调名
        public const string STR_EVENT_TAKE_PHOTO = "TakeImagePhoto";
        public const string STR_EVENT_TAKE_ALBUM = "TakeImageAlbum";
        public const string STR_EVENT_START_PURCHASE = "StartPurchase";
        #endregion OnMessage 通知事件回调名

        //通知事件的分隔符
        private static string sStrSplit = "__;__";
        public static string STR_SPLIT { get { return sStrSplit; } }

        private static PlatformBase mPlatform = new PlatformBase();

        public static void SetPlatformInstance(PlatformBase platform)
        {
            mPlatform = platform;
        }

        public static void SetNoticeObjFunc(string gameobjName, string funcName, string notifySplitStr=null)
        {
            mPlatform.SetNoticeObFunc(gameobjName, funcName);
            if(!string.IsNullOrEmpty(notifySplitStr))
            {
                mPlatform.SetNotifySplitStr(notifySplitStr);
                sStrSplit = notifySplitStr;
            }
        }

        public static void TakeImagePhoto()
        {
            if(null != mPlatform)
            {
                mPlatform.TakeImagePhoto();
            }
        }

        public static void TakeImageAlbum()
        {
            if (null != mPlatform)
            {
                mPlatform.TakeImageAlbum();
            }
        }

        /// <summary>
        /// 是否有渠道规定的退出窗口
        /// </summary>
        /// <returns><c>true</c>, if angent exit dialog was hased, <c>false</c> otherwise.</returns>
        public static bool HasAngentExitDialog()
        {
            if(null != mPlatform)
            {
                return mPlatform.HasAngentExitDialog();
            }
            return false;
        }

        public static void Restart(float delaySec)
        {
            mPlatform.Restart(delaySec);
        }

        /// <summary>
        /// Installs the new app.安卓安装apk，ios跳转到商店
        /// </summary>
        /// <param name="path">Path.</param>
        public static void InstallNewApp(string path)
        {
            mPlatform.InstallNewApp(path);
        }

        /// <summary>
        /// 请求支付订单
        /// </summary>
        /// <param name="pid">支付id</param>
        /// <param name="externalJsonData">额外数据</param>
        public static void StartPurchase(string pid, string externalJsonData)
        {
            mPlatform.StartPurchase(pid, externalJsonData);
        }

        //=====================================test--------------------------------------
        public static void test1()
        {
            mPlatform.test1();
        }

        public static void test2()
        {
            mPlatform.test2();
        }
        //--------------------------------------test=====================================
    }
}
