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

        public static bool AddToMain(string eventName, object obj, string funcName)
        {
            return _addEvent(sDicToMain, eventName, obj, funcName);
        }

        public static bool AddToThread(string eventName, object obj, string funcName)
        {
            return _addEvent(sDicToThread, eventName, obj, funcName);
        }

        protected static bool _addEvent(EventPairDic dic, string eventName, object obj, string funcName)
        {
            _removeEvent(dic, eventName, obj);

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

        public static bool RemoveFromMain(string eventName, object obj)
        {
            return _removeEvent(sDicToMain, eventName, obj);
        }

        public static bool RemoveFromThread(string eventName, object obj)
        {
            return _removeEvent(sDicToThread, eventName, obj);
        }

        protected static bool _removeEvent(EventPairDic dic, string eventName, object obj)
        {
            monitorEnter(dic);

            EventPairList list = null;
            if (!dic.TryGetValue(eventName, out list))
            {
                LogFile.Log("_deregisterEvent do not have event named\"" + eventName + "\"");
                return false;
            }

            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                Pair pair = list[count - i - 1];
                if (pair.obj == obj)
                {
                    list.Remove(pair);
                    break;
                }
            }

            monitorExit(dic);

            return true;
        }

        public static bool RemoveFromMain(object obj)
        {
            return _removeEvent(sDicToMain, obj);
        }

        public static bool RemoveFromThread(object obj)
        {
            return _removeEvent(sDicToThread, obj);
        }

        protected static bool _removeEvent(EventPairDic dic, object obj)
        {
            monitorEnter(dic);

            List<string> emptyLists = new List<string>();
            foreach (string key in dic.Keys)
            {
                List<Pair> pairs = dic[key];
                int count = pairs.Count;
                for (int i = 0; i < count; i++)
                {
                    int index = count - i - 1;
                    Pair pair = pairs[index];
                    if (pair.obj == obj)
                    {
                        pairs.RemoveAt(index);
                    }
                }
                if (pairs.Count == 0)
                {
                    emptyLists.Add(key);
                }
            }
            foreach (string key in emptyLists)
            {
                dic.Remove(key);
            }

            monitorExit(dic);

            return true;
        }

        public static bool NotifyMain(string eventName, params object[] args)
        {
            return _notifyEvent(sDicToMain, sListToMainWait, eventName, args);
        }

        public static bool NotifyThread(string eventName, params object[] args)
        {
            return _notifyEvent(sDicToThread, sListToThreadWait, eventName, args);
        }

        public static void NotifyAll(string eventName, params object[] args)
        {
            NotifyMain(eventName, args);
            NotifyThread(eventName, args);
        }

        protected static bool _notifyEvent(EventPairDic dic, EventObjList list, string eventName, params object[] args)
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

        public static void ProgressMainEvents()
        {
            _progressMainEvents(sDicToMain, sListToMainWait, sListToMainDoing, true);
        }

        public static void ProgressThreadEvents()
        {
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
