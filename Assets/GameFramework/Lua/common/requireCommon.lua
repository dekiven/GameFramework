-- 名称：        requireCommon
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-08-23 10:16:08
-- 说明：
--[[
      加载Common模块  
--]]
-- common
require('common.functions')
require('common.u3dFuncs')
require('common.gameFramework')
require('common.Platform')
require('common.EventManager')
require('common.LanguageManager')

AddLuaBundle('common.ui')
-- common.ui
require('common.ui.UIItemDataHelper')
ViewBase = require('common.ui.ViewBase')

AddLuaBundle('common')
SceneBase = require('common.SceneBase')
