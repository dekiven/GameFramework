package com.dekiven.gameframework;

import android.app.Service;
import android.content.Intent;
import android.os.Handler;
import android.os.IBinder;
import android.util.Log;

public class RestartService extends Service {

    public static String STR_PACKAGE_NAME = "packageName";
    public static String STR_DELAY_TIME_SEC = "delaySec";
    private static float mDelayDef = 1.0f;

    private Handler mHandler;
    private String mPackageName;
    private float mDelaySec;

    public RestartService() {
        mHandler = new Handler();
    }

    @Override
    public int onStartCommand(final Intent intent, int flags, int startId) {

        mDelaySec = intent.getFloatExtra(STR_DELAY_TIME_SEC, mDelayDef);
        mPackageName = intent.getStringExtra(STR_PACKAGE_NAME);
        if (null == mPackageName || mPackageName.isEmpty()) {
            GF_PluginAndroid.LogEvent("重启应用失败，包名为空");
            killSelf();
            return super.onStartCommand(intent, flags, startId);
        }

        mHandler.postDelayed(new Runnable() {
            @Override
            public void run() {
                GF_PluginAndroid.LogEvent("delay:" + mDelaySec + "秒, 准备重启");
                Intent _intent = getPackageManager().getLaunchIntentForPackage(mPackageName);
                GF_PluginAndroid.LogEvent("重启 intent:"+intent);
                startActivity(_intent);
                killSelf();
            }
        }, (long) (mDelaySec * 1000));

        return super.onStartCommand(intent, flags, startId);
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    private void killSelf() {
        RestartService.this.stopSelf();
    }
}
