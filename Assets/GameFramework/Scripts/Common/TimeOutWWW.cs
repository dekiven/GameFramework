using UnityEngine;
using System.Collections;
using System;
using LuaInterface;
using System.Collections.Generic;
using System.IO;

namespace GameFramework
{
    public class TimeOutWWW : MonoBehaviour
    {
        public delegate void WWWRstDel(double progress, int index, string msg);

        private WWW mWWW = null;
        private WWWRstDel mRstDel = null;
        private LuaFunction mLuaFunc = null;
        private float mTimeoutSec = 1f;
        private Coroutine mCoroutine = null;
        private bool mIsTimeOut;
        private long mTotalSize = 0;
        private long mDoneSize = 0;
        private List<DownloadInfo> mList;
        private List<DownloadInfo> mFialedList;
        private int mDoneCount = 0;

        #region MonoBehaviour
        void Awake()
        {
            mList = new List<DownloadInfo>();
            mFialedList = new List<DownloadInfo>();
        }

        void Start()
        {
            if (null != mList && mList.Count > 0)
            {
                startNewWWW();
            }
        }

        void OnDestroy()
        {
            if (null != mList)
            {

                mList.Clear();
            }
            if (null != mFialedList)
            {

                mFialedList.Clear();
            }
            if (null != mWWW)
            {
                mWWW.Dispose();
                mWWW = null;
            }

        }
        #endregion MonoBehaviour

        public void DownloadFile(DownloadInfo info, float timeoutSec, WWWRstDel del, LuaFunction lua)
        {
            mRstDel = del;
            setLuaCallback(lua);
            mTimeoutSec = timeoutSec;
            mTotalSize = info.Size;

            mList.Clear();
            mList.Add(info);
        }

        public void DownloadFiles(List<DownloadInfo> infos, float timeoutSec, WWWRstDel del, LuaFunction lua)
        {
            if(null == infos || infos.Count == 0)
            {
                callback(-1, "download files info list is null or empty!");
                return;
            }
            mRstDel = del;
            setLuaCallback(lua);
            mTimeoutSec = timeoutSec;
            for (int i = 0; i < infos.Count; i++)
            {
                mTotalSize = infos[i].Size;
            }

            mList.Clear();
            mList.AddRange(infos);
            mFialedList.Clear();
        }

        public void UploadFile()
        {

        }

        public void UploadFiles()
        {

        }

        public void GetRequest()
        {

        }

        public void PostRequest()
        {

        }
        #region 私有方法
        private void setLuaCallback(LuaFunction lua)
        {
            if (null != mLuaFunc)
            {
                mLuaFunc.Dispose();
                mLuaFunc = null;
            }
            mLuaFunc = lua;
        }

        private void callback(double progress, string msg)
        {
            if (null != mRstDel)
            {
                mRstDel(progress, mDoneCount, msg);
            }
            if (null != mLuaFunc)
            {
                mLuaFunc.Call(progress, mDoneCount, msg);
            }
            if (progress >= 1)
            {
                clear();
            }
        }

        private void clear()
        {
            mRstDel = null;
            mLuaFunc.Dispose();
            mLuaFunc = null;
            mTimeoutSec = 0f;
            mTotalSize = 0;
            if (null != mWWW)
            {
                mWWW.Dispose();
                mWWW = null;
            }
            if (null != mCoroutine)
            {
                StopCoroutine(mCoroutine);
            }
        }

        /// <summary>
        /// 检查www的超时
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private IEnumerator checkWWWTimeout(float timeout)
        {
            float progress = 0;
            float lastTime = Time.time;
            mIsTimeOut = false;
            if (null != mWWW)
            {
                while (!mWWW.isDone && !mIsTimeOut)
                {
                    if (mWWW.progress > progress)
                    {
                        lastTime = Time.time;
                        progress = mWWW.progress;
                        //TODO:可以计算下载速度并通知，以后有需求优化
                        callback(getProgress(mWWW.bytesDownloaded), string.Empty);
                    }
                    else if (Time.time - lastTime >= timeout)
                    {
                        mIsTimeOut = true;
                    }
                    yield return null;
                }
            }
        }

        private void startNewWWW()
        {
            if (null == mCoroutine)
            {
                mCoroutine = StartCoroutine(www());
            }
        }

        private void startWithWWW()
        {
            if (null == mCoroutine && null == mWWW)
            {
                mCoroutine = StartCoroutine(withWww());
            }
        }

        private IEnumerator www()
        {
            for (int i = 0; i < mList.Count; i++)
            {
                DownloadInfo info = mList[i];
                if (string.IsNullOrEmpty(info.Url) || string.IsNullOrEmpty(info.TargetPath))
                {
                    callback(-1f, "url/target is null or empty ");
                    continue;
                }
                mWWW = new WWW(info.Url);
                if (null != mWWW)
                {
                    callback(-1f, "www is not null");
                    continue;
                }
                yield return checkWWWTimeout(mTimeoutSec);
                if(mIsTimeOut)
                {
                    mWWW.Dispose();
                    mWWW = null;
                    callback(-1f, "time out");
                    mFialedList.Add(info);
                }
                else
                {
                    if (mWWW.isDone)
                    {
                        if (string.IsNullOrEmpty(mWWW.error))
                        {
                            //下载失败
                            callback(-1f, mWWW.error);
                            mFialedList.Add(info);
                        }
                        else
                        {
                            //下载完成
                            mDoneSize += info.Size;
                            mDoneCount += 1;
                            string savePath = info.TargetPath;
                            if (!string.IsNullOrEmpty(savePath))
                            {
                                if (File.Exists(savePath))
                                {
                                    File.Delete(savePath);
                                }
                                Tools.CheckDirExists(Directory.GetParent(savePath).ToString(), true);
                                FileStream fsDes = File.Create(savePath);
                                fsDes.Write(mWWW.bytes, 0, mWWW.bytes.Length);
                                fsDes.Flush();
                                fsDes.Close();
                            }
                            callback(getProgress(0), "done");
                        }
                    }
                    mWWW.Dispose();
                    mWWW = null;
                }
            }
            yield return null;
            if (mFialedList.Count == 0)
            {
                callback(1, "succeeded");
            }
            else
            {
                callback(-1, "failed");
            }
            Destroy(this);
        }

        private double getProgress(int curSize)
        {
            double progress = 0d;
            if (mTotalSize > 0)
            {
                progress = (mDoneSize + curSize) / (double)mTotalSize;
                
                return progress;
            }
            else
            {
                progress = (mDoneCount + curSize) / (double)mList.Count;
            }
            if (progress > 1d)
            {
                progress = 0.99d;
            }
            return progress;
        }

        private IEnumerator withWww()
        {
            if (null != mWWW)
            {
                yield return checkWWWTimeout(mTimeoutSec);
                if (mWWW.isDone)
                {
                    if (string.IsNullOrEmpty(mWWW.error))
                    {
                        //下载失败
                        //TODO:
                    }
                    else
                    {
                        //下载完成
                        //TODO:
                    }
                }
                yield return null;
                Destroy(this);
            }
        }
        #endregion 私有方法
    }

    public struct DownloadInfo
    {
        public string Url;
        public string TargetPath;
        public long Size;
    }
}
