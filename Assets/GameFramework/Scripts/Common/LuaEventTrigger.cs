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
            _clearLua();
            mTriggerTable = lua;
            triggers.Clear();
            if (null != lua)
            {
                int i = 0;
                foreach (EventTriggerType t in Enum.GetValues(typeof(EventTriggerType)))
                {
                    int idx = i;
                    LuaFunction function = null;
                    try
                    {
                        function = mTriggerTable.RawGet<EventTriggerType, LuaFunction>(t);
                    }
                    catch (Exception ex)
                    {
                        LogFile.Log(ex.Message);
                    }
                    if (null != function)
                    {
                        mFuncs.Add(function);
                        Entry entry = new Entry();
                        entry.eventID = t;
                        entry.callback.AddListener((BaseEventData data) => 
                        {
                            mFuncs[idx].Call(data);
                        });
                        triggers.Add(entry);
                        i += 1;
                    }
                }
            }
        }

        private void _clearLua()
        {
            for (int i = mFuncs.Count - 1; i >= 0; --i)
            {
                mFuncs[i].Dispose();
                mFuncs.RemoveAt(i);
            }
            if (null != mTriggerTable)
            {
                mTriggerTable.Dispose();
                mTriggerTable = null;
            }
        }

        private void OnDestroy()
        {
            _clearLua();
        }
    }
}
