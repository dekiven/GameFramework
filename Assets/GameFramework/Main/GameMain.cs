using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameFramework;

public class GameMain : MonoBehaviour {

    //GameManager mGameManager;

	// Use this for initialization
	void Start () {
        //加载GameManager单例组件
        var gm = GameManager.Instance;
        gm.UpdateViewResPath = "DownloadView";
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
