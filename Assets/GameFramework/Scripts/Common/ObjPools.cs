using System;
using System.Collections.Generic;

namespace GameFramework
{
    /// <summary>
    /// 管理游戏中经常用到的对象池，如：AsbInfo List<int> List<float> List<string>
    /// </summary>
    public class ObjPools
    {
        //private static Dictionary<string, System.Object> sDict;// = new Dictionary<string, System.Object>();
        static ObjPool<List<int>> sPoolLI;
        static ObjPool<List<float>> sPoolLF;
        static ObjPool<List<string>> sPoolLS;
        static ObjPool<List<bool>> sPoolLB;
        static ObjPool<AsbInfo> sPoolAsbInfo;

        public static void Init()
        {
            //sDict = new Dictionary<string, System.Object>();
            sPoolLS = new ObjPool<List<string>>(
                delegate(ref List<string> l) 
                {
                    if(null == l)
                    {
                        l = new List<string>();
                    }
                    else
                    {
                        l.Clear();
                    }
                    return true; 
                }
                , delegate(List<string> l) {
                    l.Clear();
                    return true;
                }
            );
            //sDict[listStr.GetType().ToString()] = listStr;

            sPoolLI = new ObjPool<List<int>>(
                delegate (ref List<int> l)
                {
                    if (null == l)
                    {
                        l = new List<int>();
                    }
                    else
                    {
                        l.Clear();
                    }
                    return true;
                }
                , delegate (List<int> l) {
                    l.Clear();
                    return true;
                }
            );
            //sDict[listInt.GetType().ToString()] = listInt;

            sPoolLF = new ObjPool<List<float>>(
                delegate (ref List<float> l)
                {
                    if (null == l)
                    {
                        l = new List<float>();
                    }
                    else
                    {
                        l.Clear();
                    }
                    return true;
                }
                , delegate (List<float> l) {
                    l.Clear();
                    return true;
                }
            );
            //sDict[listFloat.GetType().ToString()] = listFloat;

            sPoolLB = new ObjPool<List<bool>>(
                delegate (ref List<bool> l)
                {
                    if (null == l)
                    {
                        l = new List<bool>();
                    }
                    else
                    {
                        l.Clear();
                    }
                    return true;
                }
                , delegate (List<bool> l) {
                    l.Clear();
                    return true;
                }
            );

            sPoolAsbInfo = new ObjPool<AsbInfo>(
                delegate (ref AsbInfo info) {
                if (null == info)
                {
                    info = new AsbInfo();
                }
                return true;
                }
                , null
            );
            //sDict[asbInfo.GetType().ToString()] = asbInfo;
        }

        //public static T GetObjPool<T>() where T : class, new()
        //{
        //    T pool = null;
        //    string name = typeof(T).ToString();
        //    System.Object obj;
        //    if (sDict.TryGetValue(name, out obj))
        //    {
        //        pool = obj as T;
        //    }
        //    if (null == pool)
        //    {
        //        LogFile.Error("no ObjPool with:" + name);
        //    }
        //    return pool;
        //}

        public static List<int> GetListInt()
        {
            _checkInit();
            return sPoolLI.Get();
        }

        public static List<float> GetListFloat()
        {
            _checkInit();
            return sPoolLF.Get();
        }

        public static List<string> GetListString()
        {
            _checkInit();
            return sPoolLS.Get();
        }

        public static List<bool> GetListBool()
        {
            _checkInit();
            return sPoolLB.Get();
        }

        public static AsbInfo GetAsbInfo()
        {
            _checkInit();
            return sPoolAsbInfo.Get();
        }

        public static bool Recover(List<string> obj)
        {
            _checkInit();
            return sPoolLS.Recover(obj);
        }

        public static bool Recover(List<int> obj)
        {
            _checkInit();
            return sPoolLI.Recover(obj);
        }

        public static bool Recover(List<float> obj)
        {
            _checkInit();
            return sPoolLF.Recover(obj);
        }

        public static bool Recover(List<bool> obj)
        {
            _checkInit();
            return sPoolLB.Recover(obj);
        }

        public static bool Recover(AsbInfo obj)
        {
            _checkInit();
            return sPoolAsbInfo.Recover(obj);
        }

        private static void _checkInit()
        {
            if(null == sPoolLI)
            {
                Init();
            }
        }
    }
}
