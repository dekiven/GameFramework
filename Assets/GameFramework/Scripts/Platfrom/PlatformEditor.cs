using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GameFramework
{
#if UNITY_EDITOR
    public class PlatformEditor : PlatformBase
    {
        public override void TakeImageAlbum()
        {
            LogFile.Log("TakeAlbum");
        }

        public override void TakeImagePhoto()
        {
            LogFile.Log("TakePhoto");
        }

        public override void Restart(float delaySec)
        {
            EditorApplication.isPlaying = false;
            //TODO:延时实现
            //EditorApplication.isPlaying = true;
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
