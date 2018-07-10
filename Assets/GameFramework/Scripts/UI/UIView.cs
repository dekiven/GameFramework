﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    /// <summary>
    /// 通常UI，一般用来显示界面等
    /// </summary>
    public class UIView : UIBase
    {
        //默认使用大小等于屏幕的Panel作为View的Root
        protected override void init()
        {
            base.init();
            RectTransform trans = GetComponent<RectTransform>();
            if (Vector2.zero == trans.anchorMin && Vector2.one == trans.anchorMax)
            {
                //当大小锚点为（0,0）和（1,1）时，表示Root是跟屏幕一样大，这个时候要设置top=0， right= 0，否则显示不正常
                LogFile.Warn(trans.anchoredPosition.ToString());
                trans.offsetMax = Vector2.zero;
            }
        }

        public Vector2 CalcScreenPosFromWorld(Vector3 wPos, RectTransform rect)
        {
            Vector2 pos = CalcScreenPosFromWorld(wPos, rect.rect, rect.pivot);
            rect.position = new Vector3(pos.x, pos.y, 0);
            return pos;
        }

        /// <summary>
        /// 计算给出的3d坐标、ui矩形、ui旋转轴后能将UI完全显示的位置
        /// </summary>
        /// <param name="wPos">3d坐标</param>
        /// <param name="rect">ui矩形</param>
        /// <param name="pivot">ui旋转轴</param>
        /// <returns></returns>
        public Vector2 CalcScreenPosFromWorld(Vector3 wPos, Rect rect, Vector2 pivot)
        {
            Vector2 pos = Camera.main.WorldToScreenPoint(wPos);
            pos.x = Mathf.Clamp(pos.x, rect.width * pivot.x, Screen.width - rect.width * (1 - pivot.x));
            pos.y = Mathf.Clamp(pos.y, rect.height * pivot.y, Screen.height - rect.height * (1 - pivot.y));
            return pos;
        }
    }
}