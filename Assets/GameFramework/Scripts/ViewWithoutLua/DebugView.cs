#define __TEST__
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
#if __TEST__
        uint updateCount = 1;
#endif

        private LogFile.LogLevel mLogType = LogFile.LogLevel.L_Warning;
        private List<LogInfo> list;
        private List<LogInfo> newList;
        private List<UIItemData> datas;
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
                StartCoroutine(freshLog());
                freshAll();
            }
            else
            {
                StopAllCoroutines();
                lock (mLockObj)
                {
                    list.AddRange(newList);
                    newList.Clear();
                }
            }

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
                    freshAll();
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
                list.Clear();
                newList.Clear();
                datas.Clear();
                Handler.SetScrollViewData(LogSvIdx, datas);
            }
        }

        public void ShowLogDetal(UIItemData data)
        {
            List<UIHandlerData> l = data.DataList;
            mCurLogContent = l[1].Content as string;
            mCurLogColor = (Color)l[0].Content;
            Handler.SetTextString(7, mCurLogContent);
            Handler.SetTextColor(7, mCurLogColor);
            Handler.SetUIActive(5, true);
        }

        public void CloseLogDetal()
        {
            Handler.SetUIActive(5, false);
        }

        public void CopyCurLog2Clipboard()
        {
            GUIUtility.systemCopyBuffer = mCurLogContent;
        }
        #endregion Log 相关

        #region 继承 UIView
        protected override void init()
        {
            base.init();
            list = new List<LogInfo>();
            newList = new List<LogInfo>();
            datas = new List<UIItemData>();

            Application.logMessageReceived += handleLogCallback;

            Handler.SetSelectorTogglesOnChange(0, (int arg0) =>
            {
                for (int i = 0; i < PagesIdx.Length; ++i)
                {
                    Handler.SetUIActive(PagesIdx[i], i == arg0);
                }
            });

            Handler.SetScrollViewOnItemClick(LogSvIdx, (int idx) => 
            {
                UIItemData data = datas[idx];
                ShowLogDetal(data);
            });

            ActiveDebugView(false);
#if __TEST__
            //test
            StartCoroutine(printLog());
#endif
        }

        protected override void dispose()
        {
            base.dispose();
            Application.logMessageReceived -= handleLogCallback;
        }
        #endregion 继承 UIView

        #region 私有
        private void handleLogCallback(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    //WriteLine(LogLevel.L_Log, condition);
                    newLog(1, condition);
                    break;
                case LogType.Warning:
                    //WriteLine(LogLevel.L_Warning, condition);
                    newLog(2, condition);
                    break;
                case LogType.Error:
                    newLog(3, condition + "\n\nstackTrace:\n" + stackTrace);
                    break;
                case LogType.Exception:
                    newLog(4, condition + "\n\nstackTrace:\n" + stackTrace);
                    break;
                case LogType.Assert:
                    newLog(5, condition + "\n\nstackTrace:\n" + stackTrace);
                    break;
            }
        }

        private void newLog(int l, string codintion)
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
                if (DebugPL.activeSelf)
                {
                    newList.Add(info);
                }
                else
                {
                    list.Add(info);
                }

            }
        }

        private IEnumerator freshLog()
        {
            while (true)
            {
                yield return new WaitForSeconds(LogFreshInterval);
                if (newList.Count > 0)
                {
                    lock (mLockObj)
                    {
                        list.AddRange(newList);
                        datas.AddRange(GetDatas(newList));
                        //Handler.AddScrollViewDatas(1, GetDatas(newList));
                        int size = datas.Count - MaxLineNum;
                        if (size > 0)
                        {
                            for (int i = 0; i < size; i++)
                            {
                                datas.RemoveAt(0);
                            }
                        }
                        Handler.SetScrollViewData(LogSvIdx, datas);
                        newList.Clear();
                    }

                }
            }

        }

        private void freshAll()
        {
            lock (mLockObj)
            {
                datas.Clear();
                datas.AddRange(GetDatas(list));
                int size = datas.Count - MaxLineNum;
                if (size > 0)
                {
                    for (int i = 0; i < size; i++)
                    {
                        datas.RemoveAt(0);
                    }
                }
                Handler.SetScrollViewData(1, datas);
            }
        }

        private List<UIItemData> GetDatas(List<LogInfo> infos)
        {
            int min = (int)MinLogLevel;
            List<UIItemData> l = new List<UIItemData>();
            for (int i = 0; i < infos.Count; i++)
            {
                LogInfo info = infos[i];
                if (info.level >= min)
                {
                    List<UIHandlerData> d = new List<UIHandlerData>();
                    d.Add(new UIHandlerData("SetTextColor", 0, mLevelColors[info.level - 1]));
                    d.Add(new UIHandlerData("SetTextString", 0, info.log));
                    l.Add(new UIItemData(d));
                }
            }
            return l;
        }

#if __TEST__
        //test
        //protected override void update()
        private IEnumerator printLog()
        {
            while (true)
            {
                Debug.Log("test   " + updateCount);
                Debug.LogWarning("test" + updateCount);
                Debug.LogError("test" + updateCount);
                updateCount += 1;
                yield return new WaitForSeconds(0.2f);
            }
        }
#endif
        #endregion 私有
    }
}
