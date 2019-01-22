using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameFramework;
using UnityEngine.EventSystems;

public class ScrollviewTest2 : MonoBehaviour {

    public UIHandler handler;

    public ScrollView mScrollView;
    private int mFunc = 1;

    // Use this for initialization
    void Start()
    {
        if (null != mScrollView)
        {
            mScrollView.SetOnItemClickDelegate((int index) =>
            {
                Debug.Log("OnItemClick: " + index);
                mScrollView.Tween2Index(index);
                switch(mFunc)
                {
                    case 1:
                        mScrollView.SelectItem(index);
                        break;
                    case 2:
                        mScrollView.UnselectItem(index);
                        break;
                    case 3:
                        mScrollView.SwitchItem(index);
                        break;
                }
            });

            mScrollView.SetOnSelectChangeCall((int[] obj) => 
            {
                string s = "";
                for (int i = 0; i < obj.Length; i++)
                {
                    s += obj[i] + ",";
                }
                LogFile.Log(s);
                //Debug.Log(obj);
            });
            StartCoroutine(SetDatas());
        }
    }

    private IEnumerator SetDatas()
    {
        //yield return new WaitForSeconds(2);
        yield return null;
        if (null != mScrollView)
        {
            List<UIItemData> datas = new List<UIItemData>();
            for (int i = 0; i < 30; i++)
            {
                List<UIHandlerData> data = new List<UIHandlerData>();
                data.Add(new UIHandlerData("SetTextString", 0, "Item" + i));
                var d = new UIItemData(data);
                //d.Info = "Item" + i;
                datas.Add(d);
            }
            mScrollView.SetData(datas);
            //foreach (var item in new int[] { 40, 38, 28, 15, 5, 0 })
            //{

            //    yield return new WaitForSeconds(5);
            //    Debug.LogWarning(item);
            //    mScrollView.SetCurIndex(item);
            //}
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnScroll(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }


    public void All()
    {
        mScrollView.SelectAll();
    }

    public void None()
    {
        mScrollView.UnselectAll();
    }

    public void SelectMuilti()
    {
        mScrollView.SelectItems(new int[] { 1, 2, 3, 4, 5 });
    }

    public void UnselectMuilti()
    {
        mScrollView.UnselectItems(new int[] {  2, 3, 4 });
    }

    public void Select()
    {
        mFunc = 1;
    }

    public void Unselect()
    {
        mFunc = 2;

    }

    public void Switch()
    {
        mFunc = 3;

    }
    //void test()
    //{
    //    LuaTable lTabble = new LuaTable();
    //}
}
