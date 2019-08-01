using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  GameFramework {
    
    /// <summary>
    /// 协程管理器
    /// </summary>
    public class CoroutineMgr : SingletonComp<CoroutineMgr>
    {
        Dictionary<int, Coroutine> mMap;
        List<int> mStopList;


        void Awake()
        {
            mMap = new Dictionary<int, Coroutine>();
            mStopList = new List<int>();
        }

        public int StartCor(IEnumerator routine)
        {
            Coroutine coroutine = StartCoroutine(routine);
            if(null != coroutine)
            {
                int hashcode = coroutine.GetHashCode();
                mMap[hashcode] = coroutine;
                return hashcode;
            }
            return 0;
        }

        /// <summary>
        /// 结束协程，不是实时的，但是支持在其他线程结束线程
        /// </summary>
        /// <param name="hashCode"></param>
        public void StopCor(int hashCode)
        {
            lock (syncRoot)
            {
                if (!mStopList.Contains(hashCode))
                {
                    mStopList.Add(hashCode);
                }
            }
        }

        public void StopAllCors()
        {
            StopAllCoroutines();
            mMap.Clear();
        }

        public void Delay(float delaySec, Action act)
        {
            StartCoroutine(_delay(delaySec, act));
        }

        public override bool Dispose()
        {
            StopAllCors();
            return true;
        }

        #region MonoBehaviour
        void LateUpdate()
        {
            //结束线程
            lock (syncRoot)
            {
                foreach (var hashCode in mStopList)
                {
                    _stopCor(hashCode);
                }
                mStopList.Clear();
            }
        }
        #endregion MonoBehaviour

        #region private
        bool _stopCor(int hashCode)
        {
            Coroutine coroutine;
            if (mMap.TryGetValue(hashCode, out coroutine))
            {
                base.StopCoroutine(coroutine);
                mMap.Remove(hashCode);
                return true;
            }
            return false;
        }

        IEnumerator _delay(float delaySec, Action act)
        {
            yield return new WaitForSeconds(delaySec);
            if(null != act)
            {
                act();
            }
        }
        #endregion private

    }
}

