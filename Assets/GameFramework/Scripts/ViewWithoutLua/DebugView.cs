using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameFramework
{
    struct LogInfo
    {
        public string log;
        public int level;
    }

    public class DebugView : UIView
    {
        public float LogFreshInterval = 1f;
        public int MaxLineNum = 1000;
        public int LogSvIdx = 3;
        public int[] PagesIdx = { 1, 2 };

        public GameObject DebugPL;
        public Button MainBtn;

        private LogFile.LogLevel mLogType = LogFile.LogLevel.L_Log;
        private List<LogInfo> mList;
        private List<LogInfo> mNewList;
        private List<UIItemData> itemDatas;
        private System.Object mLockObj = new System.Object();
        private Color[] mLevelColors = { Color.green, Color.yellow, Color.red, Color.red, Color.red, };
        private string mCurLogContent;
        private Color mCurLogColor;

        public void ActiveDebugView(bool active)
        {
            DebugPL.SetActive(active);
            MainBtn.gameObject.SetActive(!active);
            if (active)
            {
                _refreshAll();
                StartCoroutine(_refreshLog());
            }
            else
            {
                StopAllCoroutines();
                lock (mLockObj)
                {
                    mList.AddRange(mNewList);
                    mNewList.Clear();
                }
            }

        }

        public void ExitApp()
        {
            Platform.Quit();
        }

#region Log 相关
        public LogFile.LogLevel MinLogLevel
        {
            get { return mLogType; }
            set
            {
                if (mLogType != value)
                {
                    mLogType = value;
                    _refreshAll();
                }
            }
        }

        public void SetMinLevel(int l)
        {
            MinLogLevel = (LogFile.LogLevel)l;
        }

        public void ClearLogs()
        {
            lock (mLockObj)
            {
                mList.Clear();
                mNewList.Clear();
                itemDatas.Clear();
                Handler.SetScrollViewData(LogSvIdx, itemDatas);
            }
        }

        public void ShowLogDetal(UIItemData data)
        {
            List<UIHandlerData> l = data.DataList;
            mCurLogContent = l[1].Content as string;
            mCurLogColor = (Color)l[0].Content;
            Handler.SetTextString(7, mCurLogContent);
            Handler.SetUIColor(7, mCurLogColor);
            Handler.SetUIActive(5, true);
        }

        public void CloseLogDetal()
        {
            Handler.SetUIActive(5, false);
        }

        public void CopyCurLog2Clipboard()
        {
#if UNITY_EDITOR
            GUIUtility.systemCopyBuffer = mCurLogContent;
#else
            Platform.Copy2Clipboard(mCurLogContent);
#endif
        }
#endregion Log 相关

#region 继承 UIView
        protected override void init()
        {
            base.init();
            mList = new List<LogInfo>();
            mNewList = new List<LogInfo>();
            itemDatas = new List<UIItemData>();

            Application.logMessageReceived += _handleLogCallback;

            Handler.SetSelectorTogglesOnChange(0, (int arg0) =>
            {
                for (int i = 0; i < PagesIdx.Length; ++i)
                {
                    Handler.SetUIActive(PagesIdx[i], i == arg0);
                }
            });

            Handler.SetScrollViewOnItemClick(LogSvIdx, (int idx) =>
            {
                UIItemData data = itemDatas[idx];
                ShowLogDetal(data);
            });

            _addMainBtnEvent();

            ActiveDebugView(false);
        }

        protected override void dispose()
        {
            base.dispose();
            Application.logMessageReceived -= _handleLogCallback;
        }
#endregion 继承 UIView

#region 私有
        private void _handleLogCallback(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    //WriteLine(LogLevel.L_Log, condition);
                    _newLog(1, condition);
                    break;
                case LogType.Warning:
                    //WriteLine(LogLevel.L_Warning, condition);
                    _newLog(2, condition);
                    break;
                case LogType.Error:
                    _newLog(3, condition + "\n\nstackTrace:\n" + stackTrace);
                    break;
                case LogType.Exception:
                    _newLog(4, condition + "\n\nstackTrace:\n" + stackTrace);
                    break;
                case LogType.Assert:
                    _newLog(5, condition + "\n\nstackTrace:\n" + stackTrace);
                    break;
            }
        }

        private void _newLog(int l, string codintion)
        {
            //TODO
            //异步，固定时间刷新一次 log，ScrollView 支持添加和删除多个数据
            lock (mLockObj)
            {
                LogInfo info = new LogInfo()
                {
                    log = codintion,
                    level = l,
                };
                mNewList.Add(info);
            }
        }

        private IEnumerator _refreshLog()
        {
            while (true)
            {
                if (mNewList.Count > 0)
                {
                    lock (mLockObj)
                    {
                        mList.AddRange(mNewList);
                        itemDatas.AddRange(_getDatas(mNewList));
                        //Handler.AddScrollViewDatas(1, GetDatas(newList));
                        int size = itemDatas.Count - MaxLineNum;
                        if (size > 0)
                        {
                            for (int i = 0; i < size; i++)
                            {
                                itemDatas.RemoveAt(0);
                            }
                        }
                        Handler.SetScrollViewData(LogSvIdx, itemDatas);
                        mNewList.Clear();
                    }
                }
                yield return new WaitForSeconds(LogFreshInterval);
            }

        }

        private void _refreshAll()
        {
            lock (mLockObj)
            {
                itemDatas.Clear();
                List<UIItemData> d = _getDatas(mList);
                itemDatas.AddRange(d);
                int size = itemDatas.Count - MaxLineNum;
                if (size > 0)
                {
                    for (int i = 0; i < size; i++)
                    {
                        itemDatas.RemoveAt(0);
                    }
                }
                Handler.SetScrollViewData(1, itemDatas);
                //TODO:解决 refreshAll只会在有新的日志才能刷新的问题
                LogFile.Log("DebugView refreshAll:"+MinLogLevel.ToString());
            }
        }

        private List<UIItemData> _getDatas(List<LogInfo> infos)
        {
            int min = (int)MinLogLevel;
            List<UIItemData> l = new List<UIItemData>();
            for (int i = 0; i < infos.Count; i++)
            {
                LogInfo info = infos[i];
                if (info.level >= min)
                {
                    List<UIHandlerData> d = new List<UIHandlerData>();
                    d.Add(new UIHandlerData("SetUIColor", 0, mLevelColors[info.level - 1]));
                    d.Add(new UIHandlerData("SetTextString", 0, info.log));
                    l.Add(new UIItemData(d));
                }
            }
            return l;
        }

        /// <summary>
        /// 定义 debug 按钮交互事件
        /// </summary>
        private void _addMainBtnEvent()
        {
            bool isDraging = false;

            //拖拽事件
            List<EventTrigger.Entry> entries = new List<EventTrigger.Entry>();
            EventTrigger.Entry entryDrag = new EventTrigger.Entry();
            entryDrag.eventID = EventTriggerType.Drag;
            entryDrag.callback.AddListener((BaseEventData d) =>
            {
                PointerEventData data = d as PointerEventData;
                if (null != data)
                {
                    Vector3 delta = data.delta;
                    MainBtn.transform.Translate(delta * 0.9f);
                }
                isDraging = true;
            });
            entries.Add(entryDrag);

            //点击事件
            EventTrigger.Entry entryClick = new EventTrigger.Entry();
            entryClick.eventID = EventTriggerType.PointerClick;
            entryClick.callback.AddListener((BaseEventData arg0) =>
            {
                if(!isDraging)
                {
                    ActiveDebugView(true);
                }
                isDraging = false;
            });
            entries.Add(entryClick);

            Tools.AddEventTrigger(MainBtn.gameObject, entries);
        }
#endregion 私有
    }
}
