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
        /// 记录当前 group 中的 T 对象名,
        /// 当释放 group 时检测 mDict 中对象是否存在当前 group 中，
        /// 如果没有则不从 mDict 释放
        /// </summary>
        Dictionary<string, Dictionary<string, HashSet<string>>> mGroupObjs;
        List<AsbInfo> mList;
        Queue<AsbInfo> mQueue;
        //Dictionary<string, List<string>> mGroups;
        ResManager mResMgr;
        #endregion

        #region public 方法

        public GameResHandler(string group)
        {
            mDict = new ObjDict<T>();
            mGroupsCounter = new ObjDict<int>();
            mGroupObjs = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            mList = new List<AsbInfo>();
            mQueue = new Queue<AsbInfo>();
            mResMgr = ResManager.Instance;
            CurGroup = group;
        }

        public void Load(string asbName, string assetName, string extral = null, bool isOrdered = false)
        {
            assetName = FixResName(assetName);
            if (null == Get(asbName, assetName))
            {
                string groupName = CurGroup;
                _addAsbInfo(asbName, assetName, extral, isOrdered);
                mResMgr.LoadRes<T>(asbName, assetName, delegate (UnityEngine.Object obj)
                {
                    T t = obj as T;
                    if (null != t)
                    {
                        _onLoad(asbName, assetName, t);
                    }
                    else
                    {
                        LogFile.Warn("load:({0},{1})error.", asbName, assetName);
                    }
                    _addAsb2Group(asbName, groupName, new string[] {assetName,});
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
                _addAsbInfo(asbName, assetName, extral, isOrdered);
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
                            _onLoad(asbName, names[i], t);
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
                _addAsb2Group(asbName, groupName, names);
            });
        }

        public T Get(string asbName, string assetName)
        {
            assetName = FixResName(assetName);
            return mDict.GetObj(asbName, assetName);
        }

        //TODO:考虑支持多个异步获取
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
                        _onLoad(asbName, assetName, t);
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
                    _addAsb2Group(asbName, groupName, new string[] {assetName,});
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

                Dictionary<string, HashSet<string>> dict;
                if(mGroupObjs.TryGetValue(group, out dict))
                {
                    foreach (var item in dict)
                    {
                        mDict.ClearObjs(item.Key, new List<string>(item.Value).ToArray());
                    }
                    //Dictionary<string, T> objs = mDict.GetSubDict(group);
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

        private bool _onLoad(string asbName, string assetName, T t)
        {
            bool ret= mDict.AddObj(asbName, assetName, t);
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
                                //不是调用序列的第一个直接返回
                                return ret;
                            }
                        }
                    }
                }
            }
            return ret;
        }

        private void _addAsbInfo(string asbName, string name, string extral, bool isOrdered)
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

        private void _addAsb2Group(string asbName, string groupName, string[] obj)
        {
            int count = mGroupsCounter.GetObj(groupName, asbName);
            mGroupsCounter.AddObj(groupName, asbName, count+1);
            Dictionary<string, HashSet<string>> groupDict;
            if(!mGroupObjs.TryGetValue(groupName, out groupDict))
            {
                groupDict = new Dictionary<string, HashSet<string>>();
            }
            HashSet<string> asbSet;
            if(!groupDict.TryGetValue(asbName, out asbSet))
            {
                asbSet = new HashSet<string>();
            }
            //并集
            asbSet.UnionWith(obj);
            groupDict[asbName] = asbSet;
            mGroupObjs[groupName] = groupDict;
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
