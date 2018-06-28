import os
from DKVTools.Funcs import *

ignoreList = [
    'Library/',
    'Temp/',
    'UnityPackageManager/',
    '.vs/',
    '*.csproj',
    '*.sln',
]

f = open('.gitignore', 'w')

for i in ignoreList :
    f.write(i+'\n')
    
f.close()