﻿using System;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameFramework
{
    /// <summary>
    /// UI组件管理器，管理以下组件：
    /// Text
    /// Image
    /// RawImage
    /// Button
    /// Toggle
    /// Slider
    /// Scrollbar
    /// Dropdown
    /// InputField
    /// Canvas
    /// Panel
    /// ScrollView
    /// </summary>
    public class UIHandler : MonoBehaviour
    {

        public UIBehaviour[] UIArray;
        public Transform RootTransform;

        private List<string> mUINames;

        #region MonoBehaviour
        void Start()
        {
            if (null == RootTransform)
            {
                RootTransform = transform;
            }
            GetUI2RootNames();
        }

        private void GetUI2RootNames()
        {
            if(null != mUINames)
            {
                mUINames.Clear();
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
        #endregion

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

        public bool ChangeUI(UIHandlerData data)
        {
            string uiName = data.UIName;
            int uiIndex = data.UIIndex;

            switch(data.FuncStr.ToLower())
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
                case "settextcolor":
                    if (uiIndex != -1)
                    {
                        return SetTextColor(uiIndex, (Color)data.Content);
                    }
                    else
                    {
                        return SetTextColor(uiName, (Color)data.Content);
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
                    if (uiIndex != -1)
                    {
                        return SetImageSprite(uiIndex, (Sprite)data.Content);
                    }
                    else
                    {
                        return SetImageSprite(uiName, (Sprite)data.Content);
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
                        if(null != (LuaFunction)data.Content)
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
                        return SetScrollViewOnItemClick(uiIndex, (LuaFunction) data.Content);
                    }
                    else
                    {
                        return SetScrollViewOnItemClick(uiName, (LuaFunction) data.Content);
                    }
                    //break;
                case "setscrollviewdatas":
                    if (uiIndex != -1)
                    {
                        if (null != (LuaTable)data.Content)
                        {
                            return SetScrollViewDatas(uiIndex, (LuaTable)data.Content);
                        }
                        else
                        {
                            return SetScrollViewDatas(uiIndex, (List<UIItemData>)data.Content);
                        }
                    }
                    else
                    {
                        if (null != (LuaTable)data.Content)
                        {
                            return SetScrollViewDatas(uiName, (LuaTable)data.Content);
                        }
                        else
                        {
                            return SetScrollViewDatas(uiName, (List<UIItemData>)data.Content);
                        }
                    }
                //break;
                case "updatescrollviewdata":
                    if (uiIndex != -1)
                    {
                        return UpdateScrollViewData(uiIndex, (LuaTable)data.Content);
                    }
                    else
                    {
                        return UpdateScrollViewData(uiName, (LuaTable)data.Content);
                    }
                    //break;
                case "addscrollviewdata":
                    if (uiIndex != -1)
                    {
                        if (null != (LuaTable)data.Content)
                        {
                            return AddScrollViewData(uiIndex, (LuaTable)data.Content);
                        }
                        else
                        {
                            return AddScrollViewData(uiIndex, (UIItemData)data.Content);
                        }
                    }
                    else
                    {
                        if (null != (LuaTable)data.Content)
                        {
                            return AddScrollViewData(uiName, (LuaTable)data.Content);
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
                        return InsertScrollViewData(uiIndex, (LuaTable)data.Content);
                    }
                    else
                    {
                        return InsertScrollViewData(uiName, (LuaTable)data.Content);
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
            }
            return false;
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
            return setUIActive(ui, value);
        }

        public bool SetUIActive(string cName, bool value)
        {
            UIBehaviour ui = GetCompByName<UIBehaviour>(cName);
            return setUIActive(ui, value);
        }

        /// <summary>
        /// 设置Button、Slider、Dropdown、InputField等UI是否可选择
        /// </summary>
        /// <returns><c>true</c>, if UIS electable was set, <c>false</c> otherwise.</returns>
        /// <param name="index">Index.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        public bool SetUISelectable(int index, bool value)
        {
            Selectable ui = GetCompByIndex<Selectable>(index);
            return setUISelectable(ui, value);
        }

        /// <summary>
        /// 设置Button、Slider、Dropdown、InputField等UI是否可选择
        /// </summary>
        /// <returns><c>true</c>, if UIS electable was set, <c>false</c> otherwise.</returns>
        /// <param name="cName">Name.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
        public bool SetUISelectable(string cName, bool value)
        {
            Selectable ui = GetCompByName<Selectable>(cName);
            return setUISelectable(ui, value);
        }
        #endregion UI 通用

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
            return setTextStr(text, content);
        }

        public bool SetTextString(string cName, string content)
        {
            Text text = GetCompByName<Text>(cName);
            return setTextStr(text, content);
        }

        public bool SetTextColor(int index, Color color)
        {
            Text text = GetCompByIndex<Text>(index);
            return setTextColor(text, color);
        }

        public bool SetTextColor(string cName, Color color)
        {
            Text text = GetCompByName<Text>(cName);
            return setTextColor(text, color);
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
        public bool SetImageSprite(int index, Sprite sprite)
        {
            Image ui = GetCompByIndex<Image>(index);
            return setImageSprite(ui, sprite);
        }

        public bool SetImageSprite(string cName, Sprite sprite)
        {
            Image ui = GetCompByName<Image>(cName);
            return setImageSprite(ui, sprite);
        }

        private static bool setImageSprite(Image ui, Sprite sprite)
        {
            if (null != ui)
            {
                ui.sprite = sprite;
                return true;
            }
            return false;
        }
        #endregion Image

        #region RawImage
        public bool SetRawImageTexture(int index, Texture texture)
        {
            RawImage ui = GetCompByIndex<RawImage>(index);
            return setRawImageTexture(ui, texture);
        }

        public bool SetRawImageTexture(string cName, Texture texture)
        {
            RawImage ui = GetCompByName<RawImage>(cName);
            return setRawImageTexture(ui, texture);
        }

        private static bool setRawImageTexture(RawImage ui, Texture texture)
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
            return setRawImageRect(ui, rect);
        }

        public bool SetRawImageRect(string cName, Rect rect)
        {
            RawImage ui = GetCompByName<RawImage>(cName);
            return setRawImageRect(ui, rect);
        }

        public bool SetRawImageRect(int index, float[] rect)
        {
            RawImage ui = GetCompByIndex<RawImage>(index);
            return setRawImageRect(ui, rect);
        }

        public bool SetRawImageRect(string cName, float[] rect)
        {
            RawImage ui = GetCompByName<RawImage>(cName);
            return setRawImageRect(ui, rect);
        }

        private static bool setRawImageRect(RawImage ui, Rect rect)
        {
            if (null != ui)
            {
                ui.uvRect = rect;
                ui.SetNativeSize();
                return true;
            }
            return false;
        }

        private static bool setRawImageRect(RawImage ui, float[] rect)
        {
            if (4 == rect.Length)
            {
                Rect _rect = Tools.GenRect(rect);
                return setRawImageRect(ui, _rect);
            }
            return false;
        }
        #endregion RawImage

        #region Button
        public bool AddBtnClick(int index, UnityAction<string> call)
        {
            Button btn = GetCompByIndex<Button>(index);
            return addBtnClick(btn, call);
        }

        public bool AddBtnClick(int index, LuaFunction call)
        {
            Button btn = GetCompByIndex<Button>(index);
            return addBtnClick(btn, call);
        }

        public bool AddBtnClick(string cName, UnityAction<string> call)
        {
            Button btn = GetCompByName<Button>(cName);
            return addBtnClick(btn, call);
        }

        public bool AddBtnClick(string cName, LuaFunction call)
        {
            Button btn = GetCompByName<Button>(cName);
            return addBtnClick(btn, call);
        }
        #endregion Button

        #region Dropdwon
        public bool AddDropdownOnValueChanged(int index, UnityAction<int> call)
        {
            Dropdown ui = GetCompByIndex<Dropdown>(index);
            return addDropdownOnValueChanged(ui, call);
        }

        public bool AddDropdownOnValueChanged(string cName, UnityAction<int> call)
        {
            Dropdown ui = GetCompByName<Dropdown>(cName);
            return addDropdownOnValueChanged(ui, call);
        }

        private static bool addDropdownOnValueChanged(Dropdown ui, UnityAction<int> call)
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
            return addDropdownOnValueChanged(ui, call);
        }

        public bool AddDropdownOnValueChanged(string cName, LuaFunction call)
        {
            Dropdown ui = GetCompByName<Dropdown>(cName);
            return addDropdownOnValueChanged(ui, call);
        }

        private static bool addDropdownOnValueChanged(Dropdown ui, LuaFunction call)
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
            return addInputFieldOnValueChanged(ui, call);
        }

        public bool AddInputFieldOnValueChanged(string cName, UnityAction<string> call)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return addInputFieldOnValueChanged(ui, call);
        }

        private static bool addInputFieldOnValueChanged(InputField ui, UnityAction<string> call)
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
            return addInputFieldOnValueChanged(ui, call);
        }

        public bool AddInputFieldOnValueChanged(string cName, LuaFunction call)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return addInputFieldOnValueChanged(ui, call);
        }

        private static bool addInputFieldOnValueChanged(InputField ui, LuaFunction call)
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
            return addInputFieldOnEndEdit(ui, call);
        }

        public bool AddInputFieldOnEndEdit(string cName, UnityAction<string> call)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return addInputFieldOnEndEdit(ui, call);
        }

        private static bool addInputFieldOnEndEdit(InputField ui, UnityAction<string> call)
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
            return addInputFieldOnEndEdit(ui, call);
        }

        public bool AddInputFieldOnEndEdit(string cName, LuaFunction call)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return addInputFieldOnEndEdit(ui, call);
        }

        private static bool addInputFieldOnEndEdit(InputField ui, LuaFunction call)
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
            return setScrollViewOnItemClick(ui, call);
        }

        public bool SetScrollViewOnItemClick(string cName, LuaFunction call)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return setScrollViewOnItemClick(ui, call);
        }

        private static bool setScrollViewOnItemClick(ScrollView ui, LuaFunction call)
        {
            if (null != ui)
            {
                ui.SetOnItemClickLua(call);
                return true;
            }
            return false;
        }

        public bool SetScrollViewDatas(int index, LuaTable table)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return setScrollViewDatas(ui, table);
        }

        public bool SetScrollViewDatas(string cName, LuaTable table)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return setScrollViewDatas(ui, table);
        }

        private static bool setScrollViewDatas(ScrollView ui, LuaTable table)
        {
            if (null != ui)
            {
                ui.SetDatas(table);
                return true;
            }
            return false;
        }

        //SetOnItemClickLua

        public bool SetScrollViewDatas(int index, List<UIItemData> datas)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return setScrollViewDatas(ui, datas);
        }

        public bool SetScrollViewDatas(string cName, List<UIItemData> datas)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return setScrollViewDatas(ui, datas);
        }

        private static bool setScrollViewDatas(ScrollView ui, List<UIItemData> datas)
        {
            if (null != ui)
            {
                ui.SetDatas(datas);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 跟新某位置的数据
        /// 由于C#部分UpdateData需要两个参数，这里只导出lua相关的方法
        /// </summary>
        /// <returns><c>true</c>, if scroll view data was updataed, <c>false</c> otherwise.</returns>
        /// <param name="index">Index.</param>
        /// <param name="table">Table.</param>
        public bool UpdateScrollViewData(int index, LuaTable table)
        {
            ScrollView ui = GetCompByIndex<ScrollView>(index);
            return updateScoriewData(ui, table);
        }

        /// <summary>
        /// 跟新某位置的数据
        /// 由于C#部分UpdateData需要两个参数，这里只导出lua相关的方法
        /// </summary>
        /// <returns><c>true</c>, if scroll view data was updataed, <c>false</c> otherwise.</returns>
        /// <param name="cName">C name.</param>
        /// <param name="table">Table.</param>
        public bool UpdateScrollViewData(string cName, LuaTable table)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return updateScoriewData(ui, table);
        }

        private static bool updateScoriewData(ScrollView ui, LuaTable table)
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
            return addScrollViewData(ui, table);
        }

        public bool AddScrollViewData(string cName, LuaTable table)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return addScrollViewData(ui, table);
        }

        private static bool addScrollViewData(ScrollView ui, LuaTable table)
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
            return addScrollViewData(ui, data);
        }

        public bool AddScrollViewData(string cName, UIItemData data)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return addScrollViewData(ui, data);
        }

        private static bool addScrollViewData(ScrollView ui, UIItemData data)
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
            return insertScrollViewData(ui, table);
        }

        public bool InsertScrollViewData(string cName, LuaTable table)
        {
            ScrollView ui = GetCompByName<ScrollView>(cName);
            return insertScrollViewData(ui, table);
        }

        private static bool insertScrollViewData(ScrollView ui, LuaTable table)
        {
            if (null != ui)
            {
                ui.InsertData(table);
                return true;
            }
            return false;
        }
        #endregion

        #region Slider
        public float GetSliderValue(int index)
        {
            Slider ui = GetCompByIndex<Slider>(index);
            return getSliderValue(ui);
        }

        public float GetSliderValue(string cName)
        {
            Slider ui = GetCompByName<Slider>(cName);
            return getSliderValue(ui);
        }

        private static float getSliderValue(Slider ui)
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
            return setSliderValue(slider, value);
        }

        public bool SetSliderValue(string cName, float value)
        {
            Slider slider = GetCompByName<Slider>(cName);
            return setSliderValue(slider, value);
        }

        public bool AddOnSliderValueChanged(int index, UnityAction<float> call)
        {
            Slider slider = GetCompByIndex<Slider>(index);
            return addOnSliderValueChanged(slider, call);
        }

        public bool AddOnSliderValueChanged(string cName, UnityAction<float> call)
        {
            Slider slider = GetCompByName<Slider>(cName);
            return addOnSliderValueChanged(slider, call);
        }

        public bool AddOnSliderValueChanged(int index, LuaFunction call)
        {
            Slider slider = GetCompByIndex<Slider>(index);
            return addOnSliderValueChanged(slider, call);
        }

        public bool AddOnSliderValueChanged(string cName, LuaFunction call)
        {
            Slider slider = GetCompByName<Slider>(cName);
            return addOnSliderValueChanged(slider, call);
        }
        #endregion Slider

        #region Toggle
        public bool GetToggleIsOn(int index)
        {
            Toggle ui = GetCompByIndex<Toggle>(index);
            return getToggleIsOn(ui);
        }

        public bool GetToggleIsOn(string cName)
        {
            Toggle ui = GetCompByName<Toggle>(cName);
            return getToggleIsOn(ui);
        }

        private static bool getToggleIsOn(Toggle ui)
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
            return setToggleIsOn(ui, isOn);
        }

        public bool SetToggleIsOn(string cName, bool isOn)
        {
            Toggle ui = GetCompByName<Toggle>(cName);
            return setToggleIsOn(ui, isOn);
        }

        private static bool setToggleIsOn(Toggle ui, bool isOn)
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
            return addToggleOnValueChanged(ui, call);
        }

        public bool AddToggleOnValueChanged(string cName, UnityAction<bool> call)
        {
            Toggle ui = GetCompByName<Toggle>(cName);
            return addToggleOnValueChanged(ui, call);
        }

        private static bool addToggleOnValueChanged(Toggle ui, UnityAction<bool> call)
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
            return addToggleOnValueChanged(ui, call);
        }

        public bool AddToggleOnValueChanged(string cName, LuaFunction call)
        {
            Toggle ui = GetCompByName<Toggle>(cName);
            return addToggleOnValueChanged(ui, call);
        }

        private static bool addToggleOnValueChanged(Toggle ui, LuaFunction call)
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
        //TODO:ToggleGroup 以后有需要实现

        #region Scrollbar
        public float GetScrollbarValue(int index)
        {
            Scrollbar ui = GetCompByIndex<Scrollbar>(index);
            return getScrollbarValue(ui);
        }

        public float GetScrollbarValue(string cName)
        {
            Scrollbar ui = GetCompByName<Scrollbar>(cName);
            return getScrollbarValue(ui);
        }

        private static float getScrollbarValue(Scrollbar ui)
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
            return setScrollbarValue(slider, value);
        }

        public bool SetScrollbarValue(string cName, float value)
        {
            Scrollbar slider = GetCompByName<Scrollbar>(cName);
            return setScrollbarValue(slider, value);
        }

        public bool AddOnScrollbarValueChanged(int index, UnityAction<float> call)
        {
            Scrollbar slider = GetCompByIndex<Scrollbar>(index);
            return addOnScrollbarValueChanged(slider, call);
        }

        public bool AddOnScrollbarValueChanged(string cName, UnityAction<float> call)
        {
            Scrollbar slider = GetCompByName<Scrollbar>(cName);
            return addOnScrollbarValueChanged(slider, call);
        }

        public bool AddOnScrollbarValueChanged(int index, LuaFunction call)
        {
            Scrollbar slider = GetCompByIndex<Scrollbar>(index);
            return addOnScrollbarValueChanged(slider, call);
        }

        public bool AddOnScrollbarValueChanged(string cName, LuaFunction call)
        {
            Scrollbar slider = GetCompByName<Scrollbar>(cName);
            return addOnScrollbarValueChanged(slider, call);
        }

        public bool SetScrollbarSize(int index, float size)
        {
            Scrollbar ui = GetCompByIndex<Scrollbar>(index);
            return setScrollbarSize(ui, size);
        }

        public bool SetScrollbarSize(string cName, float size)
        {
            Scrollbar ui = GetCompByName<Scrollbar>(cName);
            return setScrollbarSize(ui, size);
        }

        private static bool setScrollbarSize(Scrollbar ui, float size)
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
            return setScrollbarStepNumber(ui, num);
        }

        public bool SetScrollbarStepNumber(string cName, int num)
        {
            Scrollbar ui = GetCompByName<Scrollbar>(cName);
            return setScrollbarStepNumber(ui, num);
        }

        private static bool setScrollbarStepNumber(Scrollbar ui, int num)
        {
            if (null != ui)
            {
                if(num < 3)
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

        private static bool setUIActive(UIBehaviour ui, bool value)
        {
            if (null != ui)
            {
                ui.gameObject.SetActive(value);
                return true;
            }
            return false;
        }

        private static bool setUISelectable(Selectable ui, bool value)
        {
            if (null != ui)
            {
                ui.interactable = value;
                return true;
            }
            return false;
        }

        private static bool setTextStr(Text text, string content)
        {
            if (null != text)
            {
                text.text = content;
                return true;
            }
            return false;
        }

        private static bool setTextColor(Text text, Color color)
        {
            if (null != text)
            {
                text.color = color;
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

        private static bool addBtnClick(Button btn, UnityAction<string> call)
        {
            if (null != btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(()=>call(btn.name));
                return true;
            }
            return false;
        }

        private static bool addBtnClick(Button btn, LuaFunction call)
        {
            if (null != btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (null != call)
                    {
                        //TODO:想办法在Destroy时释放call（调用Dispose方法）
                        call.Call(btn.name);
                    }
                });
                return true;
            }
            return false;
        }

        private static bool setSliderValue(Slider slider, float value)
        {
            if (null != slider)
            {
                slider.value = value;
                return true;
            }
            return false;
        }

        private static bool addOnSliderValueChanged(Slider slider, UnityAction<float> call)
        {
            if(null != slider)
            {
                slider.onValueChanged.AddListener(call);
            }
            return false;
        }

        private static bool addOnSliderValueChanged(Slider slider, LuaFunction call)
        {
            if (null != slider)
            {
                slider.onValueChanged.AddListener((float value) => 
                {
                    if(null != call)
                    {
                        //TODO:释放
                        call.Call<float>(value);
                    }
                });
            }
            return false;
        }

        private static bool setScrollbarValue(Scrollbar slider, float value)
        {
            if (null != slider)
            {
                slider.value = value;
                return true;
            }
            return false;
        }

        private static bool addOnScrollbarValueChanged(Scrollbar slider, UnityAction<float> call)
        {
            if (null != slider)
            {
                slider.onValueChanged.AddListener(call);
            }
            return false;
        }

        private static bool addOnScrollbarValueChanged(Scrollbar slider, LuaFunction call)
        {
            if (null != slider)
            {
                slider.onValueChanged.AddListener((float value) =>
                {
                    if (null != call)
                    {
                        //TODO:释放
                        call.Call<float>(value);
                    }
                });
            }
            return false;
        }
        #endregion private 方法

        #region =====UI 获取，私有
        private Text getText(int index)
        {
            return GetCompByIndex<Text>(index);
        }

        private Text getText(string cName)
        {
            return GetCompByName<Text>(cName);
        }


        private Image getImage(int index)
        {
            return GetCompByIndex<Image>(index);
        }

        private Image getImage(string cName)
        {
            return GetCompByName<Image>(cName);
        }


        private RawImage getRawImage(int index)
        {
            return GetCompByIndex<RawImage>(index);
        }

        private RawImage getRawImage(string cName)
        {
            return GetCompByName<RawImage>(cName);
        }


        private Button getButton(int index)
        {
            return GetCompByIndex<Button>(index);
        }

        private Button getButton(string cName)
        {
            return GetCompByName<Button>(cName);
        }


        private Toggle getToggle(int index)
        {
            return GetCompByIndex<Toggle>(index);
        }

        private Toggle getToggle(string cName)
        {
            return GetCompByName<Toggle>(cName);
        }


        private Slider getSlider(int index)
        {
            return GetCompByIndex<Slider>(index);
        }

        private Slider getSlider(string cName)
        {
            return GetCompByName<Slider>(cName);
        }


        private Scrollbar getScrollbar(int index)
        {
            return GetCompByIndex<Scrollbar>(index);
        }

        private Scrollbar getScrollbar(string cName)
        {
            return GetCompByName<Scrollbar>(cName);
        }


        private Dropdown getDropdown(int index)
        {
            return GetCompByIndex<Dropdown>(index);
        }

        private Dropdown getDropdown(string cName)
        {
            return GetCompByName<Dropdown>(cName);
        }


        private InputField getInputField(int index)
        {
            return GetCompByIndex<InputField>(index);
        }

        private InputField getInputField(string cName)
        {
            return GetCompByName<InputField>(cName);
        }


        private Canvas getCanvas(int index)
        {
            return GetCompByIndex<Canvas>(index);
        }

        private Canvas getCanvas(string cName)
        {
            return GetCompByName<Canvas>(cName);
        }

        /// <summary>
        /// Panel是特殊的Image，如果删除Image组件则不能获取，使用子组件的transform.parent获取其RectTransform
        /// </summary>
        /// <returns>The panel.</returns>
        /// <param name="index">Index.</param>
        private Image getPanel(int index)
        {
            return GetCompByIndex<Image>(index);
        }

        /// <summary>
        /// Panel是特殊的Image，如果删除Image组件则不能获取，使用子组件的transform.parent获取其RectTransform
        /// </summary>
        /// <returns>The panel.</returns>
        /// <param name="cName">C name.</param>
        private Image getPanel(string cName)
        {
            return GetCompByName<Image>(cName);
        }


        private ScrollRect getScrollRect(int index)
        {
            return GetCompByIndex<ScrollRect>(index);
        }

        private ScrollRect getScrollRect(string cName)
        {
            return GetCompByName<ScrollRect>(cName);
        }
        #endregion =====UI 获取，私有

    }
}

        //// 正则
        //public bool \1\2\3(int index, \4 \5)
        //{
        //    \2 ui = GetCompByIndex<\2>(index);
        //    return \6\2\3(ui, \5);
        //}

        //public bool \1\2\3(string cName, \4 \5)
        //{
        //    \2 ui = GetCompByName<\2>(cName);
        //    return \6\2\3(ui, \5);
        //}

        //private static bool \6\2\3(\2 ui, \4 \5)
        //{
        //    if(null != ui)
        //    {
        //        //
        //        return true;
        //    }
        //    return false;
        //}

                ////case "\1\2\3":
                    ////if (uiIndex != -1)
                    ////{
                    ////    return \1\2\3(uiIndex, (\4) data.Content);
                    ////}
                    ////else
                    ////{
                    ////    return \1\2\3(uiName, (\4) data.Content);
                    ////}
                    //////break;

//Set,Image,Sprite,Sprite,sprite,set
//(\S+),(\S+),(\S+),(\S+),(\S+),(\S+)