using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameFramework
{

    public class ScrollViewTest : MonoBehaviour 
    {
        public ScrollView mScrollView;

        // Use this for initialization
        void Start () {
            StartCoroutine(SetDatas());
        }

        private IEnumerator SetDatas()
        {
            yield return new WaitForSeconds(2);
            if (null != mScrollView)
            {
                List<ScrollItemData> datas = new List<ScrollItemData>();
                for (int i = 0; i < 43; i++)
                {
                    var d = new ScrollItemData();
                    d.Info = "Item" + i;
                    datas.Add(d);
                }
                mScrollView.SetDatas(datas);
            }
        }

        // Update is called once per frame
        void Update () {
            
        }
    }
}
