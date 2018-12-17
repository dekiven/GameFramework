//
//  CommomDefines.cpp
//  Unity-iPhone
//
//  Created by sjytyf3 on 2018/12/4.
//

#include "CommonDefines.h"
#include <string>

//const char* GFDefine::GFNoticeGameobjName = ;
//const char* GFDefine::GFNoticeFuncName = "OnMessage";
//const char* GFDefine::GFNoticeSplitStr = "__;__";

//事件名
const char* GFDefine::STR_EVENT_TAKE_PHOTO = "TakeImagePhoto";
const char* GFDefine::STR_EVENT_TAKE_ALBUM = "TakeImageAlbum";
const char* GFDefine::STR_EVENT_START_PURCHASE = "StartPurchase";

const char* gameObjName = "GameFramework.GameManager";
const char* funcName = "OnMessage";
const char* splitStr = "__;__";

void setNoticeInfo(const char * objName, const char* func)
{
    gameObjName = strdup(objName);
    funcName = strdup(func);
}

void setSplitStr(const char* slpit)
{
    splitStr = strdup(slpit);
}

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

NSString * convertToJsonData(NSDictionary * dict)
{
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dict options:NSJSONWritingPrettyPrinted error:&error];
    NSString *jsonString;
    if (!jsonData)
    {
        NSLog(@"%@",error);
    }else{
        jsonString = [[NSString alloc]initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
    NSMutableString *mutStr = [NSMutableString stringWithString:jsonString];
    NSRange range = {0,jsonString.length};
    //去掉字符串中的空格
    [mutStr replaceOccurrencesOfString:@" " withString:@"" options:NSLiteralSearch range:range];
    NSRange range2 = {0,mutStr.length};
    //去掉字符串中的换行符
    [mutStr replaceOccurrencesOfString:@"\n" withString:@"" options:NSLiteralSearch range:range2];
    
    return mutStr;
}

NSDictionary * dictionaryWithJsonString(NSString * jsonString)
{
    if (jsonString == nil) {
        return nil;
    }
    
    NSData *jsonData = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    NSError *err;
    NSDictionary *dic = [NSJSONSerialization JSONObjectWithData:jsonData
                                                        options:NSJSONReadingMutableContainers
                                                          error:&err];
    if(err)
    {
        NSLog(@"json解析失败：%@",err);
        return nil;
    }
    return dic;
}

void NoticeUnity(const char * msg)
{
    UnitySendMessage(gameObjName, funcName, msg);
}

void NoticeUnity(const char* eventName, NSString* msg)
{
    const char* _msg = UnityStringFromNSString([NSString stringWithFormat:@"%s%s%@", eventName, funcName, msg]);
    NoticeUnity(_msg);
}

void NoticeUnity(const char* eventName, NSString* msg1, NSString* msg2)
{
    const char* split = splitStr;
    const char* _msg = UnityStringFromNSString([NSString stringWithFormat:@"%s%s%@%s%@", eventName, split, msg1, split, msg2]);
    NoticeUnity(_msg);
}

void NoticeUnity(const char* eventName, NSString* msg1, NSString* msg2, NSString* msg3)
{
    const char* split = splitStr;
    const char* _msg = UnityStringFromNSString([NSString stringWithFormat:@"%s%s%@%s%@%s%@", eventName, split, msg1, split, msg2, split, msg3]);
    NoticeUnity(_msg);
}
