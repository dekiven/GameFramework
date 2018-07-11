-- 说明：TestClass
require("common/functions")


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
    gm.LoadAsb('res/Scenes/stage01', function ()
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync('stage01')
    end)

end

return TestClass
