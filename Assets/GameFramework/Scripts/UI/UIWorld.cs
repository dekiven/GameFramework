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

        void Awake()
        {
            mRenderMode = RenderMode.WorldSpace;
        }

        public Transform UITarget
        {
            get { return mUITarget; }
            set
            {
                mUITarget = value;
            }
        }

        protected override void init()
        {
            IsBillboard = true;
            IsInStack = false;
            base.init();
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
