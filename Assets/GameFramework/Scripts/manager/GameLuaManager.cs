using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class GameLuaManager : SingletonComp<GameLuaManager>
    {
        private LuaState lua;
        private GameLuaLoader loader;
        private LuaLooper loop = null;

        public override bool Dispose()
        {
            base.Dispose();
            Close();
            return true;
        }

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
            lua.DoFile("GameMain.lua");

            LuaFunction main = lua.GetFunction("GameMain");
            main.Call();
        }

        /// <summary>
        /// 调用已经require或者doFile的lua全局函数（无参数）
        /// </summary>
        /// <param name="funcName">Func name.</param>
        public void CallGlobalFunc(string funcName)
        {
            LuaFunction func = lua.GetFunction(funcName);
            if(null != func)
            {
                func.Call();
                func.Dispose();
            }
            else
            {
                LogFile.Warn("global lua func \"{0}\" do not found!");
            }
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

            //没有使用CJson暂时屏蔽
            //this.OpenCJson();
        }

        /// <summary>
        /// 初始化Lua代码加载路径
        /// </summary>
        void InitLuaPath()
        {
            //仅在不使用assetsbundle的时候才需要配置lua searchpath
            if (!GameConfig.Instance.useAsb)
            {
                string rootPath = Tools.PathCombine(Application.dataPath, "ToLua");
                lua.AddSearchPath(rootPath + "/Lua");
                lua.AddSearchPath(rootPath + "/ToLua/Lua");
                lua.AddSearchPath(Tools.GetLuaSrcPath());
            }
        }

        /// <summary>
        /// 初始化LuaBundle
        /// </summary>
        void InitBaseLuaBundle()
        {
            if (loader.beZip)
            {
                loader.AddBundle("lua.unity3d");
                //loader.AddBundle("lua_math.unity3d");
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
                //loader.AddBundle("lua_framework.unity3d");
                //loader.AddBundle("lua_messageparser.unity3d");
                //loader.AddBundle("lua_netparser.unity3d");
                ////test
                //loader.AddBundle("lua_test.unity3d");
                //loader.AddBundle("lua_framework.unity3d");
            }

        }

        /// <summary>
        /// 添加Lua Assetbundle 文件
        /// </summary>
        /// <param name="asbName">Asb name.</param>
        public void AddLuaBundle(string asbName)
        {
            if(loader.beZip)
            {
                loader.AddBundle(asbName);
            }
        }

        /// <summary>
        /// 添加多个lua AssetBundle 文件
        /// </summary>
        /// <param name="names">Names.</param>
        public void AddLuaBundles(string[] names)
        {
            foreach (string n in names)
            {
                AddLuaBundle(n);
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

}