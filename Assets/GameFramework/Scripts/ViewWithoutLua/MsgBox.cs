using System;
using UnityEngine;

namespace GameFramework
{
    /// <summary>
    /// 默认的MsgBox弹窗UI，该UI必定是IsInStack=true
    /// 支持1~3个按钮
    /// 使用MsgBoxMgr管理
    /// </summary>
    public class MsgBox : UIView
    {
        public const int IdxL = 0;
        public const int IdxM = 1;
        public const int IdxR = 2;

        MsgBoxInfo mInfo;
        private int[] mBtns = { (int)idx.BtnL, (int)idx.BtnM, (int)idx.BtnR, };

        // UIArray index
        enum idx
        {
            Title = 0,      // BG/Title (UnityEngine.UI.Text)
            Msg = 1,        // BG/Msg (UnityEngine.UI.Text)
            BtnL = 2,       // BG/BtnL (UnityEngine.UI.Button)
            BtnTxtL = 3,    // BG/BtnL/BtnTxtL (UnityEngine.UI.Text)
            BtnM = 4,       // BG/BtnM (UnityEngine.UI.Button)
            BtnTxtM = 5,    // BG/BtnM/BtnTxtM (UnityEngine.UI.Text)
            BtnR = 6,       // BG/BtnR (UnityEngine.UI.Button)
            BtnTxtR = 7,    // BG/BtnR/BtnTxtR (UnityEngine.UI.Text)
            BtnImgL = 8,    // BG/BtnL (UnityEngine.UI.Image)
            BtnImgLM = 9,   // BG/BtnM (UnityEngine.UI.Image)
            BtnImgLR = 10,  // BG/BtnR (UnityEngine.UI.Image)
        };


        protected override void init()
        {
            base.init();

            Handler.AddBtnClick((int)idx.BtnL, _onBtnClick);
            Handler.AddBtnClick((int)idx.BtnM, _onBtnClick);
            Handler.AddBtnClick((int)idx.BtnR, _onBtnClick);

            mInfo = MsgBoxMgr.Instance.MsgInfo;

            if (null == mInfo || null == Handler)
            {
                Close();
                return;
            }

            Handler.SetTextString((int)idx.Title, mInfo.Title);
            Handler.SetTextString((int)idx.Msg, mInfo.Msg);

            Handler.SetUIActive((int)idx.BtnL, !string.IsNullOrEmpty(mInfo.BtnTxtL));
            Handler.SetTextString((int)idx.BtnTxtL, mInfo.BtnTxtL);

            Handler.SetUIActive((int)idx.BtnM, !string.IsNullOrEmpty(mInfo.BtnTxtM));
            Handler.SetTextString((int)idx.BtnTxtM, mInfo.BtnTxtM);

            Handler.SetUIActive((int)idx.BtnR, !string.IsNullOrEmpty(mInfo.BtnTxtR));
            Handler.SetTextString((int)idx.BtnTxtR, mInfo.BtnTxtR);

        }

        protected override void onDisabled()
        {
            base.onDisabled();

            MsgBoxMgr.Instance.ShowNext();
        }

        void _onBtnClick(string name)
        {
            int idx = -1;
            if (string.IsNullOrEmpty(name))
            {
                //默认回调中间的按钮
                idx = 1;
            }
            else
            {
                for (int i = 0; i < mBtns.Length; i++)
                {
                    if (Handler.GetCompByIndex<Component>(mBtns[i]).name.Equals(name))
                    {
                        idx = i;
                        break;
                    }
                }
            }                
            _callback(idx);

            Close();
        }

        void _callback(int idx)
        {
            if (null != mInfo.ClickCallback)
            {
                mInfo.ClickCallback(idx);
            }
            if (null != mInfo.LuaFunc)
            {
                mInfo.LuaFunc.Call(idx);
            }
        }
    }
}
