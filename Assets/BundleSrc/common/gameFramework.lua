-- 名称：     GameFramework
-- 编码：      utf-8
-- 创建人：     Dekiven_PC
-- 创建时间：    2018-07-10 21:39:11
-- 说明：
--[[
        
--]]


-- 修改了Class，实现单例
function singletonClass(classname, super)
    local superType = type(super)
    local cls

    if superType ~= "function" and superType ~= "table" then
        superType = nil
        super = nil
    end

    if superType == "function" or (super and super.__ctype == 1) then
        -- inherited from native C++ Object
        cls = {}

        if superType == "table" then
            -- copy fields from super
            for k,v in pairs(super) do cls[k] = v end
            cls.__create = super.__create
            cls.super   = super
        else
            cls.__create = super
            cls.ctor = function() end
        end

        cls.__cname = classname
        cls.__ctype = 1

        function cls.new(...)
            local instance = cls.instance
            if instance == nil then
                instance = cls.__create(...)
                -- copy fields from class to native object
                for k,v in pairs(cls) do instance[k] = v end
                instance.class = cls
                instance:ctor(...)
                cls.instance = instance
            end
            return instance
        end
    else
        -- inherited from Lua Object
        if super then
            cls = {}
            setmetatable(cls, {__index = super})
            cls.super = super
        else
            cls = {ctor = function() end}
        end

        cls.__cname = classname
        cls.__ctype = 2 -- lua
        cls.__index = cls

        function cls.new(...)
            local instance = cls.instance
            if instance == nil then
                instance = setmetatable({}, cls)
                instance.class = cls
                instance:ctor(...)
                cls.instance = instance
            end
            return instance
        end
    end

    function cls.getInstance( ... )
        return cls.new( ... )
    end

    return cls
end

-- 强制require，之前require的会释放，重新require
function forceRequire(modName)
    local oldMod = {}
    if package.loaded[modName] then
        -- 保存之前的模块
        oldMod = package.loaded[modName]
        -- 赋空后可以再次require
        package.loaded[modName] = nil
    end

    -- pcall下执行require
    local ok, err = pcall(require, modName)
    if not ok then
        -- 热加载失败，将之前的值赋值回去
        printLog(string.format('[%s] 热加载失败，日志：%s', modName, tostring(err)))
        package.loaded[modName] = oldMod
    else
        return err
    end

    return oldMod
end

function tryCatch(tryCall, catchCall, finalCall)
    
    if type(tryCall) == 'table' then
        catchCall = tryCall[2]
        finalCall = tryCall[3]
        tryCall = tryCall[1]
    end
    
    if type(tryCall) == 'function' then
        ok, errors = pcall(tryCall)
        -- PrintTable({ok, errors}, 'tryCall')
        if not ok then
            if type(catchCall) == 'function' then
                ok, errors = catchCall()
            end
        end
        if type(finalCall) == 'function' then
            ok, errors = finalCall()
        end
        if ok then
            -- PrintTable({ok, errors}, 'ok')
            return errors
        end
    end
end

function  printTable( table, tName, deepth, indent, maxDeep )
    deepth = deepth or 0
    indent = indent or '    '
    maxDeep = maxDeep or 4
    local _indent = string.rep(indent, deepth)
    local msg = ''
    if nil ~= tName then 
        if type(tName) == 'number' then
            msg=  _indent..'['..tName..'] = '
        else
            msg=  _indent..tName..' = '
        end
    end
    if type(table) == 'table' then
        writeLog(msg..'\n'.._indent..'{')
        if deepth < maxDeep then
            for k, v in pairs(table) do
                printTable(v, k, deepth+1, indent, maxDeep)
            end
        else
            writeLog(_indent..indent..'...')
        end
        writeLog(_indent..'},')
    elseif type(table) == 'string' then
        writeLog(msg..'"'..table..'",')
    else
        writeLog(msg..table..',')
    -- elseif type(table) == 'boolean' then
    -- elseif type(table) == 'number' then
    -- elseif type(table) == 'funtion' then
    -- elseif type(table) == 'userdata' then
    -- elseif type(table) == 'thread' then
    end
end

function string.split2(input, delimiter, handle)
    input = tostring(input)
    delimiter = tostring(delimiter)
    handle = handle or function ( v )
        return v
    end
    if (delimiter=='') then return false end
    local pos,arr = 0, {}
    -- for each divider found
    for st,sp in function() return string.find(input, delimiter, pos, true) end do
        table.insert(arr, handle(string.sub(input, pos, st - 1)))
        pos = sp + 1
    end
    table.insert(arr, handle(string.sub(input, pos)))
    return arr
end

function dump( )
    -- body
    -- TODO:
end

-- LogFile 相关
printLog = GameFramework.LogFile.Log
printWarn = GameFramework.LogFile.Warn
printError = GameFramework.LogFile.Error
-- WriteLine仅写入
writeLog = GameFramework.LogFile.WriteLine


local luaExp = GameFramework.LuaExportFuncs
-- void LoadGameObj (string abName, string name, LuaFunction luaFunc)
-- void LoadGameObj (string abName, string[] names, LuaFunction luaFunc)
LoadGameObj = luaExp.LoadGameObj
-- void LoadTextAsset (string abName, string name, LuaFunction luaFunc)
-- void LoadTextAsset (string abName, string[] names, LuaFunction luaFunc)
LoadTextAsset = luaExp.LoadTextAsset
-- void LoadTextAssetBytes (string abName, string name, LuaFunction luaFunc)
-- void LoadTextAssetBytes (string abName, string[] names, LuaFunction luaFunc)
LoadTextAssetBytes = luaExp.LoadTextAssetBytes
-- void LoadScene (string abName, string scenenName, bool sync, bool add, LuaFunction luaFunction)
LoadScene = luaExp.LoadScene
-- void CountAsbGroup (string asbName, string group)
CountAsbGroup = luaExp.CountAsbGroup
-- void UnloadAsbGroup (string group)
UnloadAsbGroup = luaExp.UnloadAsbGroup
-- void AddLuaBundle (string name)
AddLuaBundle = luaExp.AddLuaBundle
-- void AddLuaBundles (string[] names)
AddLuaBundles = luaExp.AddLuaBundles
-- void ShowView (string asbName, string viewName)
ShowView = luaExp.ShowView
-- void PopView ()
PopView = luaExp.PopView
-- void GetAtlasAsync (string asbName, string atlasName, LuaFunction luaCall)
GetAtlasAsync = luaExp.GetAtlasAsync
-- void GetSpriteAsync (string asbName, string atlasName, string spriteName, LuaFunction luaCall)
GetSpriteAsync = luaExp.GetSpriteAsync
-- void SetCurGroup (EnumResGroup e, string group)
SetCurGroup = luaExp.SetCurGroup
-- void ClearGroup (EnumResGroup e, string group)
ClearGroup = luaExp.ClearGroup



