//
//  CommomDefines.cpp
//  Unity-iPhone
//
//  Created by sjytyf3 on 2018/12/4.
//

#include "CommonDefines.h"

const char* GFDefine::GFNoticeGameobjName = "GameFramework.GameManager";
const char* GFDefine::GFNoticeFuncName = "OnMessage";
const char* GFDefine::GFNoticeSplitStr = "__;__";

//事件名
const char* GFDefine::STR_EVENT_TAKE_PHOTO = "TakeImagePhoto";
const char* GFDefine::STR_EVENT_TAKE_ALBUM = "TakeImageAlbum";
const char* GFDefine::STR_EVENT_START_PURCHASE = "StartPurchase";

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
    UnitySendMessage(GFDefine::GFNoticeGameobjName, GFDefine::GFNoticeFuncName, msg);
}

void NoticeUnity(const char* eventName, NSString* msg)
{
    const char* _msg = UnityStringFromNSString([NSString stringWithFormat:@"%s%s%@", eventName, GFDefine::GFNoticeSplitStr, msg]);
    NoticeUnity(_msg);
}

void NoticeUnity(const char* eventName, NSString* msg1, NSString* msg2)
{
    const char* _msg = UnityStringFromNSString([NSString stringWithFormat:@"%s%s%@%s%@", eventName, GFDefine::GFNoticeSplitStr, msg1, GFDefine::GFNoticeSplitStr, msg2]);
    NoticeUnity(_msg);
}

void NoticeUnity(const char* eventName, NSString* msg1, NSString* msg2, NSString* msg3)
{
    const char* _msg = UnityStringFromNSString([NSString stringWithFormat:@"%s%s%@%s%@%s%@", eventName, GFDefine::GFNoticeSplitStr, msg1, GFDefine::GFNoticeSplitStr, msg2, GFDefine::GFNoticeSplitStr, msg3]);
    NoticeUnity(_msg);
}
