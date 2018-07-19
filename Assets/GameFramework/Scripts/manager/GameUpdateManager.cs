using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuaInterface;
using UnityEngine;

namespace GameFramework
{
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

        public new string ToString()
        {
            return string.Format("{0}|{1}|{2}", path, crc, size);
        }
    }


    public class ResConf{
        public string version;
        public bool server;
        public Dictionary<string, ResInfo> files;

        public ResConf(string text)
        {
            //LogFile.Log("res text:" + text);
            files = new Dictionary<string, ResInfo>();
            server = false;
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

        public string[] getVersionNums(string v = "")
        {
            if(string.IsNullOrEmpty(v))
            {
                v = version;
            }
            string[] arr = v.Split('.');
            return arr;
        }

        public int CompareVer(ResConf other)
        {
            LogFile.Log("版本对比：[{0}] ： [{1}]", version, other.version);
            if (null == other || string.IsNullOrEmpty(other.version))
            {
                LogFile.Log("跟空版本号对比，默认本版本号大");
                return 1;
            }
            if(Equals(version, other.version))
            {
                return 0;
            }
            string[] selfVn = getVersionNums(version);
            string[] otherVn = getVersionNums(other.version);
            int sc = selfVn.Length;
            int oc = otherVn.Length;
            //以两个版本号短的部分来比较(多的部分是包体资源的版本号有.base后缀)
            int rst = 0;
            for (int i = 0; i < (sc < oc ? sc : oc); ++i)
            {
                if(int.Parse(selfVn[i]) != int.Parse(otherVn[i]))
                {
                    rst = int.Parse(selfVn[i]) > int.Parse(otherVn[i]) ? 1 : -1;
                    break;
                }
            }
            //这种情况只出现在本地刚解压好包体内的资源，这个时候资源配置版本会多一个.base后缀
            //如果前面的版本号相同则默认服务器的版本号大
            if(0 == rst && 1 == Math.Abs(sc - oc))
            {
                if(sc > oc && Equals(selfVn[sc - 1], "base"))
                {
                    rst = -1;
                }else if(sc < oc && Equals(otherVn[oc - 1], "base")) 
                {
                    rst = 1;
                }
            }
            return rst;
        }

        public void SaveToFile(string path)
        {
            string c = "version|" + version+"\n";
            foreach (var item in files)
            {
                c = c + item.Value.ToString() + "\n";
            }
            Tools.CheckDirExists(Directory.GetParent(path).ToString(), true);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            Tools.CheckDirExists(Directory.GetParent(path).ToString(), true);
            FileStream fsDes = File.Create(path);
            byte[] bytes = System.Text.Encoding.Default.GetBytes(c);
            fsDes.Write(bytes, 0, bytes.Length);
            fsDes.Flush();
            fsDes.Close();
        }
    }

    //资源更新器只在使用asb的情况下使用，editor模式使用原始资源（位于Assets/BundleRes文件夹）
    /// <summary>
    /// 配合ResUpdateView使用
    /// </summary>
    public class GameUpdateManager : SingletonComp<GameUpdateManager>
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

        /// <summary>
        /// The s URL.
        /// </summary>
        string sUrl = "";
        /// <summary>
        /// The p URL.
        /// </summary>
        string pUrl = "";

        /// <summary>
        /// ResUpdateView的UIHandler,可以快速访问和改变UI属性
        /// </summary>
        public UIHandler UIHandler;

        //Android、ios需要将StreamingAssets文件夹下的资源拷贝到可读写文件夹下
        public void CheckLocalCopy(Action<float, string> callback = null, LuaFunction luaCallback = null)
        {
            if (GameConfig.Instance.useAsb){
                Debug.LogWarning("CopyFolder");
                StartCoroutine(CheckLocalResVer(delegate (ResInfo[] resList) {
                    StartCoroutine(CopyWWWFiles(resList, "", callback, luaCallback));
                }));
            }else
            {
                float rate = 1f;
                string msg = "不使用Assetbundle不用拷贝资源";
                if (null != callback)
                {
                    callback(rate, msg);
                }
                if (null != luaCallback)
                {
                    luaCallback.Call<float, string>(rate, msg);
                    luaCallback.Dispose();
                }
            }
        }

        /// <summary>
        /// 从给出的服务器列表更新资源
        /// </summary>
        /// <returns>The server res.</returns>
        /// <param name="urls">Urls.</param>
        public IEnumerator UpdateServerRes(string[] urls, Action<float, string> callback = null, LuaFunction luaCallback = null)
        {
            string filePath = GetConfigPath();
            foreach (var url in urls)
            {
                if(!string.IsNullOrEmpty(url))
                {
                    string confUrl = Tools.PathCombine(url, filePath);
                    WWW www = new WWW(confUrl);
                    LogFile.Log("检查服务器资源配置文件:" + confUrl);
                    updateMsgInfo("检查服务器资源配置文件");
                    yield return www;
                    if (!string.IsNullOrEmpty(www.error))
                    {
                        //LogFile.Log("检查配置文件{0}出错，错误信息：{1}", confUrl, www.error);
                        LogFile.Warn("检查配置文件{0}出错，错误信息：{1}", confUrl, www.error);
                    }else
                    {
                        ResConf serConf = new ResConf(www.text);
                        if(!string.IsNullOrEmpty(serConf.version))
                        {
                            streamConf = serConf;
                            streamConf.server = true;
                            if (null != curConf)
                            {
                                //if(streamConf.CompareVer(curConf) != 0 || (-1 == streamConf.CompareVer(curConf) & !curConf.version.EndsWith("_base", StringComparison.Ordinal)))
                                int compare = streamConf.CompareVer(curConf);
                                if(compare != 1)
                                {
                                    //如果服务器版本跟本地版本一样  不下载资源
                                    //如果服务器版本比本地的版本小(正常情况下不会出现),且不是以_base结尾(包体自带的资源配置是版本号加_base) 不下载资源
                                    float _rate = 1f;
                                    string _msg = "没有资源需要更新。";
                                    if (compare == -1)
                                    {
                                        _msg = "服务器版本号小于当前版本号，请更新服务器资源！(某些平台可能会出现这种情况，是正常的)";
                                        LogFile.Warn(_msg);
                                    }
                                    updateMsgInfo(_msg);
                                    if(null != callback)
                                    {
                                        callback(_rate, _msg);
                                    }
                                    if(null != luaCallback)
                                    {
                                        luaCallback.Call<float, string>(_rate, _msg);
                                        luaCallback.Dispose();
                                    }
                                    yield break;
                                }
                                else
                                {
                                    //StartCoroutine(CopyWWWFiles(streamConf.GetUpdateFiles(curConf).ToArray(), url, callback, luaCallback));
                                    updateMsgInfo("下载中...");
                                    updateSlider(0f);
                                    yield return CopyWWWFiles(streamConf.GetUpdateFiles(curConf).ToArray(), url, callback, luaCallback);
                                }
                                yield break;
                            }
                            else
                            {
                                LogFile.Warn("curConf == null:{0}, curConf.version:{1}, serConf.version:{2}", curConf == null, curConf.version, serConf.version);
                            }
                        }
                        else
                        {
                            LogFile.Warn("{0}没有版本信息，检查是否有误。", confUrl);
                        }
                    }
                }
            }
            //如果所有服务器都检查了，但是都没有获取版本信息，返回更新失败
            float rate = -1f;
            string msg = "更新服务资源失败，没有读取到服务器资源配置文件";
            if (null != callback)
            {
                callback(rate, msg);
            }
            if (null != luaCallback)
            {
                luaCallback.Call<float, string>(rate, msg);
                luaCallback.Dispose();
            }
            LogFile.Error("不能连接资源服务器，关闭程序");
            yield return null;
            Application.Quit();
        }

        /// <summary>
        /// 比较streamingAssets下和persistentDataPath下资源配置，拷贝新文件
        /// </summary>
        /// <returns>The local res.</returns>
        /// <param name="callback">Callback.</param>
        IEnumerator CheckLocalResVer(Action<ResInfo[]> callback)
        {
            string filePath = GetConfigPath();
            sUrl = Tools.GetUrlPathStream(Tools.PathCombine(Application.streamingAssetsPath, filePath));
            pUrl = Tools.GetUrlPathWritebble(Tools.PathCombine(Tools.GetWriteableDataPath(), filePath));
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
                streamConf = sConf;
                WWW wwwP = new WWW(pUrl);
                ResConf pConf = null;
                yield return wwwP;
                if (!string.IsNullOrEmpty(wwwS.error))
                {
                    curConf = new ResConf("");
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
                    curConf = pConf;
                    persConf = pConf;
                    updateVersionInfo();
                }
                if (null != callback)
                {
                    //只有当包体内的版本号大于可读写文件夹的版本号才复制资源
                    if (sConf.CompareVer(pConf) > 0)
                    {
                        callback(sConf.GetUpdateFiles(pConf).ToArray());
                    }
                    else
                    {
                        callback(new ResInfo[] { });
                    }
                }

            }
            yield return null;
        }

        public string GetConfigPath()
        {
            return Tools.PathCombine(GameConfig.STR_ASB_MANIFIST, "resConf.bytes");
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
        public IEnumerator CopyWWWFiles(ResInfo[] res, string fromServer = "", Action<float, string> callback = null, LuaFunction luaCallback = null)
        {
            float rate = -1f;
            string msg = "";
            //string streamPath = Application.streamingAssetsPath;
            //string persPath = Application.persistentDataPath;
            //如果源文件夹不存在，则创建
            if (0 == res.Length)
            {
                rate = 1;
                msg = "无需拷贝包体资源";
                //Debug.LogWarningFormat();
                if (null != callback)
                {
                    callback(rate, msg);
                }
                if (null != luaCallback)
                {
                    luaCallback.Call<float, string>(rate, msg);
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
                //StartCoroutine(writeWwwToWriteable(f, fromServer, delegate (string name, int size)
                yield return writeWwwToWriteable(f, fromServer, delegate(string name, int size)
                {
                    if (size > 0 )
                    {
                        copySize += size;
                        msg = name;
                        rate = (float)copySize / totalSize;
                        curConf.files[name] = streamConf.files[name];
                    }
                    else
                    {
                        msg = name;
                        rate = -1;
                    }
                    if(Equals(rate, 1f))
                    {
                        curConf.version = streamConf.version;
                        curConf.SaveToFile(Tools.GetWriteableDataPath(GetConfigPath()));

                        updateVersionInfo();
                    }else if( Equals(rate, -1f))
                    {
                        curConf.SaveToFile(Tools.GetWriteableDataPath(GetConfigPath()));
                        LogFile.Warn("部分文件拷贝失败，记录以经拷贝的文件");

                        updateVersionInfo();
                    }
                    if (null != callback)
                    {
                        callback(rate, msg);
                    }
                    if (null != luaCallback)
                    {
                        luaCallback.Call<float, string>(rate, msg);
                        luaCallback.Dispose();
                    }
                });
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
        IEnumerator writeWwwToWriteable(string oriFile, string fromServer = "", Action<string, int> callback=null)
        {
            string rPath = GameConfig.STR_ASB_MANIFIST + "/" + oriFile;
            string src;
            if(string.IsNullOrEmpty(fromServer))
            {
                src = Tools.GetUrlPathStream(Tools.PathCombine(Application.streamingAssetsPath, rPath));
            }
            else
            {
                src = Tools.GetUrlPathStream(Tools.PathCombine(fromServer, rPath));
                //src = fromServer + oriFile;
            }
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

        //检查服务器资源更新
        public void CheckServerRes(Action<float, string> callback = null, LuaFunction luaCallback=null)
        {
            updateMsgInfo("检查服务器资源...");
            if(!GameConfig.Instance.useAsb)
            {
                float rate = 1;
                string msg = "不使用Assetbundle不通过服务器更新资源";
                if(null != callback)
                {
                    callback(rate, msg);
                }
                if(null != luaCallback)
                {
                    luaCallback.Call<float, string>(rate, msg);
                    luaCallback.Dispose();
                }
                updateMsgInfo(msg);
                return;
            }
            GameResManager resMgr = GameResManager.Instance;
            resMgr.LoadRes<TextAsset>("conf/common/res", "updateServer.bytes", delegate (UnityEngine.Object obj)
            {
                TextAsset text = obj as TextAsset;
                if (null != text)
                {
                    //TODO:测试，直接使用资源配置的读取类,以后修改
                    ResConf servers = new ResConf(text.text);
                    ResInfo[] arr = new ResInfo[servers.files.Values.Count];
                    servers.files.Values.CopyTo(arr, 0);
                    List<ResInfo> list = new List<ResInfo>(arr);
                    list.Sort(delegate (ResInfo a, ResInfo b)
                    {
                        return a.size < b.size ? -1 : 1;
                    });
                    List<string> urls = new List<string>();
                    foreach (var item in list)
                    {
                        urls.Add(item.path);
                    }

                    StartCoroutine(UpdateServerRes(urls.ToArray(), delegate(float rate, string msg) {
                        //TODO:更新界面
                        if(null != callback)
                        {
                            callback(rate, msg);
                        }
                        if(null != luaCallback)
                        {
                            luaCallback.Call<float, string>(rate, msg);
                        }
                        updateSlider(rate);
                    }));
                }
            });
        }

        public void CheckUpdate(Action<float, string> callback = null, LuaFunction luaCallback = null)
        {
            updateVersionInfo();
            updateMsgInfo("解压游戏资源...");

            CheckLocalCopy(delegate (float rate, string msg)
            {
                if (Equals(-1f, rate) || Equals(1f, rate))
                {
                    updateMsgInfo(msg);
                    LogFile.Log("callback of copy file:{0},{1}", rate, msg);
                    if (Equals(1f, rate))
                    {
                        updateSlider(rate);
                        GameResManager.Instance.Initialize(delegate {
                            CheckServerRes(callback, luaCallback);
                        });
                    }
                    else
                    {
                        LogFile.Error("包体资源拷贝失败，关闭程序");
                        //TODO:包体资源拷贝失败，进行相应操作
                        //Application.Quit();
                    }
                }
                else
                {
                    updateSlider(rate);
                }
            });
        }

        private void updateVersionInfo()
        {
            if (null != UIHandler)
            {
                UIHandler.SetTextString("TextVersion", string.Format("app:{0}  res:{1}", Application.version, null == curConf ? "?" : curConf.version));
            }
        }

        private void updateMsgInfo(string msg)
        {
            if (null != UIHandler)
            {
                UIHandler.SetTextString("TextInfo", msg);
            }
        }

        private void updateSlider(float value)
        {
            if (null != UIHandler)
            {
                UIHandler.SetSliderValue("Slider", value);
            }
        }
    }
}