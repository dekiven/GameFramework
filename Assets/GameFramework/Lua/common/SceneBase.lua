-- 名称：        SceneBase
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-09-08 10:17:57
-- 说明：
--[[
        
--]]

local SceneBase = class( 'SceneBase', nil )

function SceneBase:ctor( ... )
    self:initParams()

    self:show()
end

function SceneBase:initParams( ... )
    self.asbName = ''
    self.resPath = ''
    self.isSync = true
    self.destoryOther = true
end

function SceneBase:show( callback )
    LoadScene(self.asbName, self.resPath, self.isSync, not self.destoryOther, function( progress )
        if progress >= 1 or progress == -1 then
            if type(callback) == 'function' then
                callback(progress)
            end

            if progress >= 1 then
                self.onEnter()
            end
        end
    end)
end

function SceneBase:onEnter(  )
   
end

return SceneBase
