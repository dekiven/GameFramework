using System;
namespace GameFramework
{
    public static class Platform
    {
        private static PlatformBase mPlatform;

        public static void SetPlatformInstance(PlatformBase platform)
        {
            mPlatform = platform;
        }

        public static void SetNoticeObjFunc(string gameobjName, string funcName)
        {
            mPlatform.SetNoticeObFunc(gameobjName, funcName);
        }

        public static void TakePhoto()
        {
            if(null != mPlatform)
            {
                mPlatform.TakePhoto();
            }
        }

        public static void TakeAlbum()
        {
            if (null != mPlatform)
            {
                mPlatform.TakeAlbum();
            }
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
    }
}
