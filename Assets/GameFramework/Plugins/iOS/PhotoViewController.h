//
//  PhotoViewController.h
//  Unity-iPhone
//
//  Created by dekiven on 2018/9/27.
//
// 参考
// Unity调用ios的相机相册
// https://blog.csdn.net/kangying3769/article/details/80369341
#import "CommonDefines.h"

@interface PhotoViewController : UIViewController <UIImagePickerControllerDelegate, UINavigationControllerDelegate>
// 从拍照获取
-(void) takePhoto;
// 从相册获取
-(void) takeAlbum;

+ (NSString*)saveImage2Sandbox: (UIImage *)image Name:(NSString *)name;
@end
