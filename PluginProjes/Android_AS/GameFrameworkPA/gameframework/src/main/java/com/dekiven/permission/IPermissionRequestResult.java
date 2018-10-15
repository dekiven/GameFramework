package com.dekiven.permission;

public interface IPermissionRequestResult {
    void onGrantedAll(String[] permissions);
    void onDeniedSome(String[] deniedPermissions, String[] grantedPermissions);
}
