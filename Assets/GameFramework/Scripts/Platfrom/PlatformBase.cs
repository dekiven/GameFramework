using System;
namespace GameFramework
{
    public class PlatformBase
    {
        public virtual void SetNoticeObFunc(string gameobjName, string funcName)
        {
            LogFile.Log("Platform方法 SetNoticeObFunc(string gameobjName, string funcName) 待实现,请重载该方法!\nobjName:{0},funcName{1}", gameobjName, funcName);
        }

        public virtual void SetNotifySplitStr(string s)
        {
            LogFile.Log("Platform方法 SetNotifySplitStr(string s) 待实现,请重载该方法!\ns:{0}", s);
        }

        public virtual void TakeImagePhoto()
        {
            LogFile.Log("Platform方法 TakePhoto() 待实现,请重载该方法!");
        }

        public virtual void TakeImageAlbum()
        {
            LogFile.Log("Platform方法 TakeAlbum() 待实现,请重载该方法!");
        }

        /// <summary>
        /// TODO:待实现
        /// </summary>
        /// <param name="delaySec">Delay sec.</param>
        public virtual void Restart(float delaySec)
        {
            LogFile.Log("Platform方法 Restart(float delaySec) 待实现,请重载该方法!\ndelaySec:{0}", delaySec);
        }

        /// <summary>
        /// Installs the new app.安卓安装apk，ios跳转到商店
        /// </summary>
        /// <param name="path">Path.</param>
        public virtual void InstallNewApp(string path)
        {
            LogFile.Log("Platform方法 InstallNewApp(string path) 待实现,请重载该方法!\npath:{0}", path);
        }

        /// <summary>
        /// 请求支付订单
        /// </summary>
        /// <param name="pid">支付id</param>
        /// <param name="externalData">额外数据</param>
        public virtual void StartPurchase(string pid, string externalData)
        {
            LogFile.Log("Platform方法 StartPurchase(string pid, string externalJsonData) 待实现,请重载该方法!\npid:{0}, ejd:{1}", pid, externalData);
        }

        /// <summary>
        /// 渠道是否有规定的退出对话框
        /// </summary>
        /// <returns><c>true</c>, if angent exit dialog was hased, <c>false</c> otherwise.</returns>
        public virtual bool HasAngentExitDialog()
        {
            return false;
        }

        //=====================================test--------------------------------------
        public virtual void test1()
        {
            LogFile.Log("Platform方法 test1() 待实现,请重载该方法!");
        }

        public virtual void test2()
        {
            LogFile.Log("Platform方法 test2() 待实现,请重载该方法!");
        }

        //--------------------------------------test=====================================
    }
}
