using System;
using System.Collections.Generic;
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
            string srcUrl = Tools.GetUrlPathStream(Application.streamingAssetsPath, GameConfig.STR_ASB_MANIFIST);
            string tarUrl = Tools.GetUrlPathWritebble(Tools.GetWriteableDataPath(), GameConfig.STR_ASB_MANIFIST);
            checkResConf(srcUrl, tarUrl, callback, true);
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
                        list.Sort(delegate (ResInfo a, ResInfo b)
                        {
                            return a.size < b.size ? -1 : 1;
                        });

                        string tarUrl = Tools.GetUrlPathWritebble(Tools.GetWriteableDataPath(), GameConfig.STR_ASB_MANIFIST);
                        checkService(list, 0, tarUrl, callback);
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
        /// 对比resConf.bytes文件，根据需要进行拷贝或下载
        /// </summary>
        /// <param name="srcUrl">Source URL.</param>
        /// <param name="tarUrl">Tar URL.</param>
        /// <param name="callback">Callback.</param>
        private void checkResConf(string srcUrl, string tarUrl, Action<bool, string> callback, bool isLocal=false)
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

                                if (srcConf.CompareVer(tarConf) > 0)
                                {
                                    List<ResInfo> list = srcConf.GetUpdateFiles(tarConf);
                                    if (list.Count > 0)
                                    {
                                        string format = isLocal ? "正在解压资源,已完成[ {0} / {1} ] ..." : "正在下载资源,已完成[ {0} / {1} ] ...";
                                        mInfoStr = string.Format(format, 0, list.Count);
                                        //需要拷贝资源到可读写文件夹
                                        TimeOutWWW copyLocal = getTimeOutWWW();
                                        List<WWWInfo> infos = new List<WWWInfo>();
                                        for (int i = 0; i < list.Count; ++i)
                                        {
                                            ResInfo ri = list[i];
                                            string url = Tools.PathCombine(srcUrl, ri.path);
                                            string savePath = Tools.GetWriteableDataPath(GameConfig.STR_ASB_MANIFIST + "/" + ri.path);
                                            infos.Add(new WWWInfo(url, savePath, ri.size));
                                        }
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
                                                    //拷贝完成
                                                    callback(true, "包内资源拷贝完成");
                                                }
                                                else
                                                {
                                                    callback(false, "部分资源拷贝失败");
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

                                                    mInfoStr = string.Format(format, index, list.Count);
                                                }

                                                freshUI((float)progress);
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
