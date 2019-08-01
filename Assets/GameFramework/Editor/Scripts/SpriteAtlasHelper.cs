using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

//参考：自动创建SpriteAtlas并设置图集资源
//URL:https://www.jianshu.com/p/bdd223184738
//注意：unity 2017 并不支持对 SpriteAtlas 的编辑操作

#if UNITY_2018
namespace GameFramework
{
    public class SpriteAtlasHelper : UnityEditor.AssetModificationProcessor
    {
        static List<Object> sCurSelectObjs = new List<Object>();

        [MenuItem("Assets/GF/Add 2 SpriteAtlas", priority = 1)]
        public static void Add2SPriteAtlas()
        {
            string[] guids = Selection.assetGUIDs;
            sCurSelectObjs.Clear();
            foreach (var id in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(id);
                Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                LogFile.Warn("path:" + path);
                //LogFile.Log("{0} is native Asset: {1}, is valid folder:{2}", path, AssetDatabase.IsNativeAsset(obj), AssetDatabase.IsValidFolder(path));
                if (AssetDatabase.IsValidFolder(path) || obj as Sprite)
                {
                    sCurSelectObjs.Add(obj);
                }
            }

            string p = EditorUtility.OpenFilePanel("添加到", "Assets/" + GameConfig.STR_RES_FOLDER, "");
            if (string.IsNullOrEmpty(p))
            {
                return;
            }
            p = Tools.RelativeTo(p, Application.dataPath, true);
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(p);
            if (atlas)
            {
                atlas.Add(sCurSelectObjs);
            }
            _clearStatus();

            AssetDatabase.SaveAssets();
        }


        [MenuItem("Assets/GF/new SpriteAtlas", priority = 1)]
        public static void NewSpriteAtlas()
        {
            string[] guids = Selection.assetGUIDs;
            sCurSelectObjs.Clear();
            foreach (var id in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(id);
                Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                LogFile.Warn("path:" + path);
                //LogFile.Log("{0} is native Asset: {1}, is valid folder:{2}", path, AssetDatabase.IsNativeAsset(obj), AssetDatabase.IsValidFolder(path));
                if (AssetDatabase.IsValidFolder(path) || obj as Sprite)
                {
                    sCurSelectObjs.Add(obj);
                }
                
            }
            string p = EditorUtility.SaveFilePanel("新建 SpriteAtlas", "Assets/" + GameConfig.STR_RES_FOLDER, "NewSpriteAtlas", ".spriteatlas");
            if (string.IsNullOrEmpty(p))
            {
                return;
            }
            p = Tools.RelativeTo(p, Application.dataPath, true);

            SpriteAtlas atlas = new SpriteAtlas();
            atlas.Add(sCurSelectObjs);
            AssetDatabase.CreateAsset(atlas, p);

            AssetDatabase.SaveAssets();
            _clearStatus();
        }

        private static void _clearStatus()
        {
            AssetDatabase.Refresh();
            sCurSelectObjs.Clear();
        }
    }
}
#endif