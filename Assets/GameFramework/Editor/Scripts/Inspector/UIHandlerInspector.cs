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
        CustomListInspector listInspector;
        bool mIsSelecting = false;
        int mSelectingIndex = -1;

        void Awake()
        {
            mTarget = target as UIHandler;
            //SerializedProperty mUIArray = 
            listInspector = new CustomListInspector();
            listInspector.OnAddCallback = (int index) =>
            {
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                EditorGUIUtility.ShowObjectPicker<UIBehaviour>(null, true, "", controlID);
                mIsSelecting = true;
                mSelectingIndex = index + 1;
                return true;
            };
        }

        void OnDestroy()
        {
            listInspector = null;
        }
        //void onEnabled()
        //{

        //}

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();
            SerializedProperty list = serializedObject.FindProperty("UIArray");
            listInspector.Show(list);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RootTransform"));
            serializedObject.ApplyModifiedProperties();

            string commandName = Event.current.commandName;
            if (commandName == "ObjectSelectorUpdated" && mIsSelecting)
            {
                //Debug.Log("Update");
                Debug.Log(EditorGUIUtility.GetObjectPickerObject());
                setPickerObj();

            }
            if (commandName == "ObjectSelectorClosed" && mIsSelecting)
            {
                Debug.Log(EditorGUIUtility.GetObjectPickerObject());
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
                    if(checkMulti)
                    {
                        UIBehaviour[] uIBehaviours = obj.GetComponents<UIBehaviour>();
                        if(uIBehaviours.Length > 0)
                        {
                            if(uIBehaviours.Length > 1)
                            {
                                List<UIBehaviour> list = new List<UIBehaviour>();
                                List<string> options = new List<string>();
                                for (int i = 0; i < uIBehaviours.Length; i++)
                                {
                                    UIBehaviour ui = uIBehaviours[i];
                                    if(!mTarget.UIArray.Contains(ui))
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
                                        Debug.Log("Selceted:" + index);
                                    });
                                }
                                if(list.Count == 1)
                                {
                                    mTarget.UIArray[mSelectingIndex] = list[0];
                                }
                                list.Clear();
                                options.Clear();
                            }
                        }
                    }
                }
            }
        }
    }
}