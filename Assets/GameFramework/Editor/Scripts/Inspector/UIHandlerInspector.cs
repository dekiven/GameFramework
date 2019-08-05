using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameFramework
{
    [CustomEditor(typeof(UIHandler))]
    public class UIHandlerInspector : Editor
    {
        UIHandler mTarget;
        SerializedProperty mListProperty;
        SerializedProperty mSubProperty;
        SerializedProperty mRTProperty;
        bool mIsSelecting = false;
        int mSelectingIndex = -1;
        static Dictionary<string, string> sTypeDict;

        void OnEnable()
        {
            _loadTypeDict();

            mTarget = target as UIHandler;
            mListProperty = serializedObject.FindProperty("UIArray");
            mSubProperty = serializedObject.FindProperty("SubHandlers");
            mRTProperty = serializedObject.FindProperty("RTArray");
            CustomListInspector.OnElementAdd.AddListener(_onListAdd);
            UIHandlerHierarchy.CurUIHandler = mTarget;
        }

        void OnDisable()
        {
            CustomListInspector.OnElementAdd.RemoveListener(_onListAdd);
            UIHandlerHierarchy.CurUIHandler = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CustomListInspector.Show(mListProperty);
            GUILayout.Space(10);
            CustomListInspector.Show(mSubProperty);
            GUILayout.Space(10);
            CustomListInspector.Show(mRTProperty);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RootTransform"));
            serializedObject.ApplyModifiedProperties();

            Event e = Event.current;
            string commandName = e.commandName;
            if (mIsSelecting && e.type != EventType.Layout && commandName == "ObjectSelectorUpdated")
            {
                _setPickerObj();
            }
            if (mIsSelecting && e.type != EventType.Layout && commandName == "ObjectSelectorClosed")
            {
                mIsSelecting = false;
                _setPickerObj(true);
            }

            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("拷贝Idx到剪贴板(Lua)")))
            {
                _copyIndex2Clipboard(true);
            }

            GUILayout.Space(5);
            if (GUILayout.Button(new GUIContent("拷贝Idx到剪贴板(C#)")))
            {
                _copyIndex2Clipboard(false);
            }
        }

        private void _loadTypeDict()
        {
            if(null == sTypeDict)
            {
                sTypeDict = new Dictionary<string, string>();
            }
        }

        private void _setPickerObj(bool checkMulti = false)
        {
            if (mSelectingIndex >= 0 && mSelectingIndex < mTarget.UIArray.Count)
            {
                GameObject obj = EditorGUIUtility.GetObjectPickerObject() as GameObject;
                if (null != obj)
                {
                    if (checkMulti)
                    {
                        UIBehaviour[] uIBehaviours = obj.GetComponents<UIBehaviour>();
                        if (uIBehaviours.Length > 0)
                        {
                            if (uIBehaviours.Length > 1)
                            {
                                List<UIBehaviour> list = new List<UIBehaviour>();
                                List<string> options = new List<string>();
                                for (int i = 0; i < uIBehaviours.Length; i++)
                                {
                                    UIBehaviour ui = uIBehaviours[i];
                                    if (!mTarget.UIArray.Contains(ui) || mTarget.UIArray.IndexOf(ui) == mSelectingIndex)
                                    {
                                        list.Add(ui);
                                        options.Add(ui.ToString());
                                    }
                                }
                                if (list.Count > 1)
                                {
                                    //TODO:显示选择窗口
                                    SelectWindow.ShowWithOptions(options.ToArray(), (int index) =>
                                    {
                                        //Debug.Log("Selceted:" + index + ",mTarget.UIArray.Count:" + mTarget.UIArray.Count + "ui index:" + mSelectingIndex);
                                        if (-1 == index)
                                        {
                                            mTarget.UIArray.RemoveAt(mSelectingIndex);
                                        }
                                        else
                                        {
                                            mTarget.UIArray[mSelectingIndex] = list[index];
                                        }
                                    });
                                }
                                if (list.Count == 1)
                                {
                                    mTarget.UIArray[mSelectingIndex] = list[0];
                                }
                            }
                            else
                            {
                                mTarget.UIArray[mSelectingIndex] = uIBehaviours[0];
                            }
                        }
                    }
                    else
                    {
                        EditorGUIUtility.PingObject(obj);
                        mTarget.UIArray[mSelectingIndex] = obj.GetComponent<UIBehaviour>();
                    }
                }
                else
                {
                    mTarget.UIArray[mSelectingIndex] = null;
                }
            }
        }

        private void _onListAdd(SerializedProperty list, int index)
        {
            if (!list.Equals(mListProperty))
            {
                return;
            }

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<UIBehaviour>(null, true, "", controlID);
            mIsSelecting = true;
            mSelectingIndex = index + 1;
        }

        private void _copyIndex2Clipboard(bool isLua)
        {
            string s = string.Empty;
            s = s + _getLitsInfo(mTarget.UIArray, "uiIdx", "UIArray index", isLua);
            s = s + _getLitsInfo(mTarget.SubHandlers, "subIdx", "SubHandlers index", isLua);
            s = s + _getLitsInfo(mTarget.RTArray, "rtIdx", "RTArray index", isLua);
            GUIUtility.systemCopyBuffer = s;
        }


        private string _getLitsInfo<T>(List<T> list, string tname, string notes, bool isLua) where T : Component
        {
            StringBuilder s = new StringBuilder();
            if (null != list && list.Count > 0)
            {
                if(isLua)
                {
                    s.Append("-- " + notes + "\nlocal " + tname + " = \n{");
                }
                else
                {
                    s.Append("    #region " + notes);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    T t = list[i];
                    if (null != t)
                    {
                        s.Append(_getComponentInfo(t, i, isLua));
                    }
                }
                if (isLua)
                {
                    s.Append("\n}\n\n");
                }
                else
                {
                    s.Append("\n    #endregion "+ notes + "\n\n");
                }
            }
            return s.ToString();
        }

        private string _getComponentInfo(Component com, int index, bool isLua)
        {
            string name = _getCompName(com, isLua);
            string transName = Tools.GetTransformName(com.transform, null == mTarget.RootTransform ? mTarget.transform : mTarget.RootTransform);
            if(isLua)
            {
               return string.Format("\n    {0, -24} = {1, -4},     -- {2} ({3})", name, index, transName, com.GetType());
            }
            else
            {
               return string.Format("\n    {0, -24} = {1, -4};     // {2} ({3})", name, index, transName, com.GetType());
            }
        }

        private string _getCompName(Component com, bool isLua)
        {
            string name = com.GetType().ToString();
            string tName = name;
            if(!sTypeDict.TryGetValue(tName, out tName))
            {
                var arr = name.Split('.');
                tName = arr[arr.Length - 1];
            }

            name = com.name;
            if (!name.StartsWith(tName, StringComparison.OrdinalIgnoreCase))
            {
                name = tName + name;
            }
            if (isLua)
            {
                return name;
            }
            else
            {
                return "idx" + name;
            }
        }
    }
}