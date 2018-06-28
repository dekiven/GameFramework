using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    //TODO:待事件管理器完善后实现

    //资源更新器只在使用asb的情况下使用，editor模式使用原始资源（位于Assets/BundleRes文件夹）
    public class GameResUpdater : SingletonComp<GameResUpdater>
    {
        public void UpdateRes(string url)
        {

        }

        private IEnumerator onUpdateRes(string url)
        {
            yield return null;
        }
    }

}