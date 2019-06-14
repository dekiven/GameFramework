using System;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    public class SelectWindow : EditorWindow
    {
        private Action<int> mOnSelected;
        private string[] options;
        private int mSelected = 0;
        private bool mHasKeyDown = false;

        //test
        //[MenuItem("GameFramework/test")]
        public static void Test()
        {
            ShowWithOptions(new string[] { "test1", "test2", "test3", "test4" }, (int i) => { Debug.Log("callback:"+i); });
        }

        public static void ShowWithOptions(string[] options, Action<int> callback)
        {
            SelectWindow window = GetWindow(typeof(SelectWindow)) as SelectWindow;
            window.mHasKeyDown = false;
            window.mOnSelected = callback;
            window.options = options;
            window.Show();
            //window.Focus();
        }

        void OnGUI()
        {
            mSelected = GUILayout.SelectionGrid(mSelected, options, 1);
            checkKeyCodes();
        }

        void checkKeyCodes()
        {
            Event e = Event.current;
            if(null != e)
            {
                if (mHasKeyDown && e.type == EventType.KeyUp)
                {
                    if (e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.PageUp)
                    {
                        mSelected = Mathf.Clamp(--mSelected, 0, options.Length - 1);
                        Repaint();
                    }
                    if (e.keyCode == (KeyCode.DownArrow) || e.keyCode == (KeyCode.PageDown))
                    {
                        mSelected = Mathf.Clamp(++mSelected, 0, options.Length - 1);
                        Repaint();
                    }
                    if (null != mOnSelected)
                    {
                        if (e.keyCode == (KeyCode.Return) || e.keyCode == (KeyCode.KeypadEnter) || e.keyCode == (KeyCode.Space))
                        {
                            mOnSelected(mSelected);
                            Close();
                        }
                        if (e.keyCode == (KeyCode.Escape))
                        {
                            mOnSelected(-1);
                            Close();
                        }
                    }
                }
                if(e.type == EventType.KeyDown)
                {
                    mHasKeyDown = true;
                }
            }
        }
    }
}