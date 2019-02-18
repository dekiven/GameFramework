-- 名称：        PurchaseTestView
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-11-30 16:43:08
-- 说明：
--[[
        
--]]

-- UIArray index
local uiIdx = 
{
    ButtonBack = 0,  -- ButtonBack (UnityEngine.UI.Button)
    ScrollView = 1,  -- ScrollView (GameFramework.ScrollView)
}

-- UIArray index
local itemIdx = 
{
    Text = 0,  -- Button/Text (UnityEngine.UI.Text)
}

local pids = 
{
    {"sjyt_jddld_1", "10砖石"},
    {"sjyt_jddld_6", "60砖石"},
    {"sjyt_jddld_30", "300砖石"},
    {"sjyt_jddld_68", "680砖石"},
    {"sjyt_jddld_128", "1280砖石"},
    {"sjyt_jddld_198", "1980砖石"},
    {"sjyt_jddld_328", "3280砖石"},
    {"sjyt_jddld_648", "6480砖石"},
}

local PurchaseTestView = class( 'PurchaseTestView', ViewBase )

function PurchaseTestView:initViewParams( )
    self.super.initViewParams(self)

    self.asbName = 'Tests/PurchaseTest'
    self.prefabName = 'PanelPurchase'

    -- TODO:
end

function PurchaseTestView:onInit(uiBase, uiHandler)
    self.super.onInit(self, uiBase, uiHandler)

    self.uiHandler:SetScrollViewOnItemClick(uiIdx.ScrollView, handler(self, self.onItemClick))

    self.uiHandler:AddBtnClick(uiIdx.ButtonBack, function ( )
        self:close()
    end)

    EventManager.addEvent('StartPurchase', 'PurchaseTest', function ( ... )
        for i, v in ipairs({...}) do
            pLog(tostring(v))
        end
    end)

    self.uiBase:OnLuaInitResult(true)
end

function PurchaseTestView:onEnable()
    if not self.hasInit then
        self.hasInit = true

        local data = {}
        for i, v in ipairs(pids) do
            printTable(v)
            table.insert(data, {{"SetTextString", itemIdx.Text, v[2]},})
        end

        self.uiHandler:SetScrollViewData(uiIdx.ScrollView, getScrollViewData(data))
    end
end

function PurchaseTestView:dispose( ... )
    self.super.dispose(self)

    -- pWarn('PurchaseTestView:dispose')
end

function PurchaseTestView:onItemClick( index )
    local pid = pids[index+1][1]
    pLog('clicked '..pid)
    Platform.startPurchase(pid, 'ios')
end

return PurchaseTestView
