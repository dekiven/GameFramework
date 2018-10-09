package com.dekiven.gameframework;

import java.util.ArrayList;

public interface IPermissionRequestCallback{
    void onRequestFinished(ArrayList<Integer> permissionsDenied);
}
