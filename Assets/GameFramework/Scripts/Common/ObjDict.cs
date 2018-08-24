using System;
using System.Collections.Generic;

namespace GameFramework
{
    public class ObjDict<T> where T : class
    {

        #region delegate
        public delegate void DisposeDelegate(ref T obj);
        #endregion

        private Dictionary<string, Dictionary<string, T>> mDict = new Dictionary<string, Dictionary<string, T>>();
        public DisposeDelegate DisposeCallback;
        /// <summary>
        /// 同名的资源在多次载入的时候是否先处理Dispose回调
        /// </summary>
        public bool ReleaseBeforeReAdd = false;

        public ObjDict() : this(null){}

        public ObjDict(DisposeDelegate dispose)
        {
            DisposeCallback = dispose;
        }

        public void AddObj(string dictName, string key, T obj)
        {
            Dictionary<string, T> dict;
            if(!mDict.TryGetValue(dictName, out dict))
            {
                dict = new Dictionary<string, T>();
            }
            if(dict.ContainsKey(key))
            {
                LogFile.Log("{0} 已经有{1}存在！", dictName, key);
                T oriObj = dict[key];
                if(!Equals(obj, oriObj))
                {
                    if(null != DisposeCallback && ReleaseBeforeReAdd)
                    {
                        DisposeCallback(ref oriObj);
                    }
                }
            }
            dict[key] = obj;
            mDict[dictName] = dict;
        }

        public T GetObj(string dictName, string key)
        {
            T obj = null;
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
