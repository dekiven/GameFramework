using System;
using System.Collections;
using LuaInterface;
using UnityEngine;

namespace GameFramework
{
    public class LocationHelper
    {
        //private static string gps_info;
        private static int sHashcode = 0;
        private static float sTimeoutSec;
        private static bool sGpsOn = false;
        private static Action<bool, string> sStartRst;
        private static LuaFunction sLuaRst;
        private static float sDesiredAccuracyInMeters;
        private static float sUpdateDistanceInMeters;

        public static bool IsGpsOn{ get { return sGpsOn; } }

        public static void StartGPS(Action<bool, string> rst, LuaFunction lua=null, float timeoutSec=10f, float desiredAccuracyInMeters = 500, float updateDistanceInMeters = 500)
        {
            if (sHashcode != 0)
            {
                // 已经有协程在启动GPS
                return;
            }
            sStartRst = rst;
            if (null != sLuaRst)
            {
                sLuaRst.Dispose();
                sLuaRst = null;
            }
            sLuaRst = lua;
            sTimeoutSec = timeoutSec;
            sDesiredAccuracyInMeters = desiredAccuracyInMeters;
            sUpdateDistanceInMeters = updateDistanceInMeters;
            sHashcode = CoroutineMgr.Instance.StartCor(_startGPS());
        }

        public static void StopGPS()
        {
            Input.location.Stop();
            _stopCoroutine();
            sGpsOn = false;
        }

        public static LocationInfo LastData()
        {
            return Input.location.lastData;
        }

        private static IEnumerator _startGPS()
        {
            // Input.location 用于访问设备的位置属性（手持设备）, 静态的LocationService位置
            // LocationService.isEnabledByUser 用户设置里的定位服务是否启用
            if (!Input.location.isEnabledByUser)
            {
                _callback(false, "isEnabledByUser value is:" + Input.location.isEnabledByUser.ToString() + ". Please turn on the GPS");
                _stopCoroutine();
                yield break;
            }

            // LocationService.Start() 启动位置服务的更新,最后一个位置坐标会被使用
            Input.location.Start(sDesiredAccuracyInMeters, sUpdateDistanceInMeters);

            int count = 0;
            while (Input.location.status == LocationServiceStatus.Initializing && count <= sTimeoutSec)
            {
                // 暂停协同程序的执行(1秒)
                yield return new WaitForSeconds(1);
                ++count;
            }

            if (count > sTimeoutSec)
            {
                _callback(false, "Init GPS service time out");
                _stopCoroutine();
                yield break;
            }

            if (Input.location.status == LocationServiceStatus.Failed)
            {
                _callback(false, "Unable to determine device location");
            }
            else
            {
                sGpsOn = true;
                _callback(true, "latitude:" + Input.location.lastData.latitude + " longitude:" + Input.location.lastData.longitude);
            }
            _stopCoroutine();
        }

        private static void _stopCoroutine()
        {
            if (sHashcode != 0)
            {
                CoroutineMgr.Instance.StopCor(sHashcode);
                sHashcode = 0;
            }
        }

        private static void _callback(bool rst, string msg)
        {
            if(null != sStartRst)
            {
                sStartRst(rst, msg);
            }
            if (null != sLuaRst)
            {
                sLuaRst.Call(rst, msg);
                sLuaRst.Dispose();
                sLuaRst = null;
            }
        }
    }
}
