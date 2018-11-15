-- 名称：        EventManager
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-10-15 16:43:15
-- 说明：
--[[
        Lua层事件管理器
--]]
local pairs = pairs
local type = type
module "EventManager"

local _events = {}

-- eventName  事件名, 	subKey 同一个事件回调的key（区分function）, eventFunc 注册的方法
function addEvent( eventName, subKey, eventFunc )
	local event = _events[eventName] or {}
	event[subKey] = eventFunc
	_events[eventName] = event
end

function removeEvent( eventName, subKey )
	local event = _events[eventName] or {}
	event[subKey] = nil
	_events[eventName] = event
end

function clearAllEvent( )
	_events = {}
end

function notifyEvent( eventName, ... )
	local event = _events[eventName] or {}
	for k, v in pairs(event) do
		if type(v) == "function" then
			v(...)
		end
	end
end

-- function update( )
	
-- end

return EventManager
