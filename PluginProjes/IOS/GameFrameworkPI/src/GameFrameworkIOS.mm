//
//  GameFrameworkIOS.m
//  Unity-iPhone
//
//  Created by dekiven on 2018/9/27.
//

#import "GameFrameworkIOS.h"
#import "PhotoViewController.h"

#if defined(__cplusplus)
extern "C" {
#endif
    //===================================导出接口实现---------------------------------------
    void GFSetNoticeObFunc(const char* gameobjName, const char* funcName)
    {
        GFNoticeGameobjName = gameobjName;
        GFNoticeFuncName = funcName;
    }
    
    void GFTakePhoto()
    {
        PhotoViewController *app = [[PhotoViewController alloc]init];
        UIViewController *vc = UnityGetGLViewController();
        [vc.view addSubview:app.view];
        [app takePhoto];
    }
    
    void GFTakeAlbum()
    {
        PhotoViewController *app = [[PhotoViewController alloc]init];
        UIViewController *vc = UnityGetGLViewController();
        [vc.view addSubview:app.view];
        [app takeAlbum];
    }
    
//    void GFRestart(float delaySec)
//    {
//        // TODO
//    }
    //---------------------------------------导出接口实现===================================
#if defined(__cplusplus)
}
#endif
