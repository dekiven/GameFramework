-- 名称：        ViewBase
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-08-22 17:46:08
-- 说明：
--[[
        
--]]


local ViewBase = class( 'ViewBase' )

function ViewBase:ctor(  )
    self.initViewParams()
end

function ViewBase:initViewParams( )
    self.asbName = ''
    self.prefabName = ''
    self.useLuaShowAnim = false
    self.useLuaHideAnim = false
    self.uiBase = nil
    self.uiHandler = nil
end

function ViewBase:showView()
    ShowView(self.asbName, self.prefabName, self._getUIBaseListeners())
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

--  =================================UI状态改变CS调用的方法-----------------------------------
-- 子类需要实现对initCallback的处理
function ViewBase:onInit(uiBase, uiHandler, initCallback)
    self.uiBase = uiBase
    self.uiHandler = uiHandler
end

function ViewBase:onEnable()

end

function ViewBase:onDisable()

end

function ViewBase:onDestroy()

end

function ViewBase:onShowBegin()

end

function ViewBase:onHideBegin()

end
--  -----------------------------------UI状态改变CS调用的方法=================================

return ViewBase
