import subprocess, os, sys
import glob
from shutil import *

def runCommand(command):
    try:
        print "\ncommand=" + command + "\n"
        p = subprocess.Popen(command, env=os.environ, shell=True)
        p.wait()
        
        retcode = p.returncode
        if retcode < 0:
            print >>sys.stderr, command + " was terminated by signal", -retcode
        else:
            print >>sys.stderr, command + " returned code", retcode
    except OSError, e:
        print >>sys.stderr, command + " execution failed:", e    

def copyFiles(src, dst):
    makeDir(dst)
    
    for srcname in glob.glob(src):
        dstname = os.path.join(dst, os.path.split(srcname)[1])
        try:
            copyfile(srcname, dstname)
        except (IOError, os.error), why:
            print "Can't copy %s to %s: %s" % (`srcname`, `dstname`, str(why))

def moveFiles(src, dst):
    destdir = os.path.split(dst)[0]
    makeDir(destdir)
    
    for srcname in glob.glob(src):
        dstname = os.path.join(destdir, os.path.splitext(os.path.split(srcname)[1])[0]) + os.path.splitext(dst)[1]
        try:
            move(srcname, dstname)
        except (IOError, os.error), why:
            print "Can't move %s to %s: %s" % (`srcname`, `dstname`, str(why))
    
def makeDir(dirpath):
    try:
        os.mkdir(dirpath)
    except:
        print dirpath + " already exists, not creating."
