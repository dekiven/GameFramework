using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LuaInterface;
using UObj = UnityEngine.Object;

namespace GameFramework
{
    public class GameUIManager : SingletonComp<GameUIManager>
    {
        private Canvas[] mCanvas;
        private Stack<UIBase> mUIViews;

        private EventSystem mEventSystem;
        private GameResHandler<GameObject> mPrefabs;
        private Image mDarkMask;
        private Dictionary<string, LuaTable> mViewListeners;

        public bool HasInit { get { return GetCanvasByMode(RenderMode.ScreenSpaceOverlay).enabled; } }

        #region MonoBehaviour
        void Awake()
        {

            mViewListeners = new Dictionary<string, LuaTable>();
            mPrefabs = new GameResHandler<GameObject>("UI");
            mPrefabs.OnLoadCallbcak = onLoadPrefab;
            mPrefabs.Suffix = ".prefab";

            mCanvas = new Canvas[Enum.GetValues(typeof(RenderMode)).Length];
            mUIViews = new Stack<UIBase>();

            foreach (RenderMode r in Enum.GetValues(typeof(RenderMode)))
            {
                //if (r == RenderMode.ScreenSpaceCamera)
                //{
                //    //TODO: ScreenSpaceCamera 类型必须添加camera，否则相当于一个ScreenSpaceOverlay类型的Canvas
                //    continue;
                //}
                GetCanvasByMode(r);
            }

            //给UI管理器添加EventSyetem
            GameObject eventObj = new GameObject();
            eventObj.transform.SetParent(transform);
            eventObj.name = "EventSystem";
            mEventSystem = eventObj.AddComponent<EventSystem>();
            Debug.Log(mEventSystem.enabled);
            eventObj.AddComponent<StandaloneInputModule>();
        }
        #endregion

        #region Canvas 相关
        public Canvas GetCanvasByMode(RenderMode mode)
        {
            Canvas c = mCanvas[(int)mode];
            if (c == null)
            {
                GameObject obj = new GameObject();
                c = obj.AddComponent<Canvas>();
                obj.name = "Canvas_s" + mode.ToString();
                obj.AddComponent<CanvasScaler>();
                obj.AddComponent<GraphicRaycaster>();
                obj.transform.SetParent(transform);
                c.renderMode = mode;
                if(RenderMode.ScreenSpaceOverlay == mode)
                {
                    mDarkMask = getDarkMask(c);
                }
                if (RenderMode.ScreenSpaceCamera == mode)
                {
                    GameObject cameraObj = new GameObject();
                    Camera _camera = cameraObj.AddComponent<Camera>();
                    cameraObj.transform.SetParent(obj.transform);
                    c.worldCamera = _camera;
                }
                SetCanvasByMode(c);
                //mCanvas[(int)mode] = c;
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
            Debug.LogWarning("mode" + mode + ", int:" + (int)mode);
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

        public void ShowView(string asbName, string prefab, LuaTable listeners=null)
        {
            if(!HasInit)
            {
                LogFile.Warn("GameUIManager尚未初始化");
                return;
            }
            GameObject obj = getPrefab(asbName, prefab);
            if(null != obj)
            {
                showView(obj, listeners);
            }
            else
            {
                load(asbName, prefab, listeners);
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

        public void PopView()
        {
            UIView view = null;
            if (mUIViews.Count > 0)
            {
                view = mUIViews.Peek() as UIView;
                popView(view);
            }
        }

        public void ClearAllUI()
        {
            ClearUIByType(RenderMode.ScreenSpaceOverlay);
            //ClearUIByType(RenderMode.ScreenSpaceCamera);
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
            if(RenderMode.ScreenSpaceOverlay == type)
            {
                mUIViews.Clear();
            }
        }

        #region 私有方法

        private void showView(GameObject prefab, LuaTable luaTable = null)
        {
            if(null != prefab)
            {
                GameObject uiObj = Instantiate(prefab);
                UIBase ui = uiObj.GetComponent<UIBase>();
                if(null != luaTable)
                {
                    ui.SetLuaStatusListeners(luaTable);
                }
                addUIObj(ui);
                //要显示UI先SetActive(true)，防止有UIprefab中没有启用，不会进入Start方法
                uiObj.SetActive(true);
                //UI初始化后才Show（播放显示动画）
                ui.OnInitCallbcak = (bool hasInit) =>
                {
                    if (ui.IsInStack)
                    {
                        if (ui.HideBefor && mUIViews.Count > 0)
                        {
                            UIBase curView = mUIViews.Peek();
                            if (curView.isActiveAndEnabled)
                            {
                                hideUI(curView, (bool ret) =>
                                {
                                    pushUI(ui as UIView);
                                    showUI(ui, null);
                                });
                                return;
                            }
                        }
                        //之前的UI隐藏或者本UI被设置为不隐藏之前的UI则不隐藏之前的UI直接push
                        {
                            pushUI(ui as UIView);
                            showUI(ui, null);
                        }
                    }
                    else
                    {
                        showUI(ui, null);
                    }
                };
            }
        }

        private void load(string asbName, string prefab, LuaTable table=null)
        {
            if(null != table)
            {
                string extral = asbName + "_" + prefab + Time.time;
                mViewListeners[extral] = table;
                mPrefabs.Load(asbName, prefab, extral);
            }
            else
            {
                mPrefabs.Load(asbName, prefab);
            }
        }

        bool addUIObj(UIBase obj)
        {
            if (null != obj)
            {
                RenderMode mode = obj.GetUIMode();
                Canvas c = GetCanvasByMode(mode);
                if (null != c)
                {
                    //worldPositionStays = false 修复UI加载到Canvas时错位等问题
                    //worldPositionStays = false 表示setparent时保持自己的local属性（pos、scale等）不变
                    obj.transform.SetParent(c.transform, false);
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
            if (null != view && mUIViews.Contains(view))
            {
                UIBase v = mUIViews.Pop();
                if (null != v)
                {
                    hideUI(v, (bool hide) =>
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
                                    showUI(cur, null);
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

        private void showUI(UIBase view, UIBase.UIAnimResult result)
        {
            if(!view.gameObject.activeSelf)
            {
                view.Show(result);
            }
            else
            {
                if(null != result)
                {
                    result(false);
                }
            }
            if(view.HasDarkMask)
            {
                setMaskVisble(true);
                setMaskOrderByView(view);
            }
        }

        private void hideUI(UIBase view, UIBase.UIAnimResult result)
        {
            
            view.Hide((bool rst) =>
            {
                if (view.HasDarkMask)
                {
                    setMaskVisble(false);
                }
                if(null != result)
                {
                    result(rst);
                }
            });
        }

        bool popView(string viewID)
        {
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
            LuaTable table = null;
            if(mViewListeners.TryGetValue(info.extral, out table))
            {
                mViewListeners.Remove(info.extral);
            }
            showView(prefab, table);
        }

        private GameObject getPrefab(string asbName, string resName)
        {
            return mPrefabs.Get(asbName, resName);
        }

        private Image getDarkMask(Canvas c)
        {
            GameObject maskObj = new GameObject();
            maskObj.name = "DrakMask";
            maskObj.transform.SetParent(c.transform);
            RectTransform rect = maskObj.AddComponent<RectTransform>();
            rect.anchorMax = Vector2.one;
            rect.anchorMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            //rect.offsetMin = Vector2.zero;
            maskObj.AddComponent<CanvasRenderer>();
            //maskObj.AddComponent<UIView>();
            Image darkMask = maskObj.AddComponent<Image>();
            darkMask.color = new Color(0f, 0f, 0f, 0.5f);
            darkMask.gameObject.SetActive(false);

            return darkMask;
        }

        private void setMaskOrderByView(UIBase view)
        {
            mDarkMask.rectTransform.SetSiblingIndex(view.transform.GetSiblingIndex() - 1);
        }

        private void setMaskVisble(bool showMask)
        {
            mDarkMask.gameObject.SetActive(showMask);
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