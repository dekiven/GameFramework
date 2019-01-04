using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{

    /// <summary>
    /// 可以在游戏世界变更位置的UI，一般放到游戏对象上
    /// </summary>
    public class UIWorld : UIBase
    {
        [HideInInspector]
        public Transform UITarget;
        [HideInInspector]
        public Vector3 UIOffset = Vector3.up;
        //public new bool IsBillboard = true;


        void Awake()
        {
            RenderMode = RenderMode.WorldSpace;
            IsBillboard = true;
            IsStatic = false;
            IsInStack = false;
            HideBefor = false;
        }

        protected override void update()
        {
            base.update();
            if (null != UITarget)
            {
                transform.position = UITarget.position + UIOffset;
            }
        }

    }
}
