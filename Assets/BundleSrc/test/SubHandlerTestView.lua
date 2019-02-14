-- 名称：        SubHandlerTestView
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2019-02-13 17:22:27
-- 说明：
--[[
        测试修改 ScrollView Item SubHandler
--]]

-- UIArray index
local uiIdx = 
{
    svSh = 0,  -- svSh (GameFramework.ScrollView)
    btnBack = 1,  -- btnBack (UnityEngine.UI.Button)
}

-- Item index
local itIdx = 
{
    Text = 0,  -- Button/Text (UnityEngine.UI.Text)
}

-- SubHandlers index
local subIdx = 
{
    Image0 = 0,  -- Button/Image/Image0 (GameFramework.UIHandler)
    Image1 = 1,  -- Button/Image/Image1 (GameFramework.UIHandler)
    Image2 = 2,  -- Button/Image/Image2 (GameFramework.UIHandler)
    Image3 = 3,  -- Button/Image/Image3 (GameFramework.UIHandler)
}

-- UIArray index
local subUiIdx = 
{
    Image0 = 0,  -- Canvas/slSubHandler/Button/Image/Image0 (UnityEngine.UI.Image)
    Text = 1,  -- Text (UnityEngine.UI.Text)
}




local SubHandlerTestView = class( 'SubHandlerTestView', ViewBase )

function SubHandlerTestView:initViewParams( )
    self.super.initViewParams(self)

    self.asbName = 'Tests/SubHandlerTest'
    self.prefabName = 'plSubHandler'

    -- TODO:
end

function SubHandlerTestView:onInit(uiBase, uiHandler)
    self.super.onInit(self, uiBase, uiHandler)

    self.uiHandler:AddBtnClick(uiIdx.btnBack, function( ... )
        self:close()
    end)

    self.uiBase:OnLuaInitResult(true)
end

function SubHandlerTestView:onEnable()
    -- if not self.hasInit then
    --     self.hasInit = true
    -- end

    local colors = {'red', {0,1,0,1}, '0,0,1,1', 'yellow'}
    local data = {}
    for i=1,30 do
        local d = 
        {
            {'SetTextString', itIdx.Text, 'Item'..i},
        }
        for j=1,4 do
            local sprite = (i % 4 + j) % 4 + 1 
            local sprites = {12, 13, 14, 15}
            local sd = 
            {
                {'SetTextString', subUiIdx.Text, 'Item'..j},
                {'SetUIColor', subUiIdx.Text, colors[j]},
                {'SetImageSprite', subUiIdx.Image0, 'Tests/SpriteAtlasTest,TestAtlas,'..sprites[sprite]}
            }
            table.insert(d, {'ChangeSubHandlerItem', j-1, getUIItemData(sd)})        
        end
        table.insert(data, d)
    end
    self.uiHandler:SetScrollViewData(uiIdx.svSh, getScrollViewData(data))
end

function SubHandlerTestView:dispose( ... )
    self.super.dispose(self)

    -- printWarn('SubHandlerTestView:dispose')
end

return SubHandlerTestView
