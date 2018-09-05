using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
namespace GameFramework
{

    public class ScrollViewTest : MonoBehaviour 
    {
        public ScrollView mScrollView;

        // Use this for initialization
        void Start () {
            if(null != mScrollView)
            {
                mScrollView.SetOnItemClickDelegate((int index) =>
                {
                    Debug.Log("OnItemClick: " + index);
                });
                StartCoroutine(SetDatas());
            }
        }

        private IEnumerator SetDatas()
        {
            yield return new WaitForSeconds(2);
            if (null != mScrollView)
            {
                List<UIItemData> datas = new List<UIItemData>();
                for (int i = 0; i < 43; i++)
                {
                    List<UIHandlerData> data = new List<UIHandlerData>();
                    data.Add(new UIHandlerData("SetTextString", 0, "Item" + i));
                    var d = new UIItemData(data);
                    //d.Info = "Item" + i;
                    datas.Add(d);
                }
                mScrollView.SetData(datas);
                foreach (var item in new int[]{40, 38, 28, 15, 5,0})
                {

                    yield return new WaitForSeconds(5);
                    Debug.LogWarning(item);
                    mScrollView.SetCurIndex(item);
                }
            }
        }

        // Update is called once per frame
        void Update () {
            
        }

        //void test()
        //{
        //    LuaTable lTabble = new LuaTable();
        //}

    }
}
