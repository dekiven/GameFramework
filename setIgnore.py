import os
from DKVTools.Funcs import *

ignoreList = [
    'Library/',
    'Temp/',
    'UnityPackageManager/',
    'StreamingAssets',
    'StreamingAssets.meta',
    'AssetBundles',
    'Assets/Lua',
    'Assets/Lua.meta',
    '.vs/',
    'Generate',
    '*.csproj',
    '*.sln',
]

f = open('.gitignore', 'w')

for i in ignoreList :
    f.write(i+'\n')
    
f.close()