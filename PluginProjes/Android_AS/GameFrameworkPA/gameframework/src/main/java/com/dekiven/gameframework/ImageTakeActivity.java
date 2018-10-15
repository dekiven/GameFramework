package com.dekiven.gameframework;

import android.annotation.TargetApi;
import android.app.Activity;
import android.content.ContentUris;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.DocumentsContract;
import android.provider.MediaStore;
import android.widget.Toast;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;

//android 7.0之后使用FileProvider，详见：
//Android 一起来看看 7.0 的新特性 FileProvider
//https://www.jianshu.com/p/be817f3aa145

public class ImageTakeActivity extends Activity {
    private static final int PHOTO_REQUEST_CODE = 1;//相册
    public static final int PHOTOHRAPH = 2;// 拍照
    public static final int NONE = 0;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        String method = this.getIntent().getStringExtra("method");
        if ("takeFromPhoto".equals(method)) {
            takeFromPhoto();
        } else if ("takeFromAlbum".equals(method)) {
            takeFromAlbum();
        }

    }

    //调用相机
    public void takeFromPhoto() {
        //创建一个file，用来存储拍照后的照片
        Context context = ImageTakeActivity.this;
        File outputFile = new File(context.getExternalFilesDir(null), "temp.jpg");
        //GF_PluginAndroid.showToast(context, "outputFile:\n" + outputFile);
        try {
            if (outputFile.exists()) {
                outputFile.delete();//删除
            }
            outputFile.createNewFile();
        } catch (Exception e) {
            e.printStackTrace();
        }
        Uri imageUri = GF_PluginAndroid.getUri(context, outputFile);

        //启动相机程序
        Intent intent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
        intent.putExtra(MediaStore.EXTRA_OUTPUT, imageUri);
        //对目标应用临时授权该Uri所代表的文件,7.0后必须
        intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
        startActivityForResult(intent, PHOTOHRAPH);
    }

    //调用相册
    public void takeFromAlbum() {
        //Toast.makeText(this, "调用相册", Toast.LENGTH_SHORT).show();
        Intent intent = new Intent(Intent.ACTION_PICK, null);
        intent.setDataAndType(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, "image/*");
        startActivityForResult(intent, PHOTO_REQUEST_CODE);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        // TODO Auto-generated method stub
        super.onActivityResult(requestCode, resultCode, data);
        if (resultCode == NONE) {
            GF_PluginAndroid.LogEvent("拍照或选取图片 取消");
            GF_PluginAndroid.notifyUnity("TakeImageAlbum", "");
            finish();
            return;
        }
        String outPath = ImageTakeActivity.this.getExternalFilesDir(null) + "/temp.jpg";

        if (PHOTO_REQUEST_CODE == requestCode) {
            //调用相册
            if (data == null) {
                GF_PluginAndroid.LogEvent("调用相册 结果 数据为空 ");
                return;
            }
            try {
                boolean rst = saveJpg(data.getData(), outPath);
                GF_PluginAndroid.notifyUnity("TakeImageAlbum", rst ? "temp.jpg" : "");
            } catch (IOException e) {
                e.printStackTrace();
                GF_PluginAndroid.notifyUnity("TakeImageAlbum", "");
                GF_PluginAndroid.LogEvent("调用相册, 失败：" + e.toString());
            }
        }

        if (requestCode == PHOTOHRAPH) {
            //调用相机,如果成功结果已经保存到 xx.xx.xx/files/temp.jpg
            GF_PluginAndroid.notifyUnity("TakeImagePhoto", "temp.jpg");
        }
        finish();
    }

    private boolean saveJpg(String oriPath, String outPath) throws IOException {
        //如果是选择的temp.jpg（要复制到的那张图片）直接return true
        if (oriPath.equals(outPath) && !oriPath.isEmpty() && null != oriPath)
        {
            return true;
        }
        File f = new File(oriPath);
        if (!f.exists()) {
            return false;
        }

        f = new File(outPath);
        GF_PluginAndroid.LogEvent("保存 :" + oriPath + "\n到："+outPath);
        if (f.exists()) {
            f.delete();
        }
        f.createNewFile();
        FileOutputStream fout = new FileOutputStream(f);
        Bitmap bm = BitmapFactory.decodeFile(oriPath);
        bm.compress(Bitmap.CompressFormat.JPEG, 100, fout);
        fout.flush();
        fout.close();
        return true;
    }

    private boolean saveJpg(Uri uri, String outPath) throws IOException {
        String path = null;
        if (Build.VERSION.SDK_INT >= 19) {
            path = handleImageOnKitKat(uri);
        } else {
            path = handleImageBeforeKitKat(uri);
        }
        return saveJpg(path, outPath);
    }

    //--------------------
    @TargetApi(19)
    private String handleImageOnKitKat(Uri uri) {
        String path = null;
        if (DocumentsContract.isDocumentUri(this, uri)) {
            String docId = DocumentsContract.getDocumentId(uri);
            if ("com.android.providers.media.documents".equals(uri.getAuthority())) {
                String id = docId.split(":")[1];
                String selection = MediaStore.Images.Media._ID + "=" + id;
                path = getImagePath(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, selection);
            } else if ("com.android.providers.downloads.documents".equals(uri.getAuthority())) {
                Uri contentUri = ContentUris.withAppendedId(Uri.parse("content://downloads/public_downloads"), Long.valueOf(docId));
                path = getImagePath(contentUri, null);
            }
        } else if ("content".equalsIgnoreCase(uri.getScheme())) {
            path = getImagePath(uri, null);
        } else if ("file".equalsIgnoreCase(uri.getScheme())) {
            path = uri.getPath();
        }
        return path;
    }

    private String handleImageBeforeKitKat(Uri uri) {
        String path = null;
        path = getImagePath(uri, null);
        return path;
    }

    private String getImagePath(Uri uri, String selection) {
        String Path = null;
        Cursor cursor = getContentResolver().query(uri, null, selection, null, null);
        if (cursor != null) {
            if (cursor.moveToFirst()) {
                Path = cursor.getString(cursor.getColumnIndex(MediaStore.Images.Media.DATA));
            }
            cursor.close();
        }
        return Path;
    }
    //====================
}
