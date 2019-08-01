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
            EditorApplication.hierarchyWindowItemOnGUI = _onHierarchyGUI;
        }

        private static Rect _getRect(Rect selectionrect, float width, ref float total, float offset=1)
        {
            Rect rect = new Rect(selectionrect);
            //rect.x = selectionrect.width - width - total;
            rect.x = selectionrect.width - total;
            total += width + offset;
            rect.width = width;
            return rect;
        }

        private static void _drawIcon<T>(T obj, Rect rect) where T : UnityEngine.Object
        {
            Texture icon = EditorGUIUtility.ObjectContent(obj, typeof(T)).image;
            GUI.Label(rect, icon);
        }

        private static void _drawLabel(string text, Rect rect)
        {
            GUI.Label(rect, new GUIContent(text));
        }

        private static void _onHierarchyGUI(int instanceid, Rect selectionrect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
            if(null == obj)
            {
                return;
            }

            if (null != CurUIHandler) 
            {
                float height = selectionrect.height;
                float total = 0f;
                obj.SetActive(GUI.Toggle(_getRect(selectionrect, height, ref total, 5), obj.activeSelf, string.Empty));
                if( null != CurUIHandler.UIArray && CurUIHandler.UIArray.Count > 0)
                {
                    for (int i = 0; i < CurUIHandler.UIArray.Count; i++)
                    {
                        UIBehaviour ui = CurUIHandler.UIArray[i];
                        if(null != ui && ui.gameObject.Equals(obj))
                        {
                            _drawLabel("" + i, _getRect(selectionrect, height+2, ref total));
                            _drawIcon(ui, _getRect(selectionrect, height, ref total));
                        }
                    }
                }

                if (null != CurUIHandler.SubHandlers && CurUIHandler.SubHandlers.Count > 0) 
                {
                    for (int i = 0; i < CurUIHandler.SubHandlers.Count; i++)
                    {
                        UIHandler ui = CurUIHandler.SubHandlers[i];
                        if(null != ui && ui.gameObject.Equals(obj))
                        {
                            _drawLabel(""+i, _getRect(selectionrect, height+4, ref total, 10));
                            _drawLabel("sub:  ", _getRect(selectionrect, height*2, ref total));
                        }
                    }
                }
            }
        }
    }
}
