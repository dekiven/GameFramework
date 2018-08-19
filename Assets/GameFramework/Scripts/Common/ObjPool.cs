using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class ObjPool<T> : IDisposable where T : class 
    {
        #region delegates
        public delegate bool OnGetDelegate(ref T obj);
        public delegate bool OnDisposeDelegate(ref T obj);
        public delegate bool OnRecoverDelegate(T obj);
        #endregion

        public OnGetDelegate OnGetCallback;
        public OnRecoverDelegate OnRecoverCallback;
        public OnDisposeDelegate OnDisposeCallback;
        public int TotalObjCount { get { return mObjCount; }}

        private Queue<T> mQueue = new Queue<T>();
        private int mObjCount = 0;

        public ObjPool() : this(null, null, null){}

        public ObjPool(OnGetDelegate onGet, OnRecoverDelegate onRecover) : this(onGet, onRecover, null){}

        public ObjPool(OnGetDelegate onGet, OnRecoverDelegate onRecover, OnDisposeDelegate onDispose)
        {
            OnGetCallback = onGet;
            OnRecoverCallback = onRecover;
            OnDisposeCallback = onDispose;
        }

        public T Get()
        {
            T obj = null;
            bool hasObj = mQueue.Count > 0;
            //LogFile.Log("mQueue.Count" + mQueue.Count);
            if (hasObj)
            {
                obj = mQueue.Dequeue();
            }
            if (null != OnGetCallback)
            {
                if(OnGetCallback(ref obj))
                {
                    if(!hasObj)
                    {
                        ++mObjCount;
                    }
                }
            }
            return obj;
        }

        public bool Recover(T obj)
        {
            if(!mQueue.Contains(obj))
            {
                mQueue.Enqueue(obj);
                if (null != OnRecoverCallback)
                {
                    return OnRecoverCallback(obj);
                }
                {
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            if(null != OnDisposeCallback)
            {
                for (int i = 0; i < Count; ++i)
                {
                    T obj = mQueue.Dequeue();
                    OnDisposeCallback(ref obj);
                }
            }else
            {
                mQueue.Clear();    
            }

            mQueue = null;
            OnGetCallback = null;
            OnRecoverCallback = null;
        }

        public int Count { get { return mQueue.Count; } }
    }
}
