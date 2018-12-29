using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  GameFramework {
    
    /// <summary>
    /// 协程管理器
    /// </summary>
    public class GameCoroutineManager : SingletonComp<GameCoroutineManager>
    {
        Dictionary<int, Coroutine> sMap;

        void Awake()
        {
            sMap = new Dictionary<int, Coroutine>();
        }

        public int StartCor(IEnumerator routine)
        {
            Coroutine coroutine = StartCoroutine(routine);
            if(null != coroutine)
            {
                int hashcode = coroutine.GetHashCode();
                sMap[hashcode] = coroutine;
                return hashcode;
            }
            return 0;
        }

        public bool StopCor(int hashCode)
        {
            Coroutine coroutine;
            if(sMap.TryGetValue(hashCode, out coroutine))
            {
                base.StopCoroutine(coroutine);
                sMap.Remove(hashCode);
                return true;
            }
            return false;
        }

        public void StopAllCors()
        {
            StopAllCoroutines();
            sMap.Clear();
        }

        public override bool Dispose()
        {
            StopAllCors();
            return true;
        }
    }
}

