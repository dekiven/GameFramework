-- 说明：TestClass
local TestClass = class("TestClass")

local gm = GameFramework.LuaExportFuncs

local tests = 
{
    {name='UIManager', dir='Tests/UIManagerTest', scene='TestUIManager'},
    {name='ScrollView', dir='Tests/ScrollViewTest', scene='ScrollViewTest'},
    {name='SpriteAtlas', dir='Tests/SpriteAtlasTest', scene='TestSpriteAtlas'},
    -- {name='LoadPrefab', dir='Tests/LoadPrefabTest',},
    {name='Transform', view='test/TransformTestView'},
    {name='PluginTest', view='test/PluginTestView'},
    {name='PurchaseTest', view='test/PurchaseTestView'},
    {name='SubHandlerTest', view='test/SubHandlerTestView'},
}

function TestClass:ctor( ... )
    self.testMainView = nil
    LoadScene('Tests/Scenes/TestScene', '.unity', true, false, function( progress )
        if progress >= 1 then
            self:showTestView()
        end
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
    testMainView = forceRequire('test.TestMainView').new()
    testMainView:show()
    testMainView:setTestDatas(tests)
    self.testMainView = testMainView
end

return TestClass
