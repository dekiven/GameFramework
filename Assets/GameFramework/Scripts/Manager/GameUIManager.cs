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
        public const int MaxSortOrder = 9999;
        private Canvas[] mCanvas;
        private Stack<UIBase> mStackViews;
        private List<UIBase> mStaticViews;
        private List<AsbInfo> mStaticViewInfos;

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

            //mCanvas = new Canvas[Enum.GetValues(typeof(RenderMode)).Length];
            mCanvas = new Canvas[3];
            mStackViews = new Stack<UIBase>();
            mStaticViews = new List<UIBase>();
            mStaticViewInfos = new List<AsbInfo>();

            //foreach (RenderMode r in Enum.GetValues(typeof(RenderMode)))
            {
                GetCanvasByMode(RenderMode.WorldSpace);
                //TODO:暂时不使用ScreenSpaceCamera模式，没有需求
                //GetCanvasByMode(RenderMode.ScreenSpaceCamera);
                GetCanvasByMode(RenderMode.ScreenSpaceOverlay);
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
        public Canvas GetCanvasByMode(RenderMode mode, bool createIsNot=true)
        {
            Canvas c = mCanvas[(int)mode];
            if (c == null && createIsNot)
            {
                GameObject obj = new GameObject();
                c = obj.AddComponent<Canvas>();
                c.sortingOrder = MaxSortOrder+1;
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
            //Debug.LogWarning("mode" + mode + ", int:" + (int)mode);
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
            //检查是否是已经缓存的static view，有缓存直接显示
            if(showStaticView(asbName, prefab))
            {
                return;
            }
            GameObject obj = getPrefab(asbName, prefab);
            if(null != obj)
            {
                ShowViewPrefab(obj, listeners);
            }
            else
            {
                load(asbName, prefab, listeners);
            }
        }

        public void ShowViewObj(UIBase view, UIBase.UIAnimResult result)
        {
            if (!view.gameObject.activeSelf)
            {
                view.ShowAnim(result);
            }
            else
            {
                if (null != result)
                {
                    result(false);
                }
            }
            if (view.HasDarkMask)
            {
                setMaskVisble(true);
                setMaskOrderByView(view);
            }
        }

        public void HideView(UIBase view, UIBase.UIAnimResult result)
        {

            view.HideAnim((bool rst) =>
            {
                if (view.HasDarkMask)
                {
                    setMaskVisble(false);
                }
                if (null != result)
                {
                    result(rst);
                }
            });
        }

        public void CloseView(UIBase view)
        {
            if(view.IsInStack)
            {
                popView(view as UIView);
            }
            else
            {
                removeDynamicUIObj(view);    
            }
        }

        public void PopView()
        {
            UIView view = null;
            if (mStackViews.Count > 0)
            {
                view = mStackViews.Peek() as UIView;
                popView(view);
            }
        }

        /// <summary>
        /// 从prefab显示UI，慎用，建议使用ShowView
        /// </summary>
        /// <param name="prefab">Prefab.</param>
        /// <param name="luaTable">Lua table.</param>
        /// <param name="asbName">Asb name.</param>
        /// <param name="prefabName">Prefab name.</param>
        public void ShowViewPrefab(GameObject prefab, LuaTable luaTable = null, string asbName = null, string prefabName = null)
        {
            if (null != prefab)
            {
                GameObject uiObj = Instantiate(prefab);
                UIBase ui = uiObj.GetComponent<UIBase>();
                if (null != luaTable)
                {
                    ui.SetLuaStatusListeners(luaTable);
                }
                addUIObj(ui);
                if (ui.IsStatic)
                {
                    if (mStaticViewInfos.Count == mStaticViews.Count)
                    {
                        mStaticViewInfos.Add(new AsbInfo(asbName, prefabName));
                        mStaticViews.Add(ui);
                    }
                    else
                    {
                        LogFile.Error("GameUIManager error ==> showViewPrefab mStaticViewInfos.Count != mStaticViews.Count");
                    }
                }
                //要显示UI先SetActive(true)，防止有UIprefab中没有启用，不会进入Start方法
                uiObj.SetActive(true);
                //UI初始化后才Show（播放显示动画）
                ui.OnInitCallbcak = (bool hasInit) =>
                {
                    if (ui.IsInStack)
                    {
                        if (ui.HideBefor && mStackViews.Count > 0)
                        {
                            UIBase curView = mStackViews.Peek();
                            if (curView.isActiveAndEnabled)
                            {
                                HideView(curView, (bool ret) =>
                                {
                                    pushUI(ui as UIView);
                                    ShowViewObj(ui, null);
                                });
                                return;
                            }
                        }
                        //之前的UI隐藏或者本UI被设置为不隐藏之前的UI则不隐藏之前的UI直接push
                        {
                            pushUI(ui as UIView);
                            ShowViewObj(ui, null);
                        }
                    }
                    else
                    {
                        ShowViewObj(ui, null);
                    }
                };
            }
        }

        public void ClearAllUI()
        {
            ClearUIByType(RenderMode.ScreenSpaceOverlay);
            ClearUIByType(RenderMode.ScreenSpaceCamera);
            ClearUIByType(RenderMode.WorldSpace);
        }

        public void ClearUIByType(RenderMode type)
        {
            Canvas c = GetCanvasByMode(type, false);
            if(null != c)
            {
                for (int i = 0; i < c.transform.childCount; ++i)
                {
                    GameObject child = c.transform.GetChild(i).gameObject;
                    Destroy(child);
                }
                if (RenderMode.ScreenSpaceOverlay == type)
                {
                    mStackViews.Clear();
                    //TODO:针对static的View处理
                    mStaticViews.Clear();
                    mStaticViewInfos.Clear();
                }
            }
        }

        public override bool Dispose()
        {
            ClearAllUI();
            mStackViews.Clear();
            mPrefabs.Dispose();
            return base.Dispose();
        }

        #region 私有方法

        /// <summary>
        /// static View如果已经显示过会有记录，直接显示
        /// </summary>
        /// <returns><c>true</c>, if static view was shown, <c>false</c> 之前没有显示过.</returns>
        /// <param name="asbName">Asb name.</param>
        /// <param name="prefab">Prefab.</param>
        private bool showStaticView(string asbName, string prefab)
        {
            for (int i = 0; i < mStaticViewInfos.Count; i++)
            {
                if(mStaticViewInfos[i].Equals(asbName, prefab))
                {
                    UIBase ui = mStaticViews[i];
                    if(null != ui)
                    {
                        setViewTopOfAll(ui);
                        ShowViewObj(ui, null);
                        return true;
                    }
                    break;
                }
            }
            return false;
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
                RenderMode mode = obj.RenderMode;
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

        /// <summary>
        /// 删除不是静态的UI
        /// </summary>
        /// <returns><c>true</c>, if dynamic UIO bj was removed, <c>false</c> otherwise.</returns>
        /// <param name="obj">Object.</param>
        bool removeDynamicUIObj(UIBase obj)
        {
            if (null != obj)
            {
                if (!obj.IsStatic)
                {
                    removeUIObj(obj);
                }
                return true;
            }
            return false;
        }

        bool removeUIObj(UIBase obj)
        {
            if (null != obj)
            {
                if(obj.IsStatic)
                {
                    if(mStaticViews.Contains(obj))
                    {
                        int index = mStaticViews.IndexOf(obj);
                        mStaticViews.RemoveAt(index);
                        mStaticViewInfos.RemoveAt(index);
                    }
                }
                Destroy(obj.gameObject);
                return true;
            }
            return false;
        }

        bool pushUI(UIView view)
        {
            if (null != view)
            {
                mStackViews.Push(view);
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
            if (null != view && mStackViews.Contains(view))
            {
                UIBase v = mStackViews.Pop();
                if (null != v)
                {
                    HideView(v, (bool hide) =>
                    {
                        bool ret = Equals(v, view);
                        removeDynamicUIObj(v);
                        if (!ret)
                        {
                            popView(view);
                        }
                        else
                        {
                            if (mStackViews.Count > 0)
                            {
                                UIBase cur = mStackViews.Peek();
                                if (null != cur)
                                {
                                    ShowViewObj(cur, null);
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
            if (!string.IsNullOrEmpty(viewID))
            {
                bool ret = false;
                while (!ret)
                {
                    UIBase v = mStackViews.Pop();
                    if (null != v)
                    {
                        ret = Equals(v.transform.name, viewID);
                        removeDynamicUIObj(v);
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
            if(!string.IsNullOrEmpty(info.extral) && mViewListeners.TryGetValue(info.extral, out table))
            {
                mViewListeners.Remove(info.extral);
            }
            ShowViewPrefab(prefab, table);
        }

        private GameObject getPrefab(string asbName, string resName)
        {
            return mPrefabs.Get(asbName, resName);
        }

        private Image getDarkMask(Canvas c)
        {
            GameObject maskObj = new GameObject();
            maskObj.name = "DrakMask";
            maskObj.transform.SetParent(c.transform, false);
            RectTransform rect = maskObj.AddComponent<RectTransform>();
            rect.anchorMax = Vector2.one;
            rect.anchorMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            //rect.offsetMin = Vector2.zero;
            maskObj.AddComponent<CanvasRenderer>();
            //maskObj.AddComponent<UIView>();
            Image darkMask = maskObj.AddComponent<Image>();
            darkMask.color = new Color(0f, 0f, 0f, 0.8f);
            darkMask.gameObject.SetActive(false);

            return darkMask;
        }

        private void setMaskOrderByView(UIBase view)
        {
            //view.transform.SetSiblingIndex(GetCanvasByMode(view.RenderMode).transform.childCount);
            view.transform.SetAsLastSibling();
            int idx = view.transform.GetSiblingIndex() - 1;
            if (idx < 0)
            {
                idx = 0;
            }
            mDarkMask.rectTransform.SetSiblingIndex(idx);
        }

        private void setMaskVisble(bool showMask)
        {
            mDarkMask.gameObject.SetActive(showMask);
        }

        private void setViewTopOfAll(UIBase view)
        {
            view.transform.SetSiblingIndex(view.transform.parent.childCount - 1);
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