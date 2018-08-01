using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace GameFramework
{
    public class GameSpriteAtlasManager : SingletonComp<GameSpriteAtlasManager>, IResHandler<SpriteAtlas>
    {
        public const string STR_SUFFIX = ".spriteatlas";
        #region private 属性
        private GameResHandler<SpriteAtlas> mSpriteDict;
        #endregion

        #region MonoBehaviour
        void Awake()
        {
            mSpriteDict = new GameResHandler<SpriteAtlas>("SpriteAtlas");
        }
        #endregion

        #region IResHandler
        public void Load(string asbName, string atlasName)
        {
            mSpriteDict.Load(asbName, FixResName(atlasName));
        }

        public void Load(string asbName, string[] resNames)
        {
            for (int i = 0; i < resNames.Length; i++)
            {
                resNames[i] = FixResName(resNames[i]);
            }
            mSpriteDict.Load(asbName, resNames);
        }

        public SpriteAtlas Get(string asbName, string atlasName)
        {
            return mSpriteDict.Get(asbName, FixResName(atlasName));
        }

        public void SetCurGroup(string group)
        {
            mSpriteDict.CurGroup = group;
        }

        public void ClearGroup(string group)
        {
            mSpriteDict.ClearGroup(group);
        }

        public string FixResName(string name)
        {
            if(!name.EndsWith(STR_SUFFIX))
            {
                name = name + STR_SUFFIX;
            }
            return name;
        }
        #endregion

        public void GetAtlasSync(string asbName, string atlasName, Action<SpriteAtlas> callbcak)
        {
            mSpriteDict.GetSync(asbName, atlasName, callbcak);
        }
    }
}
