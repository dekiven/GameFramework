-- 说明：TestClass
local TestClass = class("TestClass")

function TestClass:ctor( ... )
    local gm = GameFramework.LuaExportFuncs

    -- gm.LoadGameObj('res/test/test', 'Cube.prefab', function ( obj )
    --     -- print(tostring(obj[0]))
    --     print(tostring(UnityEngine.GameObject.Instantiate))
    --     local cube = u3d.Instantiate(obj[0])
    --     -- cube.transform

    --     GameFramework.LogFile.Log('Lua logFile')
    --     gm.TestDelegate(function ( num )
    --         print('delegate :'..num)
    --     end)
    -- end)
    GameFramework.LogFile.Log('gm.LoadScene')
    gm.LoadScene('res/Scenes/stage01', 'test/Stage01.unity')
    GameFramework.LogFile.Log('gm.LoadScene 2')

end

return TestClass
