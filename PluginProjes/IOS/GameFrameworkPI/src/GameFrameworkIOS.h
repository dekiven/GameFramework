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
    void GFSetNoticeObFunc(const char* gameobjName, const char* funcName);
    // 通过拍照获取一张图片，保存到沙盒/Temp.png
    void GFTakePhoto();
    // 通过相册获取一张图片，保存到沙盒/Temp.png
    void GFTakeAlbum();
    // 重启应用
    void GFRestart(float delaySec);
//---------------------------------------导出接口供外部使用===================================
#if defined(__cplusplus)
}
#endif

#endif /* __GameFrameworkIOS_h__ */
