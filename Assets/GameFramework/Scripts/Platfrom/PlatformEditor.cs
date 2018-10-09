using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GameFramework
{
#if UNITY_EDITOR
    public class PlatformEditor : PlatformBase
    {
        public override void TakeAlbum()
        {
            LogFile.Log("TakeAlbum");
        }

        public override void TakePhoto()
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
