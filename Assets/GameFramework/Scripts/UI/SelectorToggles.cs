using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameFramework
{
    public class SelectorToggles : UIBehaviour
    {
        public GameObject TogglePrefab;

        [SerializeField]
        private RectTransform mContent;
        public RectTransform Content { get { return mContent; } set { mContent = value; } }
        [SerializeField]
        private bool mEnableTouch = true;
        public bool EnableTouch
        {
            get { return mEnableTouch; }
            set
            {
                if (mEnableTouch != value)
                {
                    for (int i = 0; i < mToggls.Count; i++)
                    {
                        mToggls[i].enabled = value;
                    }
                }
                mEnableTouch = value;
            }
        }
        /// <summary>
        /// 设置的当前选中toggle时，是否通知
        /// </summary>
        [Tooltip("设置的当前选中toggle时，是否回调通知")]
        public bool CallbackOnSet=false;

        private List<Toggle> mToggls;
        private ObjPool<Toggle> mPool;
        private ToggleGroup mGroup;
        private Coroutine mSetToggleCor;
        private int mCurIndex = 0;
        private int mTargetIndex = -1;
        private UnityAction<int> mOnValueChange;
        private LuaFunction mOnValueChangeLua;

        public void SetTotalNum(int num)
        {
            if (null != mSetToggleCor)
            {
                StopCoroutine(mSetToggleCor);
                mSetToggleCor = null;
            }
            mSetToggleCor = StartCoroutine(setTotalNum(num));

        }

        public void SetCurIndex(int index)
        {
            if (null != mSetToggleCor)
            {
                mTargetIndex = index;
            }
            else
            {
                setCurIndex(index);
            }
        }

        public void SetOnIndexChange(UnityAction<int> action)
        {
            mOnValueChange = action;
        }

        public void SetOnIndexChange(LuaFunction function)
        {
            mOnValueChangeLua = function;
        }
        #region UIBehaviour
        protected override void Awake()
        {
            base.Awake();
            mToggls = new List<Toggle>();
            mPool = new ObjPool<Toggle>(onGetDelegate, onRecoverDelegate, onDisposeDelegate);
        }

        protected override void Start()
        {
            base.Start();
            mGroup = Content.GetComponent<ToggleGroup>();
            if (null == mGroup)
            {
                mGroup = Content.gameObject.AddComponent<ToggleGroup>();
                mGroup.allowSwitchOff = false;
            }


#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                StartCoroutine(test());
            }
#endif
        }

        protected override void OnDestroy()
        {
            if(null != mOnValueChangeLua)
            {
                mOnValueChangeLua.Dispose();
            }
            if (null != mPool)
            {
                mPool.Dispose();
            }
            base.OnDestroy();
        }
        #endregion UIBehaviour

        #region ObjPool 处理
        bool onGetDelegate(ref Toggle obj)
        {
            if (null == obj)
            {
                GameObject gobj = Instantiate(TogglePrefab, Content, false);
                gobj.name = "item" + mPool.TotalObjCount;
                obj = gobj.GetComponent<Toggle>();

                if (null == obj)
                {
                    LogFile.Error("TogglePrefab：{0} prefab没有添加ScrollItem组件", TogglePrefab.name);
                    return false;
                }
            }
            obj.enabled = EnableTouch;
            obj.gameObject.SetActive(true);
            mGroup.RegisterToggle(obj);
            return true;
        }

        bool onRecoverDelegate(Toggle obj)
        {
            mGroup.UnregisterToggle(obj);
            obj.onValueChanged.RemoveAllListeners();
            obj.gameObject.SetActive(false);
            return true;
        }

        bool onDisposeDelegate(ref Toggle obj)
        {
            Destroy(obj.gameObject);
            obj = null;
            return true;
        }
        #endregion ObjPool 处理

        #region 私有方法
        Toggle getToggle()
        {
            return mPool.Get();
        }

        void recover(Toggle toggle)
        {
            mPool.Recover(toggle);
        }

        void recoverAll()
        {
            for (int i = 0; i < mToggls.Count; i++)
            {
                recover(mToggls[i]);
            }
            mToggls.Clear();
        }

        private IEnumerator setTotalNum(int num)
        {
            recoverAll();
            for (int i = 0; i < num; i++)
            {
                Toggle toggle = getToggle();
                mToggls.Add(toggle);
                toggle.isOn = (i == mCurIndex);
                toggle.group = mGroup;
                toggle.onValueChanged.AddListener(delegate (bool isOn)
                {
                    onToggleOn(toggle, isOn);
                });
                yield return null;
            }
            if (null != mSetToggleCor)
            {
                StopCoroutine(mSetToggleCor);
                mSetToggleCor = null;
            }
            if(mTargetIndex != -1)
            {
                setCurIndex(mTargetIndex);
                mTargetIndex = -1;
            }
        }

        private void setCurIndex(int index)
        {
            int idx = Mathf.Clamp(index, 0, mToggls.Count - 1);
            if(mCurIndex != idx)
            {
                mCurIndex = idx;
                if (!EnableTouch)
                {
                    for (int i = 0; i < mToggls.Count; i++)
                    {
                        mToggls[i].isOn = (i == mCurIndex);
                    }
                }
                else
                {
                    if(mCurIndex >= 0 && mCurIndex < mToggls.Count)
                    {
                        mToggls[mCurIndex].isOn = true;
                    }
                    else
                    {
                        mTargetIndex = mCurIndex;
                    }
                }
            }
            if (CallbackOnSet)
            {
                noticeValueChanged();
            }
        }

        private void onToggleOn(Toggle toggle, bool value)
        {
            if(value)
            {
                int index = mToggls.IndexOf(toggle);
                if(mCurIndex != index && index >=0 && index < mToggls.Count)
                {
                    mCurIndex = index;
                    //Debug.Log("onToggleOn curIndex:" + index);
                    noticeValueChanged();
                }
            }
        }

        private void noticeValueChanged()
        {
            if(null != mOnValueChange)
            {
                mOnValueChange(mCurIndex);
            }

            if (null != mOnValueChangeLua)
            {
                mOnValueChangeLua.Call(mCurIndex);
            }
        }
        #endregion 私有方法

        #region editor 测试方法
#if UNITY_EDITOR
        private IEnumerator test()
        {
            
            yield return new WaitForSeconds(4);
            if (mToggls.Count == 0)
            {
                SetTotalNum(5);
                yield return new WaitForSeconds(4);
                SetCurIndex(3);
                
                yield return new WaitForSeconds(4);
                SetTotalNum(8);
                EnableTouch = false;
                SetCurIndex(1);
            }
        }
#endif
        #endregion editor 测试方法
    }
}