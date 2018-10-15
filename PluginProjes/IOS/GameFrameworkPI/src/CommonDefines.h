//
//  CommonDefines.h
//  GameFrameworkIOSPlugin
//
//  Created by dekiven on 2018/9/30.
//  Copyright © 2018年 dekiven. All rights reserved.
//

#ifndef __CommonDefines_h__
#define __CommonDefines_h__

#import <UIKit/UIKit.h>

extern void UnitySendMessage(const char *, const char *, const char *);
extern UIViewController *UnityGetGLViewController();

const char* GFNoticeGameobjName = "GameFramework.GameManager";
const char* GFNoticeFuncName = "OnMessage";

char* UnityStringFromNSString(NSString* string)
{
    const char* cString = string.UTF8String;
    char* _unityString = (char*)malloc(strlen(cString) + 1);
    strcpy(_unityString, cString);
    return _unityString;
}

NSString* NSStringFromUnityString(const char* unityString_)
{
    if (unityString_ == nil) return [NSString new];
    return [NSString stringWithUTF8String:unityString_];
}

void NoticeUnity(const char * msg)
{
    UnitySendMessage(GFNoticeGameobjName, GFNoticeGameobjName, msg);
}

void NoticeUnity(NSString* eventName, NSString* msg)
{
    const char* _msg = UnityStringFromNSString([NSString stringWithFormat:@"%@__;__%@", eventName, msg]);
    UnitySendMessage(GFNoticeGameobjName, GFNoticeGameobjName, _msg);
}
#endif /* __CommonDefines_h__ */
