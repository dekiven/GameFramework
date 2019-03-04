using System;
namespace GameFramework
{
    //public sealed class Singleton<T> where T : new()
    //{
    //    public static T Instance
    //    {
    //        get { return SingletonCreator.instance; }
    //    }



    //    class SingletonCreator
    //    {
    //        internal static readonly T instance = new T();
    //    }
    //}

    public class Singleton<T> where T : class, new()
    {
        protected static T sInstance = null;
        private static Object lockObj = new Object();

        public static T Instance
        {
            get 
            {
                if(null == sInstance)
                {
                    lock(lockObj)
                    {
                        if (null == sInstance)
                        {
                            sInstance = new T();
                            return sInstance;
                        }
                        else
                        {
                            return sInstance;
                        }
                    }
                }
                return sInstance;
            }
        }

        public bool Destroy()
        {
            if(null != sInstance)
            {
                lock (lockObj)
                {
                    if (null != sInstance)
                    {
                        bool ret = dispose();
                        sInstance = null;
                        return ret;
                    }
                }
            }

            return true;
        }

        public static bool HasInstance()
        {
            return sInstance != null;
        }

        protected virtual bool dispose()
        {
            //LogFile.Warn("clearComp:" + typeof(T));
            return true;
        }
    }

}