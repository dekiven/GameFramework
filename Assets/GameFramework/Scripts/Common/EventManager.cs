using System.Collections;
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
            public string funcName;
            public System.Reflection.MethodInfo method;
        };

        public struct EventObj
        {
            public Pair info;
            public object[] args;
        };


        //----------------------------------- properties ------------------------------------------
        static EventPairDic sDicToMain = new EventPairDic();
        static EventPairDic sDicToThread = new EventPairDic();

        static EventObjList sListToMainWait = new EventObjList();
        static EventObjList sListToThreadWait = new EventObjList();

        static EventObjList sListToMainDoing = new EventObjList();
        static EventObjList sListToThreadDoing = new EventObjList();


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
            _deregisterEvent(dic, eventName, obj, funcName);

            Pair pair = new Pair();
            pair.obj = obj;
            pair.funcName = funcName;
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
            dic.Add(eventName, list);

            monitorExit(dic);
            return true;
        }

        public static bool deregisterFromMain(string eventName, object obj, string funcName)
        {
            return _deregisterEvent(sDicToMain, eventName, obj, funcName);
        }

        public static bool deregisterFromThread(string eventName, object obj, string funcName)
        {
            return _deregisterEvent(sDicToThread, eventName, obj, funcName);
        }

        protected static bool _deregisterEvent(EventPairDic dic, string eventName, object obj, string funcName)
        {
            monitorEnter(dic);

            EventPairList list = null;
            if (!dic.TryGetValue(eventName, out list))
            {
                LogFile.Log("do not have event named\"" + eventName + "\"");
                return false;
            }

            foreach (EventManager.Pair item in list)
            {
                if (item.obj == obj && item.funcName == funcName)
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

        public static bool noticeToMain(string eventName, object obj, object[] args)
        {
            return _noticeEvent(sDicToMain, sListToMainWait, eventName, obj, args);
        }

        public static bool noticeToThread(string eventName, object obj, object[] args)
        {
            return _noticeEvent(sDicToThread, sListToThreadWait, eventName, obj, args);
        }

        public static void noticeToAll(string eventName, object obj, object[] args)
        {
            noticeToMain(eventName, obj, args);
            noticeToThread(eventName, obj, args);
        }

        protected static bool _noticeEvent(EventPairDic dic, EventObjList list, string eventName, object obj, object[] args)
        {
            monitorEnter(dic);

            EventPairList _list = null;
            if (!dic.TryGetValue(eventName, out _list))
            {
                if (dic == sDicToMain)
                {
                    LogFile.Warn("sDicToView do not have event named \"" + eventName + "\"");
                }
                else
                {
                    LogFile.Warn("sDicToModel do not have event named \"" + eventName + "\"");
                }

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
            monitorEnter(sDicToMain);

            foreach (EventObj item in sListToMainWait)
            {
                sListToMainDoing.AddLast(item);
            }
            sListToMainWait.Clear();

            monitorExit(sDicToMain);

            foreach (EventObj item in sListToMainDoing)
            {
                try
                {
                    item.info.method.Invoke(item.info.obj, item.args);
                }
                catch (System.Exception e)
                {
                    LogFile.Error("progress view event error: func[" + item.info.funcName + "]; msg: " + e.ToString());
                }
                sListToMainDoing.Remove(item);
            }
        }

        public static void progressThreadEvents()
        {
            monitorEnter(sDicToThread);

            foreach (EventObj item in sListToThreadWait)
            {
                sListToThreadDoing.AddLast(item);
            }
            sListToThreadWait.Clear();

            monitorExit(sDicToThread);

            foreach (EventObj item in sListToThreadDoing)
            {
                try
                {
                    item.info.method.Invoke(item.info.obj, item.args);
                }
                catch (System.Exception e)
                {
                    LogFile.Error("progress Model event error: func[" + item.info.funcName + "]; msg: " + e.ToString());
                }
                sListToThreadDoing.Remove(item);
            }
        }
    }
}
