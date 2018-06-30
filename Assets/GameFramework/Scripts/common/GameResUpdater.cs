using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuaInterface;
using UnityEngine;

namespace GameFramework
{
    //TODO:待事件管理器完善后实现

    public struct ResInfo{
        public string path;
        public string crc;
        public long size;

        public ResInfo(string[] info)
        {
            if(info.Length == 3)
            {
                path = info[0];
                crc = info[1];
                size = int.Parse(info[2]);
            }
            else
            {
                path = "";
                crc = "";
                size = -1;
            }
        }
    }


    public class ResConf{
        public string version;
        public Dictionary<string, ResInfo> files;

        public ResConf(string text)
        {
            //LogFile.Log("res text:" + text);
            files = new Dictionary<string, ResInfo>();
            if(!string.IsNullOrEmpty(text))
            {
                foreach (var item in text.Split('\n'))
                {
                    //LogFile.Log(item);
                    var resInfo = item.Split('|');
                    //LogFile.Log(resInfo.ToString());
                    if(2 == resInfo.Length)
                    {
                        version = resInfo[1];
                    }else if (3 == resInfo.Length)
                    {
                        ResInfo res = new ResInfo(resInfo);
                        files[res.path] = res;
                    }
                }
                LogFile.Log("res file count:" + files.Keys.Count);
            }
        }

        public void Update(ResConf newConf)
        {
            version = newConf.version;
            foreach (var item in newConf.files)
            {
                files[item.Key] = item.Value;
            }
        }

        public List<ResInfo> GetUpdateFiles(ResConf newConf=null)
        {
            //version = newConf.version;
            List<ResInfo> newFiles = new List<ResInfo>();
            Dictionary<string, ResInfo> of;
            Dictionary<string, ResInfo> nf;
            //如果跟空的比较就把自己记录的所有文件更新
            if (null == newConf)
            {
                of = new Dictionary<string, ResInfo>();
                nf = files;
            }else
            {
                int rst = CompareVer(newConf);
                if (-1 == rst)
                {
                    LogFile.Log("当newConf的版本号比较大");
                    //当newConf的版本号比较大
                    of = files;
                    nf = newConf.files;
                }
                else if (1 == rst)
                {
                    LogFile.Log("当自己的的版本号比较大");
                    //当自己的的版本号比较大
                    of = newConf.files;
                    nf = files;
                }
                else
                {
                    LogFile.Log("版本号相同");
                    //==0 版本号相同
                    return newFiles;
                }
            }
            foreach (var item in nf)
            {
                ResInfo or;
                if (of.TryGetValue(item.Key, out or))
                {
                    if(or.crc == item.Value.crc)
                    {
                        continue;
                    }
                }
                newFiles.Add(item.Value);
                LogFile.Log("检测到 {0} 需要拷贝", item.Value.path);
            }

            return newFiles;
        }

        public int CompareVer(ResConf other)
        {
            LogFile.Log("版本对比：{0} ： {1}", version, other.version);
            return string.Compare(version, other.version);
        }
    }

    //资源更新器只在使用asb的情况下使用，editor模式使用原始资源（位于Assets/BundleRes文件夹）
    public class GameResUpdater : SingletonComp<GameResUpdater>
    {
        /// <summary>
        /// 当前的资源配置，资源拷贝后就跟persConf一样
        /// </summary>
        ResConf curConf;
        /// <summary>
        /// 安装包里的资源配置
        /// </summary>
        ResConf streamConf;
        /// <summary>
        /// 可读写文件夹的资源配置
        /// </summary>
        ResConf persConf;

        //Android、ios需要将StreamingAssets文件夹下的资源拷贝到可读写文件夹下
        public void CheckLocalCopy(Action<float, string> callback = null, LuaFunction luaCallback = null)
        {
            if (GameConfig.Instance.useAsb){
                Debug.LogWarning("CopyFolder");
                StartCoroutine(CheckLocalRes(delegate (ResInfo[] resList) {
                    StartCoroutine(CopyStreamFiles(resList, callback, luaCallback));
                }));
            }else
            {
                float percent = 1f;
                string msg = "不使用Assetbundle不用拷贝资源";
                if (null != callback)
                {
                    callback(percent, msg);
                }
                if (null != luaCallback)
                {
                    luaCallback.Call<float, string>(percent, msg);
                    luaCallback.Dispose();
                }
            }
        }

        public void UpdateRes(string url)
        {
            
        }

        private IEnumerator onUpdateRes(string url)
        {
            yield return null;
        }

        /// <summary>
        /// 比较streamingAssets下和persistentDataPath下资源配置，拷贝新文件
        /// </summary>
        /// <returns>The local res.</returns>
        /// <param name="callback">Callback.</param>
        IEnumerator CheckLocalRes(Action<ResInfo[]> callback)
        {
            string filePath = GameConfig.STR_ASB_MANIFIST + "/resConf.bytes";
            string sUrl = Tools.GetUrlPathStream(Tools.PathCombine(Application.streamingAssetsPath, filePath));
            string pUrl = Tools.GetUrlPathWritebble(Tools.PathCombine(Tools.GetWriteableDataPath(), filePath));
            LogFile.Log("surl:{0}, pUrl:{1}", sUrl, pUrl);
            WWW wwwS = new WWW(sUrl);
            ResConf sConf = null;
            yield return wwwS;
            if (!string.IsNullOrEmpty(wwwS.error))
            {
                LogFile.Error("打开本地资源配置文件{0}失败，原因：{1}", filePath, wwwS.error);
                if (null != callback)
                {
                    callback(null);
                }
                yield break;
            }
            else{
                sConf = new ResConf(wwwS.text);
                WWW wwwP = new WWW(pUrl);
                ResConf pConf = null;
                yield return wwwP;
                if (!string.IsNullOrEmpty(wwwS.error))
                {
                    LogFile.Warn("可读写文件夹没有配置文件", filePath, wwwS.error);
                    if (null != callback)
                    {
                        callback(null);
                    }
                    //yield break;
                    wwwP.Dispose();
                }
                else
                {
                    pConf = new ResConf(wwwP.text);
                }
                callback(sConf.GetUpdateFiles(pConf).ToArray());

            }
            yield return null;
        }

        /// <summary>
        /// 在协程中拷贝文
        /// 回调的第一个参数（float）取值-1（不需要拷贝）0~1（拷贝进度）
        /// 第二个参数（string）为传递的额外参数，失败原因或者当前文件名
        /// </summary>
        /// <returns></returns>
        /// <param name="res">Callback.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="luaCallback">Lua callback.</param>
        public IEnumerator CopyStreamFiles(ResInfo[] res ,Action<float, string> callback = null, LuaFunction luaCallback = null)
        {
            float percent = -1f;
            string msg = "";
            string streamPath = Application.streamingAssetsPath;
            string persPath = Application.persistentDataPath;
            //如果源文件夹不存在，则创建
            if (0 == res.Length)
            {
                percent = 1;
                msg = string.Format("无需拷贝资源", streamPath);
                //Debug.LogWarningFormat();
                if (null != callback)
                {
                    callback(percent, msg);
                }
                if (null != luaCallback)
                {
                    luaCallback.Call<float, string>(percent, msg);
                    luaCallback.Dispose();
                }
                yield break;
            }
            Tools.CheckDirExists(Tools.GetWriteableDataPath(), true);

            long totalSize = 0;
            long copySize = 0;
            List<string> files = new List<string>();
            foreach (var item in res)
            {
                files.Add(item.path);
                totalSize += item.size;
            }
            int count = 0;
            int steep = 2;
            int fileCount = files.Count;

            foreach (var f in files)
            {
                StartCoroutine(copyStreamToWriteable(f, delegate(string name, int size)
                {
                    if (size > 0 )
                    {
                        copySize += size;
                        msg = name;
                        percent = (float)copySize / totalSize;

                    }
                    else
                    {
                        msg = name;
                        percent = -1;
                    }
                    if (null != callback)
                    {
                        callback(percent, msg);
                    }
                    if (null != luaCallback)
                    {
                        luaCallback.Call<float, string>(percent, msg);
                        luaCallback.Dispose();
                    }
                }));
                count++;
                if (count % steep == 0 || count == fileCount)
                {
                    yield return null;
                }

            }
        }


        /// <summary>
        /// 将streaming path 下的文件copy到对应用
        /// 为什么不直接用io函数拷贝，原因在于streaming目录不支持，
        /// 不管理是用getStreamingPath_for_www，还是Application.streamingAssetsPath，
        /// io方法都会说文件不存在
        /// </summary>
        /// <param name="oriFile"></param>
        IEnumerator copyStreamToWriteable(string oriFile, Action<string, int> callback)
        {
            string rPath = GameConfig.STR_ASB_MANIFIST + "/" + oriFile;
            string src = Tools.GetUrlPathStream(Tools.PathCombine(Application.streamingAssetsPath, rPath));
            string des = Tools.GetWriteableDataPath(rPath);//Application.persistentDataPath + "/" + rPath;
            //LogFile.Log("des:" + des);
            //LogFile.Log("src:" + src);
            WWW www = new WWW(src);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                LogFile.Error("www.error:{0}, url:{1}", www.error, src);
                if (null != callback)
                {
                    callback(oriFile, -1);
                }
            }
            else
            {
                //des = Application.persistentDataPath + "/" + fileName;
                if (File.Exists(des))
                {
                    File.Delete(des);
                }
                Tools.CheckDirExists(Directory.GetParent(des).ToString(), true);
                FileStream fsDes = File.Create(des);
                fsDes.Write(www.bytes, 0, www.bytes.Length);
                fsDes.Flush();
                fsDes.Close();
                if (null != callback)
                {
                    callback(oriFile, www.bytes.Length);
                }
            }
            www.Dispose();
        }
    }
}