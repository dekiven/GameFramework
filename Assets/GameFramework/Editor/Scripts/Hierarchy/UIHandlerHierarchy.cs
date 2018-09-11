using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameFramework
{
    [InitializeOnLoad]
    public class UIHandlerHierarchy
    {
        public static UIHandler CurUIHandler = null; 

        static UIHandlerHierarchy()
        {
            EditorApplication.hierarchyWindowItemOnGUI = onHierarchyGUI;
        }

        private static Rect getRect(Rect selectionrect, float width, ref float total, float offset=1)
        {
            Rect rect = new Rect(selectionrect);
            //rect.x = selectionrect.width - width - total;
            rect.x = selectionrect.width - total;
            total += width + offset;
            rect.width = width;
            return rect;
        }

        private static void drawIcon<T>(T obj, Rect rect) where T : UnityEngine.Object
        {
            Texture icon = EditorGUIUtility.ObjectContent(obj, typeof(T)).image;
            GUI.Label(rect, icon);
        }

        private static void drawLabel(string text, Rect rect)
        {
            GUI.Label(rect, new GUIContent(text));
        }

        private static void onHierarchyGUI(int instanceid, Rect selectionrect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
            if(null == obj)
            {
                return;
            }            

            if(null != CurUIHandler && CurUIHandler.UIArray.Count > 0)
            {
                float height = selectionrect.height;
                float total = 0f;
                obj.SetActive(GUI.Toggle(getRect(selectionrect, height, ref total, 5), obj.activeSelf, string.Empty));
                for (int i = 0; i < CurUIHandler.UIArray.Count; i++)
                {
                    UIBehaviour ui = CurUIHandler.UIArray[i];
                    if(null != ui && ui.gameObject.Equals(obj))
                    {
                        drawLabel("" + (i+10), getRect(selectionrect, height+2, ref total));
                        drawIcon(ui, getRect(selectionrect, height, ref total));
                    }
                }
            }

        }
    }
}
