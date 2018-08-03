using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameFramework;
using System;

public class TestUI : MonoBehaviour {
    private GameUIManager mMgr;
    bool hasInit = false;
    void Awake()
    {
        mMgr = GameUIManager.Instance;
    }

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if(mMgr.HasInit && !hasInit)
        {
            mMgr.ShowView("res/UI/test", "TestUI.prefab");
            hasInit = true;

            StartCoroutine(showAnotherUI());
        }
	}

    private IEnumerator showAnotherUI()
    {
        yield return new WaitForSeconds(3);
        mMgr.ShowView("res/UI/test", "TestUI2");
        yield return new WaitForSeconds(5);
        mMgr.ShowView("res/UI/test", "TestUI3");
        //mMgr.ClearAllUI();
        yield return new WaitForSeconds(5);
        mMgr.PopView();
        yield return new WaitForSeconds(5);
        mMgr.PopView();
    }
}
