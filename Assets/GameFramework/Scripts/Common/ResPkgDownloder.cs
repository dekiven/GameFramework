
using System;
using System.Collections.Generic;
using System.IO;
using LuaInterface;
using UnityEngine;

namespace GameFramework
{
    /// <summary>
    /// 资源包下载器
    /// </summary>
    public class ResPkgDownloder
    {
        public const string STR_CONFIG_MISSING = "config_missing";
        public const string sVerDirName = "version";

        public string PackageName { get { return mPkgName; } }
        string mPkgName = string.Empty;

        /// <summary>
        /// 当前的资源配置
        /// </summary>
        ResConf mCurConf;
        /// <summary>
        /// 服务器的配置
        /// </summary>
        ResConf servConf = null;
        /// <summary>
        /// StreamPath(包内)资源配置 
        /// </summary>
        ResConf mStreamConf = null;
        /// <summary>
        /// writeablePath(下载路径)资源配置
        /// </summary>
        ResConf mWriteableConf = null;
        //ResConf mCurConf = null;

        /// <summary>
        /// 需要更新的文件
        /// </summary>
        List<ResInfo> mNewFiles;
        /// <summary>
        /// 资源服列表
        /// </summary>
        List<ResInfo> mServList;
        /// <summary>
        /// 当前使用的服务器地址所对应的 idx 
        /// </summary>
        int mCurIdx = -1;
        //检测本地资源的回调
        Action<bool, string> mLocalCall;
        LuaFunction mLocalLua;

        //检测服务器资源的回调
        Action<bool, long, string> mServCall;
        LuaFunction mServLua;

        //下载更新资源的回调
        Action<double, long, string> mDownloadCall;
        LuaFunction mDownloadLua;

        public ResPkgDownloder(string pkgName)
        {
            mPkgName = pkgName;
        }

        /// <summary>
        /// 检测本地资源，完成后回调
        /// </summary>
        /// <param name="callback">Callback.</param>
        /// <param name="lua">Lua.</param>
        public void CheckLocalRes(Action<bool, string> callback, LuaFunction lua = null)
        {
            mLocalCall = callback;
            if (null != mLocalLua)
            {
                mLocalLua.Dispose();
                mLocalLua = lua;
            }
            checkLocalRes();
        }

        /// <summary>
        /// 检测服务器资源，保存需要下载的文件信息在内部
        /// </summary>
        /// <param name="callback">Callback.</param>
        /// <param name="lua">Lua.</param>
        public void CheckServRes(Action<bool, long, string> callback, LuaFunction lua = null)
        {
            mServCall = callback;
            mServLua = lua;
            UpdateMgr.Instance.LoadResServList(
                (List<ResInfo> list) =>
                {
                    mServList = list;
                    mCurIdx = -1;
                    _checkSerRes();
                }
            );
        }

        /// <summary>
        /// 下载资源包
        /// </summary>
        /// <param name="action">Action.</param>
        /// <param name="lua">Lua.</param>
        public void Download(Action<double, long, string> action, LuaFunction lua)
        {
            mDownloadCall = action;
            if (null != mDownloadLua)
            {
                mDownloadLua.Dispose();
                mDownloadLua = lua;
            }
            mDownloadLua = lua;
            _download();
        }

        #region 私有方法

        string _confPathStream()
        {
            return Tools.GetUrlPathStream(Application.streamingAssetsPath, GameConfig.STR_ASB_MANIFIST + "/" + sVerDirName + "/" + mPkgName);
        }

        string _confPathWrite()
        {
            return Tools.GetUrlPathWriteabble(Tools.GetWriteableDataPath(), GameConfig.STR_ASB_MANIFIST + "/" + sVerDirName + "/" + mPkgName);
        }

        /// <summary>
        /// 传入的 URL 必须带平台路径
        /// </summary>
        /// <returns>The URL.</returns>
        /// <param name="url">URL.</param>
        string _confUrl(string url)
        {
            return Tools.PathCombine(url, sVerDirName + "/" + mPkgName);
        }

        /// <summary>
        /// 对比可读写文件夹下资源版本和包内资源版本
        /// 加载asb可以从streaming路径加载，不需要将资源拷贝到可读写文件夹
        /// 当包体资源有更新时，删除老的已下载资源包
        /// </summary>
        private void checkLocalRes()
        {
            if (GameConfig.useAsb)
            {
                string sUrl = _confPathStream();
                string wUrl = _confPathWrite();

                WWWTO streamWWW = WWWTO.ReadFileStr(sUrl, (rst, msg) =>
                {
                    if (rst)
                    {
                        mStreamConf = new ResConf(msg);
                    }
                    else
                    {
                        // 该包没有在游戏包体内
                        mStreamConf = new ResConf("");
                    }
                    WWWTO writableWWW = WWWTO.ReadFileStr(wUrl, _onWriteableConf, null);
                    writableWWW.Start();
                }, null);
                streamWWW.Start();
            }
            else
            {
                _callbackLocal(true, "");
            }
        }

        private void _checkSerRes()
        {
            mCurIdx += 1;
            string srcUrl = getServUrl();
            if (string.IsNullOrEmpty(srcUrl))
            {
                _callbackServ(false, -1, "服务器列表尚未初始化");
                return;
            }
            compareNewFiles(srcUrl, (bool rst, long size, string msg) =>
            {
                if (rst)
                {
                    _callbackServ(rst, size, msg);
                    return;
                }
                _checkSerRes();
            });
        }

        void _download()
        {
            string srcUrl = getServUrl();
            float last = Time.time;
            long lastSize = 0L;
            if (null != mNewFiles && mNewFiles.Count > 0)
            {
                List<WWWInfo> infos = new List<WWWInfo>();
                long totalSize = 0;
                for (int i = 0; i < mNewFiles.Count; ++i)
                {
                    ResInfo ri = mNewFiles[i];
                    string url = Tools.PathCombine(srcUrl, ri.path);
                    string savePath = Tools.GetWriteableDataPath(GameConfig.STR_ASB_MANIFIST + "/" + ri.path);
                    totalSize += ri.size;
                    infos.Add(new WWWInfo(url, savePath, ri.size));
                }

                //需要拷贝资源到可读写文件夹
                WWWTO downloader = WWWTO.DownloadFiles(
                    infos,
                    (double progress, int index, string __msg) =>
                    {
                        if (Tools.Equals(progress, 1d))
                        {
                            if (__msg.Equals(WWWTO.STR_SUCCEEDED))
                            {
                                mCurConf.version = servConf.version;
                                //保存新的资源版本号
                                saveVersionCode(mCurConf.VersionCode);
                                _callbackDownload(1d, 0, "资源更新完成");
                            }
                            else
                            {
                                _callbackDownload(1d, -1, "部分资源更新失败");
                            }

                            mCurConf.SaveToFile(_confPathWrite());
                        }
                        else
                        {
                            string filePath = mNewFiles[index - 1].path;
                            if (Tools.Equals(progress, -1d))
                            {
                                //有文件下载失败
                                LogFile.Warn("[" + filePath + "]下载失败,url:" + infos[index - 1].Url);
                            }
                            else
                            {
                                if (__msg.Equals(WWWTO.STR_DONE))
                                {
                                    //有文件下载成功
                                    mCurConf.files[filePath] = servConf.files[filePath];
                                }
                                float now = Time.time;
                                float dt = now - last;
                                long doneSize = (long)(totalSize * progress);
                                long sizePerSec = (long)((doneSize - lastSize) / dt);
                                if (sizePerSec >= 0)
                                {
                                    _callbackDownload(progress, sizePerSec, filePath);
                                }
                                last = now;
                                lastSize = doneSize;
                            }
                        }
                    },
                    null
                );
                downloader.Start();
            }
        }

        /// <summary>
        /// 对比服务器配置文件，记录新的文件
        /// </summary>
        /// <param name="url">服务器地址 URL.</param>
        /// <param name="callback">Callback.</param>
        private void compareNewFiles(string url, Action<bool, long, string> callback)
        {
            if (null != callback)
            {
                ResConf srcConf = null;

                if (GameConfig.useAsb)
                {
                    string sUrl = _confUrl(url);
                    string pUrl = _confPathWrite();

                    WWWTO streamWWW = WWWTO.ReadFileStr(sUrl, (rst, msg) =>
                    {
                        if (rst)
                        {
                            srcConf = new ResConf(msg);

                            WWWTO writableWWW = WWWTO.ReadFileStr(pUrl, (_rst, _msg) =>
                           {
                               if (_rst)
                               {
                                   //之前已经拷贝过资源
                                   servConf = new ResConf(_msg);
                               }
                               else
                               {
                                   servConf = new ResConf("");
                               }

                               mCurConf = servConf;
                               long size = 0;

                               if (srcConf.CompareVer(servConf) > 0)
                               {
                                   mNewFiles = srcConf.GetUpdateFiles(servConf);
                                   foreach (var f in mNewFiles)
                                   {
                                       size += f.size;
                                   }
                                   callback(true, size, servConf.version);
                               }
                               else
                               {
                                   LogFile.Log("没有检测到新版本资源，跳过更新步骤");
                                   callback(true, size, "没有检测到新版本资源，跳过更新步骤");
                               }

                           }, null);
                            writableWWW.Start();
                        }
                        else
                        {
                            LogFile.Warn("资源配置文件" + sUrl + "丢失");
                            callback(false, 0, STR_CONFIG_MISSING);
                        }

                    }, null);
                    streamWWW.Start();
                }
                else
                {
                    callback(true, 0, "不使用Assetbundle不用拷贝/下载资源");
                }
            }
        }

        /// <summary>
        /// 返回当前服务器 URL(带平台后缀)
        /// </summary>
        /// <returns>The serv URL.</returns>
        private string getServUrl()
        {
            if (mCurIdx < mServList.Count)
            {
                return Tools.PathCombine(mServList[mCurIdx].path, GameConfig.STR_ASB_MANIFIST);
            }
            else
            {
                return string.Empty;
            }
        }

        void saveVersionCode(int code)
        {
            string key = GameDefine.STR_CONF_KEY_RES_VER_I;
            int curCode = GameConfig.GetInt(key);
            if (curCode < code)
            {
                GameConfig.SetInt(key, code);
            }
            else
            {
                LogFile.Warn("保存 VersionCode 失败，当前包名：" + mPkgName + "; 新code:" + code + "; 当前 code:" + curCode);
            }
        }

        void _callbackLocal(bool rst, string msg)
        {
            if (null != mLocalCall)
            {
                mLocalCall(rst, msg);
            }
            if (null != mLocalLua)
            {
                mLocalLua.Call(rst, msg);
                mLocalLua.Dispose();
                mLocalLua = null;
            }
        }

        void _callbackServ(bool hasNewRes, long size, string msg)
        {
            if (null != mServCall)
            {
                mServCall(hasNewRes, size, msg);
            }
            if (null != mServLua)
            {
                mServLua.Call(hasNewRes, size, msg);
                mServLua.Dispose();
                mServLua = null;
            }
        }

        void _callbackDownload(double progress, long sizePerSec, string msg)
        {
            if (null != mDownloadCall)
            {
                mDownloadCall(progress, sizePerSec, msg);
            }
            if (null != mDownloadLua)
            {
                mDownloadLua.Call(progress, sizePerSec, msg);
            }
        }

        void _disposDownload()
        {
            if (null != mDownloadLua)
            {
                mDownloadLua.Dispose();
                mDownloadLua = null;
            }
        }

        void _onWriteableConf(bool rst, string msg)
        {
            if (rst)
            {
                //之前已经拷贝过资源
                mWriteableConf = new ResConf(msg);
            }
            else
            {
                mWriteableConf = new ResConf("");
            }

            mCurConf = mWriteableConf;

            if (mStreamConf.CompareVer(mWriteableConf) > 0)
            {
                List<ResInfo> list = mStreamConf.GetUpdateFiles(mWriteableConf);
                if (list.Count > 0)
                {
                    //将可读写文件夹下的老版本资源删除
                    for (int i = 0; i < list.Count; i++)
                    {
                        ResInfo ri = list[i];
                        string savePath = Tools.GetWriteableDataPath(GameConfig.STR_ASB_MANIFIST + "/" + ri.path);
                        if (File.Exists(savePath))
                        {
                            File.Delete(savePath);
                            LogFile.Log("删除已下载的老资源：" + savePath);
                        }
                        mCurConf.files[ri.path] = mStreamConf.files[ri.path];
                    }
                }
                mCurConf.version = mStreamConf.version;
                //保存新的资源版本号
                saveVersionCode(mCurConf.VersionCode);
                mCurConf.SaveToFile(_confPathWrite());
                _callbackLocal(true, "");
            }
        }
        #endregion 私有方法
    }
}