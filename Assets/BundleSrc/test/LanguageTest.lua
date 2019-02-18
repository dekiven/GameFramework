-- 名称：        LanguageTest
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2019-02-18 14:33:20
-- 说明：
--[[
        
--]]

-- UIArray index
local uiIdx = 
{
    btnBack = 0,  -- btnBack (UnityEngine.UI.Button)
    sv = 1,  -- sv (GameFramework.ScrollView)
    Text1 = 2,  -- Panel/Text1 (UnityEngine.UI.Text)
    Text2 = 3,  -- Panel/Text2 (UnityEngine.UI.Text)
    Text3 = 4,  -- Panel/Text3 (UnityEngine.UI.Text)
    Text4 = 5,  -- Panel/Text4 (UnityEngine.UI.Text)
    Text5 = 6,  -- Panel/Text5 (UnityEngine.UI.Text)
}



local LanguageTest = class( 'LanguageTest', ViewBase )

function LanguageTest:initViewParams( )
    self.super.initViewParams(self)

    self.asbName = 'Tests/LanguageTest'
    self.prefabName = 'plLanguage'

    self.languages = getValidLanguages()
end

function LanguageTest:onInit(uiBase, uiHandler)
    self.super.onInit(self, uiBase, uiHandler)

    self.uiHandler:AddBtnClick(uiIdx.btnBack, function ( ... )
        self:close()
    end)

    self.uiHandler:SetScrollViewOnItemClick(uiIdx.sv, function( index )
        changeLanguage(self.languages[index+1])
    end)

    EventManager.addEvent(STR_ON_LANGUAGE_CHANGED, 'test', handler(self, self.onEnable))

    loadLangMod('test.language.test')

    self.uiBase:OnLuaInitResult(true)
end

function LanguageTest:onEnable()
    self.uiHandler:SetTextString(2, getStr('1测试'))
    self.uiHandler:SetTextString(3, getStr('2测试'))
    self.uiHandler:SetTextString(4, getStr('3测试\n3测试'))
    self.uiHandler:SetTextString(5, getNativeStr('C#测试'))
    self.uiHandler:SetTextString(6, getNativeStr('检测服务器资源[ '))

    local data = {}
    for i,v in ipairs(self.languages) do
        table.insert(data, {{'SetTextString', 0, v,},})
    end
    self.uiHandler:SetScrollViewData(uiIdx.sv, getScrollViewData(data))
end

function LanguageTest:dispose( ... )
    self.super.dispose(self)

    EventManager.removeEvent(STR_ON_LANGUAGE_CHANGED, 'test')
end

return LanguageTest
