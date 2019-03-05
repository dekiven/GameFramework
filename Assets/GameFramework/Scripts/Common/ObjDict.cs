using System;
using System.Collections.Generic;

namespace GameFramework
{
    public class ObjDict<T>
    {

        #region delegate
        public delegate void DisposeDelegate(ref T obj);
        #endregion

        private Dictionary<string, Dictionary<string, T>> mDict = new Dictionary<string, Dictionary<string, T>>();
        public DisposeDelegate DisposeCallback;
        /// <summary>
        /// 同名的资源在多次载入的时候是否先处理Dispose回调
        /// </summary>
        public bool ReleaseBeforeReAdd = true;

        public ObjDict() : this(null){}

        public ObjDict(DisposeDelegate dispose)
        {
            DisposeCallback = dispose;
        }

        /// <summary>
        /// 将 T 对象加入名字为 dictName 的 subDictionary，其 key 值为 key,返回 false 表示之前添加过，这次添加是替换
        /// </summary>
        /// <returns><c>true</c>, if object was added, <c>false</c> otherwise.</returns>
        /// <param name="dictName"> ObjDict 中子 Dictionary 的 key</param>
        /// <param name="key">添加对象在子 Dictionary 中的 key</param>
        /// <param name="obj">添加的对象</param>
        public bool AddObj(string dictName, string key, T obj)
        {
            bool ret = true;
            Dictionary<string, T> dict;
            if(!mDict.TryGetValue(dictName, out dict))
            {
                dict = new Dictionary<string, T>();
            }
            if(dict.ContainsKey(key))
            {
                //LogFile.Log("objDict[{0}] 已经有{1}存在！", dictName, key);
                T oriObj = dict[key];
                if(!Equals(obj, oriObj))
                {
                    if(null != DisposeCallback && ReleaseBeforeReAdd)
                    {
                        DisposeCallback(ref oriObj);
                    }
                }
                ret = false;
            }
            dict[key] = obj;
            mDict[dictName] = dict;
            return ret;
        }

        public T GetObj(string dictName, string key)
        {
            T obj = default(T);
            Dictionary<string, T> objs;
            if (mDict.TryGetValue(dictName, out objs))
            {
                if (objs.TryGetValue(key, out obj))
                {
                    return obj;
                }
            }
            return obj;
        }

        public void ClearSubDict(string dictName)
        {
            Dictionary<string, T> dict;
            if (mDict.TryGetValue(dictName, out dict))
            {
                if (null != DisposeCallback)
                {
                    foreach (var item in dict)
                    {
                        T obj = item.Value;
                        DisposeCallback(ref obj);
                    }
                }

                dict.Clear();
                mDict.Remove(dictName);
            }
        }

        public void ClearObj(string dictName, string key)
        {
            Dictionary<string, T> dict = GetSubDict(dictName);
            if (null != dict)
            {
                T obj;
                if(dict.TryGetValue(key, out obj))
                {
                    if (null != DisposeCallback)
                    {
                        DisposeCallback(ref obj);
                        dict.Remove(key);
                    }
                    if (dict.Count == 0)
                    {
                        mDict.Remove(dictName);
                    }
                }
            }
        }

        public void ClearObjs(string dictName, string[] keys)
        {
            Dictionary<string, T> dict = GetSubDict(dictName);
            if (null != dict)
            {
                for (int i = 0; i < keys.Length; ++i)
                {
                    string key = keys[i];
                    T obj;
                    if (dict.TryGetValue(key, out obj))
                    {
                        if (null != DisposeCallback)
                        {
                            DisposeCallback(ref obj);
                            dict.Remove(key);
                        }
                    }
                }
                if (dict.Count == 0)
                {
                    mDict.Remove(dictName);
                }
            }
        }

        public Dictionary<string, T> GetSubDict(string dictName)
        {
            Dictionary<string, T> dict = null;
            mDict.TryGetValue(dictName, out dict);
            return dict;
        }


        public void ClearAll()
        {
            int len = mDict.Count;
            string[] keys = new string[len];
            int idx = 0;
            foreach (string dictName in mDict.Keys)
            {
                keys[idx++] = dictName;
            }
            for (int i = 0; i < len; i++)
            {
                ClearSubDict(keys[i]);
            }
        }
    }
}
