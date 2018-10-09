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

        mDelaySec = intent.getFloatExtra(STR_PACKAGE_NAME, mDelayDef);
        mPackageName = intent.getStringExtra(STR_DELAY_TIME_SEC);
        if(null == mPackageName || mPackageName.isEmpty())
        {
            if(null != GF_PluginAndroid.getInstance())
            {
                GF_PluginAndroid.getInstance().showToast("重启应用失败，包名为空");
            }
            killSelf();
            return super.onStartCommand(intent, flags, startId);
        }

        mHandler.postDelayed(new Runnable() {
            @Override
            public void run() {
                Intent _intent = getPackageManager().getLaunchIntentForPackage(mPackageName);
                startActivity(_intent);
                killSelf();
            }
        }, (long)(mDelaySec * 1000));

        return super.onStartCommand(intent, flags, startId);
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    private void killSelf()
    {
        RestartService.this.stopSelf();
    }
}
