using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameFramework
{
    public class GameManager : SingletonComp<GameManager>
    {
        GameLuaManager mLuaMgr;
        GameResManager mResMgr;
        GameResUpdater mResUp;
        //Game

        void Start()
        {
            mResMgr = GameResManager.Instance;
            mLuaMgr = GameLuaManager.Instance;
            mResUp = GameResUpdater.Instance;
            LogFile.Init(Tools.GetWriteableDataPath("game.log"));

            //启动lua虚拟机之前先将lua等资源拷贝到writeblePath
            mResUp.CheckLocalCopy(delegate (float percent, string msg) {
                if (Equals(-1f, percent) || Equals(1f, percent))
                {
                    LogFile.Log("callback of copy file:{0},{1}", percent, msg);
                    if (Equals(1f, percent))
                    {
                        LogFile.Log("TestLoadRes");
                        mLuaMgr.InitStart();
                        mLuaMgr.CallGlobalFunc("TestLoadRes");
                    }
                }
            });



        }

        protected override bool clearComp()
        {
            base.DestroyComp();
            
            mLuaMgr.DestroyComp();
            mResMgr.DestroyComp();
            LogFile.CloseLog();

            return true;
        }

        /// <summary>
        /// 在强退的情况下不会被调用到，可以在clear中处理
        /// </summary>
        void OnApplicationQuit()
        {
            DestroyComp();
            Debug.Log("OnApplicationQuit");
        }

        void OnApplicationFocus(bool focus)
        {
            Debug.Log("OnApplicationFocus:"+focus);
        }

        void OnApplicationPause(bool pause)
        {
            Debug.Log("OnApplicationPause");
        }
    }
}