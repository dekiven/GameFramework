# -*- coding:utf-8 -*-
# 创建时间：2018-06-10 10:41:22
# 创建人：  Dekiven_PC

import os
import json
from DKVTools.Funcs import *
from TkToolsD.CommonWidgets import *

tk, ttk = getTk()
STR_UGUI_FRAME = 'uguiFrame'
STR_TOLUA = 'tolua'
STR_START_LINE = '//===============customRigister begin---------------'
STR_END_LINE = '//---------------customRigister end================='
STR_REPLACE_START = 'public static List<Type> dynamicList = new List<Type>()'

class UpdateLibs(object) :
    '''UpdateLibs
    '''
    def __init__(self, *args, **dArgs) :
        super(UpdateLibs, self).__init__()
        self.configPath = '../../../UpdateLibs.config'
        self.config = {}
        # GameFramework ThirdPart 路径
        self.GFThirdPart = pathJoin(os.getcwd(), 'Assets/GameFramework/ThirdPart')
        self.app = None

        self.loadConf()
        self.initUI()
        # self.saveConfig()

    def loadConf(self) :
        p = pathJoin(self.GFThirdPart, self.configPath)
        if not os.path.isfile(p) :
           self.saveConfig()
        f = open(p, 'rb')
        jsonStr = bytes2utf8Str(f.read(), 'utf-8')
        f.close()
        self.config = json.loads(jsonStr, encoding='utf-8')

    def saveConfig(self):
        p = pathJoin(self.GFThirdPart, self.configPath)
        f = None
        
        jsonStr = json.dumps(self.config, ensure_ascii=False, check_circular=True, indent=2, sort_keys=False)
        if isPython3() :
            f = open(p, 'w', encoding='utf-8')
        else :
            f = open(p, 'w')
        f.write(jsonStr)
        f.close()

    def setConf(self, k, v):
        if k and v and k != '' and v != '' :
            self.config[k] = v
        self.saveConfig()

    def initUI(self):
        # import TkToolsD
        # help(TkToolsD.CommonWidgets)
        app = tk.Tk()
        app.title('ToLua更新工具')

        app.columnconfigure(0, weight=1, minsize=400)

        count = getCounter()
        w = GetDirWidget(app,
            title='Tolua dir',
            titleUp='path of tolua dir',
            pathSaved=self.config.get(STR_TOLUA),
            callback=lambda _dir : self.setConf(STR_TOLUA, _dir),
            enableEmpty=False)
        w.grid(row=count(), column =0, sticky='nswe', padx=10, pady=5)

        w = GetDirWidget(app,
            title='UGUI',
            titleUp='path of LuaFramework_UGUI dir，only use the LuaEncoder folder',
            pathSaved=self.config.get(STR_UGUI_FRAME),
            callback=lambda _dir : self.setConf(STR_UGUI_FRAME, _dir),
            enableEmpty=False)
        w.grid(row=count(), column =0, sticky='nswe', padx=10, pady=5)

        w = ttk.LabelFrame(app)
        w.grid(row=count(), column =0, padx=10, pady=10)

        btn = ttk.Button(w, command=lambda : self.updateLibs(True), text=u'强制更新')
        btn.grid(row=0, column =0, padx=10, pady=10)
        btn = ttk.Button(w, command=self.updateLibs, text=u'更新')
        btn.grid(row=0, column =1, padx=10, pady=10)        

        self.app = app
        app.update()
        centerToplevel(app)
        app.mainloop()

    def updateLibs(self, compulsory=False):
        '''
        updateLibs(compulsory=Flase)  compulsory 强制， compulsory = True时强制更新所有lib
        '''
        # print('update')
        tolua = self.config.get(STR_TOLUA)
        uguiFrame = self.config.get(STR_UGUI_FRAME)
        if not (tolua and uguiFrame) :
            ShowInfoDialog(u'选择正确的lib路径！')
            return
        else :
            # 拷贝tolua并修改相应文件
            rst2 = self.checkLib(tolua) or compulsory
            if rst2 :
                # 获取注册过的类名
                GTPath = pathJoin(self.GFThirdPart, 'ToLua/Editor/Custom/CustomSettings.cs')
                modifyLines = self.getLines(GTPath, STR_START_LINE, STR_END_LINE)

                target = pathJoin(self.GFThirdPart, 'ToLua')
                origin = pathJoin(tolua, 'Assets')
                copyTree(origin, target, removeBefore=True, skipDirs=('ToLua/Examples',))
                origin = pathJoin(tolua, 'Unity5.x/Assets')
                copyTree(origin, target)
                modifyFileByDatas = self.config.get('modifyFiles')
                if modifyFileByDatas :
                    for f in list(modifyFileByDatas.keys()) :
                        self.modifyFileByData(pathJoin(target, f), modifyFileByDatas.get(f))
                
                # 将获取的注册代码添加到新的代码中
                if os.path.isfile(GTPath) and len(modifyLines) > 0 :
                    lf = open(GTPath, 'rb')
                    ls = lf.readlines()
                    lf.close()
                    idx = -1
                    for i in range(len(ls)) :
                        l = ls[i]
                        l = bytes2utf8Str(l, 'utf-8')
                        ls[i] = l.rstrip('\r\n')
                        if l.strip() == STR_REPLACE_START :
                            idx = i
                    if idx > 1:
                        f = None
                        if isPython3() :
                            f = open(GTPath, 'w', encoding='utf-8')
                        else :
                            f = open(GTPath, 'w')
                        i = 0
                        for l in ls :
                            if i == idx-2 :
                                f.write('\n')
                                for ll in modifyLines :
                                    f.write(ll+'\n')
                                f.write('\n')
                            f.write(l+'\n')
                            i += 1
                        f.close()

            # 拷贝LuaFramework_UGUI的Plugins
            rst1 = self.checkLib(uguiFrame) or compulsory
            # 不拷贝LuaEncoder，TODO:android在mac上要使用luuajit_mac_32编译脚本
            if rst1 :
                # target = pathJoin(self.GFThirdPart, '../LuaEncoder')
                origin = pathJoin(uguiFrame, 'Assets/Plugins')
                # copyTree(origin, target, removeBefore=True)
                target = pathJoin(self.GFThirdPart, 'ToLua/Plugins')
                copyTree(origin, target)

            msg = ''
            if rst1 :
                msg = u'更新libs： LuaFramework_UGUI'
            if rst2 :
                if msg == '' :
                    msg = u'更新libs： ToLua'
                else :
                    msg = msg + ', ToLua'
            if msg == '' :
                msg = u'所有libs都是最新，无需更新。'
            else :
                msg = msg + '请注意对比代码，修改被还原的部分。'
            ShowInfoDialog(msg)
            self.app.quit()

    def checkLib(self, path):
        os.chdir(path)
        rst = tryCmd('git pull')
        rst = bytes2utf8Str(rst[0]).strip()
        # TODO:待优化，这里默认pull都是成功的，如果已经更新，就表示没有必要再更新
        # 但是如果出错会将出错的lib更新到项目
        updated = not ('Already up to date.' == rst)
        return updated

    def modifyFileByData(self, path, data) :
        if os.path.isfile(path) :
            newStrs = data.get('n')
            oldStrs = data.get('o')
            modifyFile(path, oldStrs, newStrs)

    def getLines(self, path, startLineStr, endLineStr) :
        '''获取给定的startLineStr, endLineStr两行字符串之间的所有行（包含start和end）
        '''
        lines = []
        if os.path.isfile(path) :
            f = open(path, 'rb')
            hasstartLineStr = False
            for l in f.readlines() :
                if not hasstartLineStr :
                    line = bytes2utf8Str(l, 'utf-8').strip()
                    if line == startLineStr.strip() :
                        hasstartLineStr = True
                        lines.append(bytes2utf8Str(l, 'utf-8').rstrip('\r\n'))
                else :
                    lines.append(bytes2utf8Str(l, 'utf-8').rstrip('\r\n'))
                    line = bytes2utf8Str(l, 'utf-8').strip()
                    if line == endLineStr.strip() :
                        f.close()
                        return lines
        return lines

def __main() :
    # print('Module named:'+str(UpdateLibs))
    try:
        tool = UpdateLibs()
    except Exception as e:
        # raise e
        ShowInfoDialog('异常！\n'+str(e))

if __name__ == '__main__' :
    __main()
