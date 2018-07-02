-- 说明：TestClass
local TestClass = class("TestClass")

function TestClass:ctor( ... )
    local gm = GameFramework.GameResManager.Instance
    print('gm:'..tostring(gm))

    gm:LoadGameObj('res/test/test', 'Cube.prefab', function ( obj )
        -- print(tostring(obj[0]))
        print(tostring(UnityEngine.GameObject.Instantiate))
        local cube = u3d.Instantiate(obj[0])
        -- cube.transform

        GameFramework.LogFile.Log('更新游戏资源版本')
    end)

end

return TestClass
