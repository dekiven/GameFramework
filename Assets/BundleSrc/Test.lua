-- 说明：TestClass
local TestClass = class("TestClass")

function TestClass:ctor( ... )
    local gm = GameFramework.LuaExportFuncs

    
    GameFramework.LogFile.Log('gm.LoadScene')
    -- gm.LoadScene('res/Scenes/stage01/test', 'Stage01.unity', true, false, function ( progress )
    --     GameFramework.LogFile.Log('progress:'..progress)
    --     if progress >= 1 then
    --         GameFramework.LogFile.Log('gm.LoadScene 2')

    --         gm.LoadGameObj('res/test/test/Cube', '.prefab', function ( obj )
    --             -- print(tostring(obj[0]))
    --             -- GameFramework.LogFile.Log(obj[0])
    --             print(tostring(UnityEngine.GameObject.Instantiate))
    --             local cube = u3d.Instantiate(obj[0])
    --             -- cube.transform

    --             -- GameFramework.LogFile.Log(cube)

    --             GameFramework.LogFile.Log('Lua logFile')
    --             gm.TestDelegate(function ( num )
    --                 print('delegate :'..num)
    --             end)
    --         end)
    --     end
    -- end)
    gm.LoadScene('res/Scenes/TestSpriteAtlas', '.unity', true, false, function ( progress )
        -- body
        -- printLog('SpriteAtlas 测试场景载入完成！')
        printLog('progress:'..progress)
    end)

end

return TestClass
