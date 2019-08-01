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
            SpriteAtlasMgr.Instance.SetCurGroup("test");
            SpriteAtlasMgr.Instance.Load(asb, "TestAtlas");
        }

        void Update()
        {
            if (!hasLoad)
            {
                SpriteAtlas atlas = SpriteAtlasMgr.Instance.Get(asb, "TestAtlas");
                if(null != atlas)
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        images[i].sprite = atlas.GetSprite((i + 1).ToString());
                        images[i].SetNativeSize();
                    }
                    hasLoad = true;

                    Debug.LogFormat("atlas.GetSprite:[{0}]",atlas.GetSprite("1"));
                    SpriteAtlasMgr.Instance.ClearGroup("test");
                }
            }
        }
	}
}