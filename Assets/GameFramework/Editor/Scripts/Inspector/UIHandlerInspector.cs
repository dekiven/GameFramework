using System.Collections;
using System.Collections.Generic;
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

        void OnEnable()
        {
            mTarget = target as UIHandler;
            mListProperty = serializedObject.FindProperty("UIArray");
            mSubProperty = serializedObject.FindProperty("SubHandlers");
            mRTProperty = serializedObject.FindProperty("RTArray");
            CustomListInspector.OnElementAdd.AddListener(onListAdd);
            UIHandlerHierarchy.CurUIHandler = mTarget;
        }

        void OnDisable()
        {
            CustomListInspector.OnElementAdd.RemoveListener(onListAdd);
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
                setPickerObj();
            }
            if (mIsSelecting && e.type != EventType.Layout && commandName == "ObjectSelectorClosed")
            {
                mIsSelecting = false;
                setPickerObj(true);
            }
            if (GUILayout.Button(new GUIContent("拷贝Index信息到剪贴板")))
            {
                copyIndex2ClipboardLua();
            }
        }

        private void setPickerObj(bool checkMulti = false)
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
                                    SeclectWindow.ShowWithOptions(options.ToArray(), (int index) =>
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

        private void onListAdd(SerializedProperty list, int index)
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

        private void copyIndex2ClipboardLua()
        {
            string s = string.Empty;
            s = s + getLitsInfoLua(mTarget.UIArray, "uiIdx", "UIArray index");
            s = s + getLitsInfoLua(mTarget.SubHandlers, "subIdx", "SubHandlers index");
            s = s + getLitsInfoLua(mTarget.RTArray, "rtIdx", "RTArray index");
            GUIUtility.systemCopyBuffer = s;
        }

        private string getLitsInfoLua<T>(List<T> list, string tname, string notes) where T : Component
        {
            string s = string.Empty;
            if (null != list && list.Count > 0)
            {
                s = "-- " + notes + "\nlocal " + tname + " = \n{";
                for (int i = 0; i < list.Count; i++)
                {
                    T t = list[i];
                    if (null != t)
                    {
                        s = s + getComponentInfoLua(t, i);
                    }
                }
                s = s + "\n}\n\n";
            }
            return s;
        }

        private string getComponentInfoLua(Component com, int index)
        {
            string transName = Tools.GetTransformName(com.transform, null == mTarget.RootTransform ? mTarget.transform : mTarget.RootTransform);
            return "\n\t" + com.name + " = " + index + ",  -- " + transName + " (" + com.GetType() + ")";
        }
    }
}