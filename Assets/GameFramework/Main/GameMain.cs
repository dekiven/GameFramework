using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameFramework;

public class GameMain : MonoBehaviour {

    //GameManager mGameManager;
    [Tooltip("程序启动界面在Resources的路径，在此界面检查资源和app版本，更新完成后调用GameManager.Instance.StartGameLogic()启动游戏")]
    public string DownloadViewPath = "DownloadView";

	// Use this for initialization
	void Start () {
        //加载GameManager单例组件
        var gm = GameManager.Instance;
        gm.UpdateViewResPath = DownloadViewPath;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
