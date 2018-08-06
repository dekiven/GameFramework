using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    public class EditorTools
    {
        ///// <summary>
        ///// 清理Consle日志
        ///// </summary>
        //[MenuItem("GameFramework/Clear Log Command %#_c")]
        //private static void ClearConsole()
        //{
        //    Type logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        //    MethodInfo clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        //    clearMethod.Invoke(null, null);
        //}

        /// <summary>
        /// 保存obj所在的prefab
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>返回是否保存成功</returns>
        public static bool SavePrefab(GameObject obj)
        {
            UnityEngine.Object prefab = PrefabUtility.GetPrefabParent(obj);
            if (null != prefab)
            {
                PrefabUtility.ReplacePrefab(PrefabUtility.FindPrefabRoot(obj), prefab, ReplacePrefabOptions.ConnectToPrefab);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 返回obj是否在prefab中
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsPrefab(GameObject obj)
        {
            return PrefabUtility.GetPrefabType(obj) == PrefabType.PrefabInstance;
        }

        /// <summary>
        /// 获取某个视图（EditorWindow）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static EditorWindow GetWindow(Type type)
        {
            EditorWindow window = null;
            if (null != type)
            {
                window = EditorWindow.GetWindow(type);
            }
            return window;
        }

        /// <summary>
        /// 获取某个视图（EditorWindow）
        /// </summary>
        /// <param name="typeStr"></param>
        /// <returns></returns>
        public static EditorWindow GetWindow(string typeStr)
        {
            EditorWindow window = null;
            var type = typeof(EditorWindow).Assembly.GetType(typeStr);
            if (null != type)
            {
                window = EditorWindow.GetWindow(type);
            }
            return window;
        }

        /// <summary>
        /// 使某个视图获取焦点
        /// </summary>
        /// <param name="typeStr"></param>
        public static void FocusWindow(string typeStr)
        {
            EditorWindow window = GetWindow(typeStr);
            if (null != window)
            {
                window.Focus();
            }
        }

        /// <summary>
        /// 使某个视图获取焦点
        /// </summary>
        /// <param name="type"></param>
        public static void FocusWindow(Type type)
        {
            EditorWindow window = GetWindow(type);
            if (null != window)
            {
                window.Focus();
            }
        }

        /// <summary>
        /// 使某个视图获取焦点
        /// </summary>
        /// <param name="view"></param>
        public static void FocusWindow(EditorViews view)
        {
            EditorWindow window = GetWindow(GetViewTypeStr(view));
            if (null != window)
            {
                window.Focus();
            }
        }

        /// <summary>
        /// 展开或关闭Inspector中某类基类组件
        /// </summary>
        /// <param name="types">组件Editor的Type字符串列表</param>
        /// <param name="visible">是否展开</param>
        /// <param name="contraOther">是否将没有在types中的组件做相反的操作，传false保持不变</param>
        public static void SetInspectorTrackerVisible(string[] types, bool visible, bool contraOther = true)
        {
            var windowType = typeof(EditorWindow).Assembly.GetType(GetViewTypeStr(EditorViews.InspectorWindow));
            var window = GetWindow(windowType);
            System.Reflection.FieldInfo info = windowType.GetField("m_Tracker", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ActiveEditorTracker tracker = info.GetValue(window) as ActiveEditorTracker;
            var editors = tracker.activeEditors;
            for (int i = 0; i < editors.Length; i++)
            {
                bool isSame = false;
                foreach (var type in types)
                {
                    isSame = editors[i].GetType().ToString().Equals(type);
                    if (isSame)
                    {
                        tracker.SetVisible(i, isSame && visible ? 1 : 0);
                        break;
                    }
                }
                if (!isSame && contraOther)
                {
                    tracker.SetVisible(i, isSame && visible ? 1 : 0);
                }
            }
        }

        /// <summary>
        /// 展开或关闭Inspector中某类基类组件
        /// </summary>
        /// <param name="type">组件Editor的Type字符串</param>
        /// <param name="visible">是否展开</param>
        /// <param name="contraOther">是否将没有在types中的组件做相反的操作，传false保持不变</param>
        public static void SetInspectorTrackerVisible(string type, bool visible, bool contraOther = true)
        {
            SetInspectorTrackerVisible(new string[] { type, }, visible, contraOther);
        }

        public static Editor GetInspectorEditor(string type)
        {
            var windowType = typeof(EditorWindow).Assembly.GetType(GetViewTypeStr(EditorViews.InspectorWindow));
            var window = GetWindow(windowType);
            System.Reflection.FieldInfo info = windowType.GetField("m_Tracker", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ActiveEditorTracker tracker = info.GetValue(window) as ActiveEditorTracker;
            var editors = tracker.activeEditors;
            for (int i = 0; i < editors.Length; i++)
            {
                bool isSame = false;
                Editor editor = editors[i];
                isSame = editor.GetType().ToString().Equals(type);
                if (isSame)
                {
                    return editor;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据枚举类型获取反射Editor view 类的string
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static string GetViewTypeStr(EditorViews view)
        {
            switch (view)
            {
                case EditorViews.GameView:
                    return "UnityEditor.GameView";
                case EditorViews.SceneView:
                    return "UnityEditor.SceneView";
                case EditorViews.InspectorWindow:
                    return "UnityEditor.InspectorWindow";
                case EditorViews.ProjectBrowser:
                    return "UnityEditor.ProjectBrowser";
                case EditorViews.SceneHierarchyWindow:
                    return "UnityEditor.SceneHierarchyWindow";
                case EditorViews.AnimationWindow:
                    return "UnityEditor.AnimationWindow";
                case EditorViews.AssetStoreWindow:
                    return "UnityEditor.AssetStoreWindow";
                case EditorViews.AnimatorControllerTool:
                    return "UnityEditor.Graphs.AnimatorControllerTool";
                case EditorViews.NavMeshEditorWindow:
                    return "UnityEditor.NavMeshEditorWindow";
            }
            return null;
        }

        ///// <summary>
        ///// 按下Ctrl+w（win32）或command+w（mac）输出当前获得焦点的Window Type名字
        ///// </summary>
        //[MenuItem("Test/WindowTypeName Command %w")]
        //public static void DebugWindowName()
        //{
        //    Debug.LogWarning(EditorWindow.focusedWindow.ToString());
        //}
    }

    /// <summary>
    /// Editor中常用视图（Game、Scene、Inspector等）枚举
    /// </summary>
    public enum EditorViews
    {
        GameView,
        SceneView,
        InspectorWindow,
        ProjectBrowser,
        SceneHierarchyWindow,
        AnimationWindow,
        AssetStoreWindow,
        AnimatorControllerTool,
        NavMeshEditorWindow,
    }
}