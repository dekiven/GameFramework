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
        private ObjPool<AsbInfo> objPool;

        void Start()
        {
            GameSpriteAtlasManager.Instance.Load("res/sprites/test", "TestAtlas");

            //objPool = new ObjPool<AsbInfo>(delegate (ref AsbInfo info) {
            //    if (null == info)
            //    {
            //        info = new AsbInfo();
            //    }
            //    return true;
            //}, null, null);

        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log(objPool);
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
                }
            }


            //Debug.LogFormat("objPool:{0}", objPool);
            //Debug.LogFormat("objPool.Get:[{0}]", objPool.Get()); 
        }
	}
}