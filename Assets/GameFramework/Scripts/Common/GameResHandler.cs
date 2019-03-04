using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

namespace GameFramework
{
    public class GameResHandler<T> where T : UnityEngine.Object
    {
        //#region delegates
        //public delegate void OnRelease(ref T t);
        //#endregion

        public string CurGroup;
        public Action<T, AsbInfo> OnLoadCallbcak;
        public ObjDict<T>.DisposeDelegate OnReleaseCallback {set { if (null != mDict) { mDict.DisposeCallback = value; }}}
        public string Suffix = null;

        #region private 属性
        /// <summary>
        /// 保存所有已经加载到内存的 T 对象，释放也是通过 mDict 释放 
        /// </summary>
        ObjDict<T> mDict;
        /// <summary>
        /// 当前 Group 的引用计数器
        /// </summary>
        ObjDict<int> mGroupsCounter;
        /// <summary>
        /// 记录当前 group 中的 T 对象,
        /// 当释放 group 时检测 mDict 中对象是否存在当前 group 中，
        /// 如果没有则不从 mDict 释放
        /// </summary>
        Dictionary<string, List<T>> mGroupObjs;
        List<AsbInfo> mList;
        Queue<AsbInfo> mQueue;
        //Dictionary<string, List<string>> mGroups;
        GameResManager mResMgr;
        #endregion

        #region public 方法

        public GameResHandler(string group)
        {
            mDict = new ObjDict<T>();
            mGroupsCounter = new ObjDict<int>();
            mGroupObjs = new Dictionary<string, List<T>>();
            mList = new List<AsbInfo>();
            mQueue = new Queue<AsbInfo>();
            mResMgr = GameResManager.Instance;
            CurGroup = group;
        }

        public void Load(string asbName, string assetName, string extral = null, bool isOrdered = false)
        {
            assetName = FixResName(assetName);
            if (null == Get(asbName, assetName))
            {
                string groupName = CurGroup;
                addAsbInfo(asbName, assetName, extral, isOrdered);
                mResMgr.LoadRes<T>(asbName, assetName, delegate (UnityEngine.Object obj)
                {
                    T t = obj as T;
                    if (null != t)
                    {
                        onLoad(asbName, assetName, t);
                    }
                    else
                    {
                        LogFile.Warn("load:({0},{1})error.", asbName, assetName);
                    }
                    addAsb2Group(asbName, groupName, new T[] {t,});
                });
            }
        }

        public void Load(string asbName, string[] names, string extral = null, bool isOrdered= false)
        {

            List<string> list = new List<string>();
            foreach (var name in names)
            {
                string assetName = FixResName(name);
                if (null == Get(asbName, assetName))
                {
                    list.Add(assetName);
                }
                addAsbInfo(asbName, assetName, extral, isOrdered);
            }
            string groupName = CurGroup;
            mResMgr.LoadRes<T>(asbName, names, delegate (UnityEngine.Object[] obj)
            {
                List<T> l = new List<T>();
                if(obj.Length == names.Length)
                {
                    for (int i = 0; i < names.Length; i++)
                    {
                        T t = obj[i] as T;
                        if (null != t)
                        {
                            onLoad(asbName, names[i], t);
                            l.Add(t);
                        }
                        else
                        {
                            LogFile.Warn("load:({0},{1})error.", asbName, names[i]);
                        }
                    }
                }
                else
                {
                    LogFile.Warn("load {0} error, names.count={1}, obj.Count={2}", asbName, names.Length, obj.Length);
                }
                addAsb2Group(asbName, groupName, l.ToArray());
            });
        }

        public T Get(string asbName, string assetName)
        {
            assetName = FixResName(assetName);
            return mDict.GetObj(asbName, assetName);
        }

        public void GetAsync(string asbName, string assetName, Action<T> callback=null, LuaFunction luaFunction=null)
        {
            assetName = FixResName(assetName);
            T t = Get(asbName, assetName);
            if(null == t)
            {
                string groupName = CurGroup;
                mResMgr.LoadRes<T>(asbName, assetName, delegate (UnityEngine.Object obj)
                {
                    t = obj as T;
                    if (null != t)
                    {
                        onLoad(asbName, assetName, t);
                        if(null != callback)
                        {
                            callback(t);
                        }
                        if(null != luaFunction)
                        {
                            luaFunction.Call<T>(t);
                            luaFunction.Dispose();
                        }
                    }
                    else
                    {
                        LogFile.Warn("GetAsync load:({0},{1})error.", asbName, assetName);
                    }
                    addAsb2Group(asbName, groupName, new T[] {t,});
                });               
            }
            else
            {
                if (null != callback)
                {
                    callback(t);
                }
                if (null != luaFunction)
                {
                    luaFunction.Call<T>(t);
                    luaFunction.Dispose();
                }
            }
        }

        public void ClearGroup(string group)
        {
            Dictionary<string, int> counts = mGroupsCounter.GetSubDict(group);
            if (null != counts)
            {
                foreach (var item in counts)
                {
                    mResMgr.UnloadAssetBundle(item.Key, false, item.Value);
                }
                mGroupsCounter.ClearSubDict(group);

                List<T> list = null;
                if(mGroupObjs.TryGetValue(group, out list))
                {
                    Dictionary<string, T> objs = mDict.GetSubDict(group);
                    if (null != objs)
                    {
                        foreach (var obj in objs)
                        {
                            if (list.Contains(obj.Value))
                            {
                                list.Remove(obj.Value);
                                mDict.ClearObj(group, obj.Key);
                            }
                        }
                    }
                    mGroupObjs.Remove(group);
                }
            }
        }

        public void Dispose()
        {
            mDict.ClearAll();
            mList.Clear();
            mQueue.Clear();
            mGroupsCounter.ClearAll();
        }

        #endregion


        #region private 方法

        private void onLoad(string asbName, string assetName, T t)
        {
            mDict.AddObj(asbName, assetName, t);
            if(null != OnLoadCallbcak)
            {
                if (mList.Count > 0)
                {
                    for (int i = mList.Count - 1; i > -1; --i)
                    {
                        AsbInfo info = mList[i];
                        if (info.Equals(asbName, assetName))
                        {
                            OnLoadCallbcak(t, info);
                            mList.Remove(info);
                            ObjPools.Recover(info);
                        }
                    }
                }

                if(mQueue.Count > 0)
                {
                    AsbInfo info = mQueue.Peek();
                    if(info.Equals(asbName, assetName))
                    {
                        OnLoadCallbcak(t, info);
                        mQueue.Dequeue();
                        ObjPools.Recover(info);

                        for (int i = 0; i < mQueue.Count; i++)
                        {
                            AsbInfo head = mQueue.Peek();
                            T obj = Get(head.asbName, head.assetName);
                            if (null != obj)
                            {
                                OnLoadCallbcak(obj, head);
                                mQueue.Dequeue();
                                ObjPools.Recover(head);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
            }

        }

        private void addAsbInfo(string asbName, string name, string extral, bool isOrdered)
        {
            if(null == OnLoadCallbcak)
            {
                return;
            }
            AsbInfo info = ObjPools.GetAsbInfo();
            info.Set(asbName, name, extral);
            if (isOrdered)
            {
                if (!mQueue.Contains(info))
                {
                    mQueue.Enqueue(info);
                    return;
                }
            }
            else
            {
                if (!mList.Contains(info))
                {
                    mList.Add(info);
                    return;
                }
            }
            //如果已经在list或者队列里则回收
            ObjPools.Recover(info);
            return;
        }

        private void addAsb2Group(string asbName, string groupName, T[] obj)
        {
            int count = mGroupsCounter.GetObj(groupName, asbName);
            mGroupsCounter.AddObj(groupName, asbName, count+1);
            List<T> ts;
            if(!mGroupObjs.TryGetValue(groupName, out ts))
            {
                ts = new List<T>();
            }
            ts.AddRange(obj);
            mGroupObjs[groupName] = ts;
        }

        public string FixResName(string name)
        {
            if (null != Suffix && !name.EndsWith(Suffix, StringComparison.Ordinal))
            {
                name = name + Suffix;
            }
            return name;
        }
        #endregion
    }
}
