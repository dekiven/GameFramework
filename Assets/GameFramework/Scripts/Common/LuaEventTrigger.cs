using System;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
using UnityEngine.EventSystems;
namespace GameFramework
{
    public class LuaEventTrigger : EventTrigger
    {
        public string EventName;
        public string External;
        private LuaTable mTriggerTable;
        private List<LuaFunction> mFuncs = new List<LuaFunction>();

        public void SetTriggers(LuaTable lua)
        {
            clearLua();
            mTriggerTable = lua;
            triggers.Clear();
            if (null != lua)
            {
                foreach (EventTriggerType t in Enum.GetValues(typeof(EventTriggerType)))
                {
                    LuaFunction function = mTriggerTable.RawGet<EventTriggerType, LuaFunction>(t);
                    if (null != function)
                    {
                        mFuncs.Add(function);
                        Entry entry = new Entry();
                        entry.eventID = t;
                        entry.callback.AddListener((BaseEventData data) => 
                        {
                            function.Call(data);
                        });
                        triggers.Add(entry);
                    }
                }
            }
        }

        private void clearLua()
        {
            for (int i = mFuncs.Count - 1; i >= 0; --i)
            {
                mFuncs[i].Dispose();
                mFuncs.RemoveAt(i);
            }
            if (null != mTriggerTable)
            {
                mTriggerTable.Dispose();
            }
        }

        private void OnDestroy()
        {
            clearLua();
        }
    }
}
