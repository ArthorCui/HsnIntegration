import subprocess, os, sys
import glob, ibslib
from optparse import OptionParser
from shutil import *

class SetVersion():
    def __init__(self):
        self.main()

    def showUsage(self):
        print("\
Usage: SetVersion.py [options]\n\n\
Options:\n\
    -h, --help            show this help message and exit\n\
    --tfs-user=TFS_USER\n\
    --tfs-pass=TFS_PASS\n\
    --version=(version to set)\n\
    --versioncsfile=(path to version cs file)\n\
    --projectroot=(path of project)\n")
        exit(1)

    def checkoutVersionFiles(self):
        ibslib.tfsGet(self.options.versioncsfile, self.options.tfs_user, self.options.tfs_pass)
        ibslib.tfsCheckout(self.options.versioncsfile, self.options.tfs_user, self.options.tfs_pass)
        
        # For Telenor builds only, check out config files
        if self.options.projectroot.__contains__('THOR'):
            ibslib.tfsGet(self.configfile, self.options.tfs_user, self.options.tfs_pass)
            ibslib.tfsCheckout(self.configfile, self.options.tfs_user, self.options.tfs_pass)

    def checkinVersionFiles(self):
        tfscheckincomment = "SetVersion script: Updated IC version.cs to " + self.newversion
        ibslib.tfsCheckin(self.options.versioncsfile, tfscheckincomment, self.options.tfs_user, self.options.tfs_pass)

        # For Telenor builds only, check in config files
        if self.options.projectroot.__contains__('THOR'):
            tfscheckincomment = "SetVersion script: Updated Telenor " + self.configfile.split("\\")[len(self.configfile.split("\\"))-1] + " to " + self.newversion
            ibslib.tfsCheckin(self.configfile, tfscheckincomment, self.options.tfs_user, self.options.tfs_pass)

    def setVersion(self):
        if os.path.exists(self.options.versionxmlfile):
            self.newversion = self.options.version
            newmajor = int(self.newversion.split('.')[0])
            newminor = int(self.newversion.split('.')[1])
            newrevision = int(self.newversion.split('.')[2])
            newbuild = int(self.newversion.split('.')[3])
    
            fi = open(self.options.versioncsfile, "r")
            lines = fi.readlines()
            outlines = []
            for line in lines:
                if line.startswith("[assembly: AssemblyVersion"):
                    assemblyVersion = line.split('"')[1]
                    assemblyVersionMajor = assemblyVersion.split('.')[0]
                    assemblyVersionMinor = assemblyVersion.split('.')[1]
                    assemblyVersionRevision = assemblyVersion.split('.')[2]
                    assemblyVersionBuild = assemblyVersion.split('.')[3]
                    if (assemblyVersionMajor != "*"):
                        assemblyVersionMajor = str(newmajor)
                    if (assemblyVersionMinor != "*"):
                        assemblyVersionMinor = str(newminor)
                    if (assemblyVersionRevision != "*"):
                        assemblyVersionRevision = str(newrevision)
                    if (assemblyVersionBuild != "*"):
                        assemblyVersionBuild = str(newbuild)
                    newAssemblyVersion = assemblyVersionMajor + "." + assemblyVersionMinor + "." + assemblyVersionRevision + "." + assemblyVersionBuild
                    outlines.append(line.replace(assemblyVersion, newAssemblyVersion))
                elif line.startswith("[assembly: AssemblyFileVersion"):
                    assemblyFileVersion = line.split('"')[1]
                    assemblyFileVersionMajor = assemblyFileVersion.split('.')[0]
                    assemblyFileVersionMinor = assemblyFileVersion.split('.')[1]
                    assemblyFileVersionRevision = assemblyFileVersion.split('.')[2]
                    assemblyFileVersionBuild = assemblyFileVersion.split('.')[3]
                    if (assemblyFileVersionMajor != "*"):
                        assemblyFileVersionMajor = str(newmajor)
                    if (assemblyFileVersionMinor != "*"):
                        assemblyFileVersionMinor = str(newminor)
                    if (assemblyFileVersionRevision != "*"):
                        assemblyFileVersionRevision = str(newrevision)
                    if (assemblyFileVersionBuild != "*"):
                        assemblyFileVersionBuild = str(newbuild)
                    newassemblyFileVersion = assemblyFileVersionMajor + "." + assemblyFileVersionMinor + "." + assemblyFileVersionRevision + "." + assemblyFileVersionBuild
                    outlines.append(line.replace(assemblyFileVersion, newassemblyFileVersion))
                else:
                    outlines.append(line)
            fi.close()
            fi = open(self.options.versioncsfile, "w")
            for line in outlines:
                fi.write(line)
            fi.close()
    
            # If Telenor build, update config file
            if self.options.projectroot.__contains__('THOR'):
                configfile = self.configfile
                template = configfile + '.template'
                if os.path.exists(configfile):
                    os.remove(configfile)
                fi = open(template, "r")
                fo = open(configfile, "w")
                for line in fi.readlines():
                    fo.write(line.replace("@@VERSION@@", newassemblyFileVersion))
                fo.close()
                fi.close()
        else:
            print("Version XML file not found!")
            exit(1)

    def main(self):
        parser = OptionParser()
        parser.add_option("--version", action="store", type="string", dest="version")
        parser.add_option("--versioncsfile", action="store", type="string", dest="versioncsfile")
        parser.add_option("--tfs-user", action="store", type="string", dest="tfs_user")
        parser.add_option("--tfs-pass", action="store", type="string", dest="tfs_pass")
        parser.add_option("--projectroot", action="store", type="string", dest="projectroot")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            self.showUsage
        else:
            self.options = options
        if self.options.projectroot.__contains__('THOR'):
            self.configfile = self.options.projectroot + "\\Applications\\WindowsService\\App.config"
        self.checkoutVersionFiles()
        self.setVersion()
        self.checkinVersionFiles()

    def validateOptions(self, options):
        if options.version == None or \
            options.versioncsfile == None or \
            options.tfs_user == None or \
            options.tfs_pass == None or \
            options.projectroot == None:
            return False
        else:
            return True

SetVersion()
