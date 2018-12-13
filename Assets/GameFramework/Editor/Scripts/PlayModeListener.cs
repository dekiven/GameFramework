using System;
using UnityEditor;

namespace GameFramework
{
    [InitializeOnLoad]
    public class PlayModeListener
    {
        static PlayModeListener()
        {
            if(!GameConfig.useAsb)
            {
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
            }
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
                        SceneSettingManager.setAllSceneToBuildSetting();
                    }
                    break;
                case PlayModeStateChange.ExitingPlayMode :
                    //SceneManager.setReleaseSceneToBuildSetting();
                    break;
            }
        }
    }
}
