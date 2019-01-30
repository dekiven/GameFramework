using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace GameFramework
{
    //TODO:多个文件多线程下载支持
    /// <summary>
    /// 从多个（也可以是1个）URL找到第一个存在的大文件下载
    /// </summary>
    public class LargeFileDownloader : IDisposable
    {
        public static string STR_SUCCEEDED = "succeeded";
        /// <summary>
        /// 有操作执行失败
        /// </summary>
        public static string STR_FAILED = "failed";

        //public int ThreadNum = 3;
        public List<string> Urls;
        public string SavePath;
        public int BufferSize = 2048;
        public long TotalSize { get { return mTotalSize; } }
        public long DoneSize { get { return mDoneSize; } }

        Action<double, string> DownloadCallback;

        private long mTotalSize;
        private long mDoneSize;
        private int mCoroutine;
        private HttpWebRequest mRequest;
        private int mIdx = 0;

        public void DownloadFile(List<string> urls, string savePath, Action<double, string> callback)
        {
            Urls = urls;
            SavePath = savePath;
            DownloadCallback = callback;
            mIdx = -1;
            startDownload();
        }

        public void Dispose()
        {
            mRequest.Abort();
            
            if(mCoroutine != 0)
            {
                GameCoroutineManager.Instance.StopCor(mCoroutine);
            }
        }

        private void startDownload()
        {
            mIdx += 1;
            LogFile.Log("startDownload：" + Urls[mIdx]);
            Tools.CheckDirExists(Directory.GetParent(SavePath).FullName, true);
            if(mIdx >= 0 && mIdx < Urls.Count)
            {
                
                LogFile.Log("下载："+Urls[mIdx]);
                if(null != mRequest)
                {
                    mRequest.Abort();
                }
                mRequest = (HttpWebRequest)WebRequest.Create(Urls[mIdx]);
                mRequest.ReadWriteTimeout = 500;
                mRequest.Timeout = 500;

                try
                {
                    mTotalSize = mRequest.GetResponse().ContentLength;
                }
                catch (Exception ex)
                {
                    LogFile.Log("error =>> " + ex.Message);
                    mRequest.Abort();
                    mRequest = null;
                    startDownload();
                    return;
                }

                Loom.RunAsync(() =>
                {
                    onRecave();
                });


                if(mCoroutine == 0)
                {
                    //GameCoroutineManager.Instance.StopCor(mCoroutine);
                    mCoroutine = GameCoroutineManager.Instance.StartCor(onProgress());
                }
            }
            else
            {
                //TODO:回调
            }
        }

        private IEnumerator onProgress()
        {
            bool hasFinish = false;
            while (!hasFinish)
            {
                hasFinish = mDoneSize == mTotalSize;
                double progress = (double)mDoneSize / mTotalSize;
                if (hasFinish)
                {
                    callback(0.99d, "重命名");
                    if(Tools.CheckFileExists(SavePath))
                    {
                        FileInfo info = new FileInfo(SavePath);
                        info.Delete();
                    }
                    File.Move(SavePath + ".tmp", SavePath);
                }
                callback(progress, (progress.Equals(1d) ? STR_SUCCEEDED : string.Empty));
                yield return new WaitForSeconds(0.5f);
            }
            GameCoroutineManager.Instance.StopCor(mCoroutine);

            //UnityEditor.EditorApplication.isPlaying = false;
        }

        private void callback(double progress, string msg)
        {
            if(null != DownloadCallback)
            {
                DownloadCallback(progress, msg);
            }
        }

        private void onRecave()
        {
            string fileName = SavePath + ".tmp";
            //LogFile.Log("onRecave idx:{0}, fileName:{1}", idx, fileName);
            byte[] buffer = new byte[BufferSize];

            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            long readSize = fs.Length;
            try
            {
                //LogFile.Log("onRecave idx:{0}, readSize:{1}", idx, mPartDoneSize[idx]);
                if(readSize < mTotalSize)
                {
                    //TODO:断点续传还有问题
                    //断点续传核心，设置本地文件流的起始位置
                    fs.Seek(readSize, SeekOrigin.Begin);
                    mRequest.AddRange((int)readSize);
                    Stream stream = mRequest.GetResponse().GetResponseStream();
                    int length = stream.Read(buffer, 0, BufferSize);
                    while(length > 0)
                    {
                        fs.Write(buffer, 0, length);
                        mDoneSize += length;
                        length = stream.Read(buffer, 0, BufferSize);
                        //LogFile.Log("onRecave while idx:{0}, readSize:{1}", idx, mPartDoneSize[idx]);
                    }
                    stream.Close();
                    stream.Dispose();
                }
                else
                {
                    //TODO:之前已经下载完成
                    mDoneSize = mTotalSize;
                }
                fs.Close();
                fs.Dispose();

            }
            catch (Exception ex)
            {
                LogFile.Warn("download {0} error, msg:{1}", SavePath, ex.Message);
                fs.Close();
                fs.Dispose();
            }
        }
    }
}
