using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
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
            //EditorTools.StartCoroutine()
            EdiorCoroutine.Delay(delaySec, () => { EditorApplication.isPlaying = true; });
        }
    }

    public class EdiorCoroutine
    {
        #region Editor 协程相关
        //Editor 协程相关
        private static List<IEnumerator> sCoroutineInProgress = new List<IEnumerator>();
        private static int sCurrentExecute = 0;

        public static void StartCoroutine(IEnumerator newCorou)
        {
            sCoroutineInProgress.Add(newCorou);
        }

        /// <summary>
        /// 绑定到 EditorApplication.update ，处理 Editor 协程
        /// </summary>
        public static void Update()
        {
            if (sCoroutineInProgress.Count <= 0)
            {
                return;
            }

            sCurrentExecute = (sCurrentExecute + 1) % sCoroutineInProgress.Count;
            bool finish = !sCoroutineInProgress[sCurrentExecute].MoveNext();
            if (finish)
            {
                sCoroutineInProgress.RemoveAt(sCurrentExecute);
            }
        }

        /// <summary>
        /// 延时启动，这个时间不精确
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="call"></param>
        public static void Delay(float sec, Action call)
        {
            StartCoroutine(_delay(sec, call));
        }

        static IEnumerator _delay(float sec, Action call)
        {
            var start = EditorApplication.timeSinceStartup;

            yield return null;

            while(start+sec > EditorApplication.timeSinceStartup)
            {
               yield return null;
            }
            if (null != call)
            {
                call();
            }
        }

        #endregion Editor 协程相关
    }
#else
    #region
    public class PlatformEditor : PlatformBase
    {
    }
    #endregion
#endif
}
