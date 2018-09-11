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
        bool mIsSelecting = false;
        int mSelectingIndex = -1;

        void OnEnable()
        {
            mTarget = target as UIHandler;
            mListProperty = serializedObject.FindProperty("UIArray");
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
                                        if(-1 == index)
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
            if(!list.Equals(mListProperty))
            {
                return;
            }

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            EditorGUIUtility.ShowObjectPicker<UIBehaviour>(null, true, "", controlID);
            mIsSelecting = true;
            mSelectingIndex = index + 1;
        }
    }
}