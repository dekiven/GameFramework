-- 名称：        LanguageManager
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2019-02-16 11:38:40
-- 说明：
--[[
        
--]]
-- 需要一下模块，请在一下模块初始化后 require LanguageManager
-- require('common. EventManager')
-- require('common.functions')
-- require('common.gameFramework')
local luaExp = GameFramework.LuaExportFuncs

local GetStr = luaExp.GetStr
-- string GetLanguage
local GetLanguage = luaExp.GetLanguage
-- void SetLanguage(string language, LuaFunction function)
local SetLanguage = luaExp.SetLanguage
-- string GetValidLanguages()
local GetValidLanguages = luaExp.GetValidLanguages

luaExp = nil

STR_ON_LANGUAGE_CHANGED = 'onLanguageChanged'

LanguageManager = 
{
    curLanguage = 'cn',
    validLanguages = {'cn',},
    mods = {},
    data = {},
}

local function initLanguage( )
    LanguageManager.curLanguage = GetLanguage()
    LanguageManager.validLanguages = GetValidLanguages():split(',')
end

function loadLangMod( modName, force )
    local modPath = getModPath(modName)
    local modFile = modName:sub(#modPath+1, #modName)
    modPath = modPath..'.'..LanguageManager.curLanguage
    local realMod = modPath..modFile

    local hasMod = table.indexof(LanguageManager.mods, modName)
    if (not hasMod) or force then
        AddLuaBundle(modPath)
        local data = forceRequire(realMod)
        local data2 = require(realMod)
        table.merge(LanguageManager.data, data)
        if not hasMod then
            table.insert(LanguageManager.mods, modName)
        end
    end
end

function changeLanguage( language )
    if table.indexof(LanguageManager.validLanguages, language) and language ~= LanguageManager.curLanguage then
        SetLanguage(language, function ( ret )
            if ret then
                LanguageManager.data = {}
                LanguageManager.curLanguage = language
                for i, v in ipairs(LanguageManager.mods) do
                    loadLangMod(v, true)
                end
            end
            -- 通知语言切换，接收消息后刷新界面
            EventManager.notifyEvent(STR_ON_LANGUAGE_CHANGED, ret)
        end)
    end
end

-- 获取当前语言（lua）给定 key 对应的值，没有则返回 key
function getStr( key )
    return LanguageManager.data[key] or key
end

-- 获取当前语言（C#）给定 key 对应的值，没有则返回 key
function getNativeStr( key )
    return GetStr(key)
end

-- 获取当前语言的 asb path
function getAsbPath( path )
    return path..'/'..LanguageManager.curLanguage
end

function getValidLanguages( )
    return LanguageManager.validLanguages
end

initLanguage()