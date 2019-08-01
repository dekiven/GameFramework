using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameFramework
{
    public class ResPkgMgr : Singleton<ResPkgMgr>
    {
        public const string ConfFile = "pkgs.bytes";
        public string[] Pkgs { get { return mCotainPkgs.ToArray(); } }
        List<string> mCotainPkgs;

        public string[] AllPkgNames { get { return mAllPkgNames.ToArray(); } }
        List<string> mAllPkgNames;

        Action<bool> mReqAllCall;
        Action<bool> mCotainCall;
        Action<long> mUpCotainCall;

        Dictionary<string, ResPkgDownloder> mDownloaders;
        Dictionary<string, bool> mDownloadRst;
        List<string> mDownloadingPkg;
        long mTotalSize;
        private long mLastSize;
        private float mLastTime;
        private string mDownloadEventKey;
        private string mFinishEventKey;

        public ResPkgMgr()
        {
            mCotainPkgs = new List<string>();
            mAllPkgNames = new List<string>();
            mDownloaders = new Dictionary<string, ResPkgDownloder>();
            mDownloadingPkg = new List<string>();
            mTotalSize = 0;
        }

        public void ReqAllPkgs(Action<bool> callback)
        {
            mReqAllCall = callback;
            List<string> urls = new List<string>();
            foreach (var item in UpdateMgr.Instance.ResServList)
            {
                urls.Add(Tools.PathCombine(item.path, GameConfig.STR_ASB_MANIFIST + "/version/" + ConfFile));
            }
            WWWTO www = WWWTO.ReadFirstExistsStr(urls, _onAllPkgResp, null);
            www.Start();
        }

        public void ReadCotainPkgs(Action<bool> callback)
        {
            mCotainCall = callback;
            string url = Tools.GetFileUrl(GameConfig.STR_ASB_MANIFIST + "/version/" + ConfFile);
            WWWTO www = WWWTO.ReadFileStr(url, _onReadConf, null);
            www.Start();
        }

        /// <summary>
        /// 检测已经下载的包是否有更新(会先检测本地和服务器资源)
        /// </summary>
        /// <param name="callback">回调结果-1:检测更新失败, 0:没有更新, >0:有更新 </param>
        public void CheckCotainedPkg(Action<long> callback)
        {
            mUpCotainCall = callback;
            ReqAllPkgs((bool reqRst) =>
            {
                if (reqRst)
                {
                    ReadCotainPkgs((bool rst) =>
                    {
                        if (rst)
                        {
                            _checkCotainedPkg();
                        }
                        else
                        {
                            _onUpCotain(-1);
                        }
                    });
                }
                else
                {
                    _onUpCotain(-1);
                }
            });
        }

        /// <summary>
        /// 开始更新已下载的包，必须在CheckCotainedPkg返回的大小大于0后调用
        /// </summary>
        /// <param name="downloadKey">EventManager事件名称，事件通知double progress，long sizePerSec</param>
        /// <param name="finishKey">EventManager事件名称，事件通知double progress，long sizePerSec</param>
        public void DownloadCotainedPkgs(string downloadKey, string finishKey)
        {
            mLastSize = 0;
            mLastTime = Time.time;
            mDownloadEventKey = downloadKey;
            mFinishEventKey = finishKey;
            mDownloadRst = new Dictionary<string, bool>();
            foreach (var item in mDownloaders)
            {
                string pkg = item.Key;
                item.Value.Download((double progress, long sizePerSec, string msg) =>
                {
                    _onDownloadCotain(pkg, progress, msg);
                });
            }
        }

        public void AddCotainedPkg(string pkg)
        {
            if(mAllPkgNames.Contains(pkg) && !mCotainPkgs.Contains(pkg))
            {
                mCotainPkgs.Add(pkg);
                _saveCotainPkgs();
            }

        }

        #region 私有方法
        void _onAllPkgResp(bool rst, string msg)
        {
            if (rst)
            {
                mAllPkgNames = new List<string>(msg.Split(','));
            }
            if (null != mReqAllCall)
            {
                mReqAllCall(rst);
            }
        }

        void _onReadConf(bool rst, string msg)
        {
            if (rst)
            {
                mCotainPkgs = new List<string>(msg.Split(','));
            }
            else
            {
                LogFile.Warn(msg);
            }
            if (null != mAllPkgNames)
            {
                mCotainCall(rst);
            }
        }

        void _checkCotainedPkg()
        {
            mDownloadingPkg = new List<string>(mCotainPkgs);
            for (int i = 0; i < mCotainPkgs.Count; i++)
            {
                string pkg = mCotainPkgs[i];
                ResPkgDownloder downloader;
                if (!mDownloaders.TryGetValue(pkg, out downloader))
                {
                    downloader = new ResPkgDownloder(pkg);
                    mDownloaders[pkg] = downloader;
                }
                downloader.CheckUpdate((long size, string msg) =>
                {
                    _onPkgChecked(pkg, size);
                });
            }
        }

        void _onUpCotain(long size)
        {
            if (null != mUpCotainCall)
            {
                mUpCotainCall(size);
            }
        }

        void _onPkgChecked(string pkg, long size)
        {
            if (null == mDownloadingPkg)
            {
                return;
            }
            if (size >= 0 && mDownloadingPkg.Remove(pkg))
            {
                mTotalSize += size;
                if (0 == size)
                {
                    mDownloaders.Remove(pkg);
                }
                if (mDownloadingPkg.Count == 0)
                {
                    _onUpCotain(mTotalSize);
                }
            }
            else
            {
                if (null != mDownloadingPkg)
                {
                    _onUpCotain(-1);
                    mDownloadingPkg = null;
                }
            }
        }

        void _onDownloadCotain(string pkg, double progress, string msg)
        {
            long size = 0;
            float time = Time.time;
            foreach (var item in mDownloaders)
            {
                ResPkgDownloder downloader = item.Value;
                size += downloader.DoneSize;
            }

            long sizePerSec = (long)((size - mLastSize) / (time - mLastTime));
            double progr = size / (double)mTotalSize;

            // 通知下载进度
            EventManager.NotifyAll(mDownloadEventKey, progr, sizePerSec);

            mLastTime = time;
            mLastSize = size;

            //通知下载是否完全成功
            if (Tools.Equals(1d, progress))
            {
                mDownloadRst[pkg] = WWWTO.STR_SUCCEEDED.Equals(msg);
                if (mDownloadRst.Keys.Count == mDownloaders.Keys.Count)
                {
                    bool rst = true;
                    foreach (var r in mDownloadRst)
                    {
                        if (!r.Value)
                        {
                            rst = false;
                            break;
                        }
                    }
                    EventManager.NotifyAll(mFinishEventKey, rst);
                }
            }
        }

        private void _saveCotainPkgs()
        {
            string path = Tools.GetWriteableDataPath(GameConfig.STR_ASB_MANIFIST + "/version/" + ConfFile);
            Tools.CheckFileExists(path, true);
            FileStream stream = new FileStream(path, FileMode.Create);
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(string.Join(",", Pkgs));
            writer.Flush();
            writer.Close();
        }
        #endregion 私有方法
    }

}