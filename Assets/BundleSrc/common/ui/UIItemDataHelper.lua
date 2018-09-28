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
                -- 如果v[3]是table，需要转换
                if type(v[3]) == 'table' then
                    -- v[3]暂时不支持 index和key混用，
                    local _v = clone(v)
                    if #v[3] > 0 then
                        -- 使用index表示是vector或者color，直接转化为string
                        _v[3] = getVecColStr(v[3], ',')
                    else
                        -- 使用key主要是修改RectTransform等需要再c#将table转化为Dictionary
                        _v[3] = getCsTable(v[3])
                    end
                    table.insert(data, _v)
                else
                    table.insert(data, v)
                end
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
            if type(v) == 'table' and #v >= 0 then
                item = getUIItemData(v)
                -- if item.count > 0 then
                table.insert(data, item)
                count = count + 1
                -- end
            end
        end
    end
    data.count = count
    return data
end

-- 将Vector2(3)、Color的table转换为string,以逗号分隔,供C#使用，详见Tools。GenxxxByStr方法
function getVecColStr( t )
    if type(t) == 'table' then
        return table.concat( t, ',' )
    else
        return tostring(t)
    end
end

-- 将修改RectTransform的table转化成C#可以解析的形式
function getCsTable( t )
    if type(t) == 'table' then
        table.map(t, function ( v, k )
            if type(v) == 'table' then
                return getVecColStr(v)
            else
                return v
            end
        end)
    end
    return t
end
