package com.dekiven.permission;

import android.app.Fragment;
import android.content.pm.PackageManager;
import android.os.Build;
import android.support.annotation.NonNull;

import java.util.ArrayList;

public class PermissionFragment extends Fragment {
    public static final String TAG = PermissionFragment.class.toString();
    public static final int PERMISSION_REQ_CODE = 1;
    public static IPermissionRequestResult requestResult = null;

    public String[] permissions;
    public boolean requestOnStart;

    @Override
    public void onStart() {
        super.onStart();
        if(requestOnStart && permissions != null && Build.VERSION.SDK_INT >= Build.VERSION_CODES.M)
        {
            this.requestPermissions(permissions, PERMISSION_REQ_CODE);
            requestOnStart = false;
        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        if (PERMISSION_REQ_CODE == requestCode) {
            ArrayList<String> granted = new ArrayList<String>();
            ArrayList<String> denied = new ArrayList<String>();
            for (int i = 0; i < permissions.length; ++i) {
                String p = permissions[i];
                int r = grantResults[i];
                if (r == PackageManager.PERMISSION_GRANTED) {
                    granted.add(p);
                } else {
                    denied.add(p);
                }
            }
            if (null != requestResult) {
                if (denied.size() > 0) {
                    requestResult.onDeniedSome(denied.toArray(new String[denied.size()]), granted.toArray(new String[granted.size()]));
                } else {
                    requestResult.onGrantedAll(granted.toArray(new String[granted.size()]));
                }
            }
        }
    }
}
