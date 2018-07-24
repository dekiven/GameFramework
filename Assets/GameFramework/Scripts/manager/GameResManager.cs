﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using UnityEngine.SceneManagement;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UObj = UnityEngine.Object;
//载入方式参考LuaFramework_UGUI->ResourceManager.cs
//github: https://github.com/jarjin/LuaFramework_UGUI

namespace GameFramework
{
    public class GameResManager : SingletonComp<GameResManager>
    {
        //逻辑 begin ------------------------------------------------------------
        List<string> mAllBundles = null;
        AssetBundleManifest m_AssetBundleManifest = null;
        Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();
        Dictionary<string, AssetBundleInfo> m_LoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
        Dictionary<string, List<LoadAssetRequest>> m_LoadRequests = new Dictionary<string, List<LoadAssetRequest>>();

        string mResPath = "";

        public void Initialize(Action initOK, string manifestName = GameConfig.STR_ASB_MANIFIST)
        {
            mResPath = Tools.GetResPath();
            if (GameConfig.Instance.useAsb)
            {
                //这里由于manifest所在的bundle没有后缀名，所以直接走LoadAsset
                LoadAsset<AssetBundleManifest>(manifestName, new string[] { "AssetBundleManifest" }, delegate (UObj[] objs)
                {
                    if (objs.Length > 0)
                    {
                        m_AssetBundleManifest = objs[0] as AssetBundleManifest;
                        mAllBundles = new List<string>(m_AssetBundleManifest.GetAllAssetBundles());
                    }
                    if (initOK != null) initOK();
                });
            }
            else
            {
                if (initOK != null) initOK();
            }
        }

        /// <summary>
        /// 从Assetbundle或者原始资源中加载一个资源，编辑器和正式游戏均使用本函数加载资源
        /// 注意：lua回调是gameobjectlist，c#是gameobject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asbPath"></param>
        /// <param name="resName"></param>
        /// <param name="action"></param>
        public void LoadRes<T>(string asbPath, string resName, Action<UObj> action = null, LuaFunction luaFunc = null) where T : UObj
        {
            //#if UNITY_EDITOR
            if (!GameConfig.Instance.useAsb)
            {
                loadRes<T>(
                    asbPath
                    , new string[] { resName, }
                    , delegate (UObj[] objs)
                    {
                        if (null != action && objs.Length == 1)
                        {
                            action(objs[0]);
                        }else if (null != action && objs.Length == 0 && string.IsNullOrEmpty(resName))
                        {
                            action(null);
                        }
                    }
                    , luaFunc
                );

            }
            else
            {
                string asbName = Tools.GetAsbName(asbPath);
                if (!mAllBundles.Contains(asbName))
                {
                    asbName = Tools.GetAsbName(asbPath, true);
                }
                LoadAsset<T>(
                    asbName
                    , new string[] { resName, }
                    , delegate (UObj[] objs)
                    {
                    //LogFile.Log("LoadRes -> loadAsset 回调，asbName:{0}, obj.length:{1}", asbName, objs.Length);
                        if (null != action && objs.Length == 1)
                        {
                            action(objs[0]);
                        }else if(null != action && objs.Length == 0 && string.IsNullOrEmpty(resName))
                        {
                            action(null);
                        }
                    }
                    , luaFunc
                );
            }
        }

        /// <summary>
        /// 从Assetbundle或者原始资源中加载多个资源，编辑器和正式游戏均使用本函数加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asbName"></param>
        /// <param name="names">文件名需带后缀</param>
        /// <param name="action"></param>
        public void LoadRes<T>(string asbName, string[] names, Action<UObj[]> action = null, LuaFunction luaFunc = null) where T : UObj
        {
            if (!GameConfig.Instance.useAsb)
            {
                loadRes<T>(asbName, names, action, luaFunc);
            }
            else
            {
                asbName = Tools.GetAsbName(asbName);
                LoadAsset<T>(asbName, names, action, luaFunc);
            }
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="asbName"></param>
        /// <param name="sceneName"></param>
        /// <param name="callbcak"></param>
        /// <param name="luaFunc"></param>
        public void LoadScene(string asbName, string sceneName, Action<bool> callbcak = null, LuaFunction luaFunc = null)
        {
            string scenePath = Tools.GetResInAssetsName(asbName, sceneName);
#if UNITY_EDITOR
            if (!GameConfig.Instance.useAsb)
            {
                //Tools.RelativeTo(Tools.GetResPath(Tools.PathCombine(asbName, sceneName)), Application.dataPath, true);
                Debug.LogWarning(scenePath);
                int index =  SceneUtility.GetBuildIndexByScenePath(scenePath);
                Debug.LogWarning(index);

                bool rst = index >= 0;
                if(rst)
                {
                    SceneManager.LoadScene(index);
                }
                if (null != callbcak)
                {
                    callbcak(rst);
                }
                if (null != luaFunc)
                {
                    luaFunc.Call<bool>(rst);
                    luaFunc.Dispose();
                }
                return;
            }
#endif
            LoadRes<UObj>(asbName, string.Empty
            , delegate (UObj obj)
            {
                //TODO:根据是否获取到asb判断
                AssetBundleInfo info = GetLoadedAssetBundle(Tools.GetAsbName(asbName));
                bool rst = false;

                //LogFile.Log("obj is null:{0}", rst);
                if(null != info)
                {
                    //int idx = sceneName.LastIndexOf('/');
                    //if(idx > 0)
                    //{
                    //    sceneName = sceneName.Substring(idx + 1);
                    //}
                    string[] scenes = info.m_AssetBundle.GetAllScenePaths();
                    for (int i = 0; i < scenes.Length; ++i)
                    {
                        string s = scenes[i];
                        //LogFile.Log("Scenename {0}: {1}, inputName:{2}", i, s, scenePath);
                        if (s.Equals(scenePath))
                        {
                            SceneManager.LoadScene(s);
                            rst = true;
                            //LogFile.Log("找到名字相同的scene，break");
                            break;
                        }
                    }
                    if(!rst)
                    {
                        LogFile.Error("LoadScene加载Assetbundl:{0},查找{1}失败！！", asbName, scenePath);
                    }
                }
                else
                {
                    LogFile.Error("LoadScene找不到Assetbundle：{0}", asbName);
                }
                if (null != callbcak)
                {
                    callbcak(rst);
                }
                if (null != luaFunc)
                {
                    luaFunc.Call<bool>(obj);
                    luaFunc.Dispose();
                }
            });

        }

        /// <summary>
        /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="isThorough"></param>
        public void UnloadAssetBundle(string abName, bool isThorough = false)
        {
            abName = Tools.GetAsbName(abName);
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName);
            UnloadAssetBundleInternal(abName, isThorough);
            UnloadDependencies(abName, isThorough);
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName);
        }

        #region 私有方法
        //public void LoadAsset<T>(string abName, string[] assetNames, Action<UObj[]> action = null) where T : UObj
        void LoadAsset<T>(string abName, string[] assetNames, Action<UObj[]> action = null, LuaFunction func = null) where T : UObj
        {
            string path = abName;
            // manifest Assetbundle文件没有后缀，文件名跟release的Assetbundle根目录同名
            bool isManifest = string.Equals(GameConfig.STR_ASB_MANIFIST, path);
            if (path.EndsWith(GameConfig.STR_ASB_EXT))
            {
                //isManifest = false;
                path = path.Substring(0, path.Length - GameConfig.STR_ASB_EXT.Length);
            }
            if (!isManifest)
            {
                List<string> names = new List<string>();
                foreach (var item in assetNames)
                {
                    if(!string.IsNullOrEmpty(item))
                    {
                        //names.Add(Tools.PathCombine("Assets/" + GameConfig.STR_RES_FOLDER, path, item));
                        names.Add(Tools.GetResInAssetsName(path, item));
                    }
                }
                assetNames = names.ToArray();
            }
            else
            {
                abName = abName + GameConfig.STR_ASB_EXT;
            }

            LoadAssetRequest request = new LoadAssetRequest();
            request.assetType = typeof(T);
            request.assetNames = assetNames;
            request.luaFunc = func;
            request.sharpFunc = action;

            List<LoadAssetRequest> requests = null;
            if (!m_LoadRequests.TryGetValue(abName, out requests))
            {
                requests = new List<LoadAssetRequest>();
                requests.Add(request);
                m_LoadRequests.Add(abName, requests);
                StartCoroutine(onLoadAsset<T>(abName));
            }
            else
            {
                requests.Add(request);
            }
        }

        IEnumerator onLoadAsset<T>(string abName) where T : UObj
        {
            AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abName);
            if (bundleInfo == null)
            {
                yield return StartCoroutine(OnLoadAssetBundle(abName, typeof(T)));

                bundleInfo = GetLoadedAssetBundle(abName);
                if (bundleInfo == null)
                {
                    m_LoadRequests.Remove(abName);
                    Debug.LogError("OnLoadAsset--->>>" + abName);
                    yield break;
                }
            }
            List<LoadAssetRequest> list = null;
            if (!m_LoadRequests.TryGetValue(abName, out list))
            {
                m_LoadRequests.Remove(abName);
                yield break;
            }
            for (int i = 0; i < list.Count; i++)
            {
                string[] assetNames = list[i].assetNames;
                List<UObj> result = new List<UObj>();

                AssetBundle ab = bundleInfo.m_AssetBundle;
                for (int j = 0; j < assetNames.Length; j++)
                {
                    string assetPath = assetNames[j];
                    if(!string.IsNullOrEmpty(assetPath))
                    {
                        AssetBundleRequest request = ab.LoadAssetAsync(assetPath, list[i].assetType);
                        yield return request;
                        result.Add(request.asset);
                    }else
                    {
                        result.Add(null);
                    }
                    ////TODO:UnloadAsset
                    //Resources.UnloadAsset(request.asset);
                }
                if (list[i].sharpFunc != null)
                {
                    //LogFile.Log("call c# func of {0}, result.Count:{1}", abName, result.Count);
                    list[i].sharpFunc(result.ToArray());
                    list[i].sharpFunc = null;
                }
                if (list[i].luaFunc != null)
                {
                    list[i].luaFunc.Call((object)result.ToArray());
                    list[i].luaFunc.Dispose();
                    list[i].luaFunc = null;
                }
                bundleInfo.m_ReferencedCount++;
            }
            m_LoadRequests.Remove(abName);
        }

        IEnumerator OnLoadAssetBundle(string abName, Type type)
        {
            string url = Tools.GetAsbUrl(abName);

            WWW download = null;
            if (type == typeof(AssetBundleManifest))
                download = new WWW(url);
            else
            {
                string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                if (dependencies.Length > 0)
                {
                    m_Dependencies.Add(abName, dependencies);
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        string depName = dependencies[i];
                        AssetBundleInfo bundleInfo = null;
                        if (m_LoadedAssetBundles.TryGetValue(depName, out bundleInfo))
                        {
                            bundleInfo.m_ReferencedCount++;
                        }
                        else if (!m_LoadRequests.ContainsKey(depName))
                        {
                            yield return StartCoroutine(OnLoadAssetBundle(depName, type));
                        }
                    }
                }
                download = WWW.LoadFromCacheOrDownload(url, m_AssetBundleManifest.GetAssetBundleHash(abName), 0);
            }
            yield return download;

            AssetBundle assetObj = download.assetBundle;
            if (assetObj != null)
            {
                m_LoadedAssetBundles.Add(abName, new AssetBundleInfo(assetObj));
            }
        }

        AssetBundleInfo GetLoadedAssetBundle(string abName)
        {
            AssetBundleInfo bundle = null;
            m_LoadedAssetBundles.TryGetValue(abName, out bundle);
            if (bundle == null) return null;

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies))
                return bundle;

            // Make sure all dependencies are loaded
            foreach (var dependency in dependencies)
            {
                AssetBundleInfo dependentBundle;
                m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
                if (dependentBundle == null) return null;
            }
            return bundle;
        }

        void UnloadDependencies(string abName, bool isThorough)
        {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies)
            {
                UnloadAssetBundleInternal(dependency, isThorough);
            }
            m_Dependencies.Remove(abName);
        }

        void UnloadAssetBundleInternal(string abName, bool isThorough)
        {
            AssetBundleInfo bundle = GetLoadedAssetBundle(abName);
            if (bundle == null) return;

            if (--bundle.m_ReferencedCount <= 0)
            {
                if (m_LoadRequests.ContainsKey(abName))
                {
                    return;     //如果当前AB处于Async Loading过程中，卸载会崩溃，只减去引用计数即可
                }
                bundle.m_AssetBundle.Unload(isThorough);
                m_LoadedAssetBundles.Remove(abName);
                Debug.Log(abName + " has been unloaded successfully");
            }
        }

        /// <summary>
        /// 异步读取原始资源，只能在Editor模式 的情况下使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">资源相对于Assets/BundleRes的路径</param>
        /// <param name="assetNames">资源相对于path的路径，若资源在path下，则是文件名</param>
        /// <param name="action">delegate</param>
        private void loadRes<T>(string path, string[] assetNames, Action<UObj[]> action = null, LuaFunction luaFunc = null) where T : UObj
        {
            string fullPath = Tools.GetResPath(path);
            if (!Directory.Exists(fullPath))
            {
                fullPath = Directory.GetParent(path).FullName;
                path = Tools.RelativeTo(fullPath, mResPath);
            }
            List<string> names = new List<string>();
            foreach (var name in assetNames)
            {
                //if (!string.IsNullOrEmpty(name))
                //{
                names.Add(Tools.PathCombine("Assets/" + GameConfig.STR_RES_FOLDER, path, name));
                //}
            }
            StartCoroutine(onLoadRes<T>(names.ToArray(), action, luaFunc));
        }

        /// <summary>
        /// 只能在编辑器模式下使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetNames"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private IEnumerator onLoadRes<T>(string[] assetNames, Action<UObj[]> action = null, LuaFunction luaFunc = null) where T : UObj
        {
            List<T> list = new List<T>();
#if UNITY_EDITOR
            foreach (var name in assetNames)
            {
                T t = AssetDatabase.LoadAssetAtPath<T>(name);
                if (t == null)
                {
                    LogFile.Error("加载本地零散资源{0}失败，类型：{1}", name, typeof(T));
                }
                list.Add(t);
                //yield return null;
            }
#endif
            if (null != action)
            {
                action(list.ToArray());
            }
            if (luaFunc != null)
            {
                luaFunc.Call((object)list.ToArray());
                luaFunc.Dispose();
                //luaFunc = null;
            }
            yield return null;
        }

        #endregion

        #region -----------------------方便lua使用的函数 begin--------------------------
        //
        /// <summary>
        /// 加载资源，注意：lua回调是gameobjectlist，c#是gameobject
        /// </summary>
        /// <param name="abName">asb名字（三层的相对文件夹路径)</param>
        /// <param name="name">要load的文件名，全名</param>
        /// <param name="luaFunc">lua回调</param>
        public void LoadGameObj(string abName, string name, LuaFunction luaFunc)
        {
            LoadRes<GameObject>(abName, name, null, luaFunc);
        }

        public void LoadGameObj(string abName, string[] names, LuaFunction luaFunc)
        {
            LoadRes<GameObject>(abName, names, null, luaFunc);
        }

        public void LoadTextAsset(string abName, string name, LuaFunction luaFunc)
        {
            LoadRes<TextAsset>(abName, name, null, luaFunc);
        }

        //  SceneAsset 是Editor的class 打包编译不过，屏蔽
        //    public void LoadSceneAsset(string abName, string name, LuaFunction luaFunc)
        //    {
        //        LoadRes<SceneAsset>(abName, name, null, luaFunc);
        //    }
        //

        public void LoadTextAsset(string abName, string[] names, LuaFunction luaFunc)
        {
            LoadRes<TextAsset>(abName, names, null, luaFunc);
        }

        public void LoadTextAssetBytes(string abName, string name, LuaFunction luaFunc)
        {
            LoadTextAssetBytes(abName, new string[] { name, }, luaFunc);
        }

        public void LoadTextAssetBytes(string abName, string[] names, LuaFunction luaFunc)
        {
            LoadRes<TextAsset>(abName, names, delegate (UObj[] objs)
            {
                List<LuaByteBuffer> list = new List<LuaByteBuffer>();
                if (objs.Length > 0)
                {
                    foreach (var obj in objs)
                    {
                        TextAsset text = obj as TextAsset;
                        if (null != text)
                        {
                            LuaByteBuffer buffer = new LuaByteBuffer(text.bytes);
                            list.Add(buffer);
                        }
                        else
                        {
                            LuaByteBuffer buffer = new LuaByteBuffer();
                            list.Add(buffer);
                        }
                    }
                }
                luaFunc.Call(list.ToArray());
                luaFunc.Dispose();
            });
        }
        #endregion==============================方便lua使用的函数 end=========================

        #region------------------------------辅助类 begin ------------------------------------
        public class AssetBundleInfo
        {
            public AssetBundle m_AssetBundle;
            public int m_ReferencedCount;

            public AssetBundleInfo(AssetBundle assetBundle)
            {
                m_AssetBundle = assetBundle;
                m_ReferencedCount = 0;
            }
        }

        class LoadAssetRequest
        {
            public Type assetType;
            public string[] assetNames;
            public LuaFunction luaFunc;
            public Action<UObj[]> sharpFunc;
        }
        #endregion===============================辅助类 end ======================================
    }
}