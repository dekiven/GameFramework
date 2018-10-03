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
import android.util.Log;
import android.widget.Toast;

import com.unity3d.player.UnityPlayer;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;

public class ImageTakeActivity extends Activity {

    private static final String TAG = ImageTakeActivity.class.getSimpleName();
    private static final int PHOTO_REQUEST_CODE = 1;//相册
    public static final int PHOTOHRAPH = 2;// 拍照
    private static final boolean DEBUG = false;
    //  private String unitygameobjectName = "Main Camera";
    private String unitygameobjectName = "GameObject"; //Unity 中对应挂脚本对象的名称
    public static final int NONE = 0;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        String method = this.getIntent().getStringExtra("method");
        Toast.makeText(this, "method："+method, Toast.LENGTH_SHORT).show();
        if("takeFromPhoto".equals(method))
        {
            takeFromPhoto();
        }
        else if("takeFromAlbum".equals(method))
        {
            takeFromAlbum();
        }

    }

    //调用相机
    public void takeFromPhoto(){

        Toast.makeText(this, "调用相机", Toast.LENGTH_SHORT).show();
        Intent intent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
        intent.putExtra(MediaStore.EXTRA_OUTPUT, Uri.fromFile(new File(Environment.getExternalStorageDirectory(), "temp.jpg")));
        startActivityForResult(intent, PHOTOHRAPH);

    }

    //调用相册
    public void takeFromAlbum()
    {
        Toast.makeText(this, "调用相册", Toast.LENGTH_SHORT).show();
        Intent intent = new Intent(Intent.ACTION_PICK,null);
        intent.setDataAndType(MediaStore.Images.Media.EXTERNAL_CONTENT_URI,"image/*");
        startActivityForResult(intent, PHOTO_REQUEST_CODE);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        // TODO Auto-generated method stub
        super.onActivityResult(requestCode, resultCode, data);
        if (resultCode == NONE){
            return;
        }
        if(PHOTO_REQUEST_CODE == requestCode){
            //            调用相册
            if(data == null){
                return;
            }
            Uri uri = data.getData();
            String imagePath = getImagePath(uri);
            if(DEBUG){
                Log.d(TAG, imagePath);
            }
            //调用unity中方法 GetImagePath（imagePath）
            UnityPlayer.UnitySendMessage(unitygameobjectName, "Message", imagePath);
        }

        if (requestCode == PHOTOHRAPH) {
            //            调用相机
            String path = Environment.getExternalStorageDirectory() + "/temp.jpg";
            if(DEBUG){
                Log.e("path:", path);
            }
            //调用unity中方法 GetTakeImagePath（path）
            try {
                Bitmap bitmap = BitmapFactory.decodeFile(path);
                SaveBitmap(bitmap);
                UnityPlayer.UnitySendMessage(unitygameobjectName, "Message", "temp.jpg");
            } catch (IOException e) {
                e.printStackTrace();
            }
        }

    }

    private String getImagePath(Uri uri)
    {
        if(null == uri) return null;
        String path = null;
        final String scheme = uri.getScheme();
        if (null == scheme) {
            path = uri.getPath();
        } else if (ContentResolver.SCHEME_FILE.equals(scheme)) {
            path = uri.getPath();
        } else if (ContentResolver.SCHEME_CONTENT.equals(scheme)) {
            String[] proj = { MediaStore.Images.Media.DATA };
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

    public void SaveBitmap(Bitmap bitmap) throws IOException {

        FileOutputStream fOut = null;
        String path = "/mnt/sdcard/DCIM/";
        try {
            //查看这个路径是否存在，
            //如果并没有这个路径，
            //创建这个路径
            File destDir = new File(path);
            if (!destDir.exists())
            {
                destDir.mkdirs();
            }
            String FILE_NAME = System.currentTimeMillis() + ".jpg";
            fOut = new FileOutputStream(path + "/" + FILE_NAME) ;
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        }
        //将Bitmap对象写入本地路径中
        bitmap.compress(Bitmap.CompressFormat.JPEG, 100, fOut);
        try {
            fOut.flush();
        } catch (IOException e) {
            e.printStackTrace();
        }
        try {
            fOut.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}
