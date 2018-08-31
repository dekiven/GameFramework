using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFramework
{
    [InitializeOnLoad]
    public static class NewUIObjs
    {
        #region ScrollView
        [MenuItem("GameObject/UI/ScrollView")]
        public static void CreateAScrollView()
        {
            if (null != Selection.activeGameObject)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameFramework/Editor/Prefabs/UI/ScrollView.prefab");
                GameObject obj = Object.Instantiate(prefab);
                obj.name = "ScrollView";
                obj.transform.SetParent(Selection.activeGameObject.transform, false);
            }
        }
        #endregion ScrollView

        #region ScrollItem
        [MenuItem("GameObject/UI/ScrollItem")]
        public static void CreateAScrollItem()
        {
            if (null != Selection.activeGameObject)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameFramework/Editor/Prefabs/UI/ScrollItem.prefab");
                GameObject obj = Object.Instantiate(prefab);
                obj.name = "ScrollItem";
                obj.transform.SetParent(Selection.activeGameObject.transform, false);
            }
        }
        #endregion ScrollItem

        #region ScrollSelector
        [MenuItem("GameObject/UI/ScrollSelector")]
        public static void CreateAScrollSelector()
        {
            if (null != Selection.activeGameObject)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameFramework/Editor/Prefabs/UI/ScrollSelector.prefab");
                GameObject obj = Object.Instantiate(prefab);
                obj.name = "ScrollSelector";
                obj.transform.SetParent(Selection.activeGameObject.transform, false);
            }
        }
        #endregion ScrollSelector
    }
   
}