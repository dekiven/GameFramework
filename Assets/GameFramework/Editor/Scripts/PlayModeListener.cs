using System;
using UnityEditor;

namespace GameFramework
{
    [InitializeOnLoad]
    public class PlayModeListener
    {
        static PlayModeListener()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += EditorTools.Update;
        }

        static void OnPlayModeChanged(PlayModeStateChange status)
        {
            switch(status)
            {
                case PlayModeStateChange.EnteredEditMode :
                    break;
                case PlayModeStateChange.ExitingEditMode :
                    break;
                case PlayModeStateChange.EnteredPlayMode :
                    if (!GameConfig.useAsb)
                    {
                        SceneSettingManager.SetAllSceneToBuildSetting();
                    }
                    break;
                case PlayModeStateChange.ExitingPlayMode :
                    break;
            }
        }
    }
}
