using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class UpdateMgr : Singleton<UpdateMgr>
    {
        /// <summary>
        /// 服务器列表获取回调
        /// </summary>
        Action<List<ResInfo>> mServListCall;

        /// <summary>
        /// 服务器配置信息获取回调
        /// </summary>
        Action<Dictionary<string, string>> mServConfCall;

        /// <summary>
        /// 服务器列表
        /// </summary>
        /// <value>The res serv list.</value>
        public List<ResInfo> ResServList { get { return mResServList; } }
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
        public void LoadResServList(Action<List<ResInfo>> callback, bool forceLoad = false)
        {
            if (null == mResServList || mResServList.Count == 0 || forceLoad)
            {
                mServListCall = callback;
                GameResManager.Instance.GetStrAsync("UpdateServer", ".bytes", _onServListLoaded);
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
                LoadResServList((List<ResInfo> list) =>
                {
                    List<string> files = new List<string>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        files.Add(Tools.PathCombine(list[i].path, GameConfig.STR_ASB_MANIFIST + "/servConf.bytes"));
                    }
                    using(WWWTO www = WWWTO.ReadFirstExistsStr(files, _onServConfResp, null))
                    {
                        www.TimeoutSec = 1.5f;
                        www.Start();
                    }
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
        public void CheckAPP()
        {
            //TDOD:
        }

        /// <summary>
        /// 检测已经下载的包是否需要更新，需要则弹出确认框
        /// </summary>
        public void CheckPkgsDownloaded()
        {
            //TDOD:
        }

        /// <summary>
        /// 检测某个资源包的更新情况
        /// </summary>
        /// <param name="pkgName">Package name.</param>
        public void CheckPkg(string pkgName)
        {
            //TODO:
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
                mServListCall(mResServList);
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
        #endregion 私有方法
    }
}