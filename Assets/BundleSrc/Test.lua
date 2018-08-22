-- 说明：TestClass
local TestClass = class("TestClass")

local gm = GameFramework.LuaExportFuncs

local tests = 
{
    {name='UIManagerTest', dir='UIManagerTest', scene='UIManagerTest.unity'},
    {name='ScrollViewTest', dir='ScrollViewTest', scene='ScrollViewTest.unity'},
    {name='SpriteAtlasTest', dir='SpriteAtlasTest', scene='SpriteAtlasTest.unity'},
    {name='LoadPrefabTest', dir='LoadPrefabTest', scene='LoadPrefabTest.unity'},
}

function TestClass:ctor( ... )    
    LoadScene('Tests/Main', 'TestScene.unity3d', true, false, function( progress )
        if(progress >= 1)
        {
            slef:showTestView()
        }
    end)
    -- self:testBySceneName('TestUIManager')
    -- self:testBySceneName('TestSpriteAtlas')
end

function TestClass:testLoadPrefab()
    GameFramework.LogFile.Log('gm.LoadScene')
    gm.LoadScene('res/Scenes/stage01/test', 'Stage01.unity', true, false, function ( progress )
        GameFramework.LogFile.Log('progress:'..progress)
        if progress >= 1 then
            GameFramework.LogFile.Log('gm.LoadScene 2')

            gm.LoadGameObj('res/test/test/Cube', '.prefab', function ( obj )
                -- print(tostring(obj[0]))
                -- GameFramework.LogFile.Log(obj[0])
                print(tostring(UnityEngine.GameObject.Instantiate))
                local cube = u3d.Instantiate(obj[0])
                -- cube.transform

                -- GameFramework.LogFile.Log(cube)

                GameFramework.LogFile.Log('Lua logFile')
                gm.TestDelegate(function ( num )
                    print('delegate :'..num)
                end)
            end)
        end
    end)
end

function TestClass:testBySceneName(name)
    printLog('测试：加载场景：'..name)
    -- 测试UIManager
    gm.LoadScene('Tests/Scenes/'..name, '.unity', true, false, function ( progress )
        -- body
        -- printLog('SpriteAtlas 测试场景载入完成！')
        printLog('progress:'..progress)
    end)
end


function TestClass:showTestView()
    -- ShowView('', '', )
end

return TestClass
