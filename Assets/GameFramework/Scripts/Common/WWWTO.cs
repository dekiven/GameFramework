using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LuaInterface;
using UnityEngine;
namespace GameFramework
{
    /// <summary>
    /// 带超时的 WWW
    /// </summary>
    public class WWWTO : DisposableObj
    {

        public delegate void WWWRstDel(double progress, int index, string msg);
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
        /// www 回调的间隔时间，默认0.5 秒
        /// </summary>
        public float NotifyIterval = 0.5f;
        public long TotalSize { get { return mTotalSize; } }
        public long DoneSize { get { return mDoneSize; }}
        /// <summary>
        /// 超时时间（秒），默认 3 秒
        /// </summary>
        public float TimeoutSec = 3f;
        /// <summary>
        /// 失败重试次数，默认 3 次
        /// </summary>
        public int Retry = 3;
        /// <summary>
        /// 下载进度
        /// </summary>
        public double Progress { get { return mProgress; } }

        private WWW mWWW = null;
        private WWWRstDel mRstDel = null;
        private WWWUrlRstDel mUrlRstDel = null;
        private WWWUrlRstBytesDel mUrlRstBytesDel = null;
        private LuaFunction mLuaFunc = null;
        private int mCoroutine = 0;
        private bool mIsTimeOut;
        private long mTotalSize = 0;
        private long mDoneSize = 0;
        private double mProgress;
        private List<WWWInfo> mList;
        private List<WWWInfo> mFialedList;
        private int mDoneCount = 0;
        private WWWType mType = WWWType.download;
        private float mLastCallTime = 0;
        private float mRetryCount = 0;


        #region 静态方法
        public static WWWTO ReadFileStr(string fileUrl, WWWUrlRstDel del, LuaFunction lua)
        {
            WWWTO www = new WWWTO();
            www.mType = WWWType.read;
            www.mUrlRstDel = del;
            www._setLuaCallback(lua);

            www.mList.Clear();
            www.mList.Add(new WWWInfo(fileUrl, ""));

            return www;
        }

        public static WWWTO ReadFileBytes(string fileUrl, WWWUrlRstBytesDel del, LuaFunction lua)
        {
            WWWTO www = new WWWTO();
            www.mType = WWWType.readBytes;
            www.mUrlRstBytesDel = del;
            www._setLuaCallback(lua);

            www.mList.Clear();
            www.mList.Add(new WWWInfo(fileUrl, ""));

            return www;
        }

        [NoToLuaAttribute]
        public static WWWTO ReadFirstExistsStr(List<string> files, WWWUrlRstDel del, LuaFunction lua)
        {
            WWWTO www = new WWWTO();
            www.mType = WWWType.read;
            www.mUrlRstDel = del;
            www._setLuaCallback(lua);

            www.mList.Clear();
            for (int i = 0; i < files.Count; i++)
            {
                string fileUrl = files[i];
                www.mList.Add(new WWWInfo(fileUrl, ""));
            }

            return www;
        }

        [NoToLuaAttribute]
        public static WWWTO ReadFirstExistsBytes(List<string> files, WWWUrlRstBytesDel del, LuaFunction lua)
        {
            WWWTO www = new WWWTO();
            www.mType = WWWType.readBytes;
            www.mUrlRstBytesDel = del;
            www._setLuaCallback(lua);

            www.mList.Clear();
            for (int i = 0; i < files.Count; i++)
            {
                string fileUrl = files[i];
                www.mList.Add(new WWWInfo(fileUrl, ""));
            }

            return www;
        }

        /// <summary>
        /// 下载一个文件
        /// </summary>
        /// <param name="info">下载的信息</param>
        /// <param name="del">下载进度回调
        /// (double progress, int index, string msg)
        /// 下载key,下载进度（-1代表有错，1代表完成）,当前下载的文件index,msg信息（当progress==1且msg=="succeeded"才表示全部下载完成）
        /// </param>
        /// <param name="lua">lua回调,参数跟del相同</param>
        [NoToLuaAttribute]
        public static WWWTO DownloadFile(WWWInfo info, WWWRstDel del, LuaFunction lua)
        {
            WWWTO www = new WWWTO();
            www.mType = WWWType.download;
            www.mRstDel = del;
            www._setLuaCallback(lua);
            www.mTotalSize = info.Size;

            www.mList.Clear();
            www.mList.Add(info);

            return www;
        }

        /// <summary>
        /// 下载多个文件
        /// </summary>
        /// <param name="infos">下载的信息</param>
        /// <param name="del">下载进度回调
        /// (double progress, int index, string msg)
        /// 下载key,下载进度（-1代表有错；1代表全部下载过[可能完成或部分失败]）,当前下载的文件index(下载完成的个数),msg信息（当progress==1且msg=="succeeded"才表示全部下载成功）
        /// </param>
        /// <param name="lua">lua回调,参数跟del相同</param>
        [NoToLuaAttribute]
        public static WWWTO DownloadFiles(List<WWWInfo> infos, WWWRstDel del, LuaFunction lua)
        {
            WWWTO www = new WWWTO();
            www.mType = WWWType.download;
            www.mRstDel = del;
            www._setLuaCallback(lua);

            if (null == infos || infos.Count == 0)
            {
                www._callback(-1, "download files info list is null or empty!");
                www.Dispose();
                return null;
            }

            www.mTotalSize = 0;
            for (int i = 0; i < infos.Count; i++)
            {
                www.mTotalSize += infos[i].Size;
            }

            www.mList.Clear();
            www.mList.AddRange(infos);
            www.mFialedList.Clear();

            return www;
        }

        /// <summary>
        /// 上传一个文件
        /// </summary>
        /// <param name="info">上传的信息</param>
        /// <param name="del">上传进度回调
        /// (double progress, int index, string msg);
        /// 上传key,上传进度（-1代表有错；1代表全部上传过[可能完成或部分失败]）,当前上传的文件index,msg信息（当progress==1且msg=="succeeded"才表示上传成功）
        /// </param>
        /// <param name="lua">lua回调,参数跟del相同</param>
        [NoToLuaAttribute]
        public static WWWTO UploadFile(WWWInfo info, WWWRstDel del, LuaFunction lua)
        {
            WWWTO www = new WWWTO();
            www.mType = WWWType.upload;
            www.mRstDel = del;
            www._setLuaCallback(lua);
            www.mTotalSize = info.Size;

            www.mList.Clear();
            www.mList.Add(info);

            return www;
        }

        /// <summary>
        /// 上传多个文件
        /// </summary>
        /// <param name="infos">上传的信息</param>
        /// <param name="del">上传进度回调
        /// (double progress, int index, string msg)
        /// 下载进度（-1代表有错；1代表全部上传过[可能完成或部分失败]）,index当前上传的文件index（0开始）,msg信息（当progress==1且msg=="succeeded"才表示上传成功）
        /// </param>
        /// <param name="lua">lua回调,参数跟del相同</param>
        [NoToLuaAttribute]
        public static WWWTO UploadFiles(List<WWWInfo> infos, WWWRstDel del, LuaFunction lua)
        {
            WWWTO www = new WWWTO();
            www.mType = WWWType.upload;

            www.mRstDel = del;
            www._setLuaCallback(lua);

            if (null == infos || infos.Count == 0)
            {
                www._callback(-1, "download files info list is null or empty!");
                www.Dispose();
                return null;
            }
            for (int i = 0; i < infos.Count; i++)
            {
                www.mTotalSize += infos[i].Size;
            }

            www.mList.Clear();
            www.mList.AddRange(infos);
            www.mFialedList.Clear();

            return www;
        }

        /// <summary>
        /// 请求某个链接，并获取其返回值或错误信息
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="del">请求回调:
        /// (bool rst, string msg)
        /// rst 表示请求是否成功,msg信息（成返回结果，失败返回错误信息）
        /// </param>
        /// <param name="lua">lua回调,参数跟del相同</param>
        public static WWWTO RequestUrl(string url, WWWUrlRstDel del, LuaFunction lua)
        {
            WWWTO www = new WWWTO();
            www.mType = WWWType.request;
            www.mUrlRstDel = del;
            www._setLuaCallback(lua);

            www.mList.Add(new WWWInfo()
            {
                Url = url,
            });
            return www;
        }
        #endregion 静态方法

        #region public 方法
        public WWWTO()
        {
            mList = new List<WWWInfo>();
            mFialedList = new List<WWWInfo>();
        }

        public void Start()
        {
            if (mList.Count > 0)
            {
                mLastCallTime = Time.time;
                switch (mType)
                {
                    case WWWType.request:
                        _startUrlWWW();
                        break;
                    case WWWType.upload:
                    case WWWType.download:
                        _startNewWWW();
                        break;
                    case WWWType.read:
                    case WWWType.readBytes:
                        _startReadWWW();
                        break;
                }
            }
        }

        protected override void _disposMananged()
        {
            if (null != mList)
            {
                mList.Clear();
            }
            if (null != mFialedList)
            {

                mFialedList.Clear();
            }
        }


        protected override void _disposUnmananged()
        {
            _stopCoroutine();
            if (null != mWWW)
            {
                mWWW.Dispose();
                mWWW = null;
            }
            _setLuaCallback(null);
        }
        #endregion public 方法

        #region 私有方法
        private void _setLuaCallback(LuaFunction lua)
        {
            if (null != mLuaFunc)
            {
                mLuaFunc.Dispose();
                mLuaFunc = null;
            }
            mLuaFunc = lua;
        }

        private void _callback(double progress, string msg)
        {
            if (mLastCallTime + NotifyIterval >= Time.time || Tools.Equals(progress, 1d) || msg.Equals(STR_DONE))
            {
                if (null != mRstDel)
                {
                    mRstDel(progress, mDoneCount + mFialedList.Count, msg);
                }
                if (null != mLuaFunc)
                {
                    mLuaFunc.Call(progress, mDoneCount + mFialedList.Count, msg);
                }
                mLastCallTime = Time.time;
            }
            mProgress = progress;
        }

        private void _callback(bool rst, string msg)
        {
            if (null != mUrlRstDel)
            {
                mUrlRstDel(rst, msg);
            }
            if (null != mLuaFunc)
            {
                mLuaFunc.Call(rst, msg);
            }
        }

        private void _callbackBytes(bool rst, byte[] msg)
        {
            if (null != mUrlRstBytesDel)
            {
                mUrlRstBytesDel(rst, msg);
            }
            if (null != mLuaFunc)
            {
                mLuaFunc.Call(rst, mDoneCount, new LuaByteBuffer(msg));
            }
        }

        private void _stopCoroutine()
        {
            if (0 != mCoroutine)
            {
                CoroutineMgr.Instance.StopCor(mCoroutine);
            }

        }

        /// <summary>
        /// 检查www的超时
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private IEnumerator _checkWWWTimeout(float timeout)
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
                        _callback(_getProgress(mWWW.bytesDownloaded), string.Empty);
                    }
                    else if (Time.time - lastTime >= timeout)
                    {
                        mIsTimeOut = true;
                    }
                    yield return null;
                }
            }
        }

        private void _startNewWWW()
        {
            _stopCoroutine();
            mCoroutine = CoroutineMgr.Instance.StartCor(_upOrDownload());
        }

        private void _startUrlWWW()
        {
            _stopCoroutine();
            mCoroutine = CoroutineMgr.Instance.StartCor(_requestUrl());
        }

        private void _startReadWWW()
        {
            _stopCoroutine();
            mCoroutine = CoroutineMgr.Instance.StartCor(_readFile());
        }

        private IEnumerator _upOrDownload()
        {
            for (int i = 0; i < mList.Count; i++)
            {
                WWWInfo info = mList[i];
                mRetryCount = 0;
                do
                {
                    if (string.IsNullOrEmpty(info.Url) || string.IsNullOrEmpty(info.TargetPath))
                    {
                        mFialedList.Add(info);
                        _callback(-1d, "url/target is null or empty. url:" + info.Url + "; target:" + info.TargetPath);
                        break;
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
                        else
                        {
                            mFialedList.Add(info);
                            _callback(-1d, "上传文件[" + path + "]不存在");
                            break;
                        }
                    }

                    yield return _checkWWWTimeout(TimeoutSec);

                    if (mIsTimeOut || !string.IsNullOrEmpty(mWWW.error))
                    {
                        mWWW.Dispose();
                        mWWW = null;
                        if (mRetryCount < Retry)
                        {
                            ++mRetryCount;
                            continue;
                        }
                        else
                        {
                            mFialedList.Add(info);
                            _callback(-1d, "超时或失失败;"+mWWW.error);
                        }
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
                        _callback(_getProgress(0), STR_DONE);
                        mWWW.Dispose();
                        mWWW = null;
                        break;
                    }
                } while (mRetryCount <= Retry);
            }
            yield return null;
            if (mFialedList.Count == 0)
            {
                _callback(1d, STR_SUCCEEDED);
            }
            else
            {
                _callback(1d, STR_FAILED);
            }
            yield break;
        }

        private IEnumerator _requestUrl()
        {
            do {
                mWWW = new WWW(mList[0].Url);
                if (null != mWWW)
                {
                    yield return _checkWWWTimeout(TimeoutSec);
                    if (mIsTimeOut || !string.IsNullOrEmpty(mWWW.error))
                    {
                        mWWW.Dispose();
                        mWWW = null;
                        if (mRetryCount < Retry)
                        {
                            ++mRetryCount;
                            continue;
                        }
                        else
                        {
                            //请求失败
                            _callback(false, "请求超时或失败：" + mWWW.error);
                        }
                    }
                    else
                    {
                        //请求完成
                        _callback(true, mWWW.text);
                    }
                }
            }while(mRetryCount <= Retry);

            yield break;
        }

        private IEnumerator _readFile()
        {
            string error = string.Empty;
            for (int i = 0; i < mList.Count; i++)
            {
                do
                {
                    mWWW = new WWW(mList[i].Url);
                    if (null != mWWW)
                    {
                        yield return _checkWWWTimeout(TimeoutSec);
                        if (mIsTimeOut || !string.IsNullOrEmpty(mWWW.error))
                        {
                            if (mRetryCount < Retry)
                            {
                                ++mRetryCount;
                                mWWW.Dispose();
                                mWWW = null;
                                continue;
                            }
                            else
                            {
                                //请求失败
                                error = "读取 {" + mWWW.url + "} 超时或失败;" + mWWW.error;

                                if (mType == WWWType.read)
                                {
                                    _callback(false, error);
                                }
                                else
                                {
                                    _callbackBytes(true, Encoding.Default.GetBytes(error));
                                }
                                mWWW.Dispose();
                                mWWW = null;
                                yield break;
                            }
                            
                        }
                        else
                        {
                            //请求完成
                            if (mType == WWWType.read)
                            {
                                _callback(true, mWWW.text);
                            }
                            else
                            {
                                _callbackBytes(true, mWWW.bytes);
                            }
                            mWWW.Dispose();
                            mWWW = null;
                            yield break;
                        }
                    }
                } while (mRetryCount <= Retry);
            }
            //所有读取都超时或者有错
            if (mType == WWWType.read)
            {
                _callback(false, error);
            }
            else
            {
                _callbackBytes(false, Encoding.Default.GetBytes(error));
            }
            yield break;
        }

        private double _getProgress(int curSize)
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

    public class WWWInfo
    {
        public static WWWInfo Default = new WWWInfo("", "");

        public string Url;
        public string TargetPath;
        public long Size;
        public Dictionary<string, string> Headers;

        public WWWInfo()
        {

        }

        public WWWInfo(string url, string targetPath, long size = 0, Dictionary<string, string> headers = null)
        {
            Url = url;
            TargetPath = targetPath;
            Size = size;
            Headers = headers;
        }

        public WWWInfo(LuaTable table)
        {
            if (null != table)
            {
                Url = table.RawGet<string, string>("url");
                TargetPath = table.RawGet<string, string>("target");
                try
                {
                    Size = table.RawGet<string, long>("size");
                }
                catch (Exception ex)
                {
                    LogFile.Log(Url+";"+ex.Message);
                }
                LuaTable ht = table.RawGet<string, LuaTable>("headers");
                if (null != ht)
                {
                    Headers = ht.ToDictTable<string, string>().ToDictionary();
                    ht.Dispose();
                    ht = null;
                }
                table.Dispose();
                table = null;
            }
        }

        public static List<WWWInfo> GetListByLua(LuaTable table)
        {
            List<WWWInfo> infos = new List<WWWInfo>();
            LuaArrayTable luaArray = new LuaArrayTable(table);
            luaArray.ForEach((obj) =>
            {
                LuaTable t = obj as LuaTable;
                if (null != t)
                {
                    infos.Add(new WWWInfo(t));
                    t.Dispose();
                    t = null;
                }
            });
            luaArray.Dispose();
            luaArray = null;
            table.Dispose();
            table = null;
            return infos;
        }
    }

    public class WWWUploadInfo : WWWInfo
    {
        public string SaveName;
        public string FieldName;
        public string MimeType;

        public WWWUploadInfo()
        {
        }

        public WWWUploadInfo(string url, string targetPath, string fieldName, string saveName = "", string mimeType = "", long size = 0, Dictionary<string, string> headers = null) : base(url, targetPath, size, headers)
        {
            FieldName = fieldName;
            SaveName = saveName;
            if (string.IsNullOrEmpty(SaveName))
            {
                SaveName = Tools.FormatPathStr(targetPath);
                SaveName = SaveName.Substring(SaveName.LastIndexOf("/", StringComparison.Ordinal) + 1);
            }
            MimeType = mimeType;
            if (string.IsNullOrEmpty(MimeType))
            {
                MimeType = GetMimeTypeByName(saveName);
            }
        }

        public WWWUploadInfo(LuaTable table)
        {
            if (null != table)
            {
                Url = table.RawGet<string, string>("url");
                TargetPath = table.RawGet<string, string>("targetPath");
                Size = table.RawGet<string, long>("size");
                SaveName = table.RawGet<string, string>("saveName");
                FieldName = table.RawGet<string, string>("fieldName");
                MimeType = table.RawGet<string, string>("mimeType");
                LuaTable ht = table.RawGet<string, LuaTable>("headers");
                if (null != ht)
                {
                    Headers = ht.ToDictTable<string, string>().ToDictionary();
                    ht.Dispose();
                    ht = null;
                }
                table.Dispose();
                table = null;
            }
        }

        public static new List<WWWInfo> GetListByLua(LuaTable table)
        {
            List<WWWInfo> infos = new List<WWWInfo>();
            LuaArrayTable luaArray = new LuaArrayTable(table);
            luaArray.ForEach((obj) =>
            {
                LuaTable t = obj as LuaTable;
                if (null != t)
                {
                    infos.Add(new WWWUploadInfo(t));
                    t.Dispose();
                    t = null;
                }
            });
            luaArray.Dispose();
            luaArray = null;
            table.Dispose();
            table = null;
            return infos;
        }

        public static string GetMimeTypeByName(string saveName)
        {
            #region GetMimeTypeByName
            string type = "application/octet-stream";
            string extension = Path.GetExtension(saveName);
            switch (extension)
            {
                case ".swf":
                    return "application/x-shockwave-flash";
                case ".dll":
                    return "application/x-msdownload";
                case ".exe":
                    return "application/octet-stream";
                case ".rar":
                    return "application/octet-stream";
                case ".tar":
                    return "application/x-tar";
                case ".tgz":
                    return "application/x-compressed";
                case ".zip":
                    return "application/x-zip-compressed";
                case ".z":
                    return "application/x-compress";
                case ".wav":
                    return "audio/wav";
                case ".wma":
                    return "audio/x-ms-wma";
                case ".wmv":
                    return "video/x-ms-wmv";
                case ".mp3":
                case ".mp2":
                case ".mpe":
                case ".mpeg":
                case ".mpg":
                    return "audio/mpeg";
                case ".rm":
                    return "application/vnd.rn-realmedia";
                case ".mid":
                case ".midi":
                case ".rmi":
                    return "audio/mid";
                case ".bmp":
                    return "image/bmp";
                case ".gif":
                    return "image/gif";
                case ".png":
                    return "image/png";
                case ".tif":
                case ".tiff":
                    return "image/tiff";
                case ".jpe":
                case ".jpeg":
                case ".jpg":
                    return "image/jpeg";
                case ".txt":
                case ".log":
                    return "text/plain";
                case ".xml":
                    return "text/xml";
                case ".html":
                    return "text/html";
                case ".css":
                    return "text/css";
                case ".js":
                    return "text/javascript";
            }
            return type;
            #endregion GetMimeTypeByName
        }
    }

    public enum WWWType
    {
        read,
        readBytes,
        download,
        upload,
        request,
    }
}
