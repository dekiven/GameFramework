using System.IO;
using UnityEngine;
using UnityEditor;

namespace GameFramework
{
    public class CacheMgr
    {

        [MenuItem("GameFramework/清理缓存、配置")]
        public static void ClearCache()
        {
            PlayerPrefs.DeleteAll();
            Caching.ClearCache();
            //var path = Tools.GetWriteableDataPath();
            //if(Directory.Exists(path))
            //{
            //    Directory.Delete(path, true);
            //}
            EditorUtility.DisplayDialog("GameFramework", "清理完成！", "确定");
        }
    }
}

