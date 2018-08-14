using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    [RequireComponent(typeof(UIHandler))]
    public class ResUpdateView : MonoBehaviour
    {
        private GameUpdateManager mUpMgr;

        void Awake()
        {
            mUpMgr = GameUpdateManager.Instance;
            mUpMgr.UIHandler = gameObject.GetComponent<UIHandler>();
        }

        void OnDestroy()
        {
            mUpMgr.DestroyComp();
        }

        /// <summary>
        /// 检查新包资源是否拷贝，检查服务器资源更新，完成后启动lua虚拟机和其他游戏逻辑
        /// </summary>
        public void CheckUpdate(Action<float, string> callback)
        {
            //启动lua虚拟机之前先将lua等资源拷贝到writeblePath
            mUpMgr.CheckUpdate(callback);
        }
    }
}