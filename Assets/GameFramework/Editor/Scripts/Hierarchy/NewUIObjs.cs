using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameFramework
{
    using DRes = DefaultControls.Resources;

    [InitializeOnLoad]
    public class NewUIObjs
    {

        private const string kUILayerName = "UI";

        [MenuItem("GameObject/UI/GF/ScrollView", false, 1501)]
        public static void CreateAScrollView(MenuCommand menuCommand)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameFramework/Editor/Prefabs/UI/ScrollView.prefab");
            GameObject obj = Object.Instantiate(prefab);
            obj.name = "ScrollView";
            //obj.transform.SetParent(Selection.activeGameObject.transform, false);
            placeUIElementRoot(obj, menuCommand);
        }

        [MenuItem("GameObject/UI/GF/ScrollItem", false, 1502)]
        public static void CreateAScrollItem(MenuCommand menuCommand)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameFramework/Editor/Prefabs/UI/ScrollItem.prefab");
            GameObject obj = Object.Instantiate(prefab);
            obj.name = "ScrollItem";
            //obj.transform.SetParent(Selection.activeGameObject.transform, false);
            placeUIElementRoot(obj, menuCommand);
        }

        [MenuItem("GameObject/UI/GF/ScrollSelector", false, 1503)]
        public static void CreateAScrollSelector(MenuCommand menuCommand)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameFramework/Editor/Prefabs/UI/ScrollSelector.prefab");
            GameObject obj = Object.Instantiate(prefab);
            obj.name = "ScrollSelector";
            //obj.transform.SetParent(Selection.activeGameObject.transform, false);
            placeUIElementRoot(obj, menuCommand);
        }

        [MenuItem("GameObject/UI/GF/SelectorToggles", false, 1504)]
        public static void CreateASelectorToggles(MenuCommand menuCommand)
        {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameFramework/Editor/Prefabs/UI/SelectorToggles.prefab");
                GameObject obj = Object.Instantiate(prefab);
                obj.name = "SelectorToggles";
                //obj.transform.SetParent(Selection.activeGameObject.transform, false);
                placeUIElementRoot(obj, menuCommand);
        }

        [MenuItem("GameObject/UI/GF/UIView", false, 1600)]
        public static void CreateUIView(MenuCommand menuCommand)
        {
            GameObject gameObject = GFUICreateControls.CreatUIView(new DRes());
            placeUIElementRoot(gameObject, menuCommand);

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localPosition = new Vector3(0, 0, 1800);
        }

        [MenuItem("GameObject/UI/GF/UIWorld", false, 1601)]
        public static void CreateUIWorld(MenuCommand menuCommand)
        {
            GameObject gameObject = GFUICreateControls.CreatUIWorld(new DRes());
            placeUIElementRoot(gameObject, menuCommand);

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        #region 私有
        private static void placeUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = getOrCreateCanvasGameObject();
            }

            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (parent != menuCommand.context) // not a context click, so center in sceneview
                setPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
        }

        private static void setPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            // Find the best scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }

        // Helper function that returns a Canvas GameObject; preferably a parent of the selection, or other existing Canvas.
        static private GameObject getOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use just any canvas..
            canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in the scene at all? Then create a new one.
            return createNewUI();
        }

        static private GameObject createNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer(kUILayerName);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            // if there is no event system add one...
            createEventSystem(false);
            return root;
        }

        private static void createEventSystem(bool select)
        {
            createEventSystem(select, null);
        }

        private static void createEventSystem(bool select, GameObject parent)
        {
            var esys = Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }
        #endregion
    }
   
}