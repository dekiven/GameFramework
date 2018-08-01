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
        -- 热更新失败，将之前的值赋值回去
        print(string.format('[%s] 热加载失败，日志：%s', modName, tostring(err)))
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

function printLog( msg )
    GameFramework.LogFile.Log(msg)
end

function printWarn( msg )
    GameFramework.LogFile.Warn(msg)
end

function printError( msg )
    GameFramework.LogFile.Error(msg)
end
