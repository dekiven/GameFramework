using System;
namespace GameFramework
{
    public class PlatformBase
    {
        public virtual void SetNoticeObFunc(string gameobjName, string funcName)
        {
            LogFile.Log("Platform方法 SetNoticeObFunc(string gameobjName, string funcName) 待实现,请重载该方法!");
        }

        public virtual void TakeImagePhoto()
        {
            LogFile.Log("Platform方法 TakePhoto() 待实现,请重载该方法!");
        }

        public virtual void TakeImageAlbum()
        {
            LogFile.Log("Platform方法 TakeAlbum() 待实现,请重载该方法!");
        }

        public virtual void Restart(float delaySec)
        {
            LogFile.Log("Platform方法 Restart(float delaySec) 待实现,请重载该方法!");
        }

        public virtual void InstallNewApp(string path)
        {
            LogFile.Log("Platform方法 InstallNewApp(string path) 待实现,请重载该方法!");
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
