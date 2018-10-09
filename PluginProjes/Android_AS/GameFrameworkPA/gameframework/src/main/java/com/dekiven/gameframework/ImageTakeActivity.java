package com.dekiven.gameframework;

import android.app.Activity;
import android.content.ContentResolver;
import android.content.Intent;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.provider.MediaStore;
import android.widget.Toast;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.ArrayList;

public class ImageTakeActivity extends Activity {
    private static final int PHOTO_REQUEST_CODE = 1;//相册
    public static final int PHOTOHRAPH = 2;// 拍照
    public static final int NONE = 0;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        String method = this.getIntent().getStringExtra("method");
        Toast.makeText(this, "method：" + method, Toast.LENGTH_SHORT).show();
        if ("takeFromPhoto".equals(method)) {
            takeFromPhoto();
        } else if ("takeFromAlbum".equals(method)) {
            takeFromAlbum();
        }

    }

    //调用相机
    public void takeFromPhoto() {

//        Toast.makeText(this, "调用相机", Toast.LENGTH_SHORT).show();
        GF_PluginAndroid.getInstance().requestPermission(GF_PluginAndroid.P_CAMERA, true, new IPermissionRequestCallback() {
            @Override
            public void onRequestFinished(ArrayList<Integer> permissionsDenied) {
                if (permissionsDenied.size() == 0) {
                    Intent intent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
                    intent.putExtra(MediaStore.EXTRA_OUTPUT, Uri.fromFile(new File(Environment.getExternalStorageDirectory(), "temp.jpg")));
                    startActivityForResult(intent, PHOTOHRAPH);
                }
            }
        });
    }

    //调用相册
    public void takeFromAlbum() {
//        Toast.makeText(this, "调用相册", Toast.LENGTH_SHORT).show();
        Intent intent = new Intent(Intent.ACTION_PICK, null);
        intent.setDataAndType(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, "image/*");
        startActivityForResult(intent, PHOTO_REQUEST_CODE);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        // TODO Auto-generated method stub
        super.onActivityResult(requestCode, resultCode, data);
        if (resultCode == NONE) {
            finish();
            return;
        }
        String outPath = Environment.getExternalStorageDirectory() + "/temp.jpg";

        if (PHOTO_REQUEST_CODE == requestCode) {
            //            调用相册
            if (data == null) {
                return;
            }
            Uri uri = data.getData();
            try {
                savePng(uri, outPath);
                GF_PluginAndroid.noticeUnity("TakeAlbum", "temp.jpg");
            } catch (IOException e) {
                e.printStackTrace();
                GF_PluginAndroid.noticeUnity("TakeAlbum", "");
            }
        }

        if (requestCode == PHOTOHRAPH) {
            //            调用相机
            try {
                savePng("", outPath);
                GF_PluginAndroid.noticeUnity("TakePhoto", "temp.jpg");
            } catch (IOException e) {
                e.printStackTrace();
                GF_PluginAndroid.noticeUnity("TakePhoto", "");
            }
        }
        finish();
    }

    private String getImagePath(Uri uri) {
        if (null == uri) return null;
        String path = null;
        final String scheme = uri.getScheme();
        if (null == scheme) {
            path = uri.getPath();
        } else if (ContentResolver.SCHEME_FILE.equals(scheme)) {
            path = uri.getPath();
        } else if (ContentResolver.SCHEME_CONTENT.equals(scheme)) {
            String[] proj = {MediaStore.Images.Media.DATA};
            Cursor cursor = getContentResolver().query(uri, proj, null, null,
                    null);
            int nPhotoColumn = cursor
                    .getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
            if (null != cursor) {
                cursor.moveToFirst();
                path = cursor.getString(nPhotoColumn);
            }
            cursor.close();
        }
        return path;
    }

    private void savePng(String oriPath, String outPath) throws IOException {
        File f = new File(outPath);
        if (!f.exists()) {
            f.mkdirs();
        }
        f = new File(oriPath);
        if (!f.exists()) {
            f.mkdirs();
        }
        FileOutputStream fout = new FileOutputStream(f);
        Bitmap bm = BitmapFactory.decodeFile(outPath);
        bm.compress(Bitmap.CompressFormat.PNG, 100, fout);
        fout.flush();
        fout.close();
    }

    private void savePng(Uri uri, String outPath) throws IOException {
        savePng(uri.getPath(), outPath);
    }
}
