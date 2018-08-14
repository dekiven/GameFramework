using LuaInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace GameFramework
{
    public class GameSpriteAtlasManager : SingletonComp<GameSpriteAtlasManager>, IResHandler<SpriteAtlas>
    {
        #region private 属性
        private GameResHandler<SpriteAtlas> mSpriteDict;
        #endregion

        #region MonoBehaviour
        void Awake()
        {
            mSpriteDict = new GameResHandler<SpriteAtlas>("SpriteAtlas");
            mSpriteDict.Suffix = ".spriteatlas";
        }
        #endregion

        #region IResHandler
        public void Load(string asbName, string atlasName)
        {
            mSpriteDict.Load(asbName, atlasName);
        }

        public void Load(string asbName, string[] resNames)
        {
            mSpriteDict.Load(asbName, resNames);
        }

        public SpriteAtlas Get(string asbName, string atlasName)
        {
            return mSpriteDict.Get(asbName, atlasName);
        }

        public void SetCurGroup(string group)
        {
            mSpriteDict.CurGroup = group;
        }

        public void ClearGroup(string group)
        {
            mSpriteDict.ClearGroup(group);
        }
        #endregion

        public void GetAtlasSync(string asbName, string atlasName, Action<SpriteAtlas> callbcak, LuaFunction luaCall)
        {
            mSpriteDict.GetSync(asbName, atlasName, callbcak, luaCall);
        }
    }
}
