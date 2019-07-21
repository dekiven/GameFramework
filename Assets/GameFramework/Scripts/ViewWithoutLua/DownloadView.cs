using UnityEngine.UI;

namespace GameFramework
{
    //-- UIArray index
    //local uiIdx =
    //{
    //    TextVersion = 0,  -- TextVersion (UnityEngine.UI.Text)

    //    TextInfo = 1,  -- TextInfo (UnityEngine.UI.Text)

    //    Slider = 2,  -- Slider (UnityEngine.UI.Slider)
    //}

    public class DownloadView : UIView
    {
        public string STR_NOTIFY_FUNC = GameUpManager.STR_NOTIFY_EVENT_NAME;

        Text mTextVersions;
        Text mTextInfo;
        Slider mSlider;


        protected override void init()
        {
            base.init();

            EventManager.registerToMain(STR_NOTIFY_FUNC, this, "UpdateDownloadView");
            if (null != Handler)
            {
                mTextVersions = Handler.GetCompByIndex<Text>(0);
                mTextInfo = Handler.GetCompByIndex<Text>(1);
                mSlider = Handler.GetCompByIndex<Slider>(2);
            }

            if(GameConfig.useAsb && GameConfig.checkUpdate)
            {
                GameUpManager.Instance.CheckLocalRes((bool rst, string msg) =>
                {
                    LogFile.Log("检测本地资源结果：" + rst);
                    if (rst)
                    {
                        GameUpManager.Instance.CheckAppVer((bool obj) =>
                        {
                            LogFile.Log("检测APP version资源结果：" + rst);
                            if (obj)
                            {
                                GameUpManager.Instance.CheckServerRes((bool _rst, string _msg) =>
                                {
                                    if (_rst)
                                    {
                                        startGameLogic();
                                    }
                                    else
                                    {
                                        LogFile.Error("服务器资源更新失败");
                                        //TODO:显示弹窗等
                                        startGameLogic();
                                    }
                                });
                            }
                            else
                            {
                                LogFile.Error("检测到app有更新，但是更新失败！");
                                //TODO:显示弹窗等
                            }
                        });
                    }
                    else
                    {
                        LogFile.Error("包内不含ResConf.bytes文件");
                    }
                });
            }
            else
            {
                startGameLogic();
            }

        }

        private static void startGameLogic()
        {
            //GameUpManager.Instance.Destroy();
            GameUIManager.Instance.PopView();
            GameManager.Instance.StartGameLogic();
        }

        protected override void dispose()
        {
            EventManager.deregisterFromMain(this);
        }

        public void UpdateDownloadView(string versions, string info, float progress)
        {
            if(null != mTextVersions)
            {
                mTextVersions.text = versions;
            }

            if (null != mTextInfo)
            {
                mTextInfo.text = info;
            }

            if (null != mSlider)
            {
                mSlider.value = progress;
            }
        }
    }
}
