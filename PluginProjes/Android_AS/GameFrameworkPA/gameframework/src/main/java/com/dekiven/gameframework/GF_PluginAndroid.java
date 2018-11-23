package com.dekiven.gameframework;

import android.Manifest;
import android.app.Activity;
import android.app.Fragment;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.util.Log;
import android.widget.Toast;

import com.dekiven.permission.GFPermission;
import com.dekiven.permission.IPermissionRequestResult;
import com.unity3d.player.UnityPlayer;

import java.io.File;

public class GF_PluginAndroid extends Fragment {
    private static GF_PluginAndroid instance;

    public static final String STR_PLUGIN_TAG = "GameFrameworkAnd";
    public static String sNoticeGameobjName = "GameFramework.GameManager";
    public static String sNoticeFuncName = "OnMessage";
    public static String sNoticeSplitStr = "__;__";
    public static String sPackageName = "";

    //渠道实例对象
    public static BaseAngent sAngent;

    //event名称
    public static final String STR_EVENT_TAKE_PHOTO = "TakeImagePhoto";
    public static final String STR_EVENT_TAKE_ALBUM = "TakeImageAlbum";
    public static final String STR_EVENT_START_PURCHASE = "StartPurchase";

    private static Activity mContext;

    public static GF_PluginAndroid getInstance() {
        if (null == instance) {
            start();
        }
        return instance;
    }

    public void setNoticeObFunc(String gameobjName, String funcName) {
        sNoticeGameobjName = gameobjName;
        sNoticeFuncName = funcName;
    }

    public void setNoticeSplitStr(String str)
    {
        sNoticeSplitStr = str;
    }

    public void takeFromAlbum() {
        Intent intent = new Intent(mContext, ImageTakeActivity.class);
        intent.putExtra("method", "takeFromAlbum");
        mContext.startActivity(intent);
    }

    public void takeFromPhoto() {
        LogEvent("请求拍照");
        instance.requestPermission(mContext, Manifest.permission.CAMERA, new IPermissionRequestCallback() {
            @Override
            public void onRequestFinished(boolean result) {
//                LogEvent("权限请求返回：" + result);
                if (result) {
                    if(hasExternalStorage())
                    {
                        Intent intent = new Intent(mContext, ImageTakeActivity.class);
                        intent.putExtra("method", "takeFromPhoto");
                        mContext.startActivity(intent);
                    }
                    else
                    {
                        notifyUnity("TakeImagePhoto", "no externalStorage");
                    }
                } else {
                    notifyUnity("TakeImagePhoto", "has no permission");
                }
            }
        });
    }

    public void startPurchase(String pid, String externalData)
    {
        //TODO:
        if(null != sAngent)
        {
            sAngent.startPurchase(pid, externalData);
        }
        else
        {
            GF_PluginAndroid.notifyUnity(STR_EVENT_START_PURCHASE, "false", "没有渠道实例");
        }
    }

    public boolean hasAngentExitDialog()
    {
        if(null != sAngent)
        {
            return sAngent.hasAngentExitDialog();
        }
        else
        {
            return false;
        }
    }

    public void installApk(String apkPath) {
        File file = new File(apkPath);
        if (!file.exists()) {
            LogEvent("installApk apk不存在：" + apkPath);
            notifyUnity("InstallNewApp", "");

            return;
        }
        Intent intent = new Intent(Intent.ACTION_VIEW);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        if (Build.VERSION.SDK_INT >= 24) {
            // 参数2 清单文件中provider节点里面的authorities ; 参数3  共享的文件,即apk包的file类
            Uri apkUri = getUri(mContext, file);
            //对目标应用临时授权该Uri所代表的文件
            intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            intent.setDataAndType(apkUri, "application/vnd.android.package-archive");
        } else {
            intent.setDataAndType(Uri.fromFile(file), "application/vnd.android.package-archive");
        }
        mContext.startActivity(intent);
    }

    public void restart(float delaySec) {
        Intent intent = new Intent(mContext, RestartService.class);
        intent.putExtra(RestartService.STR_PACKAGE_NAME, sPackageName);
        intent.putExtra(RestartService.STR_DELAY_TIME_SEC, delaySec);
        mContext.startService(intent);
        // TODO
        // mContext.finish();
    }

    public void showToast(String msg) {
        Toast.makeText(mContext, msg, Toast.LENGTH_SHORT).show();
    }

    public static void showToast(Context context, String msg) {
        Toast.makeText(context, msg, Toast.LENGTH_SHORT).show();
    }

    //通过发送消息将消息打印到Unity日志
    public static void LogEvent(String msg) {
        notifyUnity("LogEvent", msg);
        Log.w("LogEvent", msg);
    }

    public static Uri getUri(Context context, File path) {
        Uri uri;
        if (Build.VERSION.SDK_INT >= 24) {
            //Android 7.0及以上
//            uri = FileProvider.getUriForFile(context,sPackageName+".fileprovider", path);
            //TODO:Android 7.0及以上
            uri = Uri.fromFile(path);
        } else {
            uri = Uri.fromFile(path);
        }
        return uri;
    }

    public static void notifyUnity(String eventName, String msg) {
        UnityPlayer.UnitySendMessage(sNoticeGameobjName, sNoticeFuncName, eventName + sNoticeSplitStr + msg);
    }

    public static void notifyUnity(String eventName, String msg, String msg2) {
        UnityPlayer.UnitySendMessage(sNoticeGameobjName, sNoticeFuncName, eventName + sNoticeSplitStr + msg + sNoticeSplitStr + msg2);
    }

    public static void notifyUnity(String eventName, String msg, String msg2, String msg3) {
        UnityPlayer.UnitySendMessage(sNoticeGameobjName, sNoticeFuncName, eventName + sNoticeSplitStr + msg + sNoticeSplitStr + msg2 + sNoticeSplitStr + msg3);
    }

    public static boolean hasExternalStorage() {
        //获取内部存储状态
        return Environment.getExternalStorageState().equals(Environment.MEDIA_MOUNTED);
    }

    private void requestPermission(Activity activity, String permission, IPermissionRequestCallback callback) {
        requestPermission(activity, new String[]{permission,}, callback);
    }

    private void requestPermission(Activity activity, String[] permissions, final IPermissionRequestCallback callback) {
        LogEvent("开始申请权限");

        final boolean[] hasCallback = {false};
        LogEvent("开始申请权限  22");
        GFPermission.requestPermissions(activity, new IPermissionRequestResult() {
            @Override
            public void onGrantedAll(String[] permissions) {
                if (null != callback) {
                    //所有权限申请成功
                    callback.onRequestFinished(true);
                }
            }
            @Override
            public void onDeniedSome(String[] deniedPermissions, String[] grantedPermissions) {
                if (null != callback) {
                    //有部分权限申请失败
                    callback.onRequestFinished(false);
                }
            }
        }, permissions);
    }

    //region 私有
    private static void start() {
        mContext = UnityPlayer.currentActivity;
        instance = new GF_PluginAndroid();
        sPackageName = UnityPlayer.currentActivity.getPackageName();

        mContext.getFragmentManager().beginTransaction().add(instance, STR_PLUGIN_TAG).commit();
    }
    //endregion 私有

    // test
    public void test1() {
        LogEvent("请求拍照 测试");
        Intent intent = new Intent(mContext, ImageTakeActivity.class);
        intent.putExtra("method", "takeFromPhoto");
        mContext.startActivity(intent);
    }

    public void test2() {
        requestPermission(mContext, new String[]{Manifest.permission.CAMERA, Manifest.permission.WRITE_EXTERNAL_STORAGE}, new IPermissionRequestCallback() {
            @Override
            public void onRequestFinished(boolean result) {
                LogEvent("testPermmission 权限申请结果：" + result);
            }
        });
    }
}
