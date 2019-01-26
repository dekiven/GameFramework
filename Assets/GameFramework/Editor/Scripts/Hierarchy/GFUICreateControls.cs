//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class GFUICreateControls : MonoBehaviour {

//	// Use this for initialization
//	void Start () {
		
//	}
	
//	// Update is called once per frame
//	void Update () {
		
//	}
//}

using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    using DRes = DefaultControls.Resources;
    public static class GFUICreateControls
    {
        public static GameObject CreatUIView(DRes resources)
        {
            GameObject obj = DefaultControls.CreatePanel(resources);

            obj.AddComponent<UIView>().RenderMode = RenderMode.ScreenSpaceCamera;
            obj.name = "pl";
            return obj;
        }

        public static GameObject CreatUIWorld(DRes resources)
        {
            GameObject obj = DefaultControls.CreateImage(resources);
            obj.AddComponent<UIWorld>();
            obj.name = "pl";
            return obj;
        }
    }
}