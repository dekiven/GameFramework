package com.dekiven.gameframework;

import android.Manifest;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.Fragment;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.support.annotation.NonNull;
import android.support.v4.content.FileProvider;
import android.util.Log;
import android.widget.Toast;

import com.unity3d.player.UnityPlayer;

import java.io.File;
import java.sql.Wrapper;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import me.weyye.hipermission.HiPermission;
import me.weyye.hipermission.PermissionCallback;
import me.weyye.hipermission.PermissionItem;

public class GF_PluginAndroid extends Fragment {
    private static GF_PluginAndroid instance;

    public static final String STR_PLUGIN_TAG = "GameFrameworkAnd";
    public static String sNoticeGameobjName = "GameManager";
    public static String sNoticeFuncName = "OnMessage";
    public static String sPackageName = "";

    // 联系人相关权限
    public static final int P_CONTACTS = 1;
    // 通话相关权限
    public static final int P_PHONE = 2;
    // 日历相关权限
    public static final int P_CALENDAR = 3;
    // 摄像头相关权限
    public static final int P_CAMERA = 4;
    // 传感器相关权限
    public static final int P_SENSORS = 5;
    // 位置相关权限
    public static final int P_LOCATION = 6;
    // 存储相关权限
    public static final int P_STORAGE = 7;
    // 麦克风相关权限
    public static final int P_MICROPHONE = 8;
    // 短信相关权限
    public static final int P_SMS = 9;


    private static Activity mContext;
//    private static IPermissionRequestCallback mPerReCallback;

//    private static HashMap<Integer, String> sPermissionMap = new HashMap<>();
//    private static ArrayList<Integer> sRequestList = new ArrayList<Integer>();
//    // 请求的权限数组
//    private static int[] sPerReCodes = null;
//    // 权限请求成功的个数
//    private static int sGrantedCount = 0;
//    // 权限请求失败列表
//    private static ArrayList<Integer> sDeniedList = new ArrayList<Integer>();

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

    public void takeFromPhoto() {
        Intent intent = new Intent(mContext, ImageTakeActivity.class);
        intent.putExtra("method", "takeFromPhoto");
        mContext.startActivity(intent);
    }

    public void takeFromAlbum() {
        Intent intent = new Intent(mContext, ImageTakeActivity.class);
        intent.putExtra("method", "takeFromAlbum");
        mContext.startActivity(intent);
    }

    public void restart(float delaySec) {
        Intent intent = new Intent(mContext, RestartService.class);
        intent.putExtra(RestartService.STR_PACKAGE_NAME, sPackageName);
        intent.putExtra(RestartService.STR_DELAY_TIME_SEC, delaySec);
        mContext.startService(intent);
    }

    public void showToast(String msg) {
        Toast.makeText(mContext, msg, Toast.LENGTH_SHORT).show();
    }

    public void showToast(Context context, String msg) {
        Toast.makeText(context, msg, Toast.LENGTH_SHORT).show();
    }

    public void installApk(String apkPath) {
        File file = new File(apkPath);
        Intent intent = new Intent(Intent.ACTION_VIEW);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        if (Build.VERSION.SDK_INT >= 24) { //Android 7.0及以上
            // 参数2 清单文件中provider节点里面的authorities ; 参数3  共享的文件,即apk包的file类
            Uri apkUri = FileProvider.getUriForFile(mContext, sPackageName + ".fileprovider", file);
            //对目标应用临时授权该Uri所代表的文件
            intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
            intent.setDataAndType(apkUri, "application/vnd.android.package-archive");
        } else {
            intent.setDataAndType(Uri.fromFile(file), "application/vnd.android.package-archive");
        }
        mContext.startActivity(intent);
    }

    public static void notifyUnity(String eventName, String msg) {
        UnityPlayer.UnitySendMessage(sNoticeGameobjName, sNoticeFuncName, eventName + "__;__" + msg);
    }

    public void requestPermission(Context context, String permission , IPermissionRequestCallback callback)
    {
        requestPermission(context, new String[]{permission,}, callback);
    }

    public void requestPermission(Context context, String[] permissions, final IPermissionRequestCallback callback)
    {
        List<PermissionItem> permissionItems = new ArrayList<PermissionItem>();
        for (String p : permissions)
        {
            permissionItems.add(new PermissionItem(p));
        }
        final boolean[] hasCallback = {false};
        HiPermission.create(context)
                .permissions(permissionItems)
                .checkMutiPermission(new PermissionCallback() {
                    @Override
                    public void onClose() {

                        showToast("They cancelled our request");
                        if(null != callback && hasCallback[0])
                        {
                            hasCallback[0] = true;
                            callback.onRequestFinished(false);
                        }
                    }

                    @Override
                    public void onFinish() {
                        showToast("All permissions requested completed");
                        if(null != callback && hasCallback[0])
                        {
                            hasCallback[0] = true;
                            callback.onRequestFinished(true);
                        }
                    }

                    @Override
                    public void onDeny(String permission, int position) {
                        showToast("All permissions requested completed");
                        if(null != callback && hasCallback[0])
                        {
                            hasCallback[0] = true;
                            callback.onRequestFinished(false);
                        }
                    }

                    @Override
                    public void onGuarantee(String permission, int position) {
                        showToast("All permissions requested completed");
                        if(null != callback && hasCallback[0])
                        {
                            hasCallback[0] = true;
                            callback.onRequestFinished(false);
                        }
                    }
                });
    }
//    public void requestPermission(Context context, int code, boolean requestForce, IPermissionRequestCallback callback) {
//        requestPermissions(context, new int[]{code,}, requestForce, callback);
//    }
//
//    public void requestPermission(Context context, String permission, int code, boolean requestForce, IPermissionRequestCallback callback) {
//        requestPermissions(context, new String[]{permission,}, new int[]{code,}, requestForce, callback);
//    }
//
//    public void requestPermissions(Context context, int[] codes, boolean requestForce, IPermissionRequestCallback callback) {
//        requestPermissions(context, getPermissionArray(codes), codes, requestForce, callback);
//    }
//
//    public void requestPermissions(Context context, String[] permissions, int[] codes, boolean requestForce, IPermissionRequestCallback callback) {
//
//        /*
//        sPerReCodes = codes;
//        sRequestList.clear();
//        mPerReCallback = callback;
//        for (int c : codes) {
//            sRequestList.add(c);
//        }
//        sDeniedList.clear();
//        sGrantedCount = 0;
//        Permissions4M.get(GF_PluginAndroid.this)
//                .requestForce(requestForce)
//                .requestPermissions(permissions)
//                .requestCodes(codes)
//                .requestListener(new Wrapper.PermissionRequestListener() {
//                    @Override
//                    public void permissionGranted(int code) {
//                        onPermissionGranted(code);
//                    }
//
//                    @Override
//                    public void permissionDenied(int code) {
//                        onPermissionDenied(code);
//                    }
//
//                    @Override
//                    public void permissionRationale(int code) {
//                        onPermissionRationale(code);
//                    }
//                })
//                .requestCustomRationaleListener(new Wrapper.PermissionCustomRationaleListener() {
//                    @Override
//                    public void permissionCustomRationale(int code) {
//                        onPermissionCustomRationale(code);
//                    }
//                })
//                // 权限完全被禁时回调函数中返回 intent 类型（手机管家界面）
//                .requestPageType(Permissions4M.PageType.MANAGER_PAGE)
//                // 权限完全被禁时回调函数中返回 intent 类型（系统设置界面）
//                //.requestPageType(Permissions4M.PageType.ANDROID_SETTING_PAGE)
//                // 权限完全被禁时回调，接口函数中的参数 Intent 是由上一行决定的
//                .requestPage(new ListenerWrapper.PermissionPageListener() {
//                    @Override
//                    public void pageIntent(int code, Intent intent) {
//                        showPermissionDialog(code, getPermissionMsg(code), intent);
//                    }
//                })
//                .request();
//        */
//    }

    //region 私有
    private static void start() {
        mContext = UnityPlayer.currentActivity;
        instance = new GF_PluginAndroid();
        sPackageName = UnityPlayer.currentActivity.getPackageName();

//        //初始化权限map
//        sPermissionMap.put(P_CONTACTS, Manifest.permission.WRITE_CONTACTS);
//        sPermissionMap.put(P_PHONE, Manifest.permission.READ_CALL_LOG);
//        sPermissionMap.put(P_CALENDAR, Manifest.permission.READ_CALENDAR);
//        sPermissionMap.put(P_CAMERA, Manifest.permission.CAMERA); //( 相机 )
//        sPermissionMap.put(P_SENSORS, Manifest.permission.BODY_SENSORS);
//        sPermissionMap.put(P_LOCATION, Manifest.permission.ACCESS_FINE_LOCATION);
//        sPermissionMap.put(P_STORAGE, Manifest.permission.READ_EXTERNAL_STORAGE); //( SD卡读写权限 )
//        sPermissionMap.put(P_MICROPHONE, Manifest.permission.RECORD_AUDIO);
//        sPermissionMap.put(P_SMS, Manifest.permission.READ_SMS);  //（读取短信）


        mContext.getFragmentManager().beginTransaction().add(instance, STR_PLUGIN_TAG).commit();

//        //test
//        instance.requestPermission(mContext,4, true, new IPermissionRequestCallback() {
//            @Override
//            public void onRequestFinished(ArrayList<Integer> permissionsDenied) {
//
//            }
//        });
    }


    //endregion 私有

//    //region 权限申请相关
//    @Override
//    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[]
//            grantResults) {
//        Permissions4M.onRequestPermissionsResult(GF_PluginAndroid.this, requestCode, grantResults);
//        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
//    }

//    private void onPermissionGranted(int code) {
//        sGrantedCount++;
//        checkIsPermissionRequestFinished();
//        //        权限授权成功
////        switch (code) {
////            // 联系人相关权限
////            case P_CONTACTS:
////                break;
////            // 通话相关权限
////            case P_PHONE:
////                break;
////            // 日历相关权限
////            case P_CALENDAR:
////                break;
////            // 摄像头相关权限
////            case P_CAMERA:
////                break;
////            // 传感器相关权限
////            case P_SENSORS:
////                break;
////            // 位置相关权限
////            case P_LOCATION:
////                break;
////            // 存储相关权限
////            case P_STORAGE:
////                break;
////            // 麦克风相关权限
////            case P_MICROPHONE:
////                break;
////            // 短信相关权限
////            case P_SMS:
////                break;
////            default:
////                break;
////        }
//
//    }
//
//    private void onPermissionDenied(int code) {
//        // 授权被拒
//        sDeniedList.add(code);
//        checkIsPermissionRequestFinished();
////        switch (code) {
////            // 联系人相关权限
////            case P_CONTACTS:
////                break;
////            // 通话相关权限
////            case P_PHONE:
////                break;
////            // 日历相关权限
////            case P_CALENDAR:
////                break;
////            // 摄像头相关权限
////            case P_CAMERA:
////                break;
////            // 传感器相关权限
////            case P_SENSORS:
////                break;
////            // 位置相关权限
////            case P_LOCATION:
////                break;
////            // 存储相关权限
////            case P_STORAGE:
////                break;
////            // 麦克风相关权限
////            case P_MICROPHONE:
////                break;
////            // 短信相关权限
////            case P_SMS:
////                break;
////            default:
////                break;
////    }
//
//    }
//
//    // 检查所有权限是否都已经处理（只能是 授权、拒绝授权）
//    private void checkIsPermissionRequestFinished() {
//        if (sDeniedList.size() + sGrantedCount >= sPerReCodes.length) {
//            if (null != mPerReCallback) {
//                mPerReCallback.onRequestFinished(sDeniedList);
//            }
//        }
//    }
//
//    private void onPermissionRationale(final int code) {
//        switch (code) {
//            // 联系人相关权限
//            case P_CONTACTS:
//                break;
//            // 通话相关权限
//            case P_PHONE:
//                break;
//            // 日历相关权限
//            case P_CALENDAR:
//                break;
//            // 摄像头相关权限
//            case P_CAMERA:
//                break;
//            // 传感器相关权限
//            case P_SENSORS:
//                break;
//            // 位置相关权限
//            case P_LOCATION:
//                break;
//            // 存储相关权限
//            case P_STORAGE:
//                break;
//            // 麦克风相关权限
//            case P_MICROPHONE:
//                break;
//            // 短信相关权限
//            case P_SMS:
//                break;
//            default:
//                break;
//        }
//    }
//
//    private void onPermissionCustomRationale(final int code) {
//        showPermissionDialog(code, getPermissionMsg(code));
//    }
//
//    private void showPermissionDialog(final int code, String msg) {
//        showPermissionDialog(code, msg, null);
//    }
//
//    private void showPermissionDialog(final int code, final String msg, final Intent intent) {
////        new AlertDialog.Builder(getActivity())
////                .setMessage(msg)
////                .setPositiveButton(R.string.dialog_confirm, new DialogInterface
////                        .OnClickListener() {
////                    @Override
////                    public void onClick(DialogInterface dialog, int which) {
////                        if (null == intent) {
////                            Permissions4M.get(GF_PluginAndroid.this)
////                                    .requestOnRationale()
////                                    .requestPermissions(getPermissionStrById(code))
////                                    .requestCodes(code)
////                                    .request();
////                        } else {
////                            startActivity(intent);
////                        }
////                    }
////                })
////                .setNegativeButton(R.string.dialog_cancel, new DialogInterface.OnClickListener() {
////                    @Override
////                    public void onClick(DialogInterface dialog, int which) {
////                        dialog.cancel();
////                    }
////                })
////                .show();
//    }
//
//    private static String getStrById(int id) {
//        return mContext.getString(id);
//    }
//
//    private static String getPermissionStrById(int id) {
//        return sPermissionMap.get(id);
//    }
//
//    private static String[] getPermissionArray(int... ids) {
//        String[] pa = new String[ids.length];
//        for (int i = 0; i < ids.length; ++i) {
//            pa[i] = getPermissionStrById(ids[i]);
//        }
//        return pa;
//    }
//
//    private static String getPermissionMsg(int code) {
//        String str = "";
//        switch (code) {
//            // 联系人相关权限
//            case P_CONTACTS:
//                str = getStrById(R.string.p_msg_contacts);
//                break;
//            // 通话相关权限
//            case P_PHONE:
//                str = getStrById(R.string.p_msg_phone);
//                break;
//            // 日历相关权限
//            case P_CALENDAR:
//                str = getStrById(R.string.p_msg_calendar);
//                break;
//            // 摄像头相关权限
//            case P_CAMERA:
//                str = getStrById(R.string.p_msg_camera);
//                break;
//            // 传感器相关权限
//            case P_SENSORS:
//                str = getStrById(R.string.p_msg_sensors);
//                break;
//            // 位置相关权限
//            case P_LOCATION:
//                str = getStrById(R.string.p_msg_location);
//                break;
//            // 存储相关权限
//            case P_STORAGE:
//                str = getStrById(R.string.p_msg_storage);
//                break;
//            // 麦克风相关权限
//            case P_MICROPHONE:
//                str = getStrById(R.string.p_msg_microphone);
//                break;
//            // 短信相关权限
//            case P_SMS:
//                str = getStrById(R.string.p_msg_sms);
//                break;
//            default:
//                break;
//        }
//        return str;
//    }
////endregion 权限申请相关
}
