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
        public const string UIDir = "res/UI";

        private Canvas[] mCanvas;
        private Stack<UIBase> mUIViews;
        //private Dictionary<string, UIView> mSingleViewDic;
        //private ObjPool<GameObject> mObjPool;

        private EventSystem mEventSystem;

        void Start()
        {

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

        public string GetUIAsbStr(string name)
        {
            //if(!name.StartsWith(preffix) && !string.IsNullOrEmpty(name))
            //if (!name.StartsWith(UIDir))
            //{
            name = Tools.PathCombine(UIDir, name);
            //}
            return name;
        }

        public UIView OpenView(string viewId)
        {
            GameResManager.Instance.LoadRes<GameObject>(GetUIAsbStr(viewId), viewId + ".prefab", delegate (UObj obj)
            {
                GameObject prefab = obj as GameObject;

                if (null != prefab)
                {
                    UIView view = Instantiate(prefab).GetComponent<UIView>();
                    if (view != null)
                    {
                        view.transform.name = viewId;
                        pushUI(view);
                    }
                    //return ui;
                }
            });
            return null;
        }

        public bool CloseView(UIView view)
        {
            return popView(view);
        }

        public bool CloseViewByID(string viewID)
        {
            return popView(viewID);
        }

        public bool CloseCurView()
        {
            UIBase v = mUIViews.Pop();
            if (null != v)
            {
                removeUIObj(v);
                return true;
            }
            return false;
        }

        public UIWorld NewWorldUI(string viewId)
        {
            //TODO:待优化，加载路径等
            GameResManager.Instance.LoadRes<GameObject>(GetUIAsbStr(viewId), viewId + ".prefab", delegate (UObj obj)
            {
                GameObject prefab = obj as GameObject;

                if (null != prefab)
                {
                    UIWorld ui = Instantiate(prefab).GetComponent<UIWorld>();
                    if (ui != null)
                    {
                        addUIObj(ui);
                    }
                    //return ui;
                }
            });

            return null;
        }

        public bool ClearAllUI()
        {
            return ClearUIByType(RenderMode.ScreenSpaceOverlay) && ClearUIByType(RenderMode.WorldSpace);
        }

        public bool ClearUIByType(RenderMode type)
        {
            Canvas c = GetCanvasByMode(type);
            for (int i = 0; i < c.transform.childCount; ++i)
            {
                GameObject child = c.transform.GetChild(i).gameObject;
                Destroy(child);
            }
            return true;
        }

        #region 私有方法
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
            //TODO:
            if (null != view)
            {
                mUIViews.Push(view);
                addUIObj(view);
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
                bool ret = false;
                while (!ret)
                {
                    UIBase v = mUIViews.Pop();
                    if (null != v)
                    {
                        ret = Equals(v, view);
                        removeUIObj(v);
                    }
                    else
                    {
                        return false;
                    }
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
                    if(null != v)
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

        #endregion
    }
}