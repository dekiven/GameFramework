using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    public class DownloadView : UIView
    {
        string eventKeyDownload = "DownloadProgressAndSpeed";
        string eventKeyFinish = "DownloadPkgsFinish";

        Text mTextVersions;
        Text mTextInfo;
        Slider mSlider;

        string mAppVer;
        string mResVer;

        #region UIVIew
        protected override void init()
        {
            base.init();

            mAppVer = GameManager.Instance.AppVer;

            EventManager.AddToMain(eventKeyDownload, this, "_onProgressAndSpeed");
            EventManager.AddToMain(ResPkgDownloder.STR_EVENT_PKG_VERSION, this, "_onResVerChange");
            EventManager.AddToMain(UpdateMgr.STR_UP_MGR_STATE_CHANGE, this, "_onMsgChange");

            mTextVersions = Handler.GetCompByIndex<Text>(0);
            mTextInfo = Handler.GetCompByIndex<Text>(1);
            mSlider = Handler.GetCompByIndex<Slider>(2);

            _freshVersion();

            UpdateMgr.Instance.Init(eventKeyDownload, eventKeyFinish);
        }

        

        protected override void dispose()
        {
            EventManager.RemoveFromMain(this);
        }
        #endregion UIVIew

        #region 通知
        public void _onProgressAndSpeed(double progres, long speed)
        {
            if(Tools.Equals(-1d, progres))
            {
                mSlider.gameObject.SetActive(false);
            }
            else 
            {
                mSlider.gameObject.SetActive(true);
                mSlider.value = (float)progres;
            }
            if (speed.Equals(-1))
            {
                mTextInfo.text = string.Empty;
            }
            else
            {
                mTextInfo.text = LanguageManager.GetStr("下载中，请稍候，当前速度:{0}/s", Tools.FormatMeroySize(speed));
            }
        }

        public void _onResVerChange(string ver)
        {
            if (Tools.CompareVersion(ver, mResVer) > 0)
            {
                mResVer = ver;
            }
            _freshVersion();
        }

        public void _onMsgChange(string msg)
        {
            mTextInfo.text = msg;
            mSlider.gameObject.SetActive(false);
        }
        #endregion 通知

        #region 私有
        void _freshVersion()
        {
            if(string.IsNullOrEmpty(mResVer))
            {
                mTextVersions.text = LanguageManager.GetStr("App版本:{0}", mAppVer);
            }
            else
            {
                mTextVersions.text = LanguageManager.GetStr("App版本:{0}, 资源版本:{1}", mAppVer, mResVer);
            }
        }
        #endregion 私有
    }
}
