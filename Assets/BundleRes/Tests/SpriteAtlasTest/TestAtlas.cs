using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace GameFramework
{
	public class TestAtlas : MonoBehaviour
    {
        const string asb = "Tests/SpriteAtlasTest";
        public Image[] images;
        private bool hasLoad = false;

        void Start()
        {
            GameSpriteAtlasManager.Instance.SetCurGroup("test");
            GameSpriteAtlasManager.Instance.Load(asb, "TestAtlas");
        }

        void Update()
        {
            if (!hasLoad)
            {
                SpriteAtlas atlas = GameSpriteAtlasManager.Instance.Get(asb, "TestAtlas");
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