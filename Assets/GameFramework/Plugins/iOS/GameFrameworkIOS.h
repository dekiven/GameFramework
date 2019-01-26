//
//  GameFrameworkIOS.h
//  Unity-iPhone
//
//  Created by dekiven on 2018/9/27.
//

#ifndef __GameFrameworkIOS_h__
#define __GameFrameworkIOS_h__

#import <UIKit/UIKit.h>

#if defined(__cplusplus)
extern "C" {
#endif
//===================================导出接口供外部使用---------------------------------------
    //设置通知Unity的对象名和方法名
    void GFSetNoticeObFunc(const char* gameobjName, const char* funcName);
    //设置通知消息的分割字符串
    void GFSetNotifySplitStr(const char* splitStr);
    // 通过拍照获取一张图片，保存到沙盒/Temp.png
    void GFTakePhoto();
    // 通过相册获取一张图片，保存到沙盒/Temp.png
    void GFTakeAlbum();
    // TODO:重启应用
    void GFRestart(float delaySec);
    //请求支付订单
    void GFStartPurchase(const char* pid, const char* externalData);
    //复制文字到剪贴板
    void GFCopy2Pasteboard(const char* content);
    //从剪贴板复制文字
    const char* GFGetFirstPastboard();
//---------------------------------------导出接口供外部使用===================================
#if defined(__cplusplus)
}
#endif

#endif /* __GameFrameworkIOS_h__ */
