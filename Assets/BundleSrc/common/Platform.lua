-- 名称：        Platform
-- 编码：        utf-8
-- 创建人：      Dekiven
-- 创建时间：    2018-10-09 11:20:20
-- 说明：
--[[
        
--]]
local gf = GameFramework.Platform

module "Platform"

setNoticeObFunc = gf.SetNoticeObjFunc
takePhoto = gf.TakeImagePhoto
takeAlbum = gf.TakeImageAlbum

-- restart 待实现
restart = gf.Restart
-- android传递相对于可读写文件夹的apk路径，ios传appid，跳转到商店
installNewApp = gf.InstallNewApp

startPurchase = gf.StartPurchase

-- 测试
test1 = gf.test1
test2 = gf.test2


