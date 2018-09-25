-- 名称：        TestMainView
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-08-23 10:36:25
-- 说明：
--[[
        
--]]

local uiIndex =
{
    -- ScrollView (GameFramework.ScrollView)
    ScrollView = 0,
}
local subIndex =
{
}

local itemIndex =
{
    -- Button/Text (UnityEngine.UI.Text)
    Text = 0,
}

local TestMainView = class( 'TestMainView', ViewBase )

function TestMainView:initViewParams( )
    -- print(TestMainView)
    self.super.initViewParams(self)

    self.asbName = 'Tests/Main'
    self.prefabName = 'PanelTestMain'

    self.testDatas = nil
    self.hasInit = false
end

function TestMainView:onInit(uiBase, uiHandler)
    self.super.onInit(self, uiBase, uiHandler)

    self.uiHandler:SetScrollViewOnItemClick(uiIndex.ScrollView, handler(self, self.onItemClick))

    self.uiBase:OnLuaInitResult(true)
end

function TestMainView:onEnable()
    if not self.hasInit then
        self.hasInit = true
        self:setScrollViewDatas();
    end
end

function TestMainView:dispose( ... )
    self.super.dispose(self)

    printWarn('TestMainView:dispose')
end

function TestMainView:setTestDatas( data )
    self.testDatas = data
end

function TestMainView:setScrollViewDatas()
    local scrollData = {}
    for i,v in ipairs(self.testDatas) do
        if nil ~= v.name then
            table.insert(scrollData, {{'SetTextString', itemIndex.Text, v.name},})
        end
    end
    scrollData = getScrollViewData(scrollData)
    self.uiHandler:SetScrollViewDatas(uiIndex.ScrollView, scrollData)
end

function TestMainView:onItemClick( index )
    if index < #self.testDatas then
        local data = self.testDatas[index+1]
        printLog('测试：'..data.name)
        if nil ~= data.scene then
            self:hide()
            LoadScene('Tests/Scenes/'..data.scene, '', true, false, function ( progress )
                if progress >= 1 then
                end
            end)
        end
    end
end

return TestMainView
