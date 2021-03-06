﻿using System;
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
        private static string sStrSplit = GameDefine.STR_SPLIT_STR;
        public static string SplitStr { get { return sStrSplit; } }

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
        /// 获取渠道名称，方便区分处
        /// TODO：待实现
        /// </summary>
        /// <returns>The angent name.</returns>
        public static string GetAngentName()
        {
            if (null != mPlatform)
            {
                return mPlatform.GetAngentName();
            }
            return ""; 
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

        public static void Quit()
        {
            mPlatform.Quite();
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
        /// <param name="externalData">额外数据</param>
        public static void StartPurchase(string pid, string externalData)
        {
            mPlatform.StartPurchase(pid, externalData);
        }

        /// <summary>
        /// 检查app版本，返回true表示是最新或者已经更新完成
        /// </summary>
        /// <param name="callback">Callback.</param>
        public static void CheckAppVer(Action<bool, string> callback)
        {
            mPlatform.CheckAppVer(callback);
        }

        public static void UpdateApp()
        {
            mPlatform.UpdateApp();
        }

        public static void Copy2Clipboard(string content)
        {
            mPlatform.Copy2Clipboard(content);
        }

        public static string GetFirstClipboard()
        {
            return mPlatform.GetFirstClipboard();
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
