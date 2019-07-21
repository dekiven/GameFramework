using UnityEngine;
using System.Collections;
using System;
using LuaInterface;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameFramework
{
    public class TimeOutWWW : MonoBehaviour
    {
        public delegate void WWWRstDel(string noticeKey, double progress, int index, string msg);
        public delegate void WWWUrlRstDel(bool rst, string msg);
        public delegate void WWWUrlRstBytesDel(bool rst, byte[] msg);

        /// <summary>
        /// 所有操作(上传或下载)全部完成
        /// </summary>
        public const string STR_SUCCEEDED = "succeeded";
        /// <summary>
        /// 有操作执行失败
        /// </summary>
        public const string STR_FAILED = "failed";
        /// <summary>
        /// 有一个文件上传或下载成功
        /// </summary>
        public const string STR_DONE = "done";
        /// <summary>
        /// www 回调的间隔时间
        /// </summary>
        public float NotifyIterval = 0.5f;
        public string NoticeKey;


        private WWW mWWW = null;
        private WWWRstDel mRstDel = null;
        private WWWUrlRstDel mUrlRstDel = null;
        private WWWUrlRstBytesDel mUrlRstBytesDel = null;
        private LuaFunction mLuaFunc = null;
        private float mTimeoutSec = 1f;
        private Coroutine mCoroutine = null;
        private bool mIsTimeOut;
        private long mTotalSize = 0;
        private long mDoneSize = 0;
        private List<WWWInfo> mList;
        private List<WWWInfo> mFialedList;
        private int mDoneCount = 0;
        private WWWType mType = WWWType.download;
        private float lastCallTime = 0;


        #region MonoBehaviour
        void Awake()
        {
            mList = new List<WWWInfo>();
            mFialedList = new List<WWWInfo>();
        }

        void Start()
        {
            if (mList.Count > 0)
            {
                lastCallTime = Time.time;
                switch(mType)
                {
                    case WWWType.request :
                        startUrlWWW();
                        break;
                    case WWWType.upload :
                    case WWWType.download :
                        startNewWWW();
                        break;
                    case WWWType.read :
                    case WWWType.readBytes :
                        startReadWWW();
                        break;
                        
                }
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
            setLuaCallback(null);
        }
        #endregion MonoBehaviour

        public void ReadFileStr(string noticeKey, string fileUrl, float timeoutSec, WWWUrlRstDel del, LuaFunction lua)
        {
            mType = WWWType.read;
            mUrlRstDel = del;
            setLuaCallback(lua);
            mTimeoutSec = timeoutSec;
            NoticeKey = noticeKey;

            mList.Clear();
            mList.Add(new WWWInfo(fileUrl, ""));
        }

        public void ReadFileBytes(string noticeKey, string fileUrl, float timeoutSec, WWWUrlRstBytesDel del, LuaFunction lua)
        {
            mType = WWWType.readBytes;
            mUrlRstBytesDel = del;
            setLuaCallback(lua);
            mTimeoutSec = timeoutSec;
            NoticeKey = noticeKey;

            mList.Clear();
            mList.Add(new WWWInfo(fileUrl, ""));
        }

        public void ReadFirstExistsStr(string noticeKey, List<string> files, float timeoutSec, WWWUrlRstDel del, LuaFunction lua)
        {
            mType = WWWType.read;
            mUrlRstDel = del;
            setLuaCallback(lua);
            mTimeoutSec = timeoutSec;
            NoticeKey = noticeKey;

            mList.Clear();
            for (int i = 0; i < files.Count; i++)
            {
                string fileUrl = files[i];
                mList.Add(new WWWInfo(fileUrl, ""));
            }
        }

        public void ReadFirstExistsBytes(string noticeKey, List<string> files, float timeoutSec, WWWUrlRstBytesDel del, LuaFunction lua)
        {
            mType = WWWType.readBytes;
            mUrlRstBytesDel = del;
            setLuaCallback(lua);
            mTimeoutSec = timeoutSec;
            NoticeKey = noticeKey;

            mList.Clear();
            for (int i = 0; i < files.Count; i++)
            {
                string fileUrl = files[i];
                mList.Add(new WWWInfo(fileUrl, ""));
            }
        }

        /// <summary>
        /// 下载一个文件
        /// </summary>
        /// <param name="noticeKey">下载的key，用于多个下载组件时区分</param>
        /// <param name="info">下载的信息</param>
        /// <param name="timeoutSec">超时时间（秒）</param>
        /// <param name="del">下载进度回调
        /// (string noticeKey, double progress, int index, string msg)
        /// 下载key,下载进度（-1代表有错，1代表完成）,当前下载的文件index,msg信息（当progress==1且msg=="succeeded"才表示全部下载完成）
        /// </param>
        /// <param name="lua">lua回调,参数跟del相同</param>
        public void DownloadFile(string noticeKey, WWWInfo info, float timeoutSec, WWWRstDel del, LuaFunction lua)
        {
            mType = WWWType.download;
            mRstDel = del;
            setLuaCallback(lua);
            mTimeoutSec = timeoutSec;
            mTotalSize = info.Size;
            NoticeKey = noticeKey;

            mList.Clear();
            mList.Add(info);
        }

        /// <summary>
        /// 下载多个文件
        /// </summary>
        /// <param name="noticeKey">下载的key，用于多个下载组件时区分</param>
        /// <param name="infos">下载的信息</param>
        /// <param name="timeoutSec">超时时间（秒）</param>
        /// <param name="del">下载进度回调
        /// (string noticeKey, double progress, int index, string msg)
        /// 下载key,下载进度（-1代表有错；1代表全部下载过[可能完成或部分失败]）,当前下载的文件index(下载完成的个数),msg信息（当progress==1且msg=="succeeded"才表示全部下载成功）
        /// </param>
        /// <param name="lua">lua回调,参数跟del相同</param>
        public void DownloadFiles(string noticeKey, List<WWWInfo> infos, float timeoutSec, WWWRstDel del, LuaFunction lua)
        {
            mType = WWWType.download;
            if (null == infos || infos.Count == 0)
            {
                callback(-1, "download files info list is null or empty!");
                return;
            }
            mRstDel = del;
            setLuaCallback(lua);
            mTimeoutSec = timeoutSec;
            NoticeKey = noticeKey;
            mTotalSize = 0;
            for (int i = 0; i < infos.Count; i++)
            {
                mTotalSize += infos[i].Size;
            }

            mList.Clear();
            mList.AddRange(infos);
            mFialedList.Clear();
        }

        /// <summary>
        /// 上传一个文件
        /// </summary>
        /// <param name="noticeKey">下载的key，用于多个上传组件时区分</param>
        /// <param name="info">上传的信息</param>
        /// <param name="timeoutSec">超时时间（秒）</param>
        /// <param name="del">上传进度回调
        /// (string noticeKey, double progress, int index, string msg);
        /// 上传key,上传进度（-1代表有错；1代表全部上传过[可能完成或部分失败]）,当前上传的文件index,msg信息（当progress==1且msg=="succeeded"才表示上传成功）
        /// </param>
        /// <param name="lua">lua回调,参数跟del相同</param>
        public void UploadFile(string noticeKey, WWWInfo info, float timeoutSec, WWWRstDel del, LuaFunction lua)
        {
            mType = WWWType.upload;
            mRstDel = del;
            setLuaCallback(lua);
            mTimeoutSec = timeoutSec;
            mTotalSize = info.Size;
            NoticeKey = noticeKey;

            mList.Clear();
            mList.Add(info);
        }

        /// <summary>
        /// 上传多个文件
        /// </summary>
        /// <param name="noticeKey">上传的key，用于多个上传组件时区分</param>
        /// <param name="infos">上传的信息</param>
        /// <param name="timeoutSec">超时时间（秒）</param>
        /// <param name="del">上传进度回调
        /// (string noticeKey, double progress, int index, string msg)
        /// 上传key,下载进度（-1代表有错；1代表全部上传过[可能完成或部分失败]）,index当前上传的文件index（0开始）,msg信息（当progress==1且msg=="succeeded"才表示上传成功）
        /// </param>
        /// <param name="lua">lua回调,参数跟del相同</param>
        public void UploadFiles(string noticeKey, List<WWWInfo> infos, float timeoutSec, WWWRstDel del, LuaFunction lua)
        {
            mType = WWWType.upload;
            if (null == infos || infos.Count == 0)
            {
                callback(-1, "download files info list is null or empty!");
                return;
            }
            mRstDel = del;
            setLuaCallback(lua);
            mTimeoutSec = timeoutSec;
            NoticeKey = noticeKey;
            for (int i = 0; i < infos.Count; i++)
            {
                mTotalSize = infos[i].Size;
            }

            mList.Clear();
            mList.AddRange(infos);
            mFialedList.Clear();
        }

        /// <summary>
        /// 请求某个链接，并获取其返回值或错误信息
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="timeoutSec">超时时间（秒）</param>
        /// <param name="del">请求回调:
        /// (bool rst, string msg)
        /// rst 表示请求是否成功,msg信息（成返回结果，失败返回错误信息）
        /// </param>
        /// <param name="lua">lua回调,参数跟del相同</param>
        public void RequestUrl(string url, float timeoutSec, WWWUrlRstDel del, LuaFunction lua)
        {
            mType = WWWType.request;
            mUrlRstDel = del;
            setLuaCallback(lua);
            mTimeoutSec = timeoutSec;

            mList.Add(new WWWInfo()
            {
                Url = url,
            });
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
            if (lastCallTime+NotifyIterval >= Time.time || Math.Abs(progress).Equals(1d) || msg.Equals(STR_DONE))
            {
                if (null != mRstDel)
                {
                    mRstDel(NoticeKey, progress, mDoneCount+mFialedList.Count, msg);
                }
                if (null != mLuaFunc)
                {
                    mLuaFunc.Call(NoticeKey, progress, mDoneCount+mFialedList.Count, msg);
                }
                lastCallTime = Time.time;
                if (progress >= 1)
                {
                    clear();
                }
            }            
        }

        private void callback(bool rst, string msg)
        {
            if (null != mUrlRstDel)
            {
                mUrlRstDel(rst, msg);
            }
            if (null != mLuaFunc)
            {
                mLuaFunc.Call(rst, mDoneCount, msg);
            }
            if (rst)
            {
                clear();
            }
        }

        private void callbackBytes(bool rst, byte[] msg)
        {
            if (null != mUrlRstBytesDel)
            {
                mUrlRstBytesDel(rst, msg);
            }
            if (null != mLuaFunc)
            {
                mLuaFunc.Call(rst, mDoneCount, new LuaByteBuffer(msg));
            }
            if (rst)
            {
                clear();
            }
        }

        private void clear()
        {
            //mRstDel = null;
            //mLuaFunc.Dispose();
            //mLuaFunc = null;
            //mTimeoutSec = 0f;
            //mTotalSize = 0;
            //if (null != mWWW)
            //{
            //    mWWW.Dispose();
            //    mWWW = null;
            //}
            if (null != mCoroutine)
            {
                StopCoroutine(mCoroutine);
            }

            Destroy(this);
        }

        /// <summary>
        /// 检查www的超时
        /// </summary>
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

        private void startUrlWWW()
        {
            if (null == mCoroutine)
            {
                mCoroutine = StartCoroutine(withUrlWww());
            }
        }

        private void startReadWWW()
        {
            if (null == mCoroutine)
            {
                mCoroutine = StartCoroutine(readWww());
            }
        }

        private IEnumerator www()
        {
            for (int i = 0; i < mList.Count; i++)
            {
                WWWInfo info = mList[i];
                if (string.IsNullOrEmpty(info.Url) || string.IsNullOrEmpty(info.TargetPath))
                {
                    callback(-1f, "url/target is null or empty ");
                    continue;
                }
                if (WWWType.download == mType)
                {
                    mWWW = new WWW(info.Url);
                }
                if (WWWType.upload == mType)
                {
                    WWWUploadInfo uploadInfo = info as WWWUploadInfo;
                    string path = uploadInfo.TargetPath;
                    if (!string.IsNullOrEmpty(path) && Tools.CheckFileExists(path))
                    {
                        WWW file = new WWW(Tools.GetUrlPathWriteabble(path));
                        while (!file.isDone)
                        {
                            yield return null;
                        }
                        if (string.IsNullOrEmpty(file.error))
                        {
                            //WWWForm是一个辅助类，该类用于生成表单数据，
                            //然后WWW类就可以将该表单数据post到web服务器上了
                            WWWForm form = new WWWForm();
                            //添加二进制文件到表单，使用该函数可以上传文件或者图片到Web服务器     
                            form.AddBinaryData(uploadInfo.FieldName, file.bytes, uploadInfo.SaveName, uploadInfo.MimeType);
                            mWWW = new WWW(uploadInfo.Url, form);
                        }
                    }
                }

                if (null == mWWW)
                {
                    mFialedList.Add(info);
                    callback(-1f, "www is null");
                    continue;
                }
                yield return checkWWWTimeout(mTimeoutSec);
                if (mIsTimeOut)
                {
                    mWWW.Dispose();
                    mWWW = null;
                    mFialedList.Add(info);
                    callback(-1f, "time out");
                }
                else
                {
                    if (mWWW.isDone)
                    {
                        if (!string.IsNullOrEmpty(mWWW.error))
                        {
                            //WWW上传或下载失败
                            mWWW.Dispose();
                            mWWW = null;
                            mFialedList.Add(info);
                            callback(-1f, mWWW.error);
                        }
                        else
                        {
                            mDoneSize += info.Size;
                            mDoneCount += 1;
                            if (WWWType.download == mType)
                            {
                                //下载完成,拷贝到目标位置
                                string savePath = info.TargetPath;
                                if (!string.IsNullOrEmpty(savePath))
                                {
                                    if (File.Exists(savePath))
                                    {
                                        File.Delete(savePath);
                                    }
                                    Tools.CheckDirExists(Directory.GetParent(savePath).ToString(), true);
                                    FileStream fsDes = File.Create(savePath);
                                    //TODO:优化，拷贝大文件时适当yield return
                                    fsDes.Write(mWWW.bytes, 0, mWWW.bytes.Length);
                                    fsDes.Flush();
                                    fsDes.Close();
                                }
                            }

                            callback(getProgress(0), STR_DONE);
                        }
                    }
                    mWWW.Dispose();
                    mWWW = null;
                }
            }
            yield return null;
            if (mFialedList.Count == 0)
            {
                callback(1, STR_SUCCEEDED);
            }
            else
            {
                callback(1, STR_FAILED);
            }
        }

        private IEnumerator withUrlWww()
        {
            mWWW = new WWW(mList[0].Url);
            if (null != mWWW)
            {
                yield return checkWWWTimeout(mTimeoutSec);
                if (mIsTimeOut)
                {
                    mWWW.Dispose();
                    mWWW = null;
                    //请求失败
                    callback(false, mWWW.error);
                }
                else
                {
                    if (mWWW.isDone)
                    {
                        if (!string.IsNullOrEmpty(mWWW.error))
                        {
                            //请求失败
                            callback(false, mWWW.error);
                        }
                        else
                        {
                            //请求完成
                            callback(true, mWWW.text);
                        }
                    }   
                }
            }
            yield return null;
        }

        private IEnumerator readWww()
        {
            string error = string.Empty;
            for (int i = 0; i < mList.Count; i++)
            {
                mWWW = new WWW(mList[i].Url);
                if (null != mWWW)
                {
                    yield return checkWWWTimeout(mTimeoutSec);
                    if (mIsTimeOut)
                    {
                        error = "读取 {"+mWWW.url+"} 超时";
                    }
                    else
                    {
                        if (mWWW.isDone)
                        {
                            //请求失败
                            if (!string.IsNullOrEmpty(mWWW.error))
                            {
                                error = mWWW.error;
                            }
                            else
                            {
                                //请求完成
                                if (mType == WWWType.read)
                                {
                                    callback(true, mWWW.text);
                                }
                                else
                                {
                                    callbackBytes(true, mWWW.bytes);
                                }
                                yield break;
                            }
                        }
                    }
                    mWWW.Dispose();
                    mWWW = null;
                }
            }
            //所有读取都超时或者有错
            if (mType == WWWType.read)
            {
                callback(false, error);
            }
            else
            {
                callbackBytes(false, Encoding.Default.GetBytes(error));
            }
            yield return null;
        }

        private double getProgress(int curSize)
        {
            double progress = 0d;
            if (mTotalSize > 0)
            {
                progress = (mDoneSize + curSize) / (double)mTotalSize;
            }
            else
            {
                progress = (mDoneCount + curSize) / (double)mList.Count;
            }
            if (progress >= 1d)
            {
                progress = 0.99d;
            }
            return progress;
        }
        #endregion 私有方法
    }

    //public class WWWInfo : IDisposable
    //{
    //    public static WWWInfo Default = new WWWInfo ("", "");

    //    public string Url;
    //    public string TargetPath;
    //    public long Size;
    //    public Dictionary<string, string> Headers;

    //    public WWWInfo()
    //    {

    //    }

    //    public WWWInfo(string url, string targetPath, long size = 0, Dictionary<string, string> headers = null)
    //    {
    //        Url = url;
    //        TargetPath = targetPath;
    //        Size = size;
    //        Headers = headers;
    //    }

    //    public WWWInfo(LuaTable table)
    //    {
    //        if (null != table)
    //        {
    //            Url = table.RawGet<string, string>("url");
    //            TargetPath = table.RawGet<string, string>("targetPath");
    //            Size = table.RawGet<string, long>("size");
    //            LuaTable ht = table.RawGet<string, LuaTable>("headers");
    //            if (null != ht)
    //            {
    //                Headers = ht.ToDictTable<string, string>().ToDictionary();
    //                ht.Dispose();
    //                ht = null;
    //            }
    //            table.Dispose();
    //            table = null;
    //        }
    //    }

    //    public static List<WWWInfo> GetListByLua(LuaTable table)
    //    {
    //        List<WWWInfo> infos = new List<WWWInfo>();
    //        LuaArrayTable luaArray = new LuaArrayTable(table);
    //        luaArray.ForEach((obj) =>
    //        {
    //            LuaTable t = obj as LuaTable;
    //            if (null != t)
    //            {
    //                infos.Add(new WWWInfo(t));
    //                t.Dispose();
    //                t = null;
    //            }
    //        });
    //        luaArray.Dispose();
    //        luaArray = null;
    //        table.Dispose();
    //        table = null;
    //        return infos;
    //    }

    //    public void Dispose()
    //    {
    //        Headers.Clear();
    //    }
    //}

    //public class WWWUploadInfo : WWWInfo
    //{
    //    public string SaveName;
    //    public string FieldName;
    //    public string MimeType;

    //    public WWWUploadInfo()
    //    {
    //    }

    //    public WWWUploadInfo(string url, string targetPath, string fieldName, string saveName = "", string mimeType = "", long size = 0, Dictionary<string, string> headers = null) : base(url, targetPath, size, headers)
    //    {
    //        FieldName = fieldName;
    //        SaveName = saveName;
    //        if (string.IsNullOrEmpty(SaveName))
    //        {
    //            SaveName = Tools.FormatPathStr(targetPath);
    //            SaveName = SaveName.Substring(SaveName.LastIndexOf("/", StringComparison.Ordinal) + 1);
    //        }
    //        MimeType = mimeType;
    //        if (string.IsNullOrEmpty(MimeType))
    //        {
    //            MimeType = GetMimeTypeByName(saveName);
    //        }
    //    }

    //    public WWWUploadInfo(LuaTable table)
    //    {
    //        if (null != table)
    //        {
    //            Url = table.RawGet<string, string>("url");
    //            TargetPath = table.RawGet<string, string>("targetPath");
    //            Size = table.RawGet<string, long>("size");
    //            SaveName = table.RawGet<string, string>("saveName");
    //            FieldName = table.RawGet<string, string>("fieldName");
    //            MimeType = table.RawGet<string, string>("mimeType");
    //            LuaTable ht = table.RawGet<string, LuaTable>("headers");
    //            if (null != ht)
    //            {
    //                Headers = ht.ToDictTable<string, string>().ToDictionary();
    //                ht.Dispose();
    //                ht = null;
    //            }
    //            table.Dispose();
    //            table = null;
    //        }
    //    }

    //    public static new List<WWWInfo> GetListByLua(LuaTable table)
    //    {
    //        List<WWWInfo> infos = new List<WWWInfo>();
    //        LuaArrayTable luaArray = new LuaArrayTable(table);
    //        luaArray.ForEach((obj) =>
    //        {
    //            LuaTable t = obj as LuaTable;
    //            if (null != t)
    //            {
    //                infos.Add(new WWWUploadInfo(t));
    //                t.Dispose();
    //                t = null;
    //            }
    //        });
    //        luaArray.Dispose();
    //        luaArray = null;
    //        table.Dispose();
    //        table = null;
    //        return infos;
    //    }

    //    public static string GetMimeTypeByName(string saveName)
    //    {
    //        #region GetMimeTypeByName
    //        string type = "application/octet-stream";
    //        string extension = Path.GetExtension(saveName);
    //        switch (extension)
    //        {
    //            case ".swf":
    //                return "application/x-shockwave-flash";
    //            case ".dll":
    //                return "application/x-msdownload";
    //            case ".exe":
    //                return "application/octet-stream";
    //            case ".rar":
    //                return "application/octet-stream";
    //            case ".tar":
    //                return "application/x-tar";
    //            case ".tgz":
    //                return "application/x-compressed";
    //            case ".zip":
    //                return "application/x-zip-compressed";
    //            case ".z":
    //                return "application/x-compress";
    //            case ".wav":
    //                return "audio/wav";
    //            case ".wma":
    //                return "audio/x-ms-wma";
    //            case ".wmv":
    //                return "video/x-ms-wmv";
    //            case ".mp3":
    //            case ".mp2":
    //            case ".mpe":
    //            case ".mpeg":
    //            case ".mpg":
    //                return "audio/mpeg";
    //            case ".rm":
    //                return "application/vnd.rn-realmedia";
    //            case ".mid":
    //            case ".midi":
    //            case ".rmi":
    //                return "audio/mid";
    //            case ".bmp":
    //                return "image/bmp";
    //            case ".gif":
    //                return "image/gif";
    //            case ".png":
    //                return "image/png";
    //            case ".tif":
    //            case ".tiff":
    //                return "image/tiff";
    //            case ".jpe":
    //            case ".jpeg":
    //            case ".jpg":
    //                return "image/jpeg";
    //            case ".txt":
    //            case ".log":
    //                return "text/plain";
    //            case ".xml":
    //                return "text/xml";
    //            case ".html":
    //                return "text/html";
    //            case ".css":
    //                return "text/css";
    //            case ".js":
    //                return "text/javascript";
    //        }
    //        return type;
    //        #endregion GetMimeTypeByName
    //    }
    //}

    //public enum WWWType
    //{
    //    read,
    //    readBytes,
    //    download,
    //    upload,
    //    request,
    //}
}
