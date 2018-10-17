-- 名称：        TransformTestView
-- 编码：        utf-8
-- 创建人：      sjytyf3
-- 创建时间：    2018-09-28 11:29:37
-- 说明：
--[[
        
--]]

local uiIndex =
{
    -- Panel (UnityEngine.UI.Image)
    Panel = 0,
    -- Panel/Button (UnityEngine.UI.Button)
    Button = 1,
    -- Panel/Button1 (UnityEngine.UI.Button)
    Button1 = 2,
    -- Panel/Button2 (UnityEngine.UI.Button)
    Button2 = 3,
    -- Panel/Button3 (UnityEngine.UI.Button)
    Button3 = 4,
    -- Panel/Button4 (UnityEngine.UI.Button)
    Button4 = 5,
    -- btnBack (UnityEngine.UI.Button)
    btnBack = 6,
}
local subIndex =
{
}

local TransformTestView = class( 'TransformTestView', ViewBase )

function TransformTestView:initViewParams( )
    -- print(TransformTestView)
    self.super.initViewParams(self)

    self.asbName = 'Tests/RectTransformLuaTest'
    self.prefabName = 'PanelRectTransfromTest'

    self.testDatas = nil
    self.hasInit = false
end

function TransformTestView:onInit(uiBase, uiHandler)
    self.super.onInit(self, uiBase, uiHandler)

    for i, v in ipairs({1,2,3,4,5,6}) do
        self.uiHandler:AddBtnClick(v, handler(self, self.onBtnClick))
    end

    self.uiBase:OnLuaInitResult(true)
end

function TransformTestView:onEnable()
    if not self.hasInit then
        self.hasInit = true
    end
end

function TransformTestView:dispose( ... )
    self.super.dispose(self)

    printWarn('TransformTestView:dispose')
end


function TransformTestView:onBtnClick( name )
    if name == 'btnBack' then
        self:hide()
    elseif name == 'Button' then
        local t = 
        {
            sizeDelta = {1000, 200},
            localScale = {1, 1, 2},
            localPosition = {0, 0, 100}
        }
        t = getCsTable(t)
        self.uiHandler:ModifyURectTransfrom(0, t)
    else
        local t = 
        {
            sizeDelta = {100, 200},
        }
        local data = {'ModifyURectTransfrom', 0, t}
        data = getUIData(data)
        self.uiHandler:ChangeUI(data)
    end
end

return TransformTestView
