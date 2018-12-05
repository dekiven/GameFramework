using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;

namespace GameFramework
{
    public class GameResHandler<T> where T : UnityEngine.Object
    {
        #region 静态对象
        private ObjPool<AsbInfo> mInfoPool;
        #endregion

        //#region delegates
        //public delegate void OnRelease(ref T t);
        //#endregion

        public string CurGroup;
        public Action<T, AsbInfo> OnLoadCallbcak;
        public ObjDict<T>.DisposeDelegate OnReleaseCallback {set { if (null != mDict) { mDict.DisposeCallback = value; }}}
        public string Suffix = null;

        #region private 属性
        ObjDict<T> mDict;
        List<AsbInfo> mList;
        Queue<AsbInfo> mQueue;
        Dictionary<string, List<string>> mGroups;
        GameResManager mResMgr;
        #endregion

        #region public 方法

        public GameResHandler(string group)
        {
            if (null == mInfoPool)
            {
                mInfoPool = new ObjPool<AsbInfo>(delegate(ref AsbInfo info) {
                    if(null == info)
                    {
                        info = new AsbInfo();
                    }
                    return true;
                },null, null);
            }
            mDict = new ObjDict<T>();
            mList = new List<AsbInfo>();
            mQueue = new Queue<AsbInfo>();
            mGroups = new Dictionary<string, List<string>>();
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
                    mResMgr.CountAsbGroup(asbName, groupName);
                    addAsb2Group(asbName, groupName);
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
                if(obj.Length == names.Length)
                {
                    for (int i = 0; i < names.Length; i++)
                    {
                        T t = obj[i] as T;
                        if (null != t)
                        {
                            onLoad(asbName, names[i], t);
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
                mResMgr.CountAsbGroup(asbName, groupName);
                addAsb2Group(asbName, groupName);
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
                    mResMgr.CountAsbGroup(asbName, groupName);
                    addAsb2Group(asbName, groupName);
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
            mResMgr.UnloadAsbGroup(group);
            List<string> l;
            if(mGroups.TryGetValue(group, out l))
            {
                foreach (var asbName in l)
                {
                    //TODO:优化，检测其他group是否存在asb，存在则不清理
                    mDict.ClearSubDict(asbName);
                }
                mGroups.Remove(group);
            }
        }

        public void Dispose()
        {
            mDict.ClearAll();
            mList.Clear();
            mQueue.Clear();
            mGroups.Clear();
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
                            mInfoPool.Recover(info);
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
                        mInfoPool.Recover(info);

                        for (int i = 0; i < mQueue.Count; i++)
                        {
                            AsbInfo head = mQueue.Peek();
                            T obj = Get(head.asbName, head.assetName);
                            if (null != obj)
                            {
                                OnLoadCallbcak(obj, head);
                                mQueue.Dequeue();
                                mInfoPool.Recover(head);
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
            AsbInfo info = mInfoPool.Get();
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
            mInfoPool.Recover(info);
            return;
        }

        private void addAsb2Group(string asbName, string groupName)
        {
            List<string> l;
            if (!mGroups.TryGetValue(groupName, out l))
            {
                l = new List<string>();
            }
            if (!l.Contains(asbName))
            {
                l.Add(asbName);
            }
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
