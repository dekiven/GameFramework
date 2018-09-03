using System;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    public class SeclectWindow : EditorWindow
    {
        private Action<int> mOnSelected;
        private string[] options;
        private int mSelected = 0;
        //private bool pressKey = false;

        public static void ShowWithOptions(string[] options, Action<int> callback)
        {
            SeclectWindow window = GetWindow(typeof(SeclectWindow)) as SeclectWindow;
            window.mOnSelected = callback;
            window.options = options;
            window.Show(true);
        }

        void OnGUI()
        {
            //GUILayout.BeginScrollView();
            Debug.Log("SelectionGrid:" + mSelected);
            mSelected = GUILayout.SelectionGrid(mSelected, options, 1);
            //GUILayout.EndScrollView();
        }

        void Update()
        {
            //Debug.Log("update");
            //Event.KeyboardEvent.
            Event e = Event.current;
            if(null != e)
            {
                Debug.Log("update 1");
                if (e.type == EventType.KeyUp)
                {
                    Debug.Log("up:" + e.keyCode);
                    if (e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.PageUp)
                    {
                        mSelected = Mathf.Clamp(--mSelected, 0, options.Length - 1);
                        //Debug.Log("up:" + mSelected);
                    }
                    if (e.keyCode == (KeyCode.DownArrow) || e.keyCode == (KeyCode.PageDown))
                    {
                        mSelected = Mathf.Clamp(++mSelected, 0, options.Length - 1);
                        //Debug.Log("down:" + mSelected);
                    }
                    //mSelected = EditorGUILayout.Popup(mSelected, options);
                    if (null != mOnSelected)
                    {
                        if (e.keyCode == (KeyCode.Return) || e.keyCode == (KeyCode.KeypadEnter) || e.keyCode == (KeyCode.Space))
                        {
                            //Debug.Log("return:" + mSelected);
                            mOnSelected(mSelected);
                        }
                        if (e.keyCode == (KeyCode.Escape))
                        {
                            //Debug.Log("escape:" + mSelected);
                            mOnSelected(-1);
                        }
                    }
                }
                //pressKey = e.isKey;    
            }

        }
    }
}