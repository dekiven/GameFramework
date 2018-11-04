﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace GameFramework
{
    using EventPairList = List<EventManager.Pair>;
    using EventPairDic = Dictionary<string, List<EventManager.Pair>>;
    using EventObjList = LinkedList<EventManager.EventObj>;

    public class EventManager
    {
        //--------------------------------------  struct ----------------------------------------
        public struct Pair
        {
            public object obj;
            public string eventName;
            public System.Reflection.MethodInfo method;
        };

        public struct EventObj
        {
            public Pair info;
            public object[] args;
        };

        //public delegate void ExternalNotifyDel(string eventName, object[] args);

        //----------------------------------- properties ------------------------------------------
        static EventPairDic sDicToMain = new EventPairDic();
        static EventPairDic sDicToThread = new EventPairDic();

        static EventObjList sListToMainWait = new EventObjList();
        static EventObjList sListToThreadWait = new EventObjList();

        static EventObjList sListToMainDoing = new EventObjList();
        static EventObjList sListToThreadDoing = new EventObjList();

        //public static ExternalNotifyDel MainExternalDel;
        //public static ExternalNotifyDel ThreadExternalDel;

        //-----------------------------------  methods --------------------------------------------
        public static void monitorEnter(object obj)
        {
            Monitor.Enter(obj);
        }

        public static void monitorExit(object obj)
        {
            Monitor.Exit(obj);
        }

        public static bool registerToMain(string eventName, object obj, string funcName)
        {
            return _registerEvent(sDicToMain, eventName, obj, funcName);
        }

        public static bool registerToThread(string eventName, object obj, string funcName)
        {
            return _registerEvent(sDicToThread, eventName, obj, funcName);
        }

        protected static bool _registerEvent(EventPairDic dic, string eventName, object obj, string funcName)
        {
            _deregisterEvent(dic, eventName, obj);

            Pair pair = new Pair();
            pair.obj = obj;
            pair.eventName = eventName;
            pair.method = obj.GetType().GetMethod(funcName);
            if (null == pair.method)
            {
                LogFile.Warn("Register worning: Obj(" + obj + ") do not have method named \"" + funcName + "\"");
                return false;
            }

            EventPairList list = null;

            monitorEnter(dic);

            if (!dic.TryGetValue(eventName, out list))
            {
                list = new EventPairList();
                dic.Add(eventName, list);
            }

            list.Add(pair);
            dic[eventName] = list;

            monitorExit(dic);
            return true;
        }

        public static bool deregisterFromMain(string eventName, object obj)
        {
            return _deregisterEvent(sDicToMain, eventName, obj);
        }

        public static bool deregisterFromThread(string eventName, object obj)
        {
            return _deregisterEvent(sDicToThread, eventName, obj);
        }

        protected static bool _deregisterEvent(EventPairDic dic, string eventName, object obj)
        {
            monitorEnter(dic);

            EventPairList list = null;
            if (!dic.TryGetValue(eventName, out list))
            {
                LogFile.Log("_deregisterEvent do not have event named\"" + eventName + "\"");
                return false;
            }

            foreach (EventManager.Pair item in list)
            {
                if (item.obj == obj)
                {
                    list.Remove(item);
                    break;
                }
            }

            monitorExit(dic);

            return true;
        }

        public static bool deregisterFromMain(object obj)
        {
            return _deregisterEvent(sDicToMain, obj);
        }

        public static bool deregisterFromThread(object obj)
        {
            return _deregisterEvent(sDicToThread, obj);
        }

        protected static bool _deregisterEvent(EventPairDic dic, object obj)
        {
            monitorEnter(dic);

            foreach (KeyValuePair<string, List<Pair>> item in dic)
            {
                foreach (Pair pair in item.Value)
                {
                    if (pair.obj == obj)
                    {
                        item.Value.Remove(pair);
                    }
                }
            }

            monitorExit(dic);

            return true;
        }

        public static bool notifyMain(string eventName, object[] args)
        {
            return _notifyEvent(sDicToMain, sListToMainWait, eventName, args);
        }

        public static bool notifyThread(string eventName, object[] args)
        {
            return _notifyEvent(sDicToThread, sListToThreadWait, eventName, args);
        }

        public static void notifyAll(string eventName, object[] args)
        {
            notifyMain(eventName, args);
            notifyThread(eventName, args);
        }

        protected static bool _notifyEvent(EventPairDic dic, EventObjList list, string eventName, object[] args)
        {
            monitorEnter(dic);

            EventPairList _list = null;
            if (!dic.TryGetValue(eventName, out _list))
            {
                //if (dic == sDicToMain)
                //{
                //    LogFile.Warn("sDicToMain do not have event named \"" + eventName + "\"");
                //}
                //else
                //{
                //    LogFile.Warn("sDicToThread do not have event named \"" + eventName + "\"");
                //}
                monitorExit(dic);

                return false;
            }

            foreach (Pair item in _list)
            {
                EventObj _obj = new EventObj();
                _obj.info = item;
                _obj.args = args;
                list.AddLast(_obj);
            }

            monitorExit(dic);

            return true;
        }

        public static void progressMainEvents()
        {
            //monitorEnter(sDicToMain);

            //foreach (EventObj item in sListToMainWait)
            //{
            //    sListToMainDoing.AddLast(item);
            //}
            //sListToMainWait.Clear();

            //monitorExit(sDicToMain);

            //for (int i = 0; i < sListToMainDoing.Count; i++)
            //{
            //    var item = sListToMainDoing.First.Value;
            //    try
            //    {
            //        item.info.method.Invoke(item.info.obj, item.args);
            //    }
            //    catch (System.Exception e)
            //    {
            //        LogFile.Error("progress view event error: func[" + item.info.funcName + "]; msg: " + e.ToString());
            //    }
            //    sListToMainDoing.RemoveFirst();
            //}
            _progressMainEvents(sDicToMain, sListToMainWait, sListToMainDoing, true);
        }

        public static void progressThreadEvents()
        {
            //monitorEnter(sDicToThread);

            //foreach (EventObj item in sListToThreadWait)
            //{
            //    sListToThreadDoing.AddLast(item);
            //}
            //sListToThreadWait.Clear();

            //monitorExit(sDicToThread);

            //foreach (EventObj item in sListToThreadDoing)
            //{
            //    try
            //    {
            //        item.info.method.Invoke(item.info.obj, item.args);
            //    }
            //    catch (System.Exception e)
            //    {
            //        LogFile.Error("progress Model event error: func[" + item.info.funcName + "]; msg: " + e.ToString());
            //    }
            //    sListToThreadDoing.Remove(item);
            //}
            _progressMainEvents(sDicToThread, sListToThreadWait, sListToThreadDoing);
        }

        private static void _progressMainEvents(EventPairDic dic, EventObjList listWait, EventObjList listDo, bool isMainThread=false)
        {
            monitorEnter(dic);

            foreach (EventObj item in listWait)
            {
                listDo.AddLast(item);
            }
            listWait.Clear();

            monitorExit(dic);

            for (int i = 0; i < listDo.Count; i++)
            {
                var item = listDo.First.Value;
                try
                {
                    item.info.method.Invoke(item.info.obj, item.args);
                }
                catch (System.Exception e)
                {
                    LogFile.Error("progress "+ (isMainThread ? "main" : "thread") +" event error: event[" + item.info.eventName + "]; msg: " + e.ToString());
                }
                listDo.RemoveFirst();
            }

        }
    }
}
