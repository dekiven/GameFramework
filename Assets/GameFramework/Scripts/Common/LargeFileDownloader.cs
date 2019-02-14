using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace GameFramework
{
    //TODO:多个文件多线程下载支持 单个文件多线程
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
        //public long DoneSize { get { return mDoneSize; } }
        public int ThreadNum = 3;

        Action<double, string> DownloadCallback;

        private long mTotalSize;
        //服务器是否允许部分下载（AddRange）
        private bool mPartialEnabled;
        //private long mDoneSize;
        private int mCoroutine;
        private HttpWebRequest mRequest;
        private int mIdx = 0;

        private long[] mPartSize;
        private long[] mPartDoneSize;
        private long[] mPartStartPos;

        public void DownloadFile(List<string> urls, string savePath, Action<double, string> callback)
        {
            Urls = urls;
            SavePath = savePath;
            DownloadCallback = callback;
            mIdx = -1;
            downloadNextUrl();
        }

        public void Dispose()
        {
            mRequest.Abort();
            
            if(mCoroutine != 0)
            {
                GameCoroutineManager.Instance.StopCor(mCoroutine);
            }
        }

        private void downloadNextUrl()
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
                    HttpWebResponse response = (HttpWebResponse)mRequest.GetResponse();
                    mTotalSize = response.ContentLength;
                    mPartialEnabled = response.StatusCode == HttpStatusCode.PartialContent;
                    LogFile.Log("服务器是否允许部分下载：" + mPartialEnabled);
                    if (ThreadNum > 1 && !mPartialEnabled)
                    {
                        ThreadNum = 1;
                        LogFile.Log("资源服不支持部分下载，取消多线程下载。");
                    }
                }
                catch (Exception ex)
                {
                    LogFile.Log("error =>> " + ex.Message);
                    mRequest.Abort();
                    mRequest = null;
                    downloadNextUrl();
                    return;
                }

                mPartSize = new long[ThreadNum];
                mPartDoneSize = new long[ThreadNum];
                mPartStartPos = new long[ThreadNum];

                for (int i = 0; i < ThreadNum; i++)
                {
                    int _i = i;
                    //double 
                    mPartSize[i] = mTotalSize / ThreadNum + ((i == ThreadNum - 1) ? mTotalSize % ThreadNum : 0);
                    if(i > 0)
                    {
                        mPartStartPos[i] = mPartStartPos[i - 1] + mPartSize[i - 1];
                    }
                    else
                    {
                        mPartStartPos[0] = 0;
                    }

                    Loom.RunAsync(() =>
                    {
                        onRecave(_i);
                    });
                 }

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
            double p = 100d - ThreadNum;
            while (!hasFinish)
            {
                long doneSize = 0;
                for (int i = 0; i < ThreadNum; i++)
                {
                    doneSize += mPartDoneSize[i];
                }
                hasFinish = doneSize == mTotalSize;
                double progress = Math.Min((double)doneSize / mTotalSize, p/100);
                if (hasFinish)
                {
                    if(Tools.CheckFileExists(SavePath))
                    {
                        FileInfo info = new FileInfo(SavePath);
                        info.Delete();
                    }
                    if(ThreadNum == 1)
                    {                        
                        callback(0.99d, "重命名");
                        File.Move(SavePath + ".tmp0", SavePath);
                    }
                    else
                    {
                        callback(p/100, "开始合并临时文件");
                        byte[] bytes = new byte[BufferSize];
                        FileStream fs = new FileStream(SavePath, FileMode.Create);
                        FileStream fsTemp = null;
                        for (int i = 0; i < ThreadNum; i++)
                        {
                            fsTemp = new FileStream(SavePath+".tmp"+i, FileMode.Open);
                            int len = fsTemp.Read(bytes, 0, BufferSize);
                            while(len > 0)
                            {
                                fs.Write(bytes, 0, len);
                                len = fsTemp.Read(bytes, 0, BufferSize);
                            }
                            fsTemp.Close();
                            p += 1;
                            callback(p/ 100, "合并临时文件:"+i);
                        }
                        fs.Close();

                        for (int i = 0; i < ThreadNum; i++)
                        {
                            FileInfo info = new FileInfo(SavePath + ".tmp" + i);
                            info.Delete();
                        }
                    }
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

        private void onRecave(int idx)
        {
            string fileName = SavePath + ".tmp"+idx;
            //LogFile.Log("onRecave idx:{0}, fileName:{1}", idx, fileName);
            byte[] buffer = new byte[BufferSize];

            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
            mPartDoneSize[idx] = fs.Length;
            if(!mPartialEnabled)
            {
                mPartDoneSize[idx] = 0;
            }
            try
            {
                //LogFile.Log("onRecave idx:{0}, readSize:{1}", idx, mPartDoneSize[idx]);
                if(mPartDoneSize[idx] < mPartSize[idx])
                {
                    //TODO:断点续传还有问题
                    //断点续传核心，设置本地文件流的起始位置
                    fs.Seek(mPartDoneSize[idx], SeekOrigin.Begin);
                    mRequest.AddRange((int)mPartStartPos[idx], (int)(mPartStartPos[idx] + mPartSize[idx] - 1));
                    HttpWebResponse response = (HttpWebResponse)mRequest.GetResponse();
                    Stream stream = response.GetResponseStream();
                    //TODO:判断服务器是否支持读取范围
                    int length = stream.Read(buffer, 0, BufferSize);
                    while (length > 0)
                    {
                        fs.Write(buffer, 0, length);
                        mPartDoneSize[idx] += length;
                        if(mPartDoneSize[idx] > mPartSize[idx])
                        {
                            LogFile.Error(fileName + "下载出错,写入大小大于原文件分块");
                        }
                        length = stream.Read(buffer, 0, BufferSize);
                        //LogFile.WriteLine(string.Format("tmp{0}下载进度:{1}/{2}", idx, mPartDoneSize[idx], mPartSize[idx]));
                    }
                    //LogFile.WriteLine(string.Format("tmp{0}下载完成:{1}/{2}", idx, mPartDoneSize[idx], mPartSize[idx]));
                    stream.Close();
                    stream.Dispose();
                }
                else
                {
                    //TODO:之前已经下载完成
                    mPartDoneSize[idx] = mPartSize[idx];
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
