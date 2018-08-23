-- 名称：        UIItemDataHelper
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-08-23 09:32:27
-- 说明：
--[[
        ScrollView SetDatas 相关LuaTable封装
--]]


function getUIItemData( dataTable )
    local data = {}
    local count = 0
    if type(dataTable) == 'table' then
        for i,v in ipairs(dataTable) do
            -- 当v = {'funcStr', intIndex, content}时加入data
            if type(v) == 'table' and #v >= 3 then
                table.insert(data, v)
                count = count + 1
            end
        end
    end
    data.count = count
    return data
end

function getUIItemDataWithIndex( dataTable, intIndex )
    local data = { index = intIndex,}
    data.data = getUIItemData(dataTable)
    return data
end

function getScrollViewData( dataTable )
    local data = {}
    local count = 0
    if type(dataTable) == 'table' then
        for i,v in ipairs(dataTable) do
            -- 当v = {'funcStr', intIndex, content}时加入data
            if type(v) == 'table' and #v >= 1 then
                item = getUIItemData(v)
                if item.count > 0 then
                    table.insert(data, item)
                    count = count + 1
                end
            end
        end
    end
    data.count = count
    return data
end
