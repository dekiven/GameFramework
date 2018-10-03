using System;
namespace GameFramework
{
#if UNITY_EDITOR
    public class PlatformEditor : PlatformBase
    {
        public override void TakeAlbum()
        {
            LogFile.Log("TakeAlbum");
            //if (null != mPluginObj)
            {
                LogFile.Log("TakeAlbum 1 ");
                //mPluginObj.Call("takeFromPhoto");
            }
        }

        public override void TakePhoto()
        {
            LogFile.Log("TakePhoto");
            //if (null != mPluginObj)
            {
                LogFile.Log("TakePhoto 1 ");
                //mPluginObj.Call("takeFromAlbum");
            }
        }
    }
#else
    #region
    public class PlatformEditor : PlatformBase
    {
    }
    #endregion
#endif
}
