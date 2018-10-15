package com.dekiven.permission;

import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.os.Build;
import android.support.annotation.RequiresApi;
import android.support.v4.content.ContextCompat;
import android.util.Log;

import com.dekiven.gameframework.GF_PluginAndroid;

import junit.framework.Test;

import java.util.ArrayList;

public class GFPermission implements IPermissionRequestResult {
    public static final String TAG = GFPermission.class.toString();

    private static GFPermission instance = null;

    private Activity mActivity = null;
    private PermissionFragment mFragment = null;
    private IPermissionRequestResult mResult;
    private String[] mPermissions;

    private GFPermission() {

    }

    public static GFPermission getInstance() {
        if (null == instance) {
            instance = new GFPermission();
        }
        return instance;
    }

    public static GFPermission setResult(IPermissionRequestResult result) {
        if (!hasInit()) {
            return null;
        }
        instance.mResult = result;
        return instance;
    }

    private static boolean hasInit() {
        boolean hasinit = null != instance;
        if (!hasinit) {
            Log.w(TAG, "请先调用init初始化GFPermission");
        }
        return hasinit;
    }

    private static boolean checkPermission(Context context, String permission) {
        int checkPermission = ContextCompat.checkSelfPermission(context, permission);
        if (checkPermission == PackageManager.PERMISSION_GRANTED) {
            return true;
        }
        return false;
    }

    public static boolean hasPermission(Context context, String permission) {
        return (Build.VERSION.SDK_INT < Build.VERSION_CODES.M | checkPermission(context, permission));
    }

    /**
     * 检查给出一组权限是否已授权，只要有一个没有授权返回false
     *
     * @param context
     * @param permissions
     * @return
     */
    public static boolean hasPermissions(Context context, String... permissions) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
            return true;
        }
        for (String permission : permissions) {
            if (!checkPermission(context, permission)) {
                return false;
            }
        }
        return true;
    }

    //    @TargetApi(Build.VERSION_CODES.M)
    public static void requestPermissions(Activity activity, IPermissionRequestResult result, String... permissions) {
        getInstance();
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
            if (null != result) {
                result.onGrantedAll(permissions);
            }
            return;
        } else {
            ArrayList<String> denyPermissions = new ArrayList<>();
            for (String p : permissions) {
                if (!checkPermission(activity, p)) {
                    denyPermissions.add(p);
                }
            }
            if (denyPermissions.size() == 0) {
                if (null != result) {
                    result.onGrantedAll(permissions);
                }
            } else {
                if (!hasInit()) {
                    if (null != result) {
                        result.onDeniedSome(permissions, (String[]) denyPermissions.toArray(new String[denyPermissions.size()]));
                    }
                    return;
                }
                instance.mResult = result;
                PermissionFragment fragment = instance.addPermissionFragment(activity);
                requestPermissions(fragment, permissions);
            }
        }
    }

    @RequiresApi(api = Build.VERSION_CODES.M)
    private static void requestPermissions(PermissionFragment fragment, String... permissions) {
        if (null != fragment) {
            if (fragment.isAdded()) {
                fragment.requestPermissions(permissions, PermissionFragment.PERMISSION_REQ_CODE);
            } else {
                fragment.permissions = permissions;
                fragment.requestOnStart = true;
            }
        } else {
            if (hasInit() && null != instance.mResult) {
                instance.mResult.onDeniedSome(permissions, new String[]{});
                instance.mResult = null;
            }
            Log.w(TAG, "requestPermissions fragment 为空！权限请求失败！！");
        }
    }

    private PermissionFragment addPermissionFragment(Activity activity) {
        if (!hasInit()) {
            return null;
        }
        if (null != activity) {
            mActivity = activity;
            PermissionFragment fragment = new PermissionFragment();
            fragment.requestResult = this;
            instance.mFragment = fragment;
            activity.getFragmentManager().beginTransaction().add(fragment, PermissionFragment.TAG).commit();
            //test
            GF_PluginAndroid.LogEvent("fragment.isAdded:" + fragment.isAdded());
        }
        return instance.mFragment;
    }

    private void removePermissionFragment() {
        if (!hasInit()) {
            return;
        }
        if (null != instance.mFragment) {
            instance.mActivity.getFragmentManager().beginTransaction().remove(instance.mFragment).commit();
            instance.mFragment = null;
            instance.mActivity = null;
            instance.mPermissions = null;
            instance.mResult = null;
        }
    }

    @Override
    public void onGrantedAll(String[] permissions) {
        if (hasInit()) {
            if (null != instance.mResult) {
                instance.mResult.onGrantedAll(permissions);
            }
            instance.removePermissionFragment();
        }
    }

    @Override
    public void onDeniedSome(String[] deniedPermissions, String[] grantedPermissions) {
        if (hasInit()) {
            if (null != instance.mResult) {
                instance.mResult.onDeniedSome(deniedPermissions, grantedPermissions);
            }
            instance.removePermissionFragment();
        }
    }
}
