-- 名称：        UIItemDataHelper
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-08-23 09:32:27
-- 说明：
--[[
        ScrollView SetDatas 相关LuaTable封装
--]]

-- 根据{'funcName', index, value,...}获取changeUI的table
function getUIData( dataTable )
    -- 当v = {'funcStr', intIndex, content,...}时加入data
    if type(dataTable) == 'table' and #dataTable >= 3 then
        -- 如果v[3]是table，需要转换
        if type(dataTable[3]) == 'table' then
            -- dataTable[3]暂时不支持 index和key混用，
            local _v = clone(dataTable)
            if #dataTable[3] > 0 then
                -- 使用index表示是vector或者color，直接转化为string
                _v[3] = getVecColStr(dataTable[3], ',')
            else
                -- 使用key主要是修改RectTransform等需要再c#将table转化为Dictionary
                _v[3] = getCsTable(dataTable[3])
            end
            return _v
        else
            return dataTable
        end
    end
    return nil
end

-- 获取ChangeItem的table
function getUIItemData( dataTable )
    local data = {}
    local count = 0
    if type(dataTable) == 'table' then
        for i, v in ipairs(dataTable) do
            local d = getUIData(v)
            if nil ~= d then
                table.insert(data, d)
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

-- 获取ScrollView、ScrollSelecltor 等修改多个Handler的table
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
