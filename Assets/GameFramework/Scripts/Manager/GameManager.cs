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
        GameCoroutineManager mCorMgr;
        GameUIManager mUiMgr;
        GameLuaManager mLuaMgr;
        GameResManager mResMgr;

        //是否开启线程处理EventManager线程上的消息
        private bool progressThreadEvent = GameConfig.progressThreadEvent;
        private LuaFunction mLuaNotifyFunc;
        //是否在游戏中（GameManager 单例整个游戏生命周期都存在）
        private bool isRunning;

        public int ScreenSleepTime { get { return Screen.sleepTimeout; } set { Screen.sleepTimeout = value; } }

        public string UpdateViewResPath = string.Empty;
        public string DebugViewRestPath = string.Empty;

        //private DebugView mDebugView;
        //public DebugView DebugView{ get { return mDebugView; }}

        #region MonoBehaviour
        void Awake()
        {
            isRunning = true;
            GameConfig.Load();
            initLogFile();
            //初始化Platform，主要是插件相关
            initGamePlatform();

            ObjPools.Init();

            mCorMgr = GameCoroutineManager.Instance;
            mResMgr = GameResManager.Instance;
            mLuaMgr = GameLuaManager.Instance;
            //mUpMgr = GameUpdateManager.Instance;
            mUiMgr = GameUIManager.Instance;
        }

        void Start()
        {
            //初始化部分信息
            init();
            if (GameConfig.HasDebugView && !string.IsNullOrEmpty(DebugViewRestPath))
            {
                ShowDebugView();
            }

            if(!string.IsNullOrEmpty(UpdateViewResPath))
            {
                GameObject prefab = Resources.Load<GameObject>(UpdateViewResPath);
                if(null != prefab)
                {
                    mUiMgr.ShowViewPrefab(prefab);
                }
            }
        }

        public void ShowDebugView()
        {
            GameObject prefab = Resources.Load<GameObject>(DebugViewRestPath);
            if (null != prefab)
            {
                mUiMgr.ShowViewPrefab(prefab);
            }
        }

        void Update()
        {
            //处理事件管理器在主线程的消息,暂时没有处理其他线程的事件分发
            EventManager.progressMainEvents();
        }

        /// <summary>
        /// 在强退的情况下不会被调用到，可以在clear中处理
        /// </summary>
        void OnApplicationQuit()
        {
            ScreenSleepTime = SleepTimeout.SystemSetting;
            //LogFile.Log("OnApplicationQuit");
            OnMessage(GameDefine.STR_EVENT_APP_QUIT);
            DestroyComp();
        }

        void OnApplicationFocus(bool focus)
        {
            ScreenSleepTime = focus ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            //LogFile.Log("OnApplicationFocus:" + focus);
            OnMessageArr(GameDefine.STR_EVENT_APP_FOCUS, focus);
        }

        void OnApplicationPause(bool pause)
        {
            ScreenSleepTime = pause ? SleepTimeout.SystemSetting : SleepTimeout.NeverSleep;
            //LogFile.Log("OnApplicationPause");
            OnMessageArr(GameDefine.STR_EVENT_APP_PAUSE, pause);
        }

        #endregion


        #region public 

        public override bool Dispose()
        {
            isRunning = false;

            if (null != mLuaNotifyFunc)
            {
                mLuaNotifyFunc.Dispose();
                mLuaNotifyFunc = null;
            }

            mCorMgr.DestroyComp();
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
                mLuaNotifyFunc = mLuaMgr.GetFunction(GameDefine.STR_LUA_EVENT_FUNC);
            });
        }

        public void LogEvent(string msg)
        {
            LogFile.WriteLine(LogFile.LogLevel.L_Warning, "LogEvent: " + msg);
        }

        /// <summary>
        /// 通知Lua层事件消息
        /// </summary>
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
        /// 通知Lua层事件消息，args[0]为事件名称
        /// </summary>
        /// <param name="args">Arguments.</param>
        public void NotifyLua(object[] args)
        {
            if (null != mLuaNotifyFunc)
            {
                mLuaMgr.CallWithFunction(mLuaNotifyFunc, args);
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
                    while (progressThreadEvent && isRunning)
                    {
                        Thread.Sleep(20);
                        EventManager.progressThreadEvents();
                    }
                });
            }

            //注册LogFile事件
            EventManager.registerToMain(GameDefine.STR_EVENT_LOG_EVENT, this, "LogEvent");
        }
        #endregion


        #region 通知相关
        public void OnMessage(string msg)
        {
            List<string> par = new List<string>(Regex.Split(msg, Platform.STR_SPLIT, RegexOptions.IgnoreCase));
            if (par.Count >= 1)
            {
                NotifyLua(par.ToArray());
                string eventName = par[0];
                par.RemoveAt(0);
                EventManager.notifyAll(eventName, par.ToArray());
            }
            //LogFile.Log("OnMessage:   " + msg);
        }

        public void OnMessageArr(string eventName, params object[] msg)
        {
            NotifyLua(eventName, msg);
            EventManager.notifyAll(eventName, msg);
        }

        /// <summary>
        /// lua创建的TimeOutWWW回调函数
        /// </summary>
        /// <param name="noticeKey">Notice key.</param>
        /// <param name="progress">Progress.</param>
        /// <param name="index">Index.</param>
        /// <param name="msg">Message.</param>
        public void OnLuaWWWRst(string noticeKey, double progress, int index, string msg)
        {
            NotifyLua(GameDefine.STR_EVENT_LUA_WWW_RST, new object[] { noticeKey, progress, index, msg });
        }
        #endregion 通知相关
    }
}