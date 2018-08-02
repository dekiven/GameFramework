using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UObj = UnityEngine.Object;

namespace GameFramework
{
    public class GameUIManager : SingletonComp<GameUIManager>
    {
        private Canvas[] mCanvas;
        private Stack<UIBase> mUIViews;
        //private Dictionary<string, UIView> mSingleViewDic;
        //private ObjPool<GameObject> mObjPool;

        private EventSystem mEventSystem;
        private GameResHandler<GameObject> mPrefabs;

        public bool HasInit { get { return GetCanvasByMode(RenderMode.ScreenSpaceOverlay).enabled; } }

        #region MonoBehaviour
        void Awake()
        {
            mPrefabs = new GameResHandler<GameObject>("UI");
            mPrefabs.OnLoadCallbcak = onLoadPrefab;
            mPrefabs.Suffix = ".prefab";

            mCanvas = new Canvas[Enum.GetValues(typeof(RenderMode)).Length];
            mUIViews = new Stack<UIBase>();
            //mSingleViewDic = new Dictionary<string, UIView>();

            foreach (RenderMode r in Enum.GetValues(typeof(RenderMode)))
            {
                if (r == RenderMode.ScreenSpaceCamera)
                {
                    //TODO: ScreenSpaceCamera 类型必须添加camera，否则相当于一个ScreenSpaceOverlay类型的Canvas
                    continue;
                }
                GameObject obj = new GameObject();
                obj.name = "UICanvas" + (int)r;
                Canvas c = obj.AddComponent<Canvas>();
                c.renderMode = r;
                SetCanvasByMode(c);
                obj.transform.SetParent(transform);
            }

            //给UI管理器添加EventSyetem
            GameObject eventObj = new GameObject();
            eventObj.transform.SetParent(transform);
            eventObj.name = "EventSystem";
            mEventSystem = eventObj.AddComponent<EventSystem>();
            eventObj.AddComponent<StandaloneInputModule>();
            eventObj.AddComponent<BaseInput>();
        }
        #endregion

        #region Canvas 相关
        public Canvas GetCanvasByMode(RenderMode mode)
        {
            Canvas c = mCanvas[(int)mode];
            if (c == null)
            {
                c = gameObject.AddComponent<Canvas>();
                c.renderMode = mode;
            }
            return c;
        }

        public bool SetCanvasByMode(Canvas canvas)
        {
            if (null == canvas)
            {
                return false;
            }
            RenderMode mode = canvas.renderMode;
            //if (mCanvas[(int)mode] == null)
            //{
            mCanvas[(int)mode] = canvas;
            return true;
            //}
            //else
            //{
            //    //该类型的Canvas已经存在
            //}
            //return false;
        }
        #endregion

        public void ShowView(string asbName, string prefab, bool isWorldView)
        {
            if(!HasInit)
            {
                LogFile.Warn("GameUIManager尚未初始化");
                return;
            }
            GameObject obj = getPrefab(asbName, prefab);
            if(null != obj)
            {
                showView(obj, isWorldView);
            }
            else
            {
                load(asbName, prefab, isWorldView);
            }
        }

        public void CloseView(UIBase view)
        {
            if(view.IsInStack)
            {
                popView(view as UIView);
            }
            else
            {
                removeUIObj(view);    
            }
        }

        public void CloseCurView()
        {
            
        }

        public void ClearAllUI()
        {
            ClearUIByType(RenderMode.ScreenSpaceOverlay);
            ClearUIByType(RenderMode.WorldSpace);
        }

        public void ClearUIByType(RenderMode type)
        {
            Canvas c = GetCanvasByMode(type);
            for (int i = 0; i < c.transform.childCount; ++i)
            {
                GameObject child = c.transform.GetChild(i).gameObject;
                Destroy(child);
            }
        }

        #region 私有方法

        private void showView(GameObject prefab, bool isWorldView)
        {
            if(null != prefab)
            {
                GameObject uiObj = Instantiate(prefab);
                UIBase ui = uiObj.GetComponent<UIBase>();
                addUIObj(ui);
                if(ui.IsInStack)
                {
                    if(ui.HideBefor && mUIViews.Count > 0)
                    {
                        UIBase curView = mUIViews.Peek();
                        if(curView.isActiveAndEnabled)
                        {
                            curView.Hide((bool ret) =>
                            {
                                pushUI(ui as UIView);
                                ui.Show(null);
                            });
                            return;
                        }
                    }
                    //之前的UI隐藏或者本UI被设置为不隐藏之前的UI则不隐藏之前的UI直接push
                    {
                        pushUI(ui as UIView);
                        ui.Show(null);
                    }
                }
                else
                {
                    ui.Show(null);
                }
            }
        }

        private void load(string asbName, string prefab, bool isWorldView)
        {
            mPrefabs.Load(asbName, prefab, isWorldView ? "y" : "n");
        }

        bool addUIObj(UIBase obj)
        {
            if (null != obj)
            {
                RenderMode mode = obj.GetUIMode();
                Canvas c = GetCanvasByMode(mode);
                //if (null != c && null == obj.transform.parent)
                if (null != c)
                {
                    obj.transform.SetParent(c.transform);
                    return true;
                }
            }
            return false;
        }

        bool removeUIObj(UIBase obj)
        {
            if (null != obj)
            {
                Destroy(obj.gameObject);
                return true;
            }
            return false;
        }

        bool pushUI(UIView view)
        {
            if (null != view)
            {
                mUIViews.Push(view);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 关闭某一个View界面，如果有子界面会先关闭子界面
        /// </summary>
        /// <param name="view">UI界面</param>
        /// <returns>操作是否成功</returns>
        bool popView(UIView view)
        {
            //TODO:
            if (null != view && mUIViews.Contains(view))
            {

                UIBase v = mUIViews.Pop();
                if (null != v)
                {
                    v.Hide((bool hide) =>
                    {
                        bool ret = Equals(v, view);
                        removeUIObj(v);
                        if (!ret)
                        {
                            popView(view);
                        }
                        else
                        {
                            if (mUIViews.Count > 0)
                            {
                                UIBase cur = mUIViews.Peek();
                                if (null != cur)
                                {
                                    cur.Show(null);
                                }
                            }
                        }
                    });

                }
                else
                {
                    return false;
                }

                return true;
            }
            return false;
        }

        bool popView(string viewID)
        {
            //TODO:
            if (!string.IsNullOrEmpty(viewID))
            {
                bool ret = false;
                while (!ret)
                {
                    UIBase v = mUIViews.Pop();
                    if (null != v)
                    {
                        ret = Equals(v.transform.name, viewID);
                        removeUIObj(v);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        private void onLoadPrefab(GameObject prefab, AsbInfo info)
        {
            //TODO:test
            showView(prefab, false);
        }

        private GameObject getPrefab(string asbName, string resName)
        {
            return mPrefabs.Get(asbName, resName);
        }
        #endregion



        public void SetCurGroup(string group)
        {
            mPrefabs.CurGroup = group;
        }

        public void ClearGroup(string group)
        {
            mPrefabs.ClearGroup(group);
        }
    }
}