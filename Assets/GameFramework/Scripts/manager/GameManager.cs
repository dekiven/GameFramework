using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameFramework
{
    public class GameManager : SingletonComp<GameManager>
    {
        GameUIManager mUiMgr;
        GameLuaManager mLuaMgr;
        GameResManager mResMgr;
        GameUpdateManager mUpMgr;
        //Game

        void Awake()
        {
            mResMgr = GameResManager.Instance;
            mLuaMgr = GameLuaManager.Instance;
            mUpMgr = GameUpdateManager.Instance;
            mUiMgr = GameUIManager.Instance;
            LogFile.Init(Tools.GetWriteableDataPath("game_log.log"));
        }

        void Start()
        {
            // 检查新包资源是否拷贝，检查服务器资源更新，完成后启动lua虚拟机和其他游戏逻辑
            checkRes();
        }


        private void Update()
        {
            //处理事件管理器在主线程的消息,暂时没有处理其他线程的事件分发
            EventManager.progressMainEvents();
        }

        /// <summary>
        /// 检查新包资源是否拷贝，检查服务器资源更新，完成后启动lua虚拟机和其他游戏逻辑
        /// </summary>
        private void checkRes()
        {
            //启动lua虚拟机之前先将lua等资源拷贝到writeblePath
            mUpMgr.CheckUpdate(delegate (float percent, string msg)
            {
                if (Equals(-1f, percent) || Equals(1f, percent))
                {
                    LogFile.Log("callback of copy file:{0},{1}", percent, msg);
                    if (Equals(1f, percent))
                    {
                        //在资源更新完毕后再次初始化GameResMranager(正常的游戏逻辑会初始化两次GameResMranager)
                        mResMgr.Initialize(delegate ()
                        {
                            LogFile.Log("TestLoadRes");
                            mLuaMgr.InitStart();
                            mLuaMgr.CallGlobalFunc("TestLoadRes");
                        });
                    }
                    else
                    {
                        LogFile.Error("更新资源失败，关闭程序");
                        //TODO:包体资源拷贝失败，进行相应操作
                        Application.Quit();
                    }
                }
            });
        }

        public override bool Dispose()
        {
            mUiMgr.DestroyComp();
            mUpMgr.DestroyComp();
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