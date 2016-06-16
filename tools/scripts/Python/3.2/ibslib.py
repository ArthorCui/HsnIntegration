import subprocess, os, sys, stat
import glob, re, fnmatch
from shutil import *
from win32api import GetFileVersionInfo, LOWORD, HIWORD

def unsetReadOnly(path):
    os.chmod(path, stat.S_IWRITE)
    for filetomodify in os.listdir(path):
        filepath = os.path.join(path, filetomodify)
        if os.path.isdir(filepath):
            unsetReadOnly(filepath)
        else:
            os.chmod(filepath, stat.S_IWRITE)
    
def compareVersions(version1, version2):
    def normalize(v):
        return [int(x) for x in re.sub(r'(\.0+)*$','', v).split(".")]
    a = normalize(version1)
    b = normalize(version2)
    return (a > b) - (a < b)

def getVersionNumber (filename):
  info = GetFileVersionInfo (filename, "\\")
  ms = info['FileVersionMS']
  ls = info['FileVersionLS']
  return str(HIWORD (ms)) + "." + str(LOWORD (ms)) + "." + str(HIWORD (ls)) + "." + str(LOWORD (ls))

def runCommand(command):
    try:
        print("\ncommand=" + command + "\n")
        p = subprocess.Popen(command, env=os.environ, shell=True, stdout=subprocess.PIPE)
        
        retcode = p.returncode
        #retcode = p.wait()
        if retcode != None:
            print(command + " was terminated by signal", -retcode, file=sys.stderr)
        else:
            print(command + " returned code None", file=sys.stderr)
        pout, perr = p.communicate()

        return str(pout)
    except OSError as e:
        #print >>sys.stderr, command + " execution failed:", e
        print(command + " execution failed:", e)

def copyFiles(src, dst):
    makeDir(dst)
    
    for srcname in glob.glob(src):
        dstname = os.path.join(dst, os.path.split(srcname)[1])
        try:
            copyfile(srcname, dstname)
        except (IOError, os.error) as why:
            print("Can't copy %s to %s: %s" % (repr(srcname), repr(dstname), str(why)))

def moveFiles(src, dst):
    destdir = os.path.split(dst)[0]
    makeDir(destdir)
    
    for srcname in glob.glob(src):
        dstname = os.path.join(destdir, os.path.splitext(os.path.split(srcname)[1])[0]) + os.path.splitext(dst)[1]
        try:
            move(srcname, dstname)
        except (IOError, os.error) as why:
            print("Can't move %s to %s: %s" % (repr(srcname), repr(dstname), str(why)))

def delFiles(src):
    for srcname in glob.glob(src):
        try:
            os.remove(srcname)
        except (IOError, os.error) as why:
            print("Can't remove %s: %s" % (repr(srcname), str(why)))
    
def makeDir(dirpath):
    try:
        os.mkdir(dirpath)
    except (IOError, os.error) as why:
        print("Error occurred while trying to create directory '%s': %s" % (dirpath, str(why)))

def tfsGet(filespec, tfs_user, tfs_pass):
    try:
        runCommand("tf.exe get \"" + filespec + "\" /login:" + tfs_user + "," + tfs_pass + " /noprompt /recursive")
    except:
        print(filespec + " - unable to perform TFS get.")
        
def tfsCheckout(filespec, tfs_user, tfs_pass):
    try:
        result = runCommand("tf.exe checkout \"" + filespec + "\" /login:" + tfs_user + "," + tfs_pass + " /noprompt /recursive")
    except:
        print(filespec + " - unable to perform TFS checkout:\r\n")
        print(" sys.exc_type: " + str(sys.exc_info()[0] ) + "\r\n")
        print(" sys.exc_value: " + str(sys.exc_info()[1]) + "\r\n")
        print(" sys.exc_traceback: " + str(sys.exc_info()[2]) + "\r\n")

    
def tfsCheckin(filespec, tfscheckincomment, tfs_user, tfs_pass):
    try:
        runCommand("tf.exe checkin \"" + filespec + "\" /login:" + tfs_user + "," + tfs_pass + " /noprompt /comment:\"" + tfscheckincomment + "\" /recursive")
    except:
        print(filespec + " - unable to perform TFS checkin.")
    
def tfsLabel(labelname, filespec, tfs_user, tfs_pass):
    try:
        runCommand("tf.exe label \"" + labelname + "\" \"" + filespec + "\" /login:" + tfs_user + "," + tfs_pass + " /recursive")
    except:
        print(filespec + " - unable to perform TFS label.")

def tfsAdd(filespec, tfs_user, tfs_pass):
    try:
        runCommand("tf.exe add \"" + filespec + "\" /login:" + tfs_user + "," + tfs_pass + " /recursive")
    except:
        print(filespec + " - unable to perform TFS add.")

def tfsDelete(filespec, tfs_user, tfs_pass):
    try:
        runCommand("tf.exe delete \"" + filespec + "\" /login:" + tfs_user + "," + tfs_pass + " /recursive")
    except:
        print(filespec + " - unable to perform TFS delete.")

def tfsUndo(filespec, tfs_user, tfs_pass):
    try:
        runCommand("tf.exe undo \"" + filespec + "\" /login:" + tfs_user + "," + tfs_pass + " /recursive /noprompt")
    except:
        print(filespec + " - unable to perform TFS undo.")

def tfsBranch(labelname, sourcepath, branchpath, getsource, tfs_user, tfs_pass):
    try:
        if getsource:
            noget = ""
        else:
            noget = " /noget"
        runCommand("tf.exe branch /version:L\"" + labelname + "\" \"" + sourcepath + "\" \"" + branchpath + "\" /login:" + tfs_user + "," + tfs_pass + noget + " /noprompt")
    except:
        print(branchpath + " - unable to perform TFS branch.")

def tfsProperties(filespec, tfs_user, tfs_pass):
    properties = ""
    try:
        properties = runCommand("tf.exe properties \"" + filespec + "\" /login:" + tfs_user + "," + tfs_pass + " /noprompt")
    except:
        print(filespec + " - unable to get TFS properties.")
    return properties

def tfsChangeset(changeset, tfs_user, tfs_pass):
    properties = ""
    try:
        properties = runCommand("tf.exe changeset " + changeset + " /login:" + tfs_user + "," + tfs_pass + " /noprompt")
        
    except:
        print(filespec + " - unable to get TFS changeset properties.")
    return properties
    
# Lock types: none|checkout|checkin
def tfsLock(filespec, lock, tfs_user, tfs_pass):
    try:
        runCommand("tf.exe lock \"" + filespec + "\" /lock:" + lock + " /login:" + tfs_user + "," + tfs_pass + " /recursive /noprompt")
    except:
        print(filespec + " - unable to perform TFS lock.")

def tfsUnlock(filespec, tfs_user, tfs_pass):
    try:
        runCommand("tf.exe lock \"" + filespec + "\" /lock:none /login:" + tfs_user + "," + tfs_pass + " /recursive /noprompt")
    except:
        print(filespec + " - unable to perform TFS unlock.")

def tfsCommandFile(filespec, tfs_user, tfs_pass):
    try:
        runCommand("tf.exe @" + filespec + " /login:" + tfs_user + "," + tfs_pass )
    except:
        print(filespec + " - unable to perform TFS CommandFile.")

def tfsCloak(filespec, tfs_user, tfs_pass):
    try:
        runCommand("tf.exe workfold /cloak \"" + filespec + "\" /login:" + tfs_user + "," + tfs_pass)
    except:
        print(filespec + " - unable to cloak.")

def tfsDecloak(filespec, tfs_user, tfs_pass):
    try:
        runCommand("tf.exe workfold /decloak \"" + filespec + "\" /login:" + tfs_user + "," + tfs_pass)
    except:
        print(filespec + " - unable to decloak.")

def tfsDir(filespec, tfs_user, tfs_pass):
    try:
        dirs = runCommand("tf.exe dir \"" + filespec + "\" /login:" + tfs_user + "," + tfs_pass).split("\r\n")
        dirs = dirs[1:dirs.__len__()-3]
        return dirs
    except (IOError, os.error) as why:
        print(filespec + " - unable to perform tf dir: %s" % str(why))
        return []
        
def replacetokens(tokens, values, templatefile, outfile):
    # Check to make sure template file exists
    try:
        ft = open(templatefile, "r")
    except:
        print("Error: unable to open template file!")
        return False
    
    # Perform token replacement
    outfile_data = ""
    try:
        template_lines = ft.readlines()
        for template_line in template_lines:
            template_line = template_line.replace("\n", "")
            for i in range(len(tokens)):
                template_line = template_line.replace(tokens[i], str(values[i]))
            outfile_data += template_line + "\n"
        ft.close()
    except (os.error) as why:
        print("Error: unable to perform token replacement: %s" & str(why))
        return False
    
    # Ensure we have output data to write
    if len(outfile_data) == 0:
        print("Error: output file data is blank.")
        return False
    
    try:
        # Write output data
        fo = open(outfile, "w")
        fo.write(outfile_data)
        fo.close()
    except (IOError, os.error) as why:
        print("Error writing output data: %s" % str(why))
        
    return True

def getFilesRecursive(path, mask):
    matches = []
    for root, dirnames, filenames in os.walk(path):
      for filename in fnmatch.filter(filenames, mask):
          matches.append(os.path.join(root, filename))
    return matches
