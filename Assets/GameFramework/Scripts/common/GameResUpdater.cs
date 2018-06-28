using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO:待事件管理器完善后实现

//资源更新器只在使用asb的情况下使用，editor模式使用原始资源（位于Assets/BundleRes文件夹）
public class GameResUpdater : MonoBehaviour {

    //单例模式组件 begin----------------------------------------------
    private static volatile GameResUpdater sInstance;
    private static object syncRoot = new object();
    public static GameResUpdater Instance
    {
        get
        {
            if (sInstance == null)
            {
                lock (syncRoot)
                {
                    if (sInstance == null)
                    {
                        GameResUpdater[] instances = FindObjectsOfType<GameResUpdater>();
                        if (instances != null)
                        {
                            for (var i = 0; i < instances.Length; i++)
                            {
                                Destroy(instances[i].gameObject);
                            }
                        }
                        GameObject go = new GameObject();
                        go.name = typeof(GameResUpdater).ToString();
                        sInstance = go.AddComponent<GameResUpdater>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return sInstance;
        }
    }
    //单例模式组件 end================================================

    public void UpdateRes(string url)
    {
        
    }

    private IEnumerator onUpdateRes(string url)
    {
        yield return null;
    }
}
