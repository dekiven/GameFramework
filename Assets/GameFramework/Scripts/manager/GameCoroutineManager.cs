using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  GameFramework {
    
    /// <summary>
    /// 协程管理器
    /// </summary>
    public class GameCoroutineManager : SingletonComp<GameCoroutineManager>
    {
        //TODO:
        public new int StartCoroutine(IEnumerator routine)
        {
            base.StartCoroutine(routine);
            return -1;
        }

        //void test()
        //{
        //    StartCoroutine
        //}
    }
}

