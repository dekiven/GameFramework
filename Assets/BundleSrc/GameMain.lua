require('common.requireCommon')

--主入口函数。从这里开始lua逻辑
function GameMain()					
	print("logic start")

    -- listner = UpdateBeat:CreateListener(MainUpdate, 0)
    -- UpdateBeat:AddListener(listner)
    
    -- print(tostring(test))
    StartTest();
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end

function OnApplicationQuit()
end

function MainUpdate()
    -- print("MainUpdate")
end

function OnEvent( eventName, ... )
    pLog('OnEvent :'..tostring(eventName))
    EventManager.notifyEvent(eventName, ...)
end

function StartTest()
    local bundles = 
    {
        'test',
    }
    AddLuaBundles(bundles)

    local test = require("test.Test").new()
    pLog('lua:StartTest')
end