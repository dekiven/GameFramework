using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class GameResHandler<T> where T : UnityEngine.Object
    {
        #region 静态对象
        static ObjPool<AsbInfo> sInfoPool;
        #endregion

        //#region delegates
        //public delegate void OnRelease(ref T t);
        //#endregion

        public string CurGroup;
        public Action<T, AsbInfo> OnLoadCallbcak;
        public ObjDict<T>.DisposeDelegate OnReleaseCallback {set { if (null != mDict) { mDict.DisposeCallback = value; }}}

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
            if (null == sInfoPool)
            {
                sInfoPool = new ObjPool<AsbInfo>(delegate(ref AsbInfo info) {
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
                if (null == Get(asbName, name))
                {
                    list.Add(name);
                }
                addAsbInfo(asbName, name, extral, isOrdered);
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
            return mDict.GetObj(asbName, assetName);
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


        #endregion


        #region private 方法

        private void onLoad(string asbName, string assetName, T t)
        {
            mDict.AddObj(asbName, assetName, t);
            if(null != OnLoadCallbcak)
            {
                if (mList.Count > 0)
                {
                    foreach (var info in mList)
                    {
                        if(info.Equals(asbName, assetName))
                        {
                            OnLoadCallbcak(t, info);
                            mList.Remove(info);
                            sInfoPool.Recover(info);

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
                        sInfoPool.Recover(info);

                        for (int i = 0; i < mQueue.Count; i++)
                        {
                            AsbInfo head = mQueue.Peek();
                            T obj = Get(head.asbName, head.assetName);
                            if (null != obj)
                            {
                                OnLoadCallbcak(obj, head);
                                mQueue.Dequeue();
                                sInfoPool.Recover(head);
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
            AsbInfo info = sInfoPool.Get();
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
            sInfoPool.Recover(info);
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
        #endregion
    }

    public class AsbInfo : IEquatable<AsbInfo>
    {
        public string asbName;
        public string assetName;
        public string extral;

        public bool Equals(string asbName, string assetName)
        {
            return this.asbName.Equals(asbName) && this.assetName.Equals(assetName);
        }

        public bool Equals(AsbInfo other)
        {
            return this.asbName.Equals(other.asbName) && this.assetName.Equals(other.assetName);
        }

        public static bool Equals(AsbInfo info, AsbInfo info2)
        {
            return info.asbName.Equals(info2.asbName) && info.assetName.Equals(info2.assetName);
        }

        public void Set(string asbName, string assetName, string extral = null)
        {
            this.asbName = asbName;
            this.assetName = assetName;
            this.extral = extral;
        }
    }
}
