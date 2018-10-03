package com.dekiven.gameframework;

import android.app.Activity;
import android.app.Fragment;
import android.content.Intent;
import android.widget.Toast;

import com.unity3d.player.UnityPlayer;

public class GF_PluginAndroid extends Fragment {
    public static GF_PluginAndroid instance;
    public static String STR_PLUGIN_TAG = "GameFrameworkAnd";

    private static Activity mContext;

    public static void start()
    {
        mContext = UnityPlayer.currentActivity;
        instance = new GF_PluginAndroid();

        mContext.getFragmentManager().beginTransaction().add(instance, STR_PLUGIN_TAG);
    }

    public void takeFromPhoto()
    {
//        Toast.makeText(mContext, "takeFromPhoto", Toast.LENGTH_SHORT).show();
        Intent intent = new Intent(mContext, ImageTakeActivity.class);
        intent.putExtra("method", "takeFromPhoto");
        mContext.startActivity(intent);
    }

    public void takeFromAlbum()
    {
        Toast.makeText(mContext, "takeFromAlbum", Toast.LENGTH_SHORT).show();
        Intent intent = new Intent(mContext, ImageTakeActivity.class);
        intent.putExtra("method", "takeFromAlbum");
        mContext.startActivity(intent);
    }
}
