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

class GFDefine
{
    public :
        static const char* GFNoticeGameobjName;
        static const char* GFNoticeFuncName;
        static const char* GFNoticeSplitStr;

        //事件名
        static const char* STR_EVENT_TAKE_PHOTO;
        static const char* STR_EVENT_TAKE_ALBUM;
        static const char* STR_EVENT_START_PURCHASE;
};

char* UnityStringFromNSString(NSString* string);

NSString* NSStringFromUnityString(const char* unityString_);

NSString * convertToJsonData(NSDictionary * dict);

NSDictionary * dictionaryWithJsonString(NSString * jsonString);

void NoticeUnity(const char * msg);

void NoticeUnity(const char* eventName, NSString* msg);

void NoticeUnity(const char* eventName, NSString* msg1, NSString* msg2);

void NoticeUnity(const char* eventName, NSString* msg1, NSString* msg2, NSString* msg3);
#endif /* __CommonDefines_h__ */
