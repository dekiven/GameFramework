-- 名称：        ViewBase
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-08-22 17:46:08
-- 说明：
--[[
        
--]]


local ViewBase = class( 'ViewBase' )

function ViewBase:ctor(  )
    self:initViewParams()
end

function ViewBase:initViewParams( )
    self.asbName = ''
    self.prefabName = ''
    -- 是否使用lua定义新的show动画
    self.useLuaShowAnim = false
    -- 是否使用lua定义新的hide动画
    self.useLuaHideAnim = false
    self.uiBase = nil
    self.uiHandler = nil
end

-- function ViewBase:showView()
--     ShowView(self.asbName, self.prefabName, self:_getUIBaseListeners())
-- end

function ViewBase:show()
    if nil ~= self.uiBase then
        self.uiBase:Show(nil)
    else
        ShowView(self.asbName, self.prefabName, self:_getUIBaseListeners())
    end
end

function ViewBase:hide()
    if nil ~= self.uiBase then
        self.uiBase:Hide(nil)
    end
end

function ViewBase:close()
    if nil ~= self.uiBase then
        self.uiBase:Close()
    end
end

function ViewBase:_getUIBaseListeners()
    local table = 
    {
        onInit=handler(self, self.onInit),
        onEnable=handler(self, self.onEnable),
        onDisable=handler(self, self.onDisable),
        onDestroy=handler(self, self.onDestroy),
        onShowBegin= self.useLuaShowAnim and handler(self, self.onShowBegin) or nil,
        onHideBegin= self.useLuaHideAnim and handler(self, self.onHideBegin) or nil,
    }
    return table
end

--  =================================UI状态改变的回调方法-----------------------------------
-- 子类需要调用self.uiBase:OnLuaInitResult(true/false)告知是否初始化成功
function ViewBase:onInit(uiBase, uiHandler)
    self.uiBase = uiBase
    self.uiHandler = uiHandler
end

function ViewBase:onEnable()

end

function ViewBase:onDisable()

end

function ViewBase:onDestroy()
    self:dispose()
end

function ViewBase:onShowBegin()

end

function ViewBase:onHideBegin()

end

-- 子类在Oninit后调用，告知c#初始化结果，以便下一步处理
function ViewBase:OnLuaInitResult( rst )
    pLog('ViewBase:OnLuaInitResult( rst ):'..tostring(rst))
    if self.uiBase then
        pLog('2 ViewBase:OnLuaInitResult( rst ):'..tostring(rst))
        self.uiBase:OnLuaInitResult(rst)
    end
end

-- 子类在自定义动画效果调用后onShowBegin、onHideBegin后调用，告知c#初始化结果，以便下一步处理
function ViewBase:OnLuaAnimResult( rst )
    if self.uiBase then
        self.uiBase:OnLuaAnimResult(rst)
    end
end

--  -----------------------------------UI状态改变CS调用的方法=================================

-- 清理函数
function ViewBase:dispose()
    self.uiBase = nil
    self.uiHandler = nil
end

return ViewBase
