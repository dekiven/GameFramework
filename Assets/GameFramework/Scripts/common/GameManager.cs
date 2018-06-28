using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameFramework
{
    public class GameManager : SingletonComp<GameManager>
    {
        GameLuaManager mLuaMgr;
        GameResManager mResMgr;

        //Game

        void Start()
        {
            mResMgr = GameResManager.Instance;
            mLuaMgr = GameLuaManager.Instance;
            LogFile.Init(Tools.GetWriteableDataPath("game.log"));

            mLuaMgr.InitStart();
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