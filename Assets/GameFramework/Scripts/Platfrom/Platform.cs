using System;
namespace GameFramework
{
    public static class Platform
    {
        private static PlatformBase mPlatform = new PlatformBase();

        public static void SetPlatformInstance(PlatformBase platform)
        {
            mPlatform = platform;
        }

        public static void SetNoticeObjFunc(string gameobjName, string funcName)
        {
            mPlatform.SetNoticeObFunc(gameobjName, funcName);
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
