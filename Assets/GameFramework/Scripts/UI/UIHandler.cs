﻿using System;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameFramework
{
    using DelScrollItemClicked = Action<int>;
    using DelBtnClickedStr = Action<int, string>;
    using DelBtnClickedIndex = Action<int, int>;
    using DelSelectChange = Action<int[]>;

    /// <summary>
    /// UI组件管理器，管理以下组件：
    /// Text
    /// Image
    /// RawImage
    /// Button
    /// Toggle
    /// Slider
    /// ScrollView
    /// ScrollSelector
    /// Scrollbar
    /// Dropdown
    /// InputField
    /// Canvas
    /// Panel
    /// ScrollView
    /// </summary>
    public class UIHandler : MonoBehaviour
    {

        public List<UIBehaviour> UIArray;
        public List<UIHandler> SubHandlers;
        public List<RectTransform> RTArray;
        public Transform RootTransform;

        private List<string> mUINames;
        private List<string> mSubNames;
        private List<string> mRTNames;

        private List<LuaFunction> mLuaFuns;

        #region MonoBehaviour
        void Awake()
        {
            if (null == RootTransform)
            {
                RootTransform = transform;
            }

            mLuaFuns = new List<LuaFunction>();

            UpdateUI2RootNames();
        }

        void OnDestroy()
        {
            foreach (var func in mLuaFuns)
            {
                if(null != func)
                {
                    func.Dispose();
                }
            }
            mLuaFuns.Clear();

            UIArray.Clear();
            SubHandlers.Clear();
            RTArray.Clear();

            mUINames.Clear();
            mSubNames.Clear();
            mRTNames.Clear();
        }
        #endregion

        #region 通用方法
        public int Count { get { if (null != UIArray) { return UIArray.Count; } else { return 0; } } }
        public int SubCount { get { if (null != SubHandlers) { return SubHandlers.Count; } else { return 0; } } }
        public int RTCount { get { if (null != RTArray) { return RTArray.Count; } else { return 0; } } }

        public void UpdateUI2RootNames()
        {
            getListNames(ref mUINames, ref UIArray);
            getListNames(ref mSubNames, ref SubHandlers);
            getListNames(ref mRTNames, ref RTArray);
        }

        public string[] CompNames { get { return mUINames.ToArray(); } }

        public T GetCompByIndex<T>(int index) where T : Component
        {
            T comp = null;
            if (index < Count)
            {
                comp = UIArray[index] as T;
                if (null != UIArray[index] && null == comp)
                {
                    comp = UIArray[index].GetComponent<T>();
                }
            }
            else
            {
                LogFile.Warn(
                   "{0}找不到index为{1}，且类型是{2}的UI组件。"
                   , Tools.GetTransformName(transform, Camera.main.transform)
                   , index
                   , typeof(T)
               );
            }
            return comp;
        }

        public T GetCompByName<T>(string cName) where T : Component
        {
            int idx = mUINames.IndexOf(cName);
            if (!Equals(-1, idx))
            {
                return GetCompByIndex<T>(idx);
            }
            else
            {
                LogFile.Warn(
                   "{0}找不到name为{1}的UI组件。"
                   , Tools.GetTransformName(transform, Camera.main.transform)
                   , cName
               );
            }
            return null;
        }

        public UIHandler GetSubHandler(int index)
        {
            UIHandler handler = null;
            if (null != SubHandlers && SubCount > 0 && index < SubCount && index >= 0)
            {
                return SubHandlers[index];
            }
            else
            {
                LogFile.Warn("{0}找不到index为{1}的Sub Handler", name, index);
            }
            return handler;
        }

        public UIHandler GetSubHandler(string path)
        {
            UIHandler handler = null;
            if (!string.IsNullOrEmpty(path) && null != mSubNames)
            {
                int index = mSubNames.IndexOf(path);
                if (index < 0)
                {
                    LogFile.Warn("{0}找不到path为{1}的Sub Handler", name, path);
                }
                else
                {
                    handler = GetSubHandler(index);
                }
            }
            return handler;
        }

        public void SetHandlerActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public int GetUIIndex(UIBehaviour ui)
        {
            return UIArray.IndexOf(ui);
        }
        #endregion

        #region ChangeUI
        public bool ChangeUI(UIHandlerData data)
        {
            string uiName = data.UIName;
            int uiIndex = data.UIIndex;

            switch (data.FuncStr.ToLower())
            {
                case "setuiname":
                    if (uiIndex != -1)
                    {
                        return SetUIName(uiIndex, (string)data.Content);
                    }
                    else
                    {
                        return SetUIName(uiName, (string)data.Content);
                    }
                //break;
                case "setuiactive":
                    if (uiIndex != -1)
                    {
                        return SetUIActive(uiIndex, (bool)data.Content);
                    }
                    else
                    {
                        return SetUIActive(uiName, (bool)data.Content);
                    }
                //break;
                case "setuiselectable":
                    if (uiIndex != -1)
                    {
                        return SetUISelectable(uiIndex, (bool)data.Content);
                    }
                    else
                    {
                        return SetUISelectable(uiName, (bool)data.Content);
                    }
                //break;
                case "setuienable":
                    if (uiIndex != -1)
                    {
                        return SetUIEnable(uiIndex, (bool)data.Content);
                    }
                    else
                    {
                        return SetUIEnable(uiName, (bool)data.Content);
                    }
                //break;
                case "setuinativesize":
                    if (uiIndex != -1)
                    {
                        return SetUINativeSize(uiIndex);
                    }
                    else
                    {
                        return SetUINativeSize(uiName);
                    }
                //break;
                case "setuimaterial":
                    if (!string.IsNullOrEmpty(data.Content as string))
                    {
                        if (uiIndex != -1)
                        {
                            return SetUIMaterial(uiIndex, (String)data.Content);
                        }
                        else
                        {
                            return SetUIMaterial(uiName, (String)data.Content);
                        }
                    }
                    Material material = data.Content as Material;
                    if (null == material)
                    {
                        UIHandlerDataAsync ds = data as UIHandlerDataAsync;
                        if (null != ds && null != ds.ContentBefor)
                        {
                            ds.OnAsyncRst = (obj) =>
                            {
                                ChangeUI(ds);
                            };
                            return true;
                        }
                    }
                    if (uiIndex != -1)
                    {
                        return SetUIMaterial(uiIndex, data.Content as Material);
                    }
                    else
                    {
                        return SetUIMaterial(uiName, data.Content as Material);
                    }
                //break;
                case "setuiraycasttarget":
                    if (uiIndex != -1)
                    {
                        return SetUIRaycastTarget(uiIndex, (bool)data.Content);
                    }
                    else
                    {
                        return SetUIRaycastTarget(uiName, (bool)data.Content);
                    }
                //break;
                case "setuicolor":
                    if (uiIndex != -1)
                    {
                        return SetUIColor(uiIndex, (Color)data.Content);
                    }
                    else
                    {
                        return SetUIColor(uiName, (Color)data.Content);
                    }
                //break;
                case "addeventtrigger" :
                    if (uiIndex != -1)
                    {
                        //return SetUIRaycastTarget(uiIndex, (bool)data.Content);
                        LuaTable lua = data.Content as LuaTable;
                        if(null == lua)
                        {
                            return AddEventTrigger(uiIndex, data.Content as List<EventTrigger.Entry>);
                        }
                        else
                        {
                            return AddEventTrigger(uiIndex, lua);
                        }
                    }
                    else
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null == lua)
                        {
                            return AddEventTrigger(uiName, data.Content as List<EventTrigger.Entry>);
                        }
                        else
                        {
                            return AddEventTrigger(uiName, lua);
                        }
                    }
                //break;
                case "removeeventtrigger" :
                    if (uiIndex != -1)
                    {
                        return RemoveEventTrigger(uiIndex);
                    }
                    else
                    {
                        return RemoveEventTrigger(uiName);
                    }
                //break;
                case "settextstring":
                    if (uiIndex != -1)
                    {
                        return SetTextString(uiIndex, (string)data.Content);
                    }
                    else
                    {
                        return SetTextString(uiName, (string)data.Content);
                    }
                //break;
                case "setrichtextstring":
                    if (uiIndex != -1)
                    {
                        return SetRichTextString(uiIndex, (string)data.Content);
                    }
                    else
                    {
                        return SetRichTextString(uiName, (string)data.Content);
                    }
                //break;
                case "setimagesprite":
                    if (!string.IsNullOrEmpty(data.Content as string))
                    {
                        if (uiIndex != -1)
                        {
                            return SetImageSprite(uiIndex, data.Content as string);
                        }
                        else
                        {
                            return SetImageSprite(uiName, data.Content as string);
                        }
                    }
                    Sprite s = data.Content as Sprite;
                    if (null == s)
                    {
                        UIHandlerDataAsync ds = data as UIHandlerDataAsync;
                        if (null != ds && null != ds.ContentBefor)
                        {
                            ds.OnAsyncRst = (obj) =>
                            {
                                ChangeUI(ds);
                            };
                            return true;
                        }
                        //TODO:图片刷新有问题，可能出现白色（null）再切换图片
                    }
                    if (uiIndex != -1)
                    {
                        return SetImageSprite(uiIndex, s);
                    }
                    else
                    {
                        return SetImageSprite(uiName, s);
                    }
                //break;
                case "setimagensizesprite":
                    {
                        if (!string.IsNullOrEmpty(data.Content as string))
                        {
                            if (uiIndex != -1)
                            {
                                return SetImageNSizeSprite(uiIndex, data.Content as string);
                            }
                            else
                            {
                                return SetImageNSizeSprite(uiName, data.Content as string);
                            }
                        }
                        if (null == data.Content as Sprite)
                        {
                            UIHandlerDataAsync ds = data as UIHandlerDataAsync;
                            if (null != ds && null != ds.ContentBefor)
                            {
                                ds.OnAsyncRst = (obj) =>
                                {
                                    ChangeUI(ds);
                                };
                                return true;
                            }
                        }
                        if (uiIndex != -1)
                        {
                            return SetImageNSizeSprite(uiIndex, data.Content as Sprite);
                        }
                        else
                        {
                            return SetImageNSizeSprite(uiName, data.Content as Sprite);
                        }
                    }

                //break;
                case "setrawimagetexture":
                    //TODO:
                    //if (uiIndex != -1)
                    //{
                    //    return SetRawImageTexture(uiIndex, data.Content);
                    //}
                    //else
                    //{
                    //    return SetRawImageTexture(uiName, data.Content);
                    //}
                    break;
                case "setrawimagerect":
                    if (uiIndex != -1)
                    {
                        return SetRawImageRect(uiIndex, (Rect)data.Content);
                    }
                    else
                    {
                        return SetRawImageRect(uiName, (Rect)data.Content);
                    }
                //break;
                case "addbtnclick":
                    if (uiIndex != -1)
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddBtnClick(uiIndex, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddBtnClick(uiIndex, (UnityAction<string>)data.Content);
                        }
                    }
                    else
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddBtnClick(uiName, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddBtnClick(uiName, (UnityAction<string>)data.Content);
                        }
                    }
                //break;
                case "adddropdownonvaluechanged":
                    if (uiIndex != -1)
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddDropdownOnValueChanged(uiIndex, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddDropdownOnValueChanged(uiIndex, (UnityAction<int>)data.Content);
                        }
                    }
                    else
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddDropdownOnValueChanged(uiName, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddDropdownOnValueChanged(uiName, (UnityAction<int>)data.Content);
                        }
                    }
                //break;
                case "setinputfeildstring":
                    if (uiIndex != -1)
                    {
                        return SetInputFeildString(uiIndex, (string)data.Content);
                    }
                    else
                    {
                        return SetInputFeildString(uiName, (string)data.Content);
                    }
                //break;
                case "addinputfieldonvaluechanged":
                    if (uiIndex != -1)
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddInputFieldOnValueChanged(uiIndex, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddInputFieldOnValueChanged(uiIndex, (UnityAction<string>)data.Content);
                        }
                    }
                    else
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddInputFieldOnValueChanged(uiName, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddInputFieldOnValueChanged(uiName, (UnityAction<string>)data.Content);
                        }
                    }
                //break;
                case "addinputfieldonendedit":
                    if (uiIndex != -1)
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddInputFieldOnEndEdit(uiIndex, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddInputFieldOnEndEdit(uiIndex, (UnityAction<string>)data.Content);
                        }
                    }
                    else
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddInputFieldOnEndEdit(uiName, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddInputFieldOnEndEdit(uiName, (UnityAction<string>)data.Content);
                        }
                    }
                //break;
                case "setslidervalue":
                    if (uiIndex != -1)
                    {
                        return SetSliderValue(uiIndex, (float)data.Content);
                    }
                    else
                    {
                        return SetSliderValue(uiName, (float)data.Content);
                    }
                //break;
                case "addonslidervaluechanged":
                    if (uiIndex != -1)
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddOnSliderValueChanged(uiIndex, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddOnSliderValueChanged(uiIndex, (UnityAction<float>)data.Content);
                        }
                    }
                    else
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddOnSliderValueChanged(uiName, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddOnSliderValueChanged(uiName, (UnityAction<float>)data.Content);
                        }
                    }
                //break;
                case "setscrollviewonitemclick":
                    if (uiIndex != -1)
                    {
                        LuaFunction lua = data.Content as LuaFunction;
                        if (null != lua)
                        {
                            return SetScrollViewOnItemClick(uiIndex, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return SetScrollViewOnItemClick(uiIndex, (DelScrollItemClicked)data.Content);
                        }
                    }
                    else
                    {
                        LuaFunction lua = data.Content as LuaFunction;
                        if (null != lua)
                        {
                            return SetScrollViewOnItemClick(uiName, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return SetScrollViewOnItemClick(uiName, (DelScrollItemClicked)data.Content);
                        }
                    }
                //break;
                case "setscrollviewdata":
                    if (uiIndex != -1)
                    {
                        if (null != data.Content as LuaTable)
                        {
                            return SetScrollViewData(uiIndex, data.Content as LuaTable);
                        }
                        else
                        {
                            return SetScrollViewData(uiIndex, (List<UIItemData>)data.Content);
                        }
                    }
                    else
                    {
                        if (null != data.Content as LuaTable)
                        {
                            return SetScrollViewData(uiName, data.Content as LuaTable);
                        }
                        else
                        {
                            return SetScrollViewData(uiName, (List<UIItemData>)data.Content);
                        }
                    }
                //break;
                case "updatescrollviewdata":
                    if (uiIndex != -1)
                    {
                        return UpdateScrollViewData(uiIndex, data.Content as LuaTable);
                    }
                    else
                    {
                        return UpdateScrollViewData(uiName, data.Content as LuaTable);
                    }
                //break;
                case "addscrollviewdata":
                    if (uiIndex != -1)
                    {
                        if (null != data.Content as LuaTable)
                        {
                            return AddScrollViewData(uiIndex, data.Content as LuaTable);
                        }
                        else
                        {
                            return AddScrollViewData(uiIndex, (UIItemData)data.Content);
                        }
                    }
                    else
                    {
                        if (null != data.Content as LuaTable)
                        {
                            return AddScrollViewData(uiName, data.Content as LuaTable);
                        }
                        else
                        {
                            return AddScrollViewData(uiName, (UIItemData)data.Content);
                        }
                    }
                //break;
                case "insertscrollviewdata":
                    if (uiIndex != -1)
                    {
                        return InsertScrollViewData(uiIndex, data.Content as LuaTable);
                    }
                    else
                    {
                        return InsertScrollViewData(uiName, data.Content as LuaTable);
                    }
                //break;
                case "setscrollviewbtnclick_s":
                    if (uiIndex != -1)
                    {
                        LuaFunction lua = data.Content as LuaFunction;
                        if (null != lua)
                        {
                            return SetScrollViewBtnClick_S(uiIndex, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return SetScrollViewBtnClick_S(uiIndex, (DelBtnClickedStr)data.Content);
                        }
                    }
                    else
                    {
                        LuaFunction lua = data.Content as LuaFunction;
                        if (null != lua)
                        {
                            return SetScrollViewBtnClick_S(uiName, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return SetScrollViewBtnClick_S(uiName, (DelBtnClickedStr)data.Content);
                        }
                    }
                // break;
                case "setscrollviewbtnclick_i":
                    if (uiIndex != -1)
                    {
                        LuaFunction lua = data.Content as LuaFunction;
                        if (null != lua)
                        {
                            return SetScrollViewBtnClick_I(uiIndex, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return SetScrollViewBtnClick_I(uiIndex, (DelBtnClickedIndex)data.Content);
                        }
                    }
                    else
                    {
                        LuaFunction lua = data.Content as LuaFunction;
                        if (null != lua)
                        {
                            return SetScrollViewBtnClick_I(uiName, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return SetScrollViewBtnClick_I(uiName, (DelBtnClickedIndex)data.Content);
                        }
                    }
                // break;
                case "removescrollviewdata":
                    if (uiIndex != -1)
                    {
                        return RemoveScrollViewData(uiIndex, (UIItemData)data.Content);
                    }
                    else
                    {
                        return RemoveScrollViewData(uiName, (UIItemData)data.Content);
                    }
                //break;
                case "removescrollviewdataat":
                    if (uiIndex != -1)
                    {
                        return RemoveScrollViewDataAt(uiIndex, (int)data.Content);
                    }
                    else
                    {
                        return RemoveScrollViewDataAt(uiName, (int)data.Content);
                    }
                //break;
                case "addscrollviewdatas":
                    if (uiIndex != -1)
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return AddScrollViewDatas(uiIndex, (List<UIItemData>)data.Content);
                        }
                        else
                        {
                            return AddScrollViewDatas(uiIndex, (LuaTable)data.Content);
                        }
                    }
                    else
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return AddScrollViewDatas(uiName, (List<UIItemData>)data.Content);
                        }
                        else
                        {
                            return AddScrollViewDatas(uiName, (LuaTable)data.Content);
                        }
                    }
                //break;
                case "removescrollviewdatasat":
                    if (uiIndex != -1)
                    {
                        string str = data.Content as string;
                        if (null != str)
                        {
                            return RemoveScrollViewDatasAt(uiIndex, (int[])data.Content);
                        }
                        else
                        {
                            return RemoveScrollViewDatasAt(uiIndex, str);
                        }
                    }
                    else
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return RemoveScrollViewDatasAt(uiName, (int[])data.Content);
                        }
                        else
                        {
                            return RemoveScrollViewDatasAt(uiName, (string)data.Content);
                        }
                    }
                //break;
                case "tween2scrollviewindex":
                    if (uiIndex != -1)
                    {
                        return Tween2ScrollViewIndex(uiIndex, (int)data.Content);
                    }
                    else
                    {
                        return Tween2ScrollViewIndex(uiName, (int)data.Content);
                    }
                //break;
                case "tween2scrollviewpos":
                    if (uiIndex != -1)
                    {
                        return Tween2ScrollViewPos(uiIndex, (float)data.Content);
                    }
                    else
                    {
                        return Tween2ScrollViewPos(uiName, (float)data.Content);
                    }
                //break;
                case "selectscrollviewitem":
                    if (uiIndex != -1)
                    {
                        return SelectScrollViewItem(uiIndex, (int)data.Content);
                    }
                    else
                    {
                        return SelectScrollViewItem(uiName, (int)data.Content);
                    }
                //break;
                case "selectscrollviewselectitems":
                    if (uiIndex != -1)
                    {
                        string lua = data.Content as string;
                        if (null != lua)
                        {
                            return SelectScrollViewItems(uiIndex, (int[])data.Content);
                        }
                        else
                        {
                            return SelectScrollViewSelectItems(uiIndex, (string)data.Content);
                        }
                    }
                    else
                    {
                        string str = data.Content as string;
                        if (null != str)
                        {
                            return SelectScrollViewItems(uiName, (int[])data.Content);
                        }
                        else
                        {
                            return SelectScrollViewSelectItems(uiName, str);
                        }
                    }
                //break;
                case "unselectscrollviewitem":
                    if (uiIndex != -1)
                    {
                        return UnselectScrollViewItem(uiIndex, (int)data.Content);
                    }
                    else
                    {
                        return UnselectScrollViewItem(uiName, (int)data.Content);
                    }
                //break;
                case "unselectscrollviewitems":
                    if (uiIndex != -1)
                    {
                        string lua = data.Content as string;
                        if (null != lua)
                        {
                            return UnselectScrollViewItems(uiIndex, (int[])data.Content);
                        }
                        else
                        {
                            return UnselectScrollViewItems(uiIndex, (string)data.Content);
                        }
                    }
                    else
                    {
                        string str = data.Content as string;
                        if (null != str)
                        {
                            return UnselectScrollViewItems(uiName, (int[])data.Content);
                        }
                        else
                        {
                            return UnselectScrollViewItems(uiName, str);
                        }
                    }
                //break;
                case "switchscrollviewitem":
                    if (uiIndex != -1)
                    {
                        return SwitchScrollViewItem(uiIndex, (int)data.Content);
                    }
                    else
                    {
                        return SwitchScrollViewItem(uiName, (int)data.Content);
                    }
                //break;
                case "switchscrollviewitems":
                    if (uiIndex != -1)
                    {
                        return SwitchScrollViewItems(uiIndex, (string)data.Content);
                    }
                    else
                    {
                        return SwitchScrollViewItems(uiName, (string)data.Content);
                    }
                //break;
                case "selectscrollviewall":
                    if (uiIndex != -1)
                    {
                        return SelectScrollViewAll(uiIndex);
                    }
                    else
                    {
                        return SelectScrollViewAll(uiName);
                    }
                //break;
                case "unselectscrollviewall":
                    if (uiIndex != -1)
                    {
                        return UnselectScrollViewAll(uiIndex);
                    }
                    else
                    {
                        return UnselectScrollViewAll(uiName);
                    }
                //break;
                case "setscrollviewonselectchange":
                    if (uiIndex != -1)
                    {
                        LuaFunction lua = data.Content as LuaFunction;
                        if (null != lua)
                        {
                            return SetScrollViewOnSelect(uiIndex, (DelSelectChange)data.Content);
                        }
                        else
                        {
                            return SetScrollViewOnSelect(uiIndex, (LuaFunction)data.Content);
                        }
                    }
                    else
                    {
                        LuaFunction lua = data.Content as LuaFunction;
                        if (null != lua)
                        {
                            return SetScrollViewOnSelect(uiName, (DelSelectChange)data.Content);
                        }
                        else
                        {
                            return SetScrollViewOnSelect(uiName, (LuaFunction)data.Content);
                        }
                    }
                //break;



                case "setscrollselectordata":
                    if (uiIndex != -1)
                    {
                        if (null != data.Content as LuaTable)
                        {
                            return SetScrollSelectorData(uiIndex, data.Content as LuaTable);
                        }
                        else
                        {
                            return SetScrollSelectorData(uiIndex, (List<UIItemData>)data.Content);
                        }
                    }
                    else
                    {
                        if (null != data.Content as LuaTable)
                        {
                            return SetScrollSelectorData(uiName, data.Content as LuaTable);
                        }
                        else
                        {
                            return SetScrollSelectorData(uiName, (List<UIItemData>)data.Content);
                        }
                    }
                //break;
                case "setscrollselectorcurindex":
                    if (uiIndex != -1)
                    {
                        return SetScrollSelectorCurIndex(uiIndex, (int)data.Content);
                    }
                    else
                    {
                        return SetScrollSelectorCurIndex(uiName, (int)data.Content);
                    }
                //break;
                case "setscrollselectoronselect":
                    if (uiIndex != -1)
                    {
                        return SetScrollSelectorOnSelect(uiIndex, (LuaFunction)data.Content);
                    }
                    else
                    {
                        return SetScrollSelectorOnSelect(uiName, (LuaFunction)data.Content);
                    }
                //break;
                case "settoggleison":
                    if (uiIndex != -1)
                    {
                        return SetToggleIsOn(uiIndex, (bool)data.Content);
                    }
                    else
                    {
                        return SetToggleIsOn(uiName, (bool)data.Content);
                    }
                //break;
                case "addtoggleonvaluechanged":
                    if (uiIndex != -1)
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddToggleOnValueChanged(uiIndex, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddToggleOnValueChanged(uiIndex, (UnityAction<bool>)data.Content);
                        }
                    }
                    else
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddToggleOnValueChanged(uiName, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddToggleOnValueChanged(uiName, (UnityAction<bool>)data.Content);
                        }
                    }
                //break;
                case "setscrollbarvalue":
                    if (uiIndex != -1)
                    {
                        return SetScrollbarValue(uiIndex, (float)data.Content);
                    }
                    else
                    {
                        return SetScrollbarValue(uiName, (float)data.Content);
                    }
                //break;
                case "addonscrollbarvaluechanged":
                    if (uiIndex != -1)
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddOnScrollbarValueChanged(uiIndex, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddOnScrollbarValueChanged(uiIndex, (UnityAction<float>)data.Content);
                        }
                    }
                    else
                    {
                        if (null != (LuaFunction)data.Content)
                        {
                            return AddOnScrollbarValueChanged(uiName, (LuaFunction)data.Content);
                        }
                        else
                        {
                            return AddOnScrollbarValueChanged(uiName, (UnityAction<float>)data.Content);
                        }
                    }
                //break;
                case "setscrollbarsize":
                    if (uiIndex != -1)
                    {
                        return SetScrollbarSize(uiIndex, (float)data.Content);
                    }
                    else
                    {
                        return SetScrollbarSize(uiName, (float)data.Content);
                    }
                //break;
                case "setscrollbarstepnumber":
                    if (uiIndex != -1)
                    {
                        return SetScrollbarStepNumber(uiIndex, (int)data.Content);
                    }
                    else
                    {
                        return SetScrollbarStepNumber(uiName, (int)data.Content);
                    }
                //break;
                case "setselectortogglescantouch":
                    if (uiIndex != -1)
                    {
                        return SetSelectorTogglesCanTouch(uiIndex, (bool)data.Content);
                    }
                    else
                    {
                        return SetSelectorTogglesCanTouch(uiName, (bool)data.Content);
                    }
                //break;
                case "setselectortogglescallonset":
                    if (uiIndex != -1)
                    {
                        return SetSelectorTogglesCallOnSet(uiIndex, (bool)data.Content);
                    }
                    else
                    {
                        return SetSelectorTogglesCallOnSet(uiName, (bool)data.Content);
                    }
                //break;
                case "setselectortogglesnum":
                    if (uiIndex != -1)
                    {
                        return SetSelectorTogglesNum(uiIndex, (int)data.Content);
                    }
                    else
                    {
                        return SetSelectorTogglesNum(uiName, (int)data.Content);
                    }
                //break;
                case "setselectortogglesdata":
                    if (uiIndex != -1)
                    {
                        if (null != data.Content as LuaTable)
                        {
                            return SetSelectorTogglesData(uiIndex, data.Content as LuaTable);
                        }
                        else
                        {
                            return SetSelectorTogglesData(uiIndex, (List<UIItemData>)data.Content);
                        }
                    }
                    else
                    {
                        if (null != data.Content as LuaTable)
                        {
                            return SetSelectorTogglesData(uiName, data.Content as LuaTable);
                        }
                        else
                        {
                            return SetSelectorTogglesData(uiName, (List<UIItemData>)data.Content);
                        }
                    }
                //break;
                case "setselectortogglesindex":
                    if (uiIndex != -1)
                    {
                        return SetSelectorTogglesIndex(uiIndex, (int)data.Content);
                    }
                    else
                    {
                        return SetSelectorTogglesIndex(uiName, (int)data.Content);
                    }
                //break;
                case "setselectortogglesonchange":
                    if (uiIndex != -1)
                    {
                        if (null != (UnityAction<int>)data.Content)
                        {
                            return SetSelectorTogglesOnChange(uiIndex, (UnityAction<int>)data.Content);
                        }
                        else
                        {
                            return SetSelectorTogglesOnChange(uiIndex, (LuaFunction)data.Content);
                        }
                    }
                    else
                    {
                        if (null != (UnityAction<int>)data.Content)
                        {
                            return SetSelectorTogglesOnChange(uiName, (UnityAction<int>)data.Content);
                        }
                        else
                        {
                            return SetSelectorTogglesOnChange(uiName, (LuaFunction)data.Content);
                        }
                    }
                //break;
                case "modifyrecttransfrom_u":
                    if (uiIndex != -1)
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return ModifyRectTransfrom_U(uiIndex, data.Content as LuaTable);
                        }
                        else
                        {
                            return ModifyRectTransfrom_U(uiName, data.Content as Dictionary<string, System.Object>);
                        }
                    }
                    else
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return ModifyRectTransfrom_U(uiName, data.Content as LuaTable);
                        }
                        else
                        {
                            return ModifyRectTransfrom_U(uiName, data.Content as Dictionary<string, System.Object>);
                        }
                    }
                //break;
                case "modifyrecttransfrom_s":
                    if (uiIndex != -1)
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return ModifyRectTransfrom_S(uiIndex, data.Content as LuaTable);
                        }
                        else
                        {
                            return ModifyRectTransfrom_S(uiIndex, data.Content as Dictionary<string, System.Object>);
                        }
                    }
                    else
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return ModifyRectTransfrom_S(uiName, data.Content as LuaTable);
                        }
                        else
                        {
                            return ModifyRectTransfrom_S(uiName, data.Content as Dictionary<string, System.Object>);
                        }
                    }
                //break;
                case "modifyrecttransfrom_r":
                    if (uiIndex != -1)
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return ModifyRectTransfrom_R(uiIndex, data.Content as LuaTable);
                        }
                        else
                        {
                            return ModifyRectTransfrom_R(uiIndex, data.Content as Dictionary<string, System.Object>);
                        }
                    }
                    else
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return ModifyRectTransfrom_R(uiName, data.Content as LuaTable);
                        }
                        else
                        {
                            return ModifyRectTransfrom_R(uiName, data.Content as Dictionary<string, System.Object>);
                        }
                    }
                //break;
                case "changesubhandleritem":
                    //TODO:在 SetScrollViewData 时使用 ChangeSubHandlerItem 判断null != lua可能会造成类型转换失败，可使用一下的操作
                    //bool isLua = data.Content is LuaTable;
                    if (uiIndex != -1)
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return ChangeSubHandlerItem(uiIndex, data.Content as LuaTable);
                        }
                        else
                        {
                            return ChangeSubHandlerItem(uiIndex, (UIItemData)data.Content);
                        }
                    }
                    else
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return ChangeSubHandlerItem(uiName, data.Content as LuaTable);
                        }
                        else
                        {
                            return ChangeSubHandlerItem(uiName, (UIItemData)data.Content);
                        }
                    }
                //break;
                case "changesubhandlerui":
                    if (uiIndex != -1)
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return ChangeSubHandlerUI(uiIndex, data.Content as LuaTable);
                        }
                        else
                        {
                            return ChangeSubHandlerUI(uiIndex, (UIHandlerData)data.Content);
                        }
                    }
                    else
                    {
                        LuaTable lua = data.Content as LuaTable;
                        if (null != lua)
                        {
                            return ChangeSubHandlerUI(uiName, data.Content as LuaTable);
                        }
                        else
                        {
                            return ChangeSubHandlerUI(uiName, (UIHandlerData)data.Content);
                        }
                    }
                    //break;
                default :
                    LogFile.Warn("ChangeUI找不到注册的函数：" + data.FuncStr);
                    break;
            }
            return false;
        }

        public bool ChangeUI(LuaTable luaTable)
        {
            UIHandlerData data = new UIHandlerData(luaTable);
            return ChangeUI(data);
        }
        #endregion ChangeUI

        public bool ChangeItem(UIItemData data)
        {
            bool ret = true;
            List<UIHandlerData> datas = data.DataList;
            for (int i = 0; i < datas.Count; i++)
            {
                ret &= ChangeUI(datas[i]);
            }
            return ret;
        }

        public bool ChangeItem(LuaTable luaTable)
        {
            UIItemData data = new UIItemData(luaTable);
            return ChangeItem(data);
        }

        #region UI 通用
        public bool SetUIName(int index, string uiName)
        {
            UIBehaviour ui = GetCompByIndex<UIBehaviour>(index);
            return setUIName(ui, uiName);
        }

        public bool SetUIName(string cName, string uiName)
        {
            UIBehaviour ui = GetCompByName<UIBehaviour>(cName);
            return setUIName(ui, uiName);
        }

        public bool SetUIActive(int index, bool value)
        {
            UIBehaviour ui = GetCompByIndex<UIBehaviour>(index);
            return _setUIActive(ui, value);
        }

        public bool SetUIActive(string cName, bool value)
        {
            UIBehaviour ui = GetCompByName<UIBehaviour>(cName);
            return _setUIActive(ui, value);
        }

        /// <summary>
        /// 设置Button、Slider、Dropdown、InputField等UI是否可选择,不能选择将变灰
        /// </summary>
        /// <returns><c>true</c>, if UIS electable was set, <c>false</c> otherwise.</returns>
        /// <param name="index">Index.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        public bool SetUISelectable(int index, bool value)
        {
            Selectable ui = GetCompByIndex<Selectable>(index);
            return _setUISelectable(ui, value);
        }

        /// <summary>
        /// 设置Button、Slider、Dropdown、InputField等UI是否可选择,不能选择将变灰
        /// </summary>
        /// <returns><c>true</c>, if UIS electable was set, <c>false</c> otherwise.</returns>
        /// <param name="cName">Name.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        public bool SetUISelectable(string cName, bool value)
        {
            Selectable ui = GetCompByName<Selectable>(cName);
            return _setUISelectable(ui, value);
        }

        public bool SetUIEnable(int index, bool value)
        {
            UIBehaviour ui = GetCompByIndex<UIBehaviour>(index);
            return _setUIEnable(ui, value);
        }

        public bool SetUIEnable(string cName, bool value)
        {
            UIBehaviour ui = GetCompByName<UIBehaviour>(cName);
            return _setUIEnable(ui, value);
        }

        // 调用Graphic（Image、RawImage）的SetNativeSize方法
        public bool SetUINativeSize(int index)
        {
            Graphic ui = GetCompByIndex<Graphic>(index);
            return _setUINativeSize(ui);
        }

        // 调用Graphic（Image、RawImage）的SetNativeSize方法
        public bool SetUINativeSize(string cName)
        {
            Graphic ui = GetCompByName<Graphic>(cName);
            return _setUINativeSize(ui);
        }

        // 调用Graphic（Image、RawImage）的SetNativeSize方法
        private static bool _setUINativeSize(Graphic ui)
        {
            if (null != ui)
            {
                ui.SetNativeSize();
                return true;
            }
            return false;
        }

        // 设置Graphic子类的material
        public bool SetUIMaterial(int index, Material material)
        {
            Graphic ui = GetCompByIndex<Graphic>(index);
            return _setUIMaterial(ui, material);
        }

        // 设置Graphic子类的material
        public bool SetUIMaterial(string cName, Material material)
        {
            Graphic ui = GetCompByName<Graphic>(cName);
            return _setUIMaterial(ui, material);
        }

        // 设置Graphic子类的material
        private static bool _setUIMaterial(Graphic ui, Material material)
        {
            if (null != ui)
            {
                ui.material = material;
                return true;
            }
            return false;
        }


        // 设置Graphic子类的material
        public bool SetUIMaterial(int index, String material)
        {
            ChangeUI(new UIHandlerDataAsync("setuimaterial", index, material));
            return true;
        }

        // 设置Graphic子类的material
        public bool SetUIMaterial(string cName, String material)
        {
            ChangeUI(new UIHandlerDataAsync("setuimaterial", cName, material));
            return true;
        }


        // 设置Graphic子类的raycastTarget
        public bool SetUIRaycastTarget(int index, bool enabled)
        {
            Graphic ui = GetCompByIndex<Graphic>(index);
            return _setUIRaycastTarget(ui, enabled);
        }

        // 设置Graphic子类的raycastTarget
        public bool SetUIRaycastTarget(string cName, bool enabled)
        {
            Graphic ui = GetCompByName<Graphic>(cName);
            return _setUIRaycastTarget(ui, enabled);
        }

        // 设置Graphic子类的raycastTarget
        private static bool _setUIRaycastTarget(Graphic ui, bool enabled)
        {
            if (null != ui)
            {
                ui.raycastTarget = enabled;
                return true;
            }
            return false;
        }

        public bool SetUIColor(int index, Color color)
        {
            Graphic text = GetCompByIndex<Graphic>(index);
            return _setUIColor(text, color);
        }

        public bool SetUIColor(string cName, Color color)
        {
            Graphic text = GetCompByName<Graphic>(cName);
            return _setUIColor(text, color);
        }

        private static bool _setUIColor(Graphic ui, Color color)
        {
            if (null != ui)
            {
                ui.color = color;
                return true;
            }
            return false;
        }

        public bool AddEventTrigger(int index, List<EventTrigger.Entry> entries)
        {
            UIBehaviour ui = GetCompByIndex<UIBehaviour>(index);
            if (null != ui)
            {
                Tools.AddEventTrigger(ui.gameObject, entries);
            }
            return false;
        }

        public bool AddEventTrigger(string cName, List<EventTrigger.Entry> entries)
        {
            UIBehaviour ui = GetCompByName<UIBehaviour>(cName);
            if (null != ui)
            {
                Tools.AddEventTrigger(ui.gameObject, entries);
            }
            return false;
        }

        public bool AddEventTrigger(int index, LuaTable table)
        {
            UIBehaviour ui = GetCompByIndex<UIBehaviour>(index);
            if(null != ui)
            {
                Tools.AddEventTrigger(ui.gameObject, table);
            }
            return false;
        }

        public bool AddEventTrigger(string cName, LuaTable table)
        {
            UIBehaviour ui = GetCompByName<UIBehaviour>(cName);
            if (null != ui)
            {
                Tools.AddEventTrigger(ui.gameObject, table);
            }
            return false;
        }

        public bool RemoveEventTrigger(int index)
        {
            UIBehaviour ui = GetCompByIndex<UIBehaviour>(index);
            if (null != ui)
            {
                Tools.RemoveEventTrigger(ui.gameObject);
            }
            return false;
        }

        public bool RemoveEventTrigger(string cName)
        {
            UIBehaviour ui = GetCompByName<UIBehaviour>(cName);
            if (null != ui)
            {
                Tools.RemoveEventTrigger(ui.gameObject);
            }
            return false;
        }
        #endregion UI 通用

        #region RectTransform
        public RectTransform GetRectTrans_U(int index)
        {
            UIBehaviour ui = GetCompByIndex<UIBehaviour>(index);
            if (null != ui)
            {
                return ui.transform as RectTransform;
            }
            return null;
        }

        public RectTransform GetRectTrans_U(string cName)
        {
            UIBehaviour ui = GetCompByName<UIBehaviour>(cName);
            if (null != ui)
            {
                return ui.transform as RectTransform;
            }
            return null;
        }

        public RectTransform GetRectTrans_S(int index)
        {
            UIHandler ui = GetSubHandler(index);
            if (null != ui)
            {
                return ui.transform as RectTransform;
            }
            return null;
        }

        public RectTransform GetRectTrans_S(string cName)
        {
            UIHandler ui = GetSubHandler(cName);
            if (null != ui)
            {
                return ui.transform as RectTransform;
            }
            return null;
        }

        public RectTransform GetRectTrans_R(int index)
        {
            return GetRectTransform(index);
        }

        public RectTransform GetRectTrans_R(string cName)
        {
            return GetRectTransform(cName);
        }

        // 修改UIArray中的RectTransform
        public bool ModifyRectTransfrom_U(int index, LuaTable table)
        {
            RectTransform ui = GetRectTrans_U(index);
            return modifyRectTransform(ui, table);
        }

        // 修改UIArray中的RectTransform
        public bool ModifyRectTransfrom_U(string cName, LuaTable table)
        {
            RectTransform ui = GetRectTrans_U(cName);
            return modifyRectTransform(ui, table);
        }

        // 修改SubHandlers中的RectTransform
        public bool ModifyRectTransfrom_S(int index, LuaTable table)
        {
            RectTransform ui = GetRectTrans_S(index);
            return modifyRectTransform(ui, table);
        }

        // 修改SubHandlers中的RectTransform
        public bool ModifyRectTransfrom_S(string cName, LuaTable table)
        {
            RectTransform ui = GetRectTrans_S(cName);
            return modifyRectTransform(ui, table);
        }

        // 修改RTArray中的RectTransform
        public bool ModifyRectTransfrom_R(int index, LuaTable table)
        {
            RectTransform ui = GetRectTrans_R(index);
            return modifyRectTransform(ui, table);
        }

        // 修改RTArray中的RectTransform
        public bool ModifyRectTransfrom_R(string cName, LuaTable table)
        {
            RectTransform ui = GetRectTrans_R(cName);
            return modifyRectTransform(ui, table);
        }


        // 修改UIArray中的RectTransform
        public bool ModifyRectTransfrom_U(int index, Dictionary<string, System.Object> dict)
        {
            RectTransform ui = GetRectTrans_U(index);
            return modifyRectTransform(ui, dict);
        }

        // 修改UIArray中的RectTransform
        public bool ModifyRectTransfrom_U(string cName, Dictionary<string, System.Object> dict)
        {
            RectTransform ui = GetRectTrans_U(cName);
            return modifyRectTransform(ui, dict);
        }


        // 修改SubHandlers中的RectTransform
        public bool ModifyRectTransfrom_S(int index, Dictionary<string, System.Object> dict)
        {
            RectTransform ui = GetRectTrans_S(index);
            return modifyRectTransform(ui, dict);
        }

        // 修改SubHandlers中的RectTransform
        public bool ModifyRectTransfrom_S(string cName, Dictionary<string, System.Object> dict)
        {
            RectTransform ui = GetRectTrans_S(cName);
            return modifyRectTransform(ui, dict);
        }

        // 修改RTArray中的RectTransform
        public bool ModifyRectTransfrom_R(int index, Dictionary<string, System.Object> dict)
        {
            RectTransform ui = GetRectTrans_R(index);
            return modifyRectTransform(ui, dict);
        }

        // 修改RTArray中的RectTransform
        public bool ModifyRectTransfrom_R(string cName, Dictionary<string, System.Object> dict)
        {
            RectTransform ui = GetRectTrans_R(cName);
            return modifyRectTransform(ui, dict);
        }
        #endregion

        #region Text
        public string GetTextString(int index)
        {
            Text ui = GetCompByIndex<Text>(index);
            return getTextString(ui);
        }

        public string GetTextString(string cName)
        {
            Text ui = GetCompByName<Text>(cName);
            return getTextString(ui);
        }

        private string getTextString(Text ui)
        {
            if (null != ui)
            {
                return ui.text;
            }
            return string.Empty;
        }

        public bool SetTextString(int index, string content)
        {
            Text text = GetCompByIndex<Text>(index);
            return _setTextStr(text, content);
        }

        public bool SetTextString(string cName, string content)
        {
            Text text = GetCompByName<Text>(cName);
            return _setTextStr(text, content);
        }

        public bool SetRichTextString(int index, string content)
        {
            Text text = GetCompByIndex<Text>(index);
            return setRichTextStr(text, content);
        }

        public bool SetRichTextString(string cName, string content)
        {
            Text text = GetCompByName<Text>(cName);
            return setRichTextStr(text, content);
        }
        #endregion

        #region Image
        public bool SetImageSprite(int index, string sprite)
        {
            return ChangeUI(new UIHandlerDataAsync("setimagesprite", index, sprite));
        }

        public bool SetImageSprite(string cName, string sprite)
        {
            return ChangeUI(new UIHandlerDataAsync("setimagesprite", cName, sprite));
        }

        public bool SetImageSprite(int index, Sprite sprite)
        {
            Image ui = GetCompByIndex<Image>(index);
            return _setImageSprite(ui, sprite);
        }

        public bool SetImageSprite(string cName, Sprite sprite)
        {
            Image ui = GetCompByName<Image>(cName);
            return _setImageSprite(ui, sprite);
        }

        private static bool _setImageSprite(Image ui, Sprite sprite)
        {
            if (null != ui)
            {
                ui.sprite = sprite;
                return true;
            }
            return false;
        }

        // 设置Image Sprite,完成后调用SetNativeSize
        public bool SetImageNSizeSprite(int index, Sprite sprite)
        {
            Image ui = GetCompByIndex<Image>(index);
            return _setImageNSizeSprite(ui, sprite);
        }

        // 设置Image Sprite,完成后调用SetNativeSize
        public bool SetImageNSizeSprite(string cName, Sprite sprite)
        {
            Image ui = GetCompByName<Image>(cName);
            return _setImageNSizeSprite(ui, sprite);
        }

        // 设置Image Sprite,完成后调用SetNativeSize
        private static bool _setImageNSizeSprite(Image ui, Sprite sprite)
        {
            if (null != ui)
            {
                ui.sprite = sprite;
                ui.SetNativeSize();
                return true;
            }
            return false;
        }

        // 设置Image Sprite,完成后调用SetNativeSize
        public bool SetImageNSizeSprite(int index, string sprite)
        {
            ChangeUI(new UIHandlerDataAsync("setimagensizesprite", index, sprite));
            return true;
        }

        // 设置Image Sprite,完成后调用SetNativeSize
        public bool SetImageNSizeSprite(string cName, string sprite)
        {
            ChangeUI(new UIHandlerDataAsync("setimagensizesprite", cName, sprite));
            return true;
        }
        #endregion Image

        #region RawImage
        public bool SetRawImageTexture(int index, Texture texture)
        {
            RawImage ui = GetCompByIndex<RawImage>(index);
            return _setRawImageTexture(ui, texture);
        }

        public bool SetRawImageTexture(string cName, Texture texture)
        {
            RawImage ui = GetCompByName<RawImage>(cName);
            return _setRawImageTexture(ui, texture);
        }

        private static bool _setRawImageTexture(RawImage ui, Texture texture)
        {
            if (null != ui)
            {
                ui.texture = texture;
                ui.SetNativeSize();
                return true;
            }
            return false;
        }

        public bool SetRawImageRect(int index, Rect rect)
        {
            RawImage ui = GetCompByIndex<RawImage>(index);
            return _setRawImageRect(ui, rect);
        }

        public bool SetRawImageRect(string cName, Rect rect)
        {
            RawImage ui = GetCompByName<RawImage>(cName);
            return _setRawImageRect(ui, rect);
        }

        public bool SetRawImageRect(int index, float[] rect)
        {
            RawImage ui = GetCompByIndex<RawImage>(index);
            return _setRawImageRect(ui, rect);
        }

        public bool SetRawImageRect(string cName, float[] rect)
        {
            RawImage ui = GetCompByName<RawImage>(cName);
            return _setRawImageRect(ui, rect);
        }

        private static bool _setRawImageRect(RawImage ui, Rect rect)
        {
            if (null != ui)
            {
                ui.uvRect = rect;
                ui.SetNativeSize();
                return true;
            }
            return false;
        }

        private static bool _setRawImageRect(RawImage ui, float[] rect)
        {
            if (4 == rect.Length)
            {
                Rect _rect = Tools.GenRect(rect);
                return _setRawImageRect(ui, _rect);
            }
            return false;
        }
        #endregion RawImage

        #region Button
        public bool AddBtnClick(int index, UnityAction<string> call)
        {
            Button btn = GetCompByIndex<Button>(index);
            return _addBtnClick(btn, call);
        }

        public bool AddBtnClick(int index, LuaFunction call)
        {
            Button btn = GetCompByIndex<Button>(index);
            return _addBtnClick(btn, call);
        }

        public bool AddBtnClick(string cName, UnityAction<string> call)
        {
            Button btn = GetCompByName<Button>(cName);
            return _addBtnClick(btn, call);
        }

        public bool AddBtnClick(string cName, LuaFunction call)
        {
            Button btn = GetCompByName<Button>(cName);
            return _addBtnClick(btn, call);
        }
        #endregion Button

        #region Dropdwon
        public bool AddDropdownOnValueChanged(int index, UnityAction<int> call)
        {
            Dropdown ui = GetCompByIndex<Dropdown>(index);
            return _addDropdownOnValueChanged(ui, call);
        }

        public bool AddDropdownOnValueChanged(string cName, UnityAction<int> call)
        {
            Dropdown ui = GetCompByName<Dropdown>(cName);
            return _addDropdownOnValueChanged(ui, call);
        }

        private static bool _addDropdownOnValueChanged(Dropdown ui, UnityAction<int> call)
        {
            if (null != ui)
            {
                ui.onValueChanged.AddListener(call);
                return true;
            }
            return false;
        }

        public bool AddDropdownOnValueChanged(int index, LuaFunction call)
        {
            Dropdown ui = GetCompByIndex<Dropdown>(index);
            return _addDropdownOnValueChanged(ui, call);
        }

        public bool AddDropdownOnValueChanged(string cName, LuaFunction call)
        {
            Dropdown ui = GetCompByName<Dropdown>(cName);
            return _addDropdownOnValueChanged(ui, call);
        }

        private static bool _addDropdownOnValueChanged(Dropdown ui, LuaFunction call)
        {
            if (null != ui)
            {
                ui.onValueChanged.AddListener((int value) =>
                {
                    call.Call<int>(value);
                });
                return true;
            }
            return false;
        }

        //TODO:Dropdown AddOptions 有需要再实现
        #endregion Dropdown

        #region InputField
        public string GetInputFieldString(int index)
        {
            InputField ui = GetCompByIndex<InputField>(index);
            return getInputFieldString(ui);
        }

        public string GetInputFieldString(string cName)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return getInputFieldString(ui);
        }

        private string getInputFieldString(InputField ui)
        {
            if (null != ui)
            {
                return ui.text;
            }
            return string.Empty;
        }

        public bool SetInputFeildString(int index, string text)
        {
            InputField ui = GetCompByIndex<InputField>(index);
            return setInputFeildString(ui, text);
        }

        public bool SetInputFeildString(string cName, string text)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return setInputFeildString(ui, text);
        }

        private bool setInputFeildString(InputField ui, string text)
        {
            if (null != ui)
            {
                ui.text = text;
                return true;
            }
            return false;
        }

        public bool AddInputFieldOnValueChanged(int index, UnityAction<string> call)
        {
            InputField ui = GetCompByIndex<InputField>(index);
            return _addInputFieldOnValueChanged(ui, call);
        }

        public bool AddInputFieldOnValueChanged(string cName, UnityAction<string> call)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return _addInputFieldOnValueChanged(ui, call);
        }

        private static bool _addInputFieldOnValueChanged(InputField ui, UnityAction<string> call)
        {
            if (null != ui)
            {
                ui.onValueChanged.AddListener(call);
                return true;
            }
            return false;
        }

        public bool AddInputFieldOnValueChanged(int index, LuaFunction call)
        {
            InputField ui = GetCompByIndex<InputField>(index);
            return _addInputFieldOnValueChanged(ui, call);
        }

        public bool AddInputFieldOnValueChanged(string cName, LuaFunction call)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return _addInputFieldOnValueChanged(ui, call);
        }

        private static bool _addInputFieldOnValueChanged(InputField ui, LuaFunction call)
        {
            if (null != ui)
            {
                ui.onValueChanged.AddListener((string value) =>
                {
                    call.Call<string>(value);
                });
                return true;
            }
            return false;
        }

        public bool AddInputFieldOnEndEdit(int index, UnityAction<string> call)
        {
            InputField ui = GetCompByIndex<InputField>(index);
            return _addInputFieldOnEndEdit(ui, call);
        }

        public bool AddInputFieldOnEndEdit(string cName, UnityAction<string> call)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return _addInputFieldOnEndEdit(ui, call);
        }

        private static bool _addInputFieldOnEndEdit(InputField ui, UnityAction<string> call)
        {
            if (null != ui)
            {
                ui.onEndEdit.AddListener(call);
                return true;
            }
            return false;
        }

        public bool AddInputFieldOnEndEdit(int index, LuaFunction call)
        {
            InputField ui = GetCompByIndex<InputField>(index);
            return _addInputFieldOnEndEdit(ui, call);
        }

        public bool AddInputFieldOnEndEdit(string cName, LuaFunction call)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return _addInputFieldOnEndEdit(ui, call);
        }

        private static bool _addInputFieldOnEndEdit(InputField ui, LuaFunction call)
        {
            if (null != ui)
            {
                ui.onEndEdit.AddListener((string value) =>
                {
                    call.Call<string>(value);
                });
                return true;
            }
            return false;
        }
        //TODO:Input.onValidateInput相关操作在需要时完善
        #endregion InputField

        #region ScrollView

        public bool SetScrollViewOnItemClick(int index, LuaFunction call)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _setScrollViewOnItemClick(ui, call);
        }

        public bool SetScrollViewOnItemClick(string cName, LuaFunction call)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _setScrollViewOnItemClick(ui, call);
        }

        private static bool _setScrollViewOnItemClick(ScrollView ui, LuaFunction call)
        {
            if (null != ui)
            {
                ui.SetOnItemClickLua(call);
                return true;
            }
            return false;
        }

        public bool SetScrollViewData(int index, LuaTable table)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _setScrollViewData(ui, table);
        }

        public bool SetScrollViewData(string cName, LuaTable table)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _setScrollViewData(ui, table);
        }

        private static bool _setScrollViewData(ScrollView ui, LuaTable table)
        {
            if (null != ui)
            {
                ui.SetData(table);
                return true;
            }
            return false;
        }

        //SetOnItemClickLua

        public bool SetScrollViewData(int index, List<UIItemData> datas)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _setScrollViewData(ui, datas);
        }

        public bool SetScrollViewData(string cName, List<UIItemData> datas)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _setScrollViewData(ui, datas);
        }

        private static bool _setScrollViewData(ScrollView ui, List<UIItemData> datas)
        {
            if (null != ui)
            {
                ui.SetData(datas);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 更新某位置的数据
        /// 由于C#部分UpdateData需要两个参数，这里只导出lua相关的方法
        /// </summary>
        /// <returns><c>true</c>, if scroll view data was updataed, <c>false</c> otherwise.</returns>
        /// <param name="index">Index.</param>
        /// <param name="table">Table.</param>
        public bool UpdateScrollViewData(int index, LuaTable table)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _updateScoriewData(ui, table);
        }

        /// <summary>
        /// 更新某位置的数据
        /// 由于C#部分UpdateData需要两个参数，这里只导出lua相关的方法
        /// </summary>
        /// <returns><c>true</c>, if scroll view data was updataed, <c>false</c> otherwise.</returns>
        /// <param name="cName">C name.</param>
        /// <param name="table">Table.</param>
        public bool UpdateScrollViewData(string cName, LuaTable table)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _updateScoriewData(ui, table);
        }

        private static bool _updateScoriewData(ScrollView ui, LuaTable table)
        {
            if (null != ui)
            {
                ui.UpdateData(table);
                return true;
            }
            return false;
        }

        public bool AddScrollViewData(int index, LuaTable table)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _addScrollViewData(ui, table);
        }

        public bool AddScrollViewData(string cName, LuaTable table)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _addScrollViewData(ui, table);
        }

        private static bool _addScrollViewData(ScrollView ui, LuaTable table)
        {
            if (null != ui)
            {
                ui.AddData(table);
                return true;
            }
            return false;
        }

        public bool AddScrollViewData(int index, UIItemData data)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _addScrollViewData(ui, data);
        }

        public bool AddScrollViewData(string cName, UIItemData data)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _addScrollViewData(ui, data);
        }

        private static bool _addScrollViewData(ScrollView ui, UIItemData data)
        {
            if (null != ui)
            {
                ui.AddData(data);
                return true;
            }
            return false;
        }

        public bool InsertScrollViewData(int index, LuaTable table)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _insertScrollViewData(ui, table);
        }

        public bool InsertScrollViewData(string cName, LuaTable table)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _insertScrollViewData(ui, table);
        }

        private static bool _insertScrollViewData(ScrollView ui, LuaTable table)
        {
            if (null != ui)
            {
                ui.InsertData(table);
                return true;
            }
            return false;
        }

        public bool SetScrollViewBtnClick_S(int index, LuaFunction call)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _setScrollViewBtnClick_S(ui, call);
        }

        public bool SetScrollViewBtnClick_S(string cName, LuaFunction call)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _setScrollViewBtnClick_S(ui, call);
        }

        /// <summary>
        /// 设置Scrolview Item上按钮回调按钮名
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="call"></param>
        /// <returns></returns>
        private static bool _setScrollViewBtnClick_S(ScrollView ui, LuaFunction call)
        {
            if (null != ui)
            {
                ui.SetOnBtnClickLua_S(call);
                return true;
            }
            return false;
        }

        public bool SetScrollViewBtnClick_I(int index, LuaFunction call)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _setScrollViewBtnClick_I(ui, call);
        }

        public bool SetScrollViewBtnClick_I(string cName, LuaFunction call)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _setScrollViewBtnClick_I(ui, call);
        }

        /// <summary>
        /// 设置Scrolview Item上按钮回调按钮 Index
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="call"></param>
        /// <returns></returns>
        private static bool _setScrollViewBtnClick_I(ScrollView ui, LuaFunction call)
        {
            if (null != ui)
            {
                ui.SetOnBtnClickLua_I(call);
                return true;
            }
            return false;
        }

        // 删除一条数据
        public bool RemoveScrollViewData(int index, UIItemData data)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _removeScrollViewData(ui, data);
        }

        // 删除一条数据
        public bool RemoveScrollViewData(string cName, UIItemData data)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _removeScrollViewData(ui, data);
        }

        // 删除一条数据
        private static bool _removeScrollViewData(ScrollView ui, UIItemData data)
        {
            if (null != ui)
            {
                ui.RemoveData(data);
                return true;
            }
            return false;
        }

        // 删除一个 Item
        public bool RemoveScrollViewDataAt(int index, int idx)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _removeScrollViewDataAt(ui, idx);
        }

        // 删除一个 Item
        public bool RemoveScrollViewDataAt(string cName, int index)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _removeScrollViewDataAt(ui, index);
        }

        // 删除一个 Item
        private static bool _removeScrollViewDataAt(ScrollView ui, int index)
        {
            if (null != ui)
            {
                ui.RemoveDataAt(index);
                return true;
            }
            return false;
        }

        // 在最后添加多个数据
        public bool AddScrollViewDatas(int index, List<UIItemData> datas)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _addScrollViewDatas(ui, datas);
        }

        // 在最后添加多个数据
        public bool AddScrollViewDatas(string cName, List<UIItemData> datas)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _addScrollViewDatas(ui, datas);
        }

        // 在最后添加多个数据
        private static bool _addScrollViewDatas(ScrollView ui, List<UIItemData> datas)
        {
            if (null != ui)
            {
                ui.AddDatas(datas);
                return true;
            }
            return false;
        }

        // 在最后添加多个数据
        public bool AddScrollViewDatas(int index, LuaTable lua)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _addScrollViewDatas(ui, lua);
        }

        // 在最后添加多个数据
        public bool AddScrollViewDatas(string cName, LuaTable lua)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _addScrollViewDatas(ui, lua);
        }

        // 在最后添加多个数据
        private static bool _addScrollViewDatas(ScrollView ui, LuaTable lua)
        {
            if (null != ui)
            {
                ui.AddDatas(lua);
                return true;
            }
            return false;
        }

        // 删除多条数据
        public bool RemoveScrollViewDatasAt(int index, int[] idxs)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _removeScrollViewDatasAt(ui, idxs);
        }

        // 删除多条数据
        public bool RemoveScrollViewDatasAt(string cName, int[] idxs)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _removeScrollViewDatasAt(ui, idxs);
        }

        // 删除多条数据
        private static bool _removeScrollViewDatasAt(ScrollView ui, int[] idxs)
        {
            if (null != ui)
            {
                ui.RemoveDatasAt(idxs);
                return true;
            }
            return false;
        }

        // 删除多条数据
        public bool RemoveScrollViewDatasAt(int index, string intArr)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _removeScrollViewDatasAt(ui, intArr);
        }

        // 删除多条数据
        public bool RemoveScrollViewDatasAt(string cName, string intArr)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _removeScrollViewDatasAt(ui, intArr);
        }

        // 删除多条数据
        private static bool _removeScrollViewDatasAt(ScrollView ui, string intArr)
        {
            if (null != ui)
            {
                ui.RemoveDatasAt(intArr);
                return true;
            }
            return false;
        }

        // 滚动到某个 item 所在位置
        public bool Tween2ScrollViewIndex(int index, int idx)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _tween2ScrollViewIndex(ui, idx);
        }

        // 滚动到某个 item 所在位置
        public bool Tween2ScrollViewIndex(string cName, int index)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _tween2ScrollViewIndex(ui, index);
        }

        // 滚动到某个 item 所在位置
        private static bool _tween2ScrollViewIndex(ScrollView ui, int index)
        {
            if (null != ui)
            {
                ui.Tween2Index(index);
                return true;
            }
            return false;
        }

        //----------------------------------------------------------------------------------
        // Tween 到 ScrollView某位置，pos 取0~1f,0表示 显示列表头，1表示显示列表末尾
        public bool Tween2ScrollViewPos(int index, float pos)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _tween2ScrollViewPos(ui, pos);
        }

        // Tween 到 ScrollView某位置，pos 取0~1f,0表示 显示列表头，1表示显示列表末尾
        public bool Tween2ScrollViewPos(string cName, float pos)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _tween2ScrollViewPos(ui, pos);
        }

        // Tween 到 ScrollView某位置，pos 取0~1f,0表示 显示列表头，1表示显示列表末尾
        private static bool _tween2ScrollViewPos(ScrollView ui, float pos)
        {
            if (null != ui)
            {
                ui.Tween2Pos(pos);
                return true;
            }
            return false;
        }
        //----------------------------------------------------------------------------------

        // 设置点击代理
        public bool SetScrollViewOnItemClick(int index, DelScrollItemClicked del)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _setScrollViewOnItemClick(ui, del);
        }

        // 设置点击代理
        public bool SetScrollViewOnItemClick(string cName, DelScrollItemClicked del)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _setScrollViewOnItemClick(ui, del);
        }

        // 设置点击代理
        private static bool _setScrollViewOnItemClick(ScrollView ui, DelScrollItemClicked del)
        {
            if (null != ui)
            {
                ui.SetOnItemClickDelegate(del);
                return true;
            }
            return false;
        }

        // 设置 ScrollView Item内部按钮（除 bg外的）被点击的回调，回调按钮名
        public bool SetScrollViewBtnClick_S(int index, DelBtnClickedStr del)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _setScrollViewOnBtnClick_S(ui, del);
        }

        // 设置 ScrollView Item内部按钮（除 bg外的）被点击的回调，回调按钮名
        public bool SetScrollViewBtnClick_S(string cName, DelBtnClickedStr del)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _setScrollViewOnBtnClick_S(ui, del);
        }

        // 设置 ScrollView Item内部按钮（除 bg外的）被点击的回调，回调按钮名
        private static bool _setScrollViewOnBtnClick_S(ScrollView ui, DelBtnClickedStr del)
        {
            if (null != ui)
            {
                ui.SetOnBtnClick_S(del);
                return true;
            }
            return false;
        }

        // 设置 ScrollView Item内部按钮（除 bg外的）被点击的回调，回调按钮index
        public bool SetScrollViewBtnClick_I(int index, DelBtnClickedIndex del)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _setScrollViewOnBtnClick_I(ui, del);
        }

        // 设置 ScrollView Item内部按钮（除 bg外的）被点击的回调，回调按钮index
        public bool SetScrollViewBtnClick_I(string cName, DelBtnClickedIndex del)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _setScrollViewOnBtnClick_I(ui, del);
        }

        // 设置 ScrollView Item内部按钮（除 bg外的）被点击的回调，回调按钮index
        private static bool _setScrollViewOnBtnClick_I(ScrollView ui, DelBtnClickedIndex del)
        {
            if (null != ui)
            {
                ui.SetOnBtnClick_I(del);
                return true;
            }
            return false;
        }

        // 选中某个Item
        public bool SelectScrollViewItem(int index, int idx)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _selectScrollViewItem(ui, idx);
        }

        // 选中某个Item
        public bool SelectScrollViewItem(string cName, int index)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _selectScrollViewItem(ui, index);
        }

        // 选中某个Item
        private static bool _selectScrollViewItem(ScrollView ui, int index)
        {
            if (null != ui)
            {
                ui.SelectItem(index);
                return true;
            }
            return false;
        }

        // 选中部分 Item
        public bool SelectScrollViewItems(int index, int[] indexArr)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _selectScrollViewItems(ui, indexArr);
        }

        // 选中部分 Item
        public bool SelectScrollViewItems(string cName, int[] indexArr)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _selectScrollViewItems(ui, indexArr);
        }

        // 选中部分 Item
        private static bool _selectScrollViewItems(ScrollView ui, int[] indexArr)
        {
            if (null != ui)
            {
                ui.SelectItems(indexArr);
                return true;
            }
            return false;
        }

        // 选中某几个Item
        public bool SelectScrollViewSelectItems(int index, string indexArr)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _selectScrollViewSelectItems(ui, indexArr);
        }

        // 选中某几个Item
        public bool SelectScrollViewSelectItems(string cName, string indexArr)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _selectScrollViewSelectItems(ui, indexArr);
        }

        // 选中某几个Item
        private static bool _selectScrollViewSelectItems(ScrollView ui, string indexArr)
        {
            if (null != ui)
            {
                ui.SelectItems(indexArr);
                return true;
            }
            return false;
        }

        // 取消选中部分 Item
        public bool UnselectScrollViewItems(int index, int[] indexArr)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _unselectScrollViewItems(ui, indexArr);
        }

        // 取消选中部分 Item
        public bool UnselectScrollViewItems(string cName, int[] indexArr)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _unselectScrollViewItems(ui, indexArr);
        }

        // 取消选中部分 Item
        private static bool _unselectScrollViewItems(ScrollView ui, int[] indexArr)
        {
            if (null != ui)
            {
                ui.UnselectItems(indexArr);
                return true;
            }
            return false;
        }

        //  取消选中某个Item
        public bool UnselectScrollViewItem(int index, int idx)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _unselectScrollViewItem(ui, idx);
        }

        //  取消选中某个Item
        public bool UnselectScrollViewItem(string cName, int index)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _unselectScrollViewItem(ui, index);
        }

        //  取消选中某个Item
        private static bool _unselectScrollViewItem(ScrollView ui, int index)
        {
            if (null != ui)
            {
                ui.UnselectItem(index);
                return true;
            }
            return false;
        }

        // 取消选中某几个 Item
        public bool UnselectScrollViewItems(int index, string indexArr)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _unselectScrollViewItems(ui, indexArr);
        }

        // 取消选中某几个 Item
        public bool UnselectScrollViewItems(string cName, string indexArr)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _unselectScrollViewItems(ui, indexArr);
        }

        // 取消选中某几个 Item
        private static bool _unselectScrollViewItems(ScrollView ui, string indexArr)
        {
            if (null != ui)
            {
                ui.UnselectItems(indexArr);
                return true;
            }
            return false;
        }

        // 改变某个 Item 选中状态
        public bool SwitchScrollViewItem(int index, int idx)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _switchScrollViewItem(ui, idx);
        }

        // 改变某个 Item 选中状态
        public bool SwitchScrollViewItem(string cName, int index)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _switchScrollViewItem(ui, index);
        }

        // 改变某个 Item 选中状态
        private static bool _switchScrollViewItem(ScrollView ui, int index)
        {
            if (null != ui)
            {
                ui.SwitchItem(index);
                return true;
            }
            return false;
        }

        // 改变某几个 Item 选中状态
        public bool SwitchScrollViewItems(int index, string indexArr)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _switchScrollViewItems(ui, indexArr);
        }

        // 改变某几个 Item 选中状态
        public bool SwitchScrollViewItems(string cName, string indexArr)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _switchScrollViewItems(ui, indexArr);
        }

        // 改变某几个 Item 选中状态
        private static bool _switchScrollViewItems(ScrollView ui, string indexArr)
        {
            if (null != ui)
            {
                ui.SwitchItems(indexArr);
                return true;
            }
            return false;
        }

        // 将所有 Item 全部选中
        public bool SelectScrollViewAll(int index)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _selectScrollViewAll(ui);
        }

        // 将所有 Item 全部选中
        public bool SelectScrollViewAll(string cName)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _selectScrollViewAll(ui);
        }

        // 将所有 Item 全部选中
        private static bool _selectScrollViewAll(ScrollView ui)
        {
            if (null != ui)
            {
                ui.SelectAll();
                return true;
            }
            return false;
        }



        // 将所有 Item 全部取消选中
        public bool UnselectScrollViewAll(int index)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _unselectScrollViewAll(ui);
        }

        // 将所有 Item 全部取消选中
        public bool UnselectScrollViewAll(string cName)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _unselectScrollViewAll(ui);
        }

        // 将所有 Item 全部取消选中
        private static bool _unselectScrollViewAll(ScrollView ui)
        {
            if (null != ui)
            {
                ui.UnselectAll();
                return true;
            }
            return false;
        }

        // 设置 Item 选中状态监听回调
        public bool SetScrollViewOnSelect(int index, DelSelectChange del)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _setScrollViewOnSelect(ui, del);
        }

        // 设置 Item 选中状态监听回调
        public bool SetScrollViewOnSelect(string cName, DelSelectChange del)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _setScrollViewOnSelect(ui, del);
        }

        // 设置 Item 选中状态监听回调
        private static bool _setScrollViewOnSelect(ScrollView ui, DelSelectChange del)
        {
            if (null != ui)
            {
                ui.SetOnSelectChangeCall(del);
                return true;
            }
            return false;
        }

        // 设置 Item 选中状态监听回调
        public bool SetScrollViewOnSelect(int index, LuaFunction lua)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return _setScrollViewOnSelect(ui, lua);
        }

        // 设置 Item 选中状态监听回调
        public bool SetScrollViewOnSelect(string cName, LuaFunction lua)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return _setScrollViewOnSelect(ui, lua);
        }

        // 设置 Item 选中状态监听回调
        private static bool _setScrollViewOnSelect(ScrollView ui, LuaFunction lua)
        {
            if (null != ui)
            {
                ui.SetOnSelectChangeCall(lua);
                return true;
            }
            return false;
        }
        #endregion

        #region ScrollSelector
        public bool SetScrollSelectorData(int index, List<UIItemData> data)
        {
            ScrollSelector ui = GetCompByIndex<ScrollSelector>(index);
            return _setScrollSelectorData(ui, data);
        }

        public bool SetScrollSelectorData(string cName, List<UIItemData> data)
        {
            ScrollSelector ui = GetCompByName<ScrollSelector>(cName);
            return _setScrollSelectorData(ui, data);
        }

        private static bool _setScrollSelectorData(ScrollSelector ui, List<UIItemData> data)
        {
            if (null != ui)
            {
                ui.SetData(data);
                return true;
            }
            return false;
        }

        public bool SetScrollSelectorData(int index, LuaTable luaTable)
        {
            ScrollSelector ui = GetCompByIndex<ScrollSelector>(index);
            return _setScrollSelectorData(ui, luaTable);
        }

        public bool SetScrollSelectorData(string cName, LuaTable luaTable)
        {
            ScrollSelector ui = GetCompByName<ScrollSelector>(cName);
            return _setScrollSelectorData(ui, luaTable);
        }

        private static bool _setScrollSelectorData(ScrollSelector ui, LuaTable luaTable)
        {
            if (null != ui)
            {
                ui.SetData(luaTable);
                return true;
            }
            return false;
        }

        public bool SetScrollSelectorCurIndex(int uiIndex, int index)
        {
            ScrollSelector ui = GetCompByIndex<ScrollSelector>(uiIndex);
            return _setScrollSelectorCurIndex(ui, index);
        }

        public bool SetScrollSelectorCurIndex(string cName, int index)
        {
            ScrollSelector ui = GetCompByName<ScrollSelector>(cName);
            return _setScrollSelectorCurIndex(ui, index);
        }

        private static bool _setScrollSelectorCurIndex(ScrollSelector ui, int index)
        {
            if (null != ui)
            {
                ui.SetCurIndex(index);
                return true;
            }
            return false;
        }

        public bool SetScrollSelectorOnSelect(int index, LuaFunction call)
        {
            ScrollSelector ui = GetCompByIndex<ScrollSelector>(index);
            return _setScrollSelectorOnSelect(ui, call);
        }

        public bool SetScrollSelectorOnSelect(string cName, LuaFunction call)
        {
            ScrollSelector ui = GetCompByName<ScrollSelector>(cName);
            return _setScrollSelectorOnSelect(ui, call);
        }

        private static bool _setScrollSelectorOnSelect(ScrollSelector ui, LuaFunction call)
        {
            if (null != ui)
            {
                ui.SetOnSelectCallbackLua(call);
                return true;
            }
            return false;
        }
        #endregion ScrollSelector

        #region SelectorToggle
        public bool SetSelectorTogglesCanTouch(int index, bool enabled)
        {
            SelectorToggles ui = GetCompByIndex<SelectorToggles>(index);
            return _setSelectorTogglesCanTouch(ui, enabled);
        }

        public bool SetSelectorTogglesCanTouch(string cName, bool enabled)
        {
            SelectorToggles ui = GetCompByName<SelectorToggles>(cName);
            return _setSelectorTogglesCanTouch(ui, enabled);
        }

        private static bool _setSelectorTogglesCanTouch(SelectorToggles ui, bool enabled)
        {
            if (null != ui)
            {
                ui.EnableTouch = enabled;
                return true;
            }
            return false;
        }

        public bool SetSelectorTogglesCallOnSet(int index, bool enabled)
        {
            SelectorToggles ui = GetCompByIndex<SelectorToggles>(index);
            return _setSelectorTogglesCallOnSet(ui, enabled);
        }

        public bool SetSelectorTogglesCallOnSet(string cName, bool enabled)
        {
            SelectorToggles ui = GetCompByName<SelectorToggles>(cName);
            return _setSelectorTogglesCallOnSet(ui, enabled);
        }

        private static bool _setSelectorTogglesCallOnSet(SelectorToggles ui, bool enabled)
        {
            if (null != ui)
            {
                ui.CallbackOnSet = enabled;
                return true;
            }
            return false;
        }

        public bool SetSelectorTogglesNum(int index, int num)
        {
            SelectorToggles ui = GetCompByIndex<SelectorToggles>(index);
            return _setSelectorTogglesNum(ui, num);
        }

        public bool SetSelectorTogglesNum(string cName, int num)
        {
            SelectorToggles ui = GetCompByName<SelectorToggles>(cName);
            return _setSelectorTogglesNum(ui, num);
        }

        private static bool _setSelectorTogglesNum(SelectorToggles ui, int num)
        {
            if (null != ui)
            {
                ui.SetTotalNum(num);
                return true;
            }
            return false;
        }

        public bool SetSelectorTogglesData(int index, List<UIItemData> data)
        {
            SelectorToggles ui = GetCompByIndex<SelectorToggles>(index);
            return _setSelectorTogglesData(ui, data);
        }

        public bool SetSelectorTogglesData(string cName, List<UIItemData> data)
        {
            SelectorToggles ui = GetCompByName<SelectorToggles>(cName);
            return _setSelectorTogglesData(ui, data);
        }

        private static bool _setSelectorTogglesData(SelectorToggles ui, List<UIItemData> data)
        {
            if (null != ui)
            {
                ui.SetData(data);
                return true;
            }
            return false;
        }

        public bool SetSelectorTogglesData(int index, LuaTable luaTable)
        {
            SelectorToggles ui = GetCompByIndex<SelectorToggles>(index);
            return _setSelectorTogglesData(ui, luaTable);
        }

        public bool SetSelectorTogglesData(string cName, LuaTable luaTable)
        {
            SelectorToggles ui = GetCompByName<SelectorToggles>(cName);
            return _setSelectorTogglesData(ui, luaTable);
        }

        private static bool _setSelectorTogglesData(SelectorToggles ui, LuaTable luaTable)
        {
            if (null != ui)
            {

                ui.SetData(luaTable);
                return true;
            }
            return false;
        }

        public bool SetSelectorTogglesIndex(int index, int curIndex)
        {
            SelectorToggles ui = GetCompByIndex<SelectorToggles>(index);
            return _setSelectorTogglesIndex(ui, curIndex);
        }

        public bool SetSelectorTogglesIndex(string cName, int curIndex)
        {
            SelectorToggles ui = GetCompByName<SelectorToggles>(cName);
            return _setSelectorTogglesIndex(ui, curIndex);
        }

        private static bool _setSelectorTogglesIndex(SelectorToggles ui, int curIndex)
        {
            if (null != ui)
            {
                ui.SetCurIndex(curIndex);
                return true;
            }
            return false;
        }

        public bool SetSelectorTogglesOnChange(int index, UnityAction<int> action)
        {
            SelectorToggles ui = GetCompByIndex<SelectorToggles>(index);
            return _setSelectorTogglesOnChange(ui, action);
        }

        public bool SetSelectorTogglesOnChange(string cName, UnityAction<int> action)
        {
            SelectorToggles ui = GetCompByName<SelectorToggles>(cName);
            return _setSelectorTogglesOnChange(ui, action);
        }

        private static bool _setSelectorTogglesOnChange(SelectorToggles ui, UnityAction<int> action)
        {
            if (null != ui)
            {
                ui.SetOnIndexChange(action);
                return true;
            }
            return false;
        }

        public bool SetSelectorTogglesOnChange(int index, LuaFunction function)
        {
            SelectorToggles ui = GetCompByIndex<SelectorToggles>(index);
            return _setSelectorTogglesOnChange(ui, function);
        }

        public bool SetSelectorTogglesOnChange(string cName, LuaFunction function)
        {
            SelectorToggles ui = GetCompByName<SelectorToggles>(cName);
            return _setSelectorTogglesOnChange(ui, function);
        }

        private static bool _setSelectorTogglesOnChange(SelectorToggles ui, LuaFunction function)
        {
            if (null != ui)
            {

                ui.SetOnIndexChange(function);
                return true;
            }
            return false;
        }
        #endregion SelectorToggle

        #region Slider
        public float GetSliderValue(int index)
        {
            Slider ui = GetCompByIndex<Slider>(index);
            return _getSliderValue(ui);
        }

        public float GetSliderValue(string cName)
        {
            Slider ui = GetCompByName<Slider>(cName);
            return _getSliderValue(ui);
        }

        private static float _getSliderValue(Slider ui)
        {
            if (null != ui)
            {
                return ui.value;
            }
            return -1f;
        }

        public bool SetSliderValue(int index, float value)
        {
            Slider slider = GetCompByIndex<Slider>(index);
            return _setSliderValue(slider, value);
        }

        public bool SetSliderValue(string cName, float value)
        {
            Slider slider = GetCompByName<Slider>(cName);
            return _setSliderValue(slider, value);
        }

        public bool AddOnSliderValueChanged(int index, UnityAction<float> call)
        {
            Slider slider = GetCompByIndex<Slider>(index);
            return _addOnSliderValueChanged(slider, call);
        }

        public bool AddOnSliderValueChanged(string cName, UnityAction<float> call)
        {
            Slider slider = GetCompByName<Slider>(cName);
            return _addOnSliderValueChanged(slider, call);
        }

        public bool AddOnSliderValueChanged(int index, LuaFunction call)
        {
            Slider slider = GetCompByIndex<Slider>(index);
            return _addOnSliderValueChanged(slider, call);
        }

        public bool AddOnSliderValueChanged(string cName, LuaFunction call)
        {
            Slider slider = GetCompByName<Slider>(cName);
            return _addOnSliderValueChanged(slider, call);
        }
        #endregion Slider

        #region Toggle
        public bool GetToggleIsOn(int index)
        {
            Toggle ui = GetCompByIndex<Toggle>(index);
            return _getToggleIsOn(ui);
        }

        public bool GetToggleIsOn(string cName)
        {
            Toggle ui = GetCompByName<Toggle>(cName);
            return _getToggleIsOn(ui);
        }

        private static bool _getToggleIsOn(Toggle ui)
        {
            if (null != ui)
            {
                return ui.isOn;
            }
            return false;
        }

        public bool SetToggleIsOn(int index, bool isOn)
        {
            Toggle ui = GetCompByIndex<Toggle>(index);
            return _setToggleIsOn(ui, isOn);
        }

        public bool SetToggleIsOn(string cName, bool isOn)
        {
            Toggle ui = GetCompByName<Toggle>(cName);
            return _setToggleIsOn(ui, isOn);
        }

        private static bool _setToggleIsOn(Toggle ui, bool isOn)
        {
            if (null != ui)
            {
                ui.isOn = isOn;
                return true;
            }
            return false;
        }

        public bool AddToggleOnValueChanged(int index, UnityAction<bool> call)
        {
            Toggle ui = GetCompByIndex<Toggle>(index);
            return _addToggleOnValueChanged(ui, call);
        }

        public bool AddToggleOnValueChanged(string cName, UnityAction<bool> call)
        {
            Toggle ui = GetCompByName<Toggle>(cName);
            return _addToggleOnValueChanged(ui, call);
        }

        private static bool _addToggleOnValueChanged(Toggle ui, UnityAction<bool> call)
        {
            if (null != ui)
            {
                ui.onValueChanged.AddListener(call);
                return true;
            }
            return false;
        }

        public bool AddToggleOnValueChanged(int index, LuaFunction call)
        {
            Toggle ui = GetCompByIndex<Toggle>(index);
            return _addToggleOnValueChanged(ui, call);
        }

        public bool AddToggleOnValueChanged(string cName, LuaFunction call)
        {
            Toggle ui = GetCompByName<Toggle>(cName);
            return _addToggleOnValueChanged(ui, call);
        }

        private static bool _addToggleOnValueChanged(Toggle ui, LuaFunction call)
        {
            if (null != ui)
            {
                ui.onValueChanged.AddListener((bool value) =>
                {
                    if (null != call)
                    {
                        call.Call<bool>(value);
                    }
                });
                return true;
            }
            return false;
        }
        #endregion Toggle

        #region SubHandler 相关
        // 调用SubHandler的ChangeItem方法
        public bool ChangeSubHandlerItem(int index, UIItemData data)
        {
            UIHandler handler = GetSubHandler(index);
            if (null != handler)
            {
                return handler.ChangeItem(data);
            }
            return false;
        }

        // 调用SubHandler的ChangeItem方法
        public bool ChangeSubHandlerItem(string cName, UIItemData data)
        {
            UIHandler handler = GetSubHandler(cName);
            if (null != handler)
            {
                return handler.ChangeItem(data);
            }
            return false;
        }

        // 调用SubHandler的ChangeItem方法
        public bool ChangeSubHandlerItem(int index, LuaTable data)
        {
            UIHandler handler = GetSubHandler(index);
            if (null != handler)
            {
                return handler.ChangeItem(data);
            }
            return false;
        }

        // 调用SubHandler的ChangeItem方法
        public bool ChangeSubHandlerItem(string cName, LuaTable data)
        {
            UIHandler handler = GetSubHandler(cName);
            if (null != handler)
            {
                return handler.ChangeItem(data);
            }
            return false;
        }


        // 调用SubHandler的ChangeUI方法
        public bool ChangeSubHandlerUI(int index, UIHandlerData data)
        {
            UIHandler handler = GetSubHandler(index);
            if (null != handler)
            {
                return handler.ChangeUI(data);
            }
            return false;
        }

        // 调用SubHandler的ChangeUI方法
        public bool ChangeSubHandlerUI(string cName, UIHandlerData data)
        {
            UIHandler handler = GetSubHandler(cName);
            if (null != handler)
            {
                return handler.ChangeUI(data);
            }
            return false;
        }

        // 调用SubHandler的ChangeUI方法
        public bool ChangeSubHandlerUI(int index, LuaTable data)
        {
            UIHandler handler = GetSubHandler(index);
            if (null != handler)
            {
                return handler.ChangeUI(data);
            }
            return false;
        }

        // 调用SubHandler的ChangeUI方法
        public bool ChangeSubHandlerUI(string cName, LuaTable data)
        {
            UIHandler handler = GetSubHandler(cName);
            if (null != handler)
            {
                return handler.ChangeUI(data);
            }
            return false;
        }
        #endregion SubHandler 相关
        //TODO:ToggleGroup 以后有需要实现

        #region Scrollbar
        public float GetScrollbarValue(int index)
        {
            Scrollbar ui = GetCompByIndex<Scrollbar>(index);
            return _getScrollbarValue(ui);
        }

        public float GetScrollbarValue(string cName)
        {
            Scrollbar ui = GetCompByName<Scrollbar>(cName);
            return _getScrollbarValue(ui);
        }

        private static float _getScrollbarValue(Scrollbar ui)
        {
            if (null != ui)
            {
                return ui.value;
            }
            return -1f;
        }

        public bool SetScrollbarValue(int index, float value)
        {
            Scrollbar slider = GetCompByIndex<Scrollbar>(index);
            return _setScrollbarValue(slider, value);
        }

        public bool SetScrollbarValue(string cName, float value)
        {
            Scrollbar slider = GetCompByName<Scrollbar>(cName);
            return _setScrollbarValue(slider, value);
        }

        public bool AddOnScrollbarValueChanged(int index, UnityAction<float> call)
        {
            Scrollbar slider = GetCompByIndex<Scrollbar>(index);
            return _addOnScrollbarValueChanged(slider, call);
        }

        public bool AddOnScrollbarValueChanged(string cName, UnityAction<float> call)
        {
            Scrollbar slider = GetCompByName<Scrollbar>(cName);
            return _addOnScrollbarValueChanged(slider, call);
        }

        public bool AddOnScrollbarValueChanged(int index, LuaFunction call)
        {
            Scrollbar slider = GetCompByIndex<Scrollbar>(index);
            return _addOnScrollbarValueChanged(slider, call);
        }

        public bool AddOnScrollbarValueChanged(string cName, LuaFunction call)
        {
            Scrollbar slider = GetCompByName<Scrollbar>(cName);
            return _addOnScrollbarValueChanged(slider, call);
        }

        public bool SetScrollbarSize(int index, float size)
        {
            Scrollbar ui = GetCompByIndex<Scrollbar>(index);
            return _setScrollbarSize(ui, size);
        }

        public bool SetScrollbarSize(string cName, float size)
        {
            Scrollbar ui = GetCompByName<Scrollbar>(cName);
            return _setScrollbarSize(ui, size);
        }

        private static bool _setScrollbarSize(Scrollbar ui, float size)
        {
            if (null != ui)
            {
                ui.size = size;
                return true;
            }
            return false;
        }

        public bool SetScrollbarStepNumber(int index, int num)
        {
            Scrollbar ui = GetCompByIndex<Scrollbar>(index);
            return _setScrollbarStepNumber(ui, num);
        }

        public bool SetScrollbarStepNumber(string cName, int num)
        {
            Scrollbar ui = GetCompByName<Scrollbar>(cName);
            return _setScrollbarStepNumber(ui, num);
        }

        private static bool _setScrollbarStepNumber(Scrollbar ui, int num)
        {
            if (null != ui)
            {
                if (num < 3)
                {
                    LogFile.Warn("Scrollbar numberOfSteps 至少应该大于2，跳过设置。");
                    return false;
                }
                ui.numberOfSteps = num;
                return true;
            }
            return false;
        }
        #endregion Scrollbar

        #region private 方法

        private bool setUIName(UIBehaviour ui, string cName)
        {
            if (null != ui)
            {
                ui.gameObject.name = cName;
                return true;
            }
            return false;
        }

        private static bool _setUIActive(UIBehaviour ui, bool value)
        {
            if (null != ui)
            {
                ui.gameObject.SetActive(value);
                return true;
            }
            return false;
        }

        private static bool _setUISelectable(Selectable ui, bool value)
        {
            if (null != ui)
            {
                ui.interactable = value;
                return true;
            }
            return false;
        }

        private static bool _setUIEnable(UIBehaviour ui, bool value)
        {
            if (null != ui)
            {
                ui.enabled = value;
                return true;
            }
            return false;
        }

        private static bool _setTextStr(Text text, string content)
        {
            if (null != text)
            {
                text.text = content;
                return true;
            }
            return false;
        }

        private bool setRichTextStr(Text text, string content)
        {
            if (null != text)
            {
                text.supportRichText = true;
                text.text = content;
                return true;
            }
            return false;
        }

        private static bool _addBtnClick(Button btn, UnityAction<string> call)
        {
            if (null != btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => call(btn.name));
                return true;
            }
            return false;
        }

        private static bool _addBtnClick(Button btn, LuaFunction call)
        {
            if (null != btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (null != call)
                    {
                        call.Call(btn.name);
                    }
                });
                return true;
            }
            return false;
        }

        private static bool _setSliderValue(Slider slider, float value)
        {
            if (null != slider)
            {
                slider.value = value;
                return true;
            }
            return false;
        }

        private static bool _addOnSliderValueChanged(Slider slider, UnityAction<float> call)
        {
            if (null != slider)
            {
                slider.onValueChanged.AddListener(call);
            }
            return false;
        }

        private static bool _addOnSliderValueChanged(Slider slider, LuaFunction call)
        {
            if (null != slider)
            {
                slider.onValueChanged.AddListener((float value) =>
                {
                    if (null != call)
                    {
                        call.Call<float>(value);
                    }
                });
            }
            return false;
        }

        private static bool _setScrollbarValue(Scrollbar slider, float value)
        {
            if (null != slider)
            {
                slider.value = value;
                return true;
            }
            return false;
        }

        private static bool _addOnScrollbarValueChanged(Scrollbar slider, UnityAction<float> call)
        {
            if (null != slider)
            {
                slider.onValueChanged.AddListener(call);
            }
            return false;
        }

        private static bool _addOnScrollbarValueChanged(Scrollbar slider, LuaFunction call)
        {
            if (null != slider)
            {
                slider.onValueChanged.AddListener((float value) =>
                {
                    if (null != call)
                    {
                        call.Call<float>(value);
                    }
                });
            }
            return false;
        }

        private bool modifyRectTransform(RectTransform rect, LuaTable table)
        {
            return modifyRectTransform(rect, Tools.LuaTable2Dict(table));
        }

        private bool modifyRectTransform(RectTransform rect, Dictionary<string, System.Object> dict)
        {
            if (null != rect)
            {
                Tools.ModifyRectTransform(rect, dict);
                return true;
            }
            return false;
        }

        private void getListNames<T>(ref List<string> listName, ref List<T> listObj) where T : Component
        {
            if (null != listName)
            {
                listName.Clear();
            }
            else
            {
                listName = new List<string>();
            }

            if (null == listObj)
            {
                listObj = new List<T>();
            }
            int c = listObj.Count;
            for (int i = 0; i < c; ++i)
            {
                if (null != listObj[i])
                {
                    listName.Add(Tools.GetTransformName(listObj[i].transform, RootTransform));
                }
                else
                {
                    listName.Add(string.Empty);
                    // LogFile.Warn("UIHandler.UIArry[{0}] is null", i);
                }
            }

        }

        void _storeLuaFunc(LuaFunction luaFunc)
        {
            if(!mLuaFuns.Contains(luaFunc))
            {
                mLuaFuns.Add(luaFunc);
            }
        }
        #endregion private 方法

        #region =====UI 获取
        [NoToLua]
        public Text GetText(int index)
        {
            return GetCompByIndex<Text>(index);
        }

        [NoToLua]
        public Text GetText(string cName)
        {
            return GetCompByName<Text>(cName);
        }


        [NoToLua]
        public Image GetImage(int index)
        {
            return GetCompByIndex<Image>(index);
        }

        [NoToLua]
        public Image GetImage(string cName)
        {
            return GetCompByName<Image>(cName);
        }


        [NoToLua]
        public RawImage GetRawImage(int index)
        {
            return GetCompByIndex<RawImage>(index);
        }

        [NoToLua]
        public RawImage GetRawImage(string cName)
        {
            return GetCompByName<RawImage>(cName);
        }


        [NoToLua]
        public Button GetButton(int index)
        {
            return GetCompByIndex<Button>(index);
        }

        [NoToLua]
        public Button GetButton(string cName)
        {
            return GetCompByName<Button>(cName);
        }


        [NoToLua]
        public Toggle GetToggle(int index)
        {
            return GetCompByIndex<Toggle>(index);
        }

        [NoToLua]
        public Toggle GetToggle(string cName)
        {
            return GetCompByName<Toggle>(cName);
        }


        [NoToLua]
        public Slider GetSlider(int index)
        {
            return GetCompByIndex<Slider>(index);
        }

        [NoToLua]
        public Slider GetSlider(string cName)
        {
            return GetCompByName<Slider>(cName);
        }


        [NoToLua]
        public Scrollbar GetScrollbar(int index)
        {
            return GetCompByIndex<Scrollbar>(index);
        }

        [NoToLua]
        public Scrollbar GetScrollbar(string cName)
        {
            return GetCompByName<Scrollbar>(cName);
        }


        [NoToLua]
        public Dropdown GetDropdown(int index)
        {
            return GetCompByIndex<Dropdown>(index);
        }

        [NoToLua]
        public Dropdown GetDropdown(string cName)
        {
            return GetCompByName<Dropdown>(cName);
        }


        [NoToLua]
        public InputField GetInputField(int index)
        {
            return GetCompByIndex<InputField>(index);
        }

        [NoToLua]
        public InputField GetInputField(string cName)
        {
            return GetCompByName<InputField>(cName);
        }


        [NoToLua]
        public Canvas GetCanvas(int index)
        {
            return GetCompByIndex<Canvas>(index);
        }

        [NoToLua]
        public Canvas GetCanvas(string cName)
        {
            return GetCompByName<Canvas>(cName);
        }

        /// <summary>
        /// Panel是特殊的Image，如果删除Image组件则不能获取，使用子组件的transform.parent获取其RectTransform
        /// </summary>
        /// <returns>The panel.</returns>
        /// <param name="index">Index.</param>
        [NoToLua]
        public Image getPanel(int index)
        {
            return GetCompByIndex<Image>(index);
        }

        /// <summary>
        /// Panel是特殊的Image，如果删除Image组件则不能获取，使用子组件的transform.parent获取其RectTransform
        /// </summary>
        /// <returns>The panel.</returns>
        /// <param name="cName">C name.</param>
        [NoToLua]
        public Image GetPanel(string cName)
        {
            return GetCompByName<Image>(cName);
        }


        [NoToLua]
        public ScrollRect GetScrollRect(int index)
        {
            return GetCompByIndex<ScrollRect>(index);
        }

        [NoToLua]
        public ScrollRect GetScrollRect(string cName)
        {
            return GetCompByName<ScrollRect>(cName);
        }

        [NoToLua]
        public RectTransform GetRectTransform(int index)
        {
            if (index >= 0 && index < RTCount)
            {
                return RTArray[index];
            }
            else
            {
                LogFile.Warn("{0}的RTArray找不到index为{1}的RectTransform", name, index);
            }
            return null;
        }

        [NoToLua]
        public RectTransform GetRectTransform(string path)
        {
            RectTransform rt = null;
            if (!string.IsNullOrEmpty(path) && null != mRTNames)
            {
                int index = mRTNames.IndexOf(path);
                if (index < 0)
                {
                    LogFile.Warn("{0}的找不到path为{1}的RectTransform", name, path);
                }
                else
                {
                    rt = GetRectTransform(index);
                }
            }
            return rt;
        }
        #endregion =====UI 获取

    }
}

/**
        //正则 替换函数名   SelectItem(int index)  ====>   ___~ScrollView~SelectItem~int~index~___~ui.SelectItem(index);//--
        ^(\S+)\((\S*) ?(\S*)\)$
        ___~ScrollView~\1~\2~\3~___~ui.\1(\3);//--
        // 正则 将上面的正则替换成 UIhandler 函数 
        
        /// <summary>
        /// \8 
        /// </summary>
        /// <param name="\5"> \5(\4) </param>
        public bool \1\2\3(int index, \4 \5)
        {
            \2 ui = GetCompByIndex<\2>(index);
            return _\6\2\3(ui, \5);
        }

        /// <summary>
        /// \8 
        /// </summary>
        /// <param name="\5"> \5(\4) </param>
        public bool \1\2\3(string cName, \4 \5)
        {
            \2 ui = GetCompByName<\2>(cName);
            return _\6\2\3(ui, \5);
        }

        // \8
        private static bool _\6\2\3(\2 ui, \4 \5)
        {
            if(null != ui)
            {
                \7
                return true;
            }
            return false;
        }
//----------------------------------------------------------------------------------
                case "\1\2\3":
                    if (uiIndex != -1)
                    {
                        return \1\2\3(uiIndex, (\4) data.Content);
                    }
                    else
                    {
                        return \1\2\3(uiName, (\4) data.Content);
                    }
                    //break;
//=====================================================================================

Set~Image~Sprite~Sprite~sprite~set~call(ui)//--注释
^(.+)~(.+)~(.+)~(.+)~(.+)~(.+)~(.*)//--(.*)$
**/

/**
                    case "ModifyURectTransfrom":
                    if (uiIndex != -1)
                    {
                        return ModifyURectTransfrom(uiIndex, data.Content as LuaTable);
                        return ModifyURectTransfrom(uiIndex, (__TYPE___) data.Content);
                    }
                    else
                    {
                        return ModifyURectTransfrom(uiName, data.Content as LuaTable);
                        return ModifyURectTransfrom(uiName, (__TYPE___) data.Content);
                    }
                    //break;

                     ^.*return(.*)$\n(.*)return(.* )\((\S+)\) data\.Content\);

                        \4 lua = data.Content as \4;
                        if (null != lua)
                        {
                            return \1
                        }
                        else
                        {
                            return \3(\4) data.Content);
                        }
**/
