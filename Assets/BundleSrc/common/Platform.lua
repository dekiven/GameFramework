-- 名称：        Platform
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-10-09 11:20:20
-- 说明：
--[[
        
--]]

local gfPlatform = GameFramework.Platform

Platform = {}

Platform.setNoticeObFunc = gfPlatform.SetNoticeObjFunc
Platform.takePhoto = gfPlatform.TakePhoto
Platform.takeAlbum = gfPlatform.TakeAlbum
-- restart 仅android有效，ios待实现
Platform.restart = gfPlatform.Restart
Platform.installNewApp = gfPlatform.InstallNewApp

