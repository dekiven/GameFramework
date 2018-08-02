using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace GameFramework
{
	public class TestAtlas : MonoBehaviour
    {

        public Image[] images;
        private bool hasLoad = false;

        void Start()
        {
            GameSpriteAtlasManager.Instance.SetCurGroup("test");
            GameSpriteAtlasManager.Instance.Load("res/sprites/test", "TestAtlas");
        }

        void Update()
        {
            if (!hasLoad)
            {
                SpriteAtlas atlas = GameSpriteAtlasManager.Instance.Get("res/sprites/test", "TestAtlas");
                if(null != atlas)
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        images[i].sprite = atlas.GetSprite((i + 1).ToString());
                        images[i].SetNativeSize();
                    }
                    hasLoad = true;

                    Debug.LogFormat("atlas.GetSprite:[{0}]",atlas.GetSprite("1"));
                    GameSpriteAtlasManager.Instance.ClearGroup("test");
                }
            }
        }
	}
}