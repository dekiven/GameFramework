using System;
using System.Collections.Generic;
using LuaInterface;

namespace GameFramework
{
    public class SceneMgr : Singleton<SceneMgr>
    {
        private string curGroup;
        private ObjDict<int> mCount;
        //private ObjDict<string> mScenes;
        //private Dictionary<string, List<string>> mGroupAsbs;

        /// <summary>
        /// 请勿直接调用构造函数，请使用 Instance 方法获取单例
        /// </summary>
        public SceneMgr()
        {
            curGroup = "common";
            mCount = new ObjDict<int>();
            //mScenes = new ObjDict<string>();
            //mGroupAsbs = new Dictionary<string, List<string>>();
        }

        public void LoadScene(string asbName, string sceneName, bool sync, bool add, Action<float> callback = null, LuaFunction luaFunc = null)
        {
            ResManager.Instance.LoadScene(asbName, sceneName, sync, add, (float progress) => 
            {
                if(progress.Equals(0))
                {
                    //开始载入场景时，计数+1
                    mCount.AddObj(curGroup, asbName, mCount.GetObj(asbName, sceneName) + 1);
                }
                if (progress.Equals(-1f))
                {
                    LogFile.Warn("加载场景失败， sceneName :{0} , asbName :{1}", sceneName, asbName);
                }
                if(null != callback)
                {
                    callback(progress);
                }
            }, luaFunc);
        }

        public void SetCurGroup(string group)
        {
            curGroup = group;
        }

        /// <summary>
        /// 游戏中所有场景均不与其他资源打包在同一个 AssetBundle，可以多个场景打同一个 AssetBundle 包，清理时直接根据引用计数清理即可
        /// </summary>
        /// <param name="group">Group.</param>
        public void ClearGroup(string group)
        {
            Dictionary<string, int> dict = mCount.GetSubDict(group);
            foreach (var item in dict)
            {
                ResManager.Instance.UnloadAssetBundle(item.Key, false, item.Value);
            }
            mCount.ClearSubDict(group);
        }
    }
}
