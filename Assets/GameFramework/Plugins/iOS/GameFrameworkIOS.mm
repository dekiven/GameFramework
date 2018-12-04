//
//  GameFrameworkIOS.m
//  Unity-iPhone
//
//  Created by dekiven on 2018/9/27.
//

#import "GameFrameworkIOS.h"
#import "PhotoViewController.h"
#import "STRIAPManager.h"
#import "CommonDefines.h"

//加载dll时初始化
STRIAPManager *_sharedIAPMgr = [STRIAPManager shareSIAPManager];

//extern const char* GFNoticeGameobjName;
//extern const char* GFNoticeFuncName;
//extern const char* GFNoticeSplitStr;

#if defined(__cplusplus)
extern "C" {
#endif
    //===================================导出接口实现---------------------------------------
    void GFSetNoticeObFunc(const char* gameobjName, const char* funcName)
    {
        GFNoticeGameobjName = gameobjName;
        GFNoticeFuncName = funcName;
    }
    
    void GFSetNotifySplitStr(const char* splitStr)
    {
        GFNoticeSplitStr = splitStr;
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
    
    void GFRestart(float delaySec)
    {
        // TODO
    }
    
    void GFStartPurchase(const char* pid, const char* externalData)
    {
        if(!_sharedIAPMgr)
        {
            _sharedIAPMgr = [STRIAPManager shareSIAPManager];
        }
        [_sharedIAPMgr startPurchWithID:NSStringFromUnityString(pid) completeHandle:^(SIAPPurchType type, NSData *data) {
            if(type == SIAPPurchVerSuccess)
            {
                NoticeUnity(STR_EVENT_START_PURCHASE, @"true",[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding]);
            }
            else
            {
                NoticeUnity(STR_EVENT_START_PURCHASE, @"false", enumToString(type));
            }
        }];
    }
    //---------------------------------------导出接口实现===================================
#if defined(__cplusplus)
}
#endif
