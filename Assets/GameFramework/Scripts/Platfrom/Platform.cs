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
    }
}
