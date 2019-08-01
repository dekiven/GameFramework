using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    using LM = LanguageManager;

    public class UpdateMgr : Singleton<UpdateMgr>
    {
        public const string STR_UP_MGR_STATE_CHANGE = "UpdateMgrStateChnage";

        string mDownLoadKey;
        string mFinishKey;

        /// <summary>
        /// 请勿直接调用，使用Instance方法获取单例
        /// 初始化后直接检查服务器资源
        /// </summary>
        public void Init(string downloadKey, string finishKey)
        {
            mDownLoadKey = downloadKey;
            mFinishKey = finishKey;
            _changeState(LM.GetStr("检测服务器资源..."));
            LoadServConf( (ret) =>
            {
                if (GameConfig.useAsb && GameConfig.checkUpdate)
                {
                    _changeState(LM.GetStr("检测App版本..."));
                    CheckAPP(() =>
                    {
                        _changeState(LM.GetStr("检测本地资源..."));
                        CheckPkgsCotained();
                    });
                }
                else
                {
                    StartGameLogic();
                }
            });
            
        }

        /// <summary>
        /// 服务器列表获取回调
        /// </summary>
        Action<ResInfo[]> mServListCall;

        /// <summary>
        /// 服务器配置信息获取回调
        /// </summary>
        Action<Dictionary<string, string>> mServConfCall;

        /// <summary>
        /// 服务器列表
        /// </summary>
        /// <value>The res serv list.</value>
        public ResInfo[] ResServList { get { return mResServList.ToArray(); } }
        List<ResInfo> mResServList;

        /// <summary>
        /// 服务器配置
        /// </summary>
        /// <value>The serv conf.</value>
        public Dictionary<string, string> ServConf { get { return mServConf; } }
        Dictionary<string, string> mServConf;

        /// <summary>
        /// 获取配置的服务器列表
        /// </summary>
        /// <param name="callback">回调</param>
        /// <param name="forceLoad">如果设置为 <c>true</c> 强制读取配置</param>
        public void LoadResServList(Action<ResInfo[]> callback, bool forceLoad = false)
        {
            mServListCall = callback;
            if (null == mResServList || mResServList.Count == 0 || forceLoad)
            {
                ResMgr.Instance.GetStrAsync("UpdateServer", ".bytes", _onServListLoaded);
            }
            else
            {
                _callbackServList();
            }
        }

        /// <summary>
        /// 获取服务器上相应平台的配置信息
        /// </summary>
        /// <param name="callback">回调.</param>
        /// <param name="forceLoad">如果设置为 <c>true</c> 强制从服务器获取</param>
        public void LoadServConf(Action<Dictionary<string, string>> callback, bool forceLoad = false)
        {
            mServConfCall = callback;
            if (null == mServConf || mServConf.Count == 0 || forceLoad)
            {
                LoadResServList((ResInfo[] list) =>
                {
                    List<string> files = new List<string>();
                    for (int i = 0; i < list.Length; i++)
                    {
                        files.Add(Tools.PathCombine(list[i].path, GameConfig.STR_ASB_MANIFIST + "/servConf.bytes"));
                    }
                    WWWTO www = WWWTO.ReadFirstExistsStr(files, _onServConfResp, null);
                    www.TimeoutSec = 1.5f;
                    www.Start();
                });
            }
            else
            {
                _callbackServConf();
            }
        }

        /// <summary>
        /// 检测 app 是否需要更新，如果有更新弹出确认框，跳转应用商店或者下载更新
        /// </summary>
        public void CheckAPP(Action skipUpAppCall)
        {
            Platform.CheckAppVer((bool isLatest, string msg) =>
            {
                if (!isLatest)
                {
                    MsgBoxMgr.Instance.ShowMsg(LM.GetStr("提示"), LM.GetStr(msg), (idx) =>
                    {
                        Platform.UpdateApp();
                    });
                }
                else
                {
                    if (null != skipUpAppCall)
                    {
                        skipUpAppCall();
                    }
                }
            });
        }

        /// <summary>
        /// 检测已经下载的包是否需要更新，需要则弹出确认框
        /// </summary>
        public void CheckPkgsCotained()
        {
            ResPkgMgr.Instance.CheckCotainedPkg((long size) =>
            {
                if(size == -1)
                {
                    // 更新检测失败提示框
                    _showCheckErrMsg();
                }
                if (size > 0)
                {
                    //弹出确认框，确认下载
                    _showDownloadMsg(size);
                }
                if (size == 0)
                {
                    //继续游戏逻辑
                    StartGameLogic();
                }
                
            });
        }

        /// <summary>
        /// 检测某个资源包的更新情况，待实现
        /// </summary>
        /// <param name="pkgName">Package name.</param>
        public void CheckPkg(string pkgName)
        {
            //TODO:
        }

        public void StartGameLogic()
        {
            // pop DownloadView
            UIMgr.Instance.PopView();
            GameMgr.Instance.StartGameLogic();

            EventManager.RemoveFromMain(this);
        }
        #region 私有方法

        /// <summary>
        /// 当从本地读取到服务器地址配置表
        /// </summary>
        /// <param name="text">Text.</param>
        void _onServListLoaded(string text)
        {
            //TODO:后期服务器配置表等数据需要加密
            if (!string.IsNullOrEmpty(text))
            {
                ResConf servers = new ResConf(text);
                ResInfo[] arr = new ResInfo[servers.files.Values.Count];
                servers.files.Values.CopyTo(arr, 0);
                if (null == mResServList)
                {
                    mResServList = new List<ResInfo>(arr);
                }
                else
                {
                    mResServList.Clear();
                    mResServList.AddRange(arr);
                }
                mResServList.Sort((ResInfo a, ResInfo b) =>
                {
                    return a.size < b.size ? -1 : 1;
                });

                _callbackServList();
            }
        }

        void _callbackServList()
        {
            if (null != mServListCall)
            {
                mServListCall(ResServList);
            }
        }

        /// <summary>
        /// 当从服务器读取到配置表
        /// </summary>
        /// <param name="rst">If set to <c>true</c> rst.</param>
        /// <param name="msg">Message.</param>
        void _onServConfResp(bool rst, string msg)
        {
            //TODO:后期服务器配置表等数据需要加密
            if (null == mServConf)
            {
                mServConf = new Dictionary<string, string>();
            }
            else
            {
                mServConf.Clear();
            }
            if (rst)
            {
                mServConf = Tools.SplitStr2Dic(msg, "\n", "|");
            }
            else
            {
                LogFile.Warn("从资源服servConf.bytes失败");
            }

            _callbackServConf();
        }

        void _callbackServConf()
        {
            if (null != mServConfCall)
            {
                mServConfCall(mServConf);
            }
        }

        void _onCheckUpErrMsgBox(int idx)
        {
            if(MsgBox.IdxL == idx)
            {
                _quiteGame();
            }
            if(MsgBox.IdxR == idx)
            {
                StartGameLogic();
            }
        }

        void _onDownloadMsgBox(int idx)
        {
            EventManager.AddToMain(mFinishKey, this, "_0nDownloadRst");
            ResPkgMgr.Instance.DownloadCotainedPkgs(mDownLoadKey, mFinishKey);
        }

        void _onAppUpdateMsgBox(int idx)
        {
            Platform.UpdateApp();
        }

        void _onDownlaodErrMsgBox(int idx)
        {
            Platform.Quit();
        }

        void _quiteGame()
        {
            Platform.Quit();
        }


        void _changeState(string state)
        {
            EventManager.NotifyMain(STR_UP_MGR_STATE_CHANGE, state);
        }

        void _showCheckErrMsg()
        {
            //更新检测失败
            var info = new MsgBoxInfo()
            {
                Title = LM.GetStr("提示"),
                BtnTxtL = LM.GetStr("退出游戏"),
                BtnTxtR = LM.GetStr("强制进入"),
                Msg = LM.GetStr("获取更新信息失败，是否强制进入游戏？\n<color=red>强制进入可能引起程序异常。</color>"),
                ClickCallback = _onCheckUpErrMsgBox,
            };
            MsgBoxMgr.Instance.ShowMsg(info);
        }

        void _showDownloadMsg(long size)
        {
            var info = new MsgBoxInfo()
            {
                Title = LM.GetStr("提示"),
                BtnTxtM = LM.GetStr("确定"),
                Msg = LM.GetStr("检测到更新资源，共计<color=blue>{0}</color>，推荐使用Wifi下载。", Tools.FormatMeroySize(size)),
                ClickCallback = _onDownloadMsgBox,
            };
            MsgBoxMgr.Instance.ShowMsg(info);
        }

        void _showDownloadErrMsg()
        {
            var info = new MsgBoxInfo()
            {
                Title = LM.GetStr("提示"),
                BtnTxtM = LM.GetStr("确定"),
                Msg = LM.GetStr("资源下载失败，请检查网络"),
                ClickCallback = _onDownlaodErrMsgBox,
            };
            MsgBoxMgr.Instance.ShowMsg(info);
        }
        #endregion 私有方法

        #region 事件回调
        public void _0nDownloadRst(bool rst)
        {
            if (rst)
            {
                StartGameLogic();
            }
            else
            {
                _showCheckErrMsg();
            }
        }
        #endregion


        #region test
        void ____test____()
        {
            _showDownloadErrMsg();
            CoroutineMgr.Instance.Delay(5, () => 
            {
                _showCheckErrMsg();
            });
        }
        #endregion test
    }
}