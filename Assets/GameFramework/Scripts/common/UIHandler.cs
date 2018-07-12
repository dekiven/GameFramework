using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameFramework
{
    public class UIHandler : MonoBehaviour
    {

        public UIBehaviour[] UIArray;
        public Transform RootTransform;
        private List<string> mUINames;

        void Start()
        {
            if (null == RootTransform)
            {
                RootTransform = transform;
            }
            if (UIArray.Length > 0)
            {
                mUINames = new List<string>();
                for (int i = 0; i < Count; ++i)
                {
                    mUINames.Add(Tools.GetTransformName(UIArray[i].transform, RootTransform));
                }
            }
        }


        public int Count { get { return UIArray.Length; } }

        public string[] CompNames { get { return mUINames.ToArray(); } }

        public T GetCompByIndex<T>(int index) where T : Component
        {
            T comp = null;
            if (index < Count)
            {
                comp = UIArray[index] as T;
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
            int idx = mUINames.IndexOf(name);
            if (!Equals(-1, idx))
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

        public bool SetTextString(int index, string content)
        {
            Text text = GetCompByIndex<Text>(index);
            if (null != text)
            {
                text.text = content;
                return true;
            }
            return false;
        }

        public bool SetTextString(string name, string content)
        {
            Text text = GetCompByName<Text>(name);
            if (null != text)
            {
                text.text = content;
                return true;
            }
            return false;
        }

        public bool SetSliderValue(int index, float value)
        {
            Slider slider = GetCompByIndex<Slider>(index);
            if (null != slider)
            {
                slider.value = value;
                return true;
            }
            return false;
        }

        public bool SetSliderValue(string name, float value)
        {
            Slider slider = GetCompByName<Slider>(name);
            if (null != slider)
            {
                slider.value = value;
                return true;
            }
            return false;
        }
    }
}