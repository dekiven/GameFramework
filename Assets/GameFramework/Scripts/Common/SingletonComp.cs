using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    /// <summary>
    /// 单例组件基类
    /// </summary>
    public class SingletonComp<T> : MonoBehaviour where T : Component
    {
        //单例模式组件 begin----------------------------------------------
        private static volatile T sInstance = null;
        protected static readonly object syncRoot = new object();
        public static T Instance
        {
            get
            {
                if (null == sInstance)
                {
                    lock (syncRoot)
                    {
                        if (null == sInstance)
                        {
                            T[] instances = FindObjectsOfType<T>();
                            if (instances != null)
                            {
                                for (var i = 0; i < instances.Length; i++)
                                {
                                    Destroy(instances[i].gameObject);
                                }
                            }
                            GameObject go = new GameObject();
                            go.name = typeof(T).ToString();
                            sInstance = go.AddComponent<T>();
                            DontDestroyOnLoad(go);
                        }
                    }
                }
                return sInstance;
            }
        }

        /// <summary>
        /// 销毁单例组件，会调用clearComp方法
        /// </summary>
        public void DestroyComp()
        {
            if(null != sInstance)
            {
                lock (syncRoot)
                {
                    if (null != sInstance)
                    {
                        Destroy(sInstance.gameObject);
                        sInstance = null;
                    }
                }
            }
        }

        void OnDestroy()
        {
            if (Dispose()) 
            {
                sInstance = null;
            }
            else
            {
                LogFile.Warn("单例组件清理失败");
            }

        }

        /// <summary>
        /// 单例组件被销毁时调用，子类可派生
        /// </summary>
        public virtual bool Dispose()
        {
            LogFile.Log("clearComp:" + typeof(T));
            return true;
        }

        public static bool HasInstance()
        {
            return sInstance != null;
        }
        //单例模式组件 end================================================
    }

}