-- 名称：        PluginTestView
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-10-09 11:08:04
-- 说明：
--[[
        
--]]
-- UIArray index
local uiIdx = 
{
    ScrollView = 0,  -- ScrollView (GameFramework.ScrollView)
}

-- UIArray index
local itemIndex = 
{
    Text = 0,  -- Button/Text (UnityEngine.UI.Text)
}



local PluginTestView = class( 'PluginTestView', ViewBase )

function PluginTestView:initViewParams( )
    self.super.initViewParams(self)

    self.asbName = 'Tests/PluginTest'
    self.prefabName = 'PluginTestPanel'
end

function PluginTestView:onInit(uiBase, uiHandler)
    self.super.onInit(self, uiBase, uiHandler)

    self.uiHandler:SetScrollViewOnItemClick(uiIdx.ScrollView, handler(self, self.onItemClick))

    self.uiBase:OnLuaInitResult(true)
end

function PluginTestView:onEnable()
    if not self.hasInit then
        self.hasInit = true

    local test = 
    {
        '拍照',
        '相册',
        '重启',
        '安装',
    }
    printTable(test, 'test')
    local scrollData = {}
    for i,v in ipairs(test) do
        -- if nil ~= v.name then
        table.insert(scrollData, {{'SetTextString', itemIndex.Text, v},})
        -- end
    end
    printTable(scrollData, 'scrollData 1')
    scrollData = getScrollViewData(scrollData)
    printTable(scrollData, 'scrollData 2')
    self.uiHandler:SetScrollViewDatas(uiIdx.ScrollView, scrollData)
    end

end

function PluginTestView:dispose( ... )
    self.super.dispose(self)

    -- printWarn('PluginTestView:dispose')
end

function PluginTestView:onItemClick( index )
    if index == 0 then
        Platform.takePhoto()
    elseif index == 1 then
        Platform.takeAlbum()
    elseif index == 2 then
        -- Platform.restart(1.1)
        Platform.test1()
    elseif index == 3 then
        -- Platform.installNewApp("test.apk")
        Platform.test2()
    end

end

return PluginTestView
