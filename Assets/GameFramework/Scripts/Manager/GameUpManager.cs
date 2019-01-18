using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameFramework
{
    /// <summary>
    /// 游戏更新管理器
    /// 检查顺序： 
    /// 1.包内资源是否释放到可读写文件夹
    /// 2.从可读写文件夹读取配置，检查app是否需要更新
    /// 3.app不需要更新，检查服务器资源是否需要更新
    /// </summary>
    public class GameUpManager : Singleton<GameUpManager>
    {

        public static string STR_NOTIFY_EVENT_NAME = "UpdateDownloadView";

        string STR_CONFIG_MISSING = "ConfigMissing";
        string STR_RES_CONF = "resConf.bytes";
        string mVersionStr = "app:v" + Application.version + " res:v0.0.0.1.base";
        string mInfoStr = "";
        /// <summary>
        /// 当前的资源配置
        /// </summary>
        ResConf curConf;
        /// <summary>
        /// 是否只从第一个连上的资源服下载资源
        /// </summary>
        public bool DownloadTheFirst = true;


        public void CheckLocalRes(Action<bool, string> callback)
        {
            mInfoStr = "Loading...";
            freshUI(0f);
            string srcUrl = Tools.GetUrlPathStream(Application.streamingAssetsPath, GameConfig.STR_ASB_MANIFIST);
            string tarUrl = Tools.GetUrlPathWriteabble(Tools.GetWriteableDataPath(), GameConfig.STR_ASB_MANIFIST);
            delOldWriteableRes(srcUrl, tarUrl, callback);
        }

        /// <summary>
        /// 检测app版本，返回true表示是最新版或者更新完成
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void CheckAppVer(Action<bool> callback)
        {
            Platform.CheckAppVer(callback);
        }

        public void CheckServerRes(Action<bool, string> callback)
        {
            GameResManager.Instance.Initialize(() =>
            {
                GameResManager.Instance.LoadRes<TextAsset>("UpdateServer", ".bytes", (obj) =>
                {
                    TextAsset text = obj as TextAsset;
                    if (null != text)
                    {
                        ResConf servers = new ResConf(text.text);
                        ResInfo[] arr = new ResInfo[servers.files.Values.Count];
                        servers.files.Values.CopyTo(arr, 0);
                        List<ResInfo> list = new List<ResInfo>(arr);
                        list.Sort((ResInfo a, ResInfo b) =>
                        {
                            return a.size < b.size ? -1 : 1;
                        });
#if UNITY_IOS
                        TimeOutWWW www = getTimeOutWWW();
                        List<string> files = new List<string>();
                        for (int i = 0; i < list.Count; i++)
                        {
                        files.Add(Tools.PathCombine(list[i].path, GameConfig.STR_ASB_MANIFIST+"/isAudit.bytes"));
                        }
                        www.ReadFirstExistsStr("isIOSAudit", files, 0.5f, (bool rst, string msg) => 
                        {
                            if (!rst || msg.Trim().Equals("1"))
                            {
                                //当资源服找不到配置文件或者配置文件内容为“1”时，表示是审核版本，跳过更新流程
                                callback(true, string.Empty);
                                return;
                            }
                                
#endif
                                string tarUrl = Tools.GetUrlPathWriteabble(Tools.GetWriteableDataPath(), GameConfig.STR_ASB_MANIFIST);
                                checkService(list, 0, tarUrl, callback);
#if UNITY_IOS
                                
                        }, null);
#endif
                    }

                });
            });
        }

#region private
        TimeOutWWW getTimeOutWWW()
        {
            return GameManager.Instance.gameObject.AddComponent<TimeOutWWW>();
        }

        /// <summary>
        /// 加载asb可以从streaming路径加载，不需要将资源拷贝到可读写文件夹
        /// 当包体资源有更新时，删除老的资源包
        /// </summary>
        /// <param name="srcUrl">Source URL.</param>
        /// <param name="tarUrl">Tar URL.</param>
        /// <param name="callback">Callback.</param>
        private void delOldWriteableRes(string srcUrl, string tarUrl, Action<bool, string> callback)
        {
            string confPath = STR_RES_CONF;

            if (null != callback)
            {
                ResConf srcConf = null;
                ResConf tarConf = null;

                if (GameConfig.useAsb)
                {
                    string sUrl = Tools.PathCombine(srcUrl, confPath);
                    string pUrl = Tools.PathCombine(tarUrl, confPath);

                    TimeOutWWW streamWWW = getTimeOutWWW();
                    streamWWW.ReadFileStr("localResConf", sUrl, 1f, (rst, msg) =>
                    {
                        if (rst)
                        {
                            srcConf = new ResConf(msg);

                            TimeOutWWW writableWWW = getTimeOutWWW();
                            writableWWW.ReadFileStr("externalResConf", pUrl, 1f, (_rst, _msg) =>
                            {
                                if (_rst)
                                {
                                    //之前已经拷贝过资源
                                    tarConf = new ResConf(_msg);
                                }
                                else
                                {
                                    tarConf = new ResConf("");
                                }

                                curConf = tarConf;

                                if (srcConf.CompareVer(tarConf) > 0)
                                {
                                    List<ResInfo> list = srcConf.GetUpdateFiles(tarConf);
                                    if (list.Count > 0)
                                    {
                                        //将可读写文件夹下的老版本资源删除
                                        for (int i = 0; i < list.Count; i++)
                                        {
                                            ResInfo ri = list[i];
                                            string savePath = Tools.GetWriteableDataPath(GameConfig.STR_ASB_MANIFIST + "/" + ri.path);
                                            if(File.Exists(savePath))
                                            {
                                                File.Delete(savePath);
                                                LogFile.Log("删除已下载的老资源："+savePath);
                                            }
                                            curConf.files[ri.path] = srcConf.files[ri.path];
                                        }
                                    }
                                    curConf.version = srcConf.version;
                                    //保存新的资源版本号
                                    GameConfig.SetInt(GameDefine.STR_CONF_KEY_RES_VER_I, curConf.VersionCode);
                                    curConf.SaveToFile(Tools.GetWriteableDataPath(GameConfig.STR_ASB_MANIFIST + "/" + STR_RES_CONF));
                                    mVersionStr = "app:v" + Application.version + " res"+curConf.version;
                                }
                            }, null);
                        }
                    }, null);
                }
                callback(true, "");
            }
        }

        /// <summary>
        /// 对比resConf.bytes文件，根据需要进行拷贝或下载
        /// </summary>
        /// <param name="srcUrl">Source URL.</param>
        /// <param name="tarUrl">Tar URL.</param>
        /// <param name="callback">Callback.</param>
        private void checkResConf(string srcUrl, string tarUrl, Action<bool, string> callback)
        {
            string confPath = STR_RES_CONF;

            if (null != callback)
            {
                ResConf srcConf = null;
                ResConf tarConf = null;

                if (GameConfig.useAsb)
                {
                    string sUrl = Tools.PathCombine(srcUrl, confPath);
                    string pUrl = Tools.PathCombine(tarUrl, confPath);

                    TimeOutWWW streamWWW = getTimeOutWWW();
                    streamWWW.ReadFileStr("localResCOnf", sUrl, 1f, (rst, msg) =>
                    {
                        if (rst)
                        {
                            srcConf = new ResConf(msg);

                            TimeOutWWW writableWWW = getTimeOutWWW();
                            writableWWW.ReadFileStr("externalResConf", pUrl, 1f, (_rst, _msg) =>
                            {
                                if (_rst)
                                {
                                    //之前已经拷贝过资源
                                    tarConf = new ResConf(_msg);
                                }
                                else
                                {
                                    tarConf = new ResConf("");
                                }

                                curConf = tarConf;
                                float last = Time.time;
                                long lastSize = 0;

                                if (srcConf.CompareVer(tarConf) > 0)
                                {
                                    List<ResInfo> list = srcConf.GetUpdateFiles(tarConf);
                                    if (list.Count > 0)
                                    {
                                        string format = "正在下载资源,已完成[ {0} / {1} ],下载速度：{2} ...";
                                        mInfoStr = string.Format(format, 0, list.Count, "0Byte/s");
                                        //需要拷贝资源到可读写文件夹
                                        TimeOutWWW copyLocal = getTimeOutWWW();
                                        List<WWWInfo> infos = new List<WWWInfo>();
                                        long totalSize = 0;
                                        for (int i = 0; i < list.Count; ++i)
                                        {
                                            ResInfo ri = list[i];
                                            string url = Tools.PathCombine(srcUrl, ri.path);
                                            string savePath = Tools.GetWriteableDataPath(GameConfig.STR_ASB_MANIFIST + "/" + ri.path);
                                            totalSize += ri.size;
                                            infos.Add(new WWWInfo(url, savePath, ri.size));
                                        }
                                        string totalSizeStr = Tools.FormatMeroySize(totalSize);
                                        copyLocal.DownloadFiles("copyLocal", infos, 2f, (string noticeKey, double progress, int index, string __msg) =>
                                        {
                                            //LogFile.Log("progress:{0}; index:{1}; msg:{2};", progress, index, __msg);

                                            if (progress.Equals(1d))
                                            {
                                                if (__msg.Equals(TimeOutWWW.STR_SUCCEEDED))
                                                {
                                                    curConf.version = srcConf.version;
                                                    //保存新的资源版本号
                                                    GameConfig.SetInt(GameDefine.STR_CONF_KEY_RES_VER_I, curConf.VersionCode);
                                                    mVersionStr = "app:v" + Application.version + " res" + curConf.version;
                                                    //拷贝完成
                                                    callback(true, "资源更新完成");
                                                }
                                                else
                                                {
                                                    callback(false, "部分资源更新失败");
                                                }
                                                curConf.SaveToFile(Tools.GetWriteableDataPath(GameConfig.STR_ASB_MANIFIST+"/"+STR_RES_CONF));

                                                return;
                                            }
                                            if(progress.Equals(-1d))
                                            {
                                                //有文件下载或者拷贝失败
                                                LogFile.Warn("[" + infos[index].Url + "]拷贝或下载失败");
                                            }
                                            else
                                            {
                                                if (__msg.Equals(TimeOutWWW.STR_DONE))
                                                {
                                                    //有文件下载成功
                                                    curConf.files[list[index-1].path] = srcConf.files[list[index-1].path];

                                                    //mInfoStr = string.Format(format, index, list.Count);
                                                }
                                                float now = Time.time;
                                                float dt = now - last;
                                                long doneSize = (long)(totalSize * progress);
                                                long siezPerSec = (long)((doneSize - lastSize) / dt);
                                                if(siezPerSec > 0)
                                                {
                                                    mInfoStr = string.Format(format, Tools.FormatMeroySize(doneSize), totalSizeStr, Tools.FormatMeroySize(siezPerSec) + "/s");
                                                    //LogFile.Log(mInfoStr);
                                                    freshUI((float)progress);
                                                }
                                                last = now;
                                                lastSize = doneSize;
                                            }
                                        }, null);
                                    }
                                }
                                else
                                {
                                    LogFile.Log("没有检测到新版本资源，跳过更新步骤");
                                    callback(true, "没有检测到新版本资源，跳过更新步骤");
                                }

                            }, null);
                        }
                        else
                        {
                            LogFile.Warn("资源配置文件" + sUrl + "丢失");
                            callback(false, STR_CONFIG_MISSING);
                        }

                    }, null);
                }
                else
                {
                    callback(true, "不使用Assetbundle不用拷贝/下载资源");
                }
            }
        }

        private void freshUI(float progress)
        {
            EventManager.notifyMain(STR_NOTIFY_EVENT_NAME, mVersionStr, mInfoStr, progress);
        }

        /// <summary>
        /// 检查资源服
        /// </summary>
        /// <param name="list">服务器地址列表.</param>
        /// <param name="idx">index</param>
        /// <param name="tarUrl">Tar URL.</param>
        /// <param name="callback">Callback.</param>
        private void checkService(List<ResInfo> list, int idx, string tarUrl, Action<bool, string> callback)
        {
            //TODO:刷新UI显示
            if (idx < list.Count)
            {
                mInfoStr = "检测服务器资源[ " + idx + " / " + list.Count + " ]...";
                string srcUrl = Tools.PathCombine(list[idx].path, GameConfig.STR_ASB_MANIFIST);
                checkResConf(srcUrl, tarUrl, (bool rst, string msg) =>
                {
                    if (rst)
                    {
                        if (null != callback)
                        {
                            callback(rst, msg);
                        }
                        return;
                    }
                    if (!msg.Equals(STR_CONFIG_MISSING) && DownloadTheFirst)
                    {
                        //服务器配置文件已经获取到，但是下载失败
                        if (null != callback)
                        {
                            callback(rst, msg);
                        }
                        return;
                    }
                    checkService(list, idx + 1, tarUrl, callback);
                });
            }
            else
            {
                mInfoStr = "检测服务器资源失败，请检查网络";
                if (null != callback)
                {
                    callback(false, "服务器资源下载失败");
                }
            }

        }
#endregion private
    }
}
