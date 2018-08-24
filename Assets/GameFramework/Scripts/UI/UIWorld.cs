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
        protected Transform mUITarget;
        public Vector3 UIOffset = Vector3.up;
        //public new bool IsBillboard = true;

        public Transform UITarget
        {
            get { return mUITarget; }
            set
            {
                mUITarget = value;
            }
        }

        void Awake()
        {
            RenderMode = RenderMode.WorldSpace;
            IsStatic = false;
            IsInStack = false;
            IsBillboard = true;
            HideBefor = false;
        }

        protected override void update()
        {
            base.update();
            if (null != mUITarget)
            {
                transform.position = mUITarget.position + UIOffset;
            }
        }

    }
}
