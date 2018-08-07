using System;
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
    /// ScrollRect
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

        #region Text
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

        private static bool setRawImageRect(RawImage ui, float[] rect)
        {
            if (null != ui && 4 == rect.Length)
            {
                Rect _rect = Tools.GenRect(rect);

                ui.uvRect = _rect;
                ui.SetNativeSize();
                return true;
            }
            return false;
        }
        #endregion RawImage

        #region Button
        public bool AddBtnClick(int index, UnityAction call)
        {
            Button btn = GetCompByIndex<Button>(index);
            return addBtnClick(btn, call);
        }

        public bool AddBtnClick(int index, LuaFunction call)
        {
            Button btn = GetCompByIndex<Button>(index);
            return addBtnClick(btn, call);
        }

        public bool AddBtnClick(string cName, UnityAction call)
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
        public string GetInputFieldText(int index)
        {
            InputField ui = GetCompByIndex<InputField>(index);
            return getInputFieldText(ui);
        }

        public string GetInputFieldText(string cName)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return getInputFieldText(ui);
        }

        private static string getInputFieldText(InputField ui)
        {
            if (null != ui)
            {
                return ui.text;
            }
            return null;
        }
        public bool AddDropdownOnValueChange(int index, UnityAction<int> call)
        {
            Dropdown ui = GetCompByIndex<Dropdown>(index);
            return addDropdownOnValueChange(ui, call);
        }

        public bool AddDropdownOnValueChange(string cName, UnityAction<int> call)
        {
            Dropdown ui = GetCompByName<Dropdown>(cName);
            return addDropdownOnValueChange(ui, call);
        }

        private static bool addDropdownOnValueChange(Dropdown ui, UnityAction<int> call)
        {
            if (null != ui)
            {
                ui.onValueChanged.AddListener(call);
                return true;
            }
            return false;
        }

        public bool AddDropdownOnValueChange(int index, LuaFunction call)
        {
            Dropdown ui = GetCompByIndex<Dropdown>(index);
            return addDropdownOnValueChange(ui, call);
        }

        public bool AddDropdownOnValueChange(string cName, LuaFunction call)
        {
            Dropdown ui = GetCompByName<Dropdown>(cName);
            return addDropdownOnValueChange(ui, call);
        }

        private static bool addDropdownOnValueChange(Dropdown ui, LuaFunction call)
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
        public bool AddInputFieldOnValueChange(int index, UnityAction<string> call)
        {
            InputField ui = GetCompByIndex<InputField>(index);
            return addInputFieldOnValueChange(ui, call);
        }

        public bool AddInputFieldOnValueChange(string cName, UnityAction<string> call)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return addInputFieldOnValueChange(ui, call);
        }

        private static bool addInputFieldOnValueChange(InputField ui, UnityAction<string> call)
        {
            if (null != ui)
            {
                ui.onValueChanged.AddListener(call);
                return true;
            }
            return false;
        }

        public bool AddInputFieldOnValueChange(int index, LuaFunction call)
        {
            InputField ui = GetCompByIndex<InputField>(index);
            return addInputFieldOnValueChange(ui, call);
        }

        public bool AddInputFieldOnValueChange(string cName, LuaFunction call)
        {
            InputField ui = GetCompByName<InputField>(cName);
            return addInputFieldOnValueChange(ui, call);
        }

        private static bool addInputFieldOnValueChange(InputField ui, LuaFunction call)
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

        #region ScrollRect

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

        public bool AddOnSliderValueChange(int index, UnityAction<float> call)
        {
            Slider slider = GetCompByIndex<Slider>(index);
            return addOnSliderValueChange(slider, call);
        }

        public bool AddOnSliderValueChange(string cName, UnityAction<float> call)
        {
            Slider slider = GetCompByName<Slider>(cName);
            return addOnSliderValueChange(slider, call);
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

        public bool AddOnScrollbarValueChange(int index, UnityAction<float> call)
        {
            Scrollbar slider = GetCompByIndex<Scrollbar>(index);
            return addOnScrollbarValueChange(slider, call);
        }

        public bool AddOnScrollbarValueChange(string cName, UnityAction<float> call)
        {
            Scrollbar slider = GetCompByName<Scrollbar>(cName);
            return addOnScrollbarValueChange(slider, call);
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

        private static bool addBtnClick(Button btn, UnityAction call)
        {
            if (null != btn)
            {
                btn.onClick.AddListener(call);
                return true;
            }
            return false;
        }

        private static bool addBtnClick(Button btn, LuaFunction call)
        {
            if (null != btn)
            {
                btn.onClick.AddListener(() =>
                {
                    if (null != call)
                    {
                        //TODO:想办法在Destroy时释放call（调用Dispose方法）
                        call.Call();
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

        private static bool addOnSliderValueChange(Slider slider, UnityAction<float> call)
        {
            if(null != slider)
            {
                slider.onValueChanged.AddListener(call);
            }
            return false;
        }

        private static bool addOnSliderValueChange(Slider slider, LuaFunction call)
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

        private static bool addOnScrollbarValueChange(Scrollbar slider, UnityAction<float> call)
        {
            if (null != slider)
            {
                slider.onValueChanged.AddListener(call);
            }
            return false;
        }

        private static bool addOnScrollbarValueChange(Scrollbar slider, LuaFunction call)
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