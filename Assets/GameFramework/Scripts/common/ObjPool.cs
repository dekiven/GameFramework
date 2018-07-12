using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class ObjPool<T> : IDisposable where T : class 
    {
        public delegate bool OnGetDelegate(ref T obj);
        public delegate bool OnDisposeDelegate(ref T obj);
        public delegate bool OnRecoverDelegate(T obj);

        //private readonly List<T> mStack = new List<T>();
        private OnGetDelegate mOnGet;
        private OnRecoverDelegate mOnRecover;
        private OnDisposeDelegate mOnDispose;
        private Queue<T> mQueue = new Queue<T>();

        public ObjPool() : this(null, null, null){}

        public ObjPool(OnGetDelegate onGet, OnRecoverDelegate onRecover) : this(onGet, onRecover, null){}

        public ObjPool(OnGetDelegate onGet, OnRecoverDelegate onRecover, OnDisposeDelegate onDispose)
        {
            mOnGet = onGet;
            mOnRecover = onRecover;
            mOnDispose = onDispose;
        }

        public T Get()
        {
            T obj = mQueue.Dequeue();
            if (null != mOnGet && mOnGet(ref obj))
            {
                return obj;
            }
            else
            {
                return null;
            }
        }

        public bool Recover(T obj)
        {
            if(!mQueue.Contains(obj))
            {
                mQueue.Enqueue(obj);
                if (null != mOnRecover)
                {
                    return mOnRecover(obj);
                }
                {
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            if(null != mOnDispose)
            {
                for (int i = 0; i < Count; ++i)
                {
                    T obj = mQueue.Dequeue();
                    mOnDispose(ref obj);
                }
            }else
            {
                mQueue.Clear();    
            }

            mQueue = null;
            mOnGet = null;
            mOnRecover = null;
        }

        public int Count { get { return mQueue.Count; } }
    }
}
