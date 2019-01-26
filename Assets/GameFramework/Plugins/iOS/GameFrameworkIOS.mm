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

const NSString * SIAPPurchTypeMap[] = {
    [SIAPPurchSuccess] = @"购买成功(SIAPPurchSuccess)",
    [SIAPPurchFailed] = @"购买失败(SIAPPurchFailed)",
    [SIAPPurchCancle] = @"取消购买(SIAPPurchCancle)",
    [SIAPPurchVerFailed] = @"订单校验失败(SIAPPurchVerFailed)",
    [SIAPPurchVerSuccess] = @"订单校验成功(SIAPPurchVerSuccess)",
    [SIAPPurchNotArrow] = @"不允许内购(SIAPPurchNotArrow)",
};

#if defined(__cplusplus)
extern "C" {
#endif
    //===================================导出接口实现---------------------------------------
    void GFSetNoticeObFunc(const char* gameobjName, const char* funcName)
    {
        setNoticeInfo(gameobjName, funcName);
    }
    
    void GFSetNotifySplitStr(const char* splitStr)
    {
        setSplitStr(splitStr);
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
        [_sharedIAPMgr startPurchWithID:NSStringFromUnityString(pid) externalData:NSStringFromUnityString(externalData) completeHandle:^(SIAPPurchType type, NSDictionary *data) {
            if(type == SIAPPurchSuccess)
            {
                NoticeUnity(GFDefine::STR_EVENT_START_PURCHASE, @"true", convertToJsonData(data));
            }
            else
            {
                NoticeUnity(GFDefine::STR_EVENT_START_PURCHASE, @"false", (NSString*)SIAPPurchTypeMap[type]);
            }
        }];
    }
    
    //复制文字到剪贴板
    void GFCopy2Pasteboard(const char* content)
    {
        const char* con = strdup(content);
        UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
        pasteboard.string = NSStringFromUnityString(con);
    }

    //从剪贴板复制文字
    const char* GFGetFirstPastboard()
    {
        UIPasteboard* pBoard = [UIPasteboard generalPasteboard];
        if(nil != pBoard)
        {
            NSString* str = pBoard.string;
            if(nil != str)
            {
                return UnityStringFromNSString(str);
            }
        }
        return "";
    }
    //---------------------------------------导出接口实现===================================
#if defined(__cplusplus)
}
#endif
