using LuaInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace GameFramework
{
    //TODO:dekiven 修改为单纯的单例
    public class GameSpriteAtlasManager : Singleton<GameSpriteAtlasManager>, IResHandler<SpriteAtlas>
    {
        #region private 属性
        private GameResHandler<SpriteAtlas> mSpriteDict;
        #endregion

        /// <summary>
        /// 请勿直接调用构造函数，请使用 Instance 方法获取单例
        /// </summary>
        public GameSpriteAtlasManager()
        {
            mSpriteDict = new GameResHandler<SpriteAtlas>("SpriteAtlas");
            mSpriteDict.Suffix = ".spriteatlas";
            mSpriteDict.OnReleaseCallback = (ref SpriteAtlas s) => 
            {
                Resources.UnloadAsset(s);
                s = null;
            };
        }

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

        public void GetAtlasAsync(string asbName, string atlasName, Action<SpriteAtlas> callbcak, LuaFunction luaCall=null)
        {
            mSpriteDict.GetAsync(asbName, atlasName, callbcak, luaCall);
        }

        public void GetSpriteAsync(string asbName, string atlasName, string spriteName,Action<Sprite> callbcak, LuaFunction luaCall=null)
        {
            mSpriteDict.GetAsync(asbName, atlasName, (SpriteAtlas atlas)=>
            {
                Sprite sprite = null;
                if(null != atlas)
                {
                    sprite = atlas.GetSprite(spriteName);
                }
                if(null != callbcak)
                {
                    callbcak(sprite);
                }
                if(null != luaCall)
                {
                    luaCall.Call(sprite);
                    luaCall.Dispose();
                }
            });
        }
    }
}
