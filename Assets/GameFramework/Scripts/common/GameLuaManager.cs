﻿using LuaFramework;
using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLuaManager : MonoBehaviour
{

    //单例模式组件 begin----------------------------------------------
    private static volatile GameLuaManager sInstance;
    private static object syncRoot = new object();
    public static GameLuaManager Instance
    {
        get
        {
            if (sInstance == null)
            {
                lock (syncRoot)
                {
                    if (sInstance == null)
                    {
                        GameLuaManager[] instances = FindObjectsOfType<GameLuaManager>();
                        if (instances != null)
                        {
                            for (var i = 0; i < instances.Length; i++)
                            {
                                Destroy(instances[i].gameObject);
                            }
                        }
                        GameObject go = new GameObject();
                        go.name = typeof(GameLuaManager).ToString();
                        sInstance = go.AddComponent<GameLuaManager>();
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return sInstance;
        }
    }
    //单例模式组件 end================================================

    private LuaState lua;
    private GameLuaLoader loader;
    private LuaLooper loop = null;

    // Use this for initialization
    void Awake()
    {
        loader = new GameLuaLoader();
        lua = new LuaState();
        this.OpenLibs();
        lua.LuaSetTop(0);

        LuaBinder.Bind(lua);
        DelegateFactory.Init();
        LuaCoroutine.Register(lua, this);
    }

    public void InitStart()
    {
        InitLuaPath();
        InitBaseLuaBundle();
        //TODO:发布的游戏会在资源更新之后再调用这个方法，甚至可以动态读取Bundle
        InitGameLuaBundle();
        this.lua.Start();    //启动LUAVM
        this.StartMain();
        this.StartLooper();
    }

    void StartLooper()
    {
        loop = gameObject.AddComponent<LuaLooper>();
        loop.luaState = lua;
    }

    //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
    protected void OpenCJson()
    {
        lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
        lua.OpenLibs(LuaDLL.luaopen_cjson);
        lua.LuaSetField(-2, "cjson");

        lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
        lua.LuaSetField(-2, "cjson.safe");
    }

    void StartMain()
    {
        lua.DoFile("Main.lua");

        LuaFunction main = lua.GetFunction("Main");
        main.Call();
        //main.Dispose();
        //main = null;    
    }

    /// <summary>
    /// 初始化加载第三方库
    /// </summary>
    void OpenLibs()
    {
        lua.OpenLibs(LuaDLL.luaopen_pb);
        lua.OpenLibs(LuaDLL.luaopen_sproto_core);
        lua.OpenLibs(LuaDLL.luaopen_protobuf_c);
        lua.OpenLibs(LuaDLL.luaopen_lpeg);
        lua.OpenLibs(LuaDLL.luaopen_bit);
        lua.OpenLibs(LuaDLL.luaopen_socket_core);

        this.OpenCJson();
    }

    /// <summary>
    /// 初始化Lua代码加载路径
    /// </summary>
    void InitLuaPath()
    {
        if (AppConst.DebugMode)
        {
            string rootPath = AppConst.FrameworkRoot;
            lua.AddSearchPath(rootPath + "/Lua");
            lua.AddSearchPath(rootPath + "/ToLua/Lua");
        }
        else
        {
            lua.AddSearchPath(Util.DataPath + "lua");
        }
#if UNITY_EDITOR
        //Test
        lua.AddSearchPath(Y3Tools.GetLuaSrcPath());
#endif
    }

    /// <summary>
    /// 初始化LuaBundle
    /// </summary>
    void InitBaseLuaBundle()
    {
        if (loader.beZip)
        {
            loader.AddBundle("lua.unity3d");
            loader.AddBundle("lua_math.unity3d");
            loader.AddBundle("lua_system.unity3d");
            loader.AddBundle("lua_system_reflection.unity3d");
            loader.AddBundle("lua_unityengine.unity3d");
            loader.AddBundle("lua_common.unity3d");
            loader.AddBundle("lua_logic.unity3d");
            loader.AddBundle("lua_view.unity3d");
            loader.AddBundle("lua_controller.unity3d");
            loader.AddBundle("lua_misc.unity3d");

            loader.AddBundle("lua_protobuf.unity3d");
            loader.AddBundle("lua_3rd_cjson.unity3d");
            loader.AddBundle("lua_3rd_luabitop.unity3d");
            loader.AddBundle("lua_3rd_pbc.unity3d");
            loader.AddBundle("lua_3rd_pblua.unity3d");
            loader.AddBundle("lua_3rd_sproto.unity3d");

        }
    }

    public void InitGameLuaBundle()
    {
        //游戏逻辑
        if (loader.beZip)
        {
            loader.AddBundle("lua_framework.unity3d");
            loader.AddBundle("lua_messageparser.unity3d");
            loader.AddBundle("lua_netparser.unity3d");
            //test
            loader.AddBundle("lua_test.unity3d");
            //loader.AddBundle("lua_framework.unity3d");
        }

    }

    public void DoFile(string filename)
    {
        lua.DoFile(filename);
    }

    public object[] CallFunction(string funcName, params object[] args)
    {
        LuaFunction func = lua.GetFunction(funcName);
        if (func != null)
        {
            //int oldTop = func.BeginPCall();
            //for (int i = 0; i < args.Length; i++)
            //{
            //    func.PushGeneric(args[i]);
            //}

            //func.PCall();
            //TODO：返回值
            //object ret1 = func.CheckObjects(oldTop);
            //func.EndPCall();
            //return ret1;
            return func.LazyCall(args);
        }
        return null;
    }

    public void LuaGC()
    {
        lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
    }

    public void Close()
    {
        if (loop != null)
        {
            loop.Destroy();
            loop = null;
        }

        if (lua != null)
        {
            lua.Dispose();
            lua = null;
        }


        loader = null;
    }
}
