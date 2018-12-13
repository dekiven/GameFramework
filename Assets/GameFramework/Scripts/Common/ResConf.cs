using System;
using System.Collections.Generic;
using System.IO;

namespace GameFramework
{
    public struct ResInfo
    {
        public string path;
        public string crc;
        public long size;

        public ResInfo(string[] info)
        {
            if (info.Length == 3)
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

    /// <summary>
    /// 游戏资源配置
    /// 游戏资源版本号为aa.bb.cc.dd[.base]格式
    /// 版本号每段最大99,最小0,
    /// .base是包内资源版本号
    /// </summary>
    public class ResConf
    {
        private string STR_BASE = "base";
        private string mVersion = "0.0.0";

        public string version { get { return mVersion; } set { mVersion = value; mVersionCode = -1; }}
        public bool server;
        public Dictionary<string, ResInfo> files;

        private int mVersionCode = -1;

        public int VersionCode
        {
            get
            {
                if (-1 == mVersionCode)
                {
                    string[] vs = getVersionNums(version);
                    int l = vs.Length;
                    if (vs.Length > 0)
                    {
                        if (!string.Equals(vs[l - 1], STR_BASE))
                        {
                            for (int i = 0; i < l; ++i)
                            {
                                mVersionCode += int.Parse(vs[vs.Length - 1 - i]) * (int)Math.Pow(10, 2 * i);
                            }
                            //LogFile.Warn("mVersionCode 1:" + mVersionCode);
                            mVersionCode *= 2;
                            //LogFile.Warn("mVersionCode 2:" + mVersionCode);
                        }
                        else
                        {
                            for (int i = 1; i < l; ++i)
                            {
                                mVersionCode += int.Parse(vs[vs.Length - 1 - i]) * (int)Math.Pow(10, 2 * (i - 1));
                            }
                            //LogFile.Warn("mVersionCode 3:" + mVersionCode);
                            mVersionCode *= 2;
                            mVersionCode -= 1;
                            //LogFile.Warn("mVersionCode 4:" + mVersionCode);
                        }
                    }
                }
                //LogFile.Warn("mVersionCode 5:" + mVersionCode);
                return mVersionCode;
            }

        }
        public ResConf(string text)
        {
            //LogFile.Log("res text:" + text);
            files = new Dictionary<string, ResInfo>();
            server = false;
            if (!string.IsNullOrEmpty(text))
            {
                foreach (var item in text.Split('\n'))
                {
                    //LogFile.Log(item);
                    var resInfo = item.Split('|');
                    //LogFile.Log(resInfo.ToString());
                    if (2 == resInfo.Length)
                    {
                        version = resInfo[1].TrimEnd();
                    }
                    else if (3 == resInfo.Length)
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

        public List<ResInfo> GetUpdateFiles(ResConf newConf = null)
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
            }
            else
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
                    if (or.crc == item.Value.crc)
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
            if (string.IsNullOrEmpty(v))
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
            if (Equals(version, other.version))
            {
                return 0;
            }
            return VersionCode > other.VersionCode ? 1 : -1;
            //string[] selfVn = getVersionNums(version);
            //string[] otherVn = getVersionNums(other.version);
            //int sc = selfVn.Length;
            //int oc = otherVn.Length;
            ////以两个版本号短的部分来比较(多的部分是包体资源的版本号有.base后缀)
            //int rst = 0;
            //for (int i = 0; i < (sc < oc ? sc : oc); ++i)
            //{
            //    if (int.Parse(selfVn[i]) != int.Parse(otherVn[i]))
            //    {
            //        rst = int.Parse(selfVn[i]) > int.Parse(otherVn[i]) ? 1 : -1;
            //        break;
            //    }
            //}
            ////这种情况只出现在本地刚解压好包体内的资源，这个时候资源配置版本会多一个.base后缀
            ////如果前面的版本号相同则默认服务器的版本号大
            //if (0 == rst && 1 == Math.Abs(sc - oc))
            //{
            //    if (sc > oc && Equals(selfVn[sc - 1], STR_BASE))
            //    {
            //        rst = -1;
            //    }
            //    else if (sc < oc && Equals(otherVn[oc - 1], STR_BASE))
            //    {
            //        rst = 1;
            //    }
            //}
            //return rst;
        }

        public void SaveToFile(string path)
        {
            string c = "version|" + version + "\n";
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
}
