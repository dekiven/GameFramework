using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    public class ComponetHandler : MonoBehaviour
    {
        public Component[] Components;
        public Transform RootTransform;
        private List<string> mCompNames;

        void Start()
        {
            if (null == RootTransform)
            {
                RootTransform = transform;
            }
            if (Components.Length > 0)
            {
                mCompNames = new List<string>();
                for (int i = 0; i < Count; ++i)
                {
                    mCompNames.Add(Tools.GetTransformName(Components[i].transform, RootTransform));
                }
            }
        }


        public int Count { get { return Components.Length; } }

        public string[] CompNames { get { return mCompNames.ToArray(); }}

        public T GetCompByIndex<T>(int index) where T : Component
        {
            T comp = null;
            if (index < Count)
            {
                comp = Components[index] as T;
            }
            else
            {
                LogFile.Warn(
                    "{0}找不到index为{1}，且类型是{2}的组件。"
                    , Tools.GetTransformName(transform, Camera.main.transform)
                    , index
                    , typeof(T)
                );
            }
            return comp;
        }

        public T GetCompByName<T>(string name) where T : Component
        {
            int idx = mCompNames.IndexOf(name);
            if(!Equals(-1, idx))
            {
                return GetCompByIndex<T>(idx); 
            }
            else
            {
                LogFile.Warn(
                    "{0}找不到name为{1}的组件。"
                    , Tools.GetTransformName(transform, Camera.main.transform)
                    , name
                );
            }
            return null;
        }
    }
}