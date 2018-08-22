import os
from DKVTools.Funcs import *

ignoreList = [
    '.vs/',
    'Library/',
    'Temp/',
    'UnityPackageManager/',
    'StreamingAssets',
    'StreamingAssets.meta',
    'AssetBundles',
    'Assets/Lua',
    'Assets/Lua.meta',
    'Assets/Temp.meta',
    'Generate/',
    '!Generate/DelegateFactory.cs',
    '!Generate/LuaBinder.cs',
    '*.csproj',
    '*.sln',
    '*.log'
]

f = open('.gitignore', 'w')

for i in ignoreList :
    f.write(i+'\n')
    
f.close()