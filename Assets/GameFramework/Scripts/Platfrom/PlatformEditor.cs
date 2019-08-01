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

        public static void Delay(float sec, Action call)
        {
            StartCoroutine(_delay(sec, call));
        }

        static IEnumerator _delay(float sec, Action call)
        {
            yield return new WaitForSeconds(sec);
            if(null != call)
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
