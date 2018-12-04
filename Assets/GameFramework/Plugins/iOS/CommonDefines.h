//
//  CommonDefines.h
//  GameFrameworkIOSPlugin
//
//  Created by dekiven on 2018/9/30.
//  Copyright © 2018年 dekiven. All rights reserved.
//

#ifndef __Common_Defines_h__
#define __Common_Defines_h__

#import <UIKit/UIKit.h>

//Enum转字符串
#define enumToString(value)  @#value

extern void UnitySendMessage(const char *, const char *, const char *);
extern UIViewController *UnityGetGLViewController();

const char* GFNoticeGameobjName = "GameFramework.GameManager";
const char* GFNoticeFuncName = "OnMessage";
const char* GFNoticeSplitStr = "__;__";

//事件名
const char* STR_EVENT_TAKE_PHOTO = "TakeImagePhoto";
const char* STR_EVENT_TAKE_ALBUM = "TakeImageAlbum";
const char* STR_EVENT_START_PURCHASE = "StartPurchase";

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

void NoticeUnity(const char* eventName, NSString* msg)
{
    const char* _msg = UnityStringFromNSString([NSString stringWithFormat:@"%s%s%@", eventName, GFNoticeSplitStr, msg]);
    NoticeUnity(_msg);
}

void NoticeUnity(const char* eventName, NSString* msg1, NSString* msg2)
{
    const char* _msg = UnityStringFromNSString([NSString stringWithFormat:@"%s%s%@%s%@", eventName, GFNoticeSplitStr, msg1, GFNoticeSplitStr, msg2]);
    NoticeUnity(_msg);
}

void NoticeUnity(const char* eventName, NSString* msg1, NSString* msg2, NSString* msg3)
{
    const char* _msg = UnityStringFromNSString([NSString stringWithFormat:@"%s%s%@%s%@%s%@", eventName, GFNoticeSplitStr, msg1, GFNoticeSplitStr, msg2, GFNoticeSplitStr, msg3]);
    NoticeUnity(_msg);
}
#endif /* __CommonDefines_h__ */
