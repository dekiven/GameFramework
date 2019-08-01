using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameFramework;
using System;

public class TestUI : MonoBehaviour {
    const string asb = "Tests/UIManagerTest";
    private UIMgr mMgr;
    bool hasInit = false;
    void Awake()
    {
        mMgr = UIMgr.Instance;
    }

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if(mMgr.HasInit && !hasInit)
        {
            mMgr.ShowView(asb, "TestUI.prefab");
            hasInit = true;

            StartCoroutine(_showAnotherUI());
        }
	}

    private IEnumerator _showAnotherUI()
    {
        yield return new WaitForSeconds(3);
        mMgr.ShowView(asb, "TestUI2");
        yield return new WaitForSeconds(5);
        mMgr.ShowView(asb, "TestUI3");
        //mMgr.ClearAllUI();
        yield return new WaitForSeconds(5);
        mMgr.PopView();
        yield return new WaitForSeconds(5);
        mMgr.PopView();
    }
}
