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
        #endregion

        public override void TakeAlbum()
        {
            GFTakePhoto();
        }

        public override void TakePhoto()
        {
            GFTakeAlbum();
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
