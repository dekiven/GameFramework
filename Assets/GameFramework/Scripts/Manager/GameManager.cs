using LuaInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
namespace GameFramework
{
    public class GameManager : SingletonComp<GameManager>
    {
        GameUIManager mUiMgr;
        GameLuaManager mLuaMgr;
        GameResManager mResMgr;

        ResUpdateView mUpdateView;

        //是否开启线程处理EventManager线程上的消息
        private bool progressThreadEvent = GameConfig.progressThreadEvent;
        private LuaFunction mLuaNotifyFunc;

        public int ScreenSleepTime { get { return Screen.sleepTimeout; } set { Screen.sleepTimeout = value; } }

        #region MonoBehaviour
        void Awake()
        {
            initLogFile();
            //初始化Platform，主要是插件相关
            initGamePlatform();

            mResMgr = GameResManager.Instance;
            mLuaMgr = GameLuaManager.Instance;
            //mUpMgr = GameUpdateManager.Instance;
            mUiMgr = GameUIManager.Instance;
        }

        void Start()
        {
            //初始化部分信息
            init();
            if (GameConfig.checkUpdate)
            {
                //检测资源更新
                checkResUpdate();
            }
            else
            {
                StartGameLogic();
            }

        }

        void Update()
        {
            //处理事件管理器在主线程的消息,暂时没有处理其他线程的事件分发
            EventManager.progressMainEvents();
        }

        void OnDestroy()
        {
            if(null != mLuaNotifyFunc)
            {
                mLuaNotifyFunc.Dispose();
                mLuaNotifyFunc = null;
            }
        }

        /// <summary>
        /// 在强退的情况下不会被调用到，可以在clear中处理
        /// </summary>
        void OnApplicationQuit()
        {
            ScreenSleepTime = SleepTimeout.SystemSetting;
            LogFile.Log("OnApplicationQuit");
            DestroyComp();
        }

        void OnApplicationFocus(bool focus)
        {
            ScreenSleepTime = focus ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            LogFile.Log("OnApplicationFocus:" + focus);
        }

        void OnApplicationPause(bool pause)
        {
            ScreenSleepTime = pause ? SleepTimeout.SystemSetting : SleepTimeout.NeverSleep;
            LogFile.Log("OnApplicationPause");
        }

        #endregion


        #region public 
        public void CloseUpdateView()
        {
            if (null != mUpdateView)
            {
                Destroy(mUpdateView);
            }
        }


        public override bool Dispose()
        {
            mUiMgr.DestroyComp();
            //mUpMgr.DestroyComp();
            mLuaMgr.DestroyComp();
            mResMgr.DestroyComp();

            LogFile.CloseLog();

            return true;
        }

        public void StartGameLogic()
        {
            //在资源更新完毕后再次初始化GameResMranager(正常的游戏逻辑会初始化两次GameResMranager)
            GameResManager.Instance.Initialize(delegate ()
            {
                LogFile.Log("启动Lua虚拟机:" + mLuaMgr);
                mLuaMgr.InitStart();
                mLuaNotifyFunc = mLuaMgr.GetFunction("OnNotice");
            });
        }

        public void OnMessage(string msg)
        {
            List<string> par = new List<string>(Regex.Split(msg, Platform.STR_SPLIT, RegexOptions.IgnoreCase));
            if (par.Count >= 2)
            {
                string eventName = par[0];
                NotifyLua(par.ToArray());
                par.RemoveAt(0);
                EventManager.notifyAll(eventName, par.ToArray());
            }
        }
        #endregion

        #region private
        private static void initLogFile()
        {
            if (
                RuntimePlatform.WindowsPlayer == Application.platform
                || RuntimePlatform.WindowsEditor == Application.platform
                || RuntimePlatform.OSXEditor == Application.platform
            )
            {
                LogFile.Init(Tools.PathCombine(Application.dataPath, "../Log/game_log.log"));
            }
            else
            {
                LogFile.Init(Tools.GetWriteableDataPath("game_log.log"));
            }
        }

        private void initGamePlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    Platform.SetPlatformInstance(new PlatformAnd());
                    break;
                case RuntimePlatform.IPhonePlayer:
                    Platform.SetPlatformInstance(new PlatformIOS());
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                    Platform.SetPlatformInstance(new PlatformEditor());
                    break;
                default:
                    Platform.SetPlatformInstance(new PlatformBase());
                    break;
            }
        }

        void init()
        {
            ScreenSleepTime = SleepTimeout.NeverSleep;

            //根据progressThreadEvent判定是否启用线程处理线程上的事件
            if (progressThreadEvent)
            {
                Loom.RunAsync(delegate ()
                {
                    while (progressThreadEvent)
                    {
                        Thread.Sleep(20);
                        EventManager.progressThreadEvents();
                    }
                });
            }

            //注册LogFile事件
            EventManager.registerToMain("LogEvent", this, "LogEvent");
        }

        void checkResUpdate()
        {
            // 检查新包资源是否拷贝，检查服务器资源更新，完成后启动lua虚拟机和其他游戏逻辑
            mUpdateView = FindObjectOfType<ResUpdateView>();
            if (null != mUpdateView)
            {
                mUpdateView.CheckUpdate(delegate (float percent, string msg)
                {
                    if (Equals(-1f, percent) || Equals(1f, percent))
                    {
                        LogFile.Log("callback of copy file:{0},{1}", percent, msg);
                        if (Equals(1f, percent))
                        {
                            StartGameLogic();
                        }
                        else
                        {
                            LogFile.Error("更新资源失败，启动游戏逻辑，如果资源不完整可能引起bug");
                            //TODO:包体资源拷贝失败，进行相应操作
                            //Application.Quit();
                            StartGameLogic();
                        }
                    }
                });
            }
        }
        #endregion

        public void LogEvent(string msg)
        {
            LogFile.WriteLine(LogFile.LogLevel.L_Warning, "LogEvent: " + msg);
        }

        public void HandleWWWRst(string noticeKey, double progress, int index, string msg)
        {
            NotifyLua("HandleWWWRst", new object[] { noticeKey , progress, index, msg});
        }

        public void NotifyLua(string eventName, object[] args)
        {
            if (null != mLuaNotifyFunc)
            {
                List<object> list = new List<object>(args);
                list.Insert(0, eventName);
                mLuaMgr.CallWithFunction(mLuaNotifyFunc, list.ToArray());
                list.Clear();
            }
        }

        /// <summary>
        /// 通知Lua层事件消息，args[0]为事件id
        /// </summary>
        /// <param name="args">Arguments.</param>
        public void NotifyLua(object[] args)
        {
            if (null != mLuaNotifyFunc)
            {
                mLuaMgr.CallWithFunction(mLuaNotifyFunc, args);
            }
        }
    }
}