using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class GameObjPool : ObjPool<GameObject>
    {
        Transform mParent = null;
        GameObject mPreafab = null;

        public Vector3 RecoverPos = Vector3.zero;
        public bool RecoverByPos = false;

        public GameObjPool(GameObject prefab, Transform parent)
        {
            mPreafab = prefab;
            mParent = parent;

            OnGetCallback = _onGetDefault;
            OnRecoverCallback = _onRecoverDefault;
            OnDisposeCallback = _onDisposeDefault;
        }

        public GameObjPool(GameObject prefab, Transform parent, Vector3 recoverPos)
            : this(prefab, parent)
        {
            RecoverByPos = true;
            RecoverPos = recoverPos;
        }

        bool _onGetDefault(ref GameObject obj)
        {
            bool ret = false;
            if (null == obj)
            {
                if(null != mPreafab && null != mParent)
                {
                    obj = GameObject.Instantiate(mPreafab, mParent);
                    ret = true;
                }
            }
            else
            {
                obj.transform.SetParent(mParent);
                if (RecoverByPos)
                {
                    obj.transform.localPosition = Vector3.zero;
                }
                else
                {
                    obj.SetActive(true);
                }
                ret = true;
            }
            return ret;
        }

        bool _onRecoverDefault(GameObject obj)
        {
            bool ret = false;
            if (null == obj)
            {
                if(RecoverByPos)
                {
                    obj.transform.position = RecoverPos;
                }
                else
                {
                    obj.SetActive(false);
                }
                ret = true;
            }
            
            return ret;
        }

        bool _onDisposeDefault(ref GameObject obj)
        {
            if(null != obj)
            {
                GameObject.Destroy(obj);
                obj = null;
            }
            return true;
        }
    }
}
