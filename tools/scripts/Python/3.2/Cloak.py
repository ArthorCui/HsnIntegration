import os,sys
import ibslib

dirs = ibslib.runCommand("tf dir").split("\r\n")
for i in range(1,dirs.__len__()-3):
    print("Cloaking %s" % dirs[i].lstrip("$"))
    ibslib.runCommand("tf workfold /cloak \"" + dirs[i].lstrip("$") + "\"")
print("Completed.")