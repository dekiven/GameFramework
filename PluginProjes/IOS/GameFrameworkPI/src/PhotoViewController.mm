//
//  PhotoViewController.m
//  Unity-iPhone
//
//  Created by dekiven on 2018/9/27.
//

#import "PhotoViewController.h"
//#import <objc/runtime.h>
#import "CommonDefines.h"


@interface PhotoViewController ()

@end

@implementation PhotoViewController

// ========================================方法---------------------------------------
// 拍照获取 TODO:多次拍照会有崩溃风险Message from debugger: Terminated due to memory issue
-(void) takePhoto
{
    UIImagePickerControllerSourceType sourceType = UIImagePickerControllerSourceTypeCamera;
    [self openImageView: sourceType AllowsEditing:NO];
//    if ([UIImagePickerController isSourceTypeAvailable: UIImagePickerControllerSourceTypeCamera])
//    {
//        UIImagePickerController *picker = [[UIImagePickerController alloc] init];
//        picker.delegate = self;
//        //设置拍照后的图片可被编辑
//        picker.allowsEditing = NO;
//        picker.sourceType = sourceType;
//        // [picker release];
//        [self presentViewController:picker animated:YES completion:^{}];
//    }else
//    {
//        NSLog(@"模拟其中无法打开照相机,请在真机中使用");
//    }
}

//从相册获取
-(void) takeAlbum
{
//    UIImagePickerController *picker = [[UIImagePickerController alloc] init];
    
    UIImagePickerControllerSourceType sourceType = UIImagePickerControllerSourceTypePhotoLibrary;
    [self openImageView: sourceType AllowsEditing:NO];
//    picker.delegate = self;
//    //设置选择后的图片可被编辑
//    picker.allowsEditing = NO;
//    [self presentViewController:picker animated:YES completion:^{}];
}

// 保存image到沙盒
+ (NSString*)saveImage2Sandbox: (UIImage *)image Name:(NSString *)name{
    NSArray *paths =NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,NSUserDomainMask,YES);
    NSString *filePath = [[paths objectAtIndex:0] stringByAppendingPathComponent:name];  // 保存文件的名称
    NSData *data = UIImagePNGRepresentation(image);
    if (nil == data) {
        data = UIImageJPEGRepresentation(image, 1);
    }
    BOOL result = [data writeToFile:filePath atomically:YES];
    if (result == YES) {
//        NSLog(@"filePath");
//        NSLog(filePath);
        return filePath;
    }
    return nil;
}

// ----------------------------------------方法=======================================

// ======================================UIImagePickerControllerDelegate 相关-----------------------------------------
// 选择完成
- (void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary<NSString *, id> *)info
{
    // 检测当前是否选择图片
    NSString *type = [info objectForKey:UIImagePickerControllerMediaType];
    if ([type isEqualToString:@"public.image"])
    {
        //得到照片
        UIImage *image = [info objectForKey:@"UIImagePickerControllerEditedImage"];
        if (image == nil) {
            image = [info objectForKey:@"UIImagePickerControllerOriginalImage"];
        }
        
        //有些时候我们拍照后经常发现导出的照片方向会有问题，要么横着，要么颠倒着，需要旋转才适合观看。但是在ios设备上是正常的
        //所以在这里处理了图片  让他旋转成我们需要的
        if (image.imageOrientation != UIImageOrientationUp) {
            //图片旋转
            image = [self fixOrientation:image];
        }
        
        // 保存图片到沙盒/Temp.png
        NSString *fullPath = [PhotoViewController saveImage2Sandbox:image Name:@"Temp.png"];
        if (nil != fullPath)
        {
            NoticeUnity( "__,__Temp.png");
        }
        else
        {
            UnitySendMessage("GameObject", "Message", "");
        }
        
    }
    //关闭界面
    [picker dismissViewControllerAnimated:YES completion:^{}];
}
// 取消选择
- (void)imagePickerControllerDidCancel:(UIImagePickerController *)picker
{
    //关闭界面
    [picker dismissViewControllerAnimated:YES completion:^{}];
    UnitySendMessage("GameObject", "Message", "");
}
// -----------------------------------------UIImagePickerControllerDelegate 相关======================================

// ========================================私有方法---------------------------------------
-(void)openImageView:(UIImagePickerControllerSourceType)type AllowsEditing:(BOOL)allow{
    //创建UIImagePickerController实例
    UIImagePickerController *picker;
    picker= [[UIImagePickerController alloc]init];
    //设置代理
    picker.delegate = (id)self;
    //是否允许编辑 (默认为NO)
    picker.allowsEditing = allow;
    //设置照片的来源
    // UIImagePickerControllerSourceTypePhotoLibrary,      // 来自图库
    // UIImagePickerControllerSourceTypeCamera,            // 来自相机
    // UIImagePickerControllerSourceTypeSavedPhotosAlbum   // 来自相册
    picker.sourceType = type;
    
    //这里需要判断设备是iphone还是ipad  如果使用的是iphone并没有问题 但是如果 是在ipad上调用相册获取图片 会出现没有确定(选择)的按钮 所以这里判断
    //了一下设备，针对ipad 使用另一种方法 但是这种方法是弹出一个界面 并不是覆盖整个界面 需要改进 试过另一种方式 重写一个相册界面
    //（QQ的ipad选择头像的界面 就使用了这种方式 但是这里我们先不讲 （因为我也不太懂 但是我按照简书的一位老哥的文章写出来了 这里放一下这个简书的链接
    //https://www.jianshu.com/p/0ddf4f7476aa）
    if (picker.sourceType == UIImagePickerControllerSourceTypePhotoLibrary &&[[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPad) {
        // 设置弹出的控制器的显示样式
        picker.modalPresentationStyle = UIModalPresentationPopover;
        //获取这个弹出控制器
        UIPopoverPresentationController *popover = picker.popoverPresentationController;
        //设置代理
        popover.delegate = (id)self;
        //下面两个属性设置弹出位置
        popover.sourceRect = CGRectMake(0, 0, 0, 0);
        popover.sourceView = self.view;
        //设置箭头的位置
        popover.permittedArrowDirections = UIPopoverArrowDirectionAny;
        //展示选取照片控制器
        [self presentViewController:picker animated:YES completion:nil];
    } else {
        //展示选取照片控制器
        [self presentViewController:picker animated:YES completion:^{}];
    }
    
}

//图片旋转处理
- (UIImage *)fixOrientation:(UIImage *)aImage {
    CGAffineTransform transform = CGAffineTransformIdentity;
    
    switch (aImage.imageOrientation) {
        case UIImageOrientationDown:
        case UIImageOrientationDownMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.width, aImage.size.height);
            transform = CGAffineTransformRotate(transform, M_PI);
            break;
            
        case UIImageOrientationLeft:
        case UIImageOrientationLeftMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.width, 0);
            transform = CGAffineTransformRotate(transform, M_PI_2);
            break;
            
        case UIImageOrientationRight:
        case UIImageOrientationRightMirrored:
            transform = CGAffineTransformTranslate(transform, 0, aImage.size.height);
            transform = CGAffineTransformRotate(transform, -M_PI_2);
            break;
        default:
            break;
    }
    
    switch (aImage.imageOrientation) {
        case UIImageOrientationUpMirrored:
        case UIImageOrientationDownMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.width, 0);
            transform = CGAffineTransformScale(transform, -1, 1);
            break;
            
        case UIImageOrientationLeftMirrored:
        case UIImageOrientationRightMirrored:
            transform = CGAffineTransformTranslate(transform, aImage.size.height, 0);
            transform = CGAffineTransformScale(transform, -1, 1);
            break;
        default:
            break;
    }
    
    // Now we draw the underlying CGImage into a new context, applying the transform
    // calculated above.
    CGContextRef ctx = CGBitmapContextCreate(NULL, aImage.size.width, aImage.size.height,
                                             CGImageGetBitsPerComponent(aImage.CGImage), 0,
                                             CGImageGetColorSpace(aImage.CGImage),
                                             CGImageGetBitmapInfo(aImage.CGImage));
    CGContextConcatCTM(ctx, transform);
    switch (aImage.imageOrientation) {
        case UIImageOrientationLeft:
        case UIImageOrientationLeftMirrored:
        case UIImageOrientationRight:
        case UIImageOrientationRightMirrored:
            // Grr...
            CGContextDrawImage(ctx, CGRectMake(0,0,aImage.size.height,aImage.size.width), aImage.CGImage);
            break;
            
        default:
            CGContextDrawImage(ctx, CGRectMake(0,0,aImage.size.width,aImage.size.height), aImage.CGImage);
            break;
    }
    // And now we just create a new UIImage from the drawing context
    CGImageRef cgimg = CGBitmapContextCreateImage(ctx);
    UIImage *img = [UIImage imageWithCGImage:cgimg];
    CGContextRelease(ctx);
    CGImageRelease(cgimg);
    return img;
}
// ----------------------------------------私有方法=======================================
@end
