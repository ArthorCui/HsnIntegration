import subprocess, os, sys
import glob, ibslib
from optparse import OptionParser
from shutil import *
from xml.etree.ElementTree import Element, SubElement, ElementTree

class IncrementVersion():
    def __init__(self):
        self.main()

    def showUsage(self):
        print("\
Usage: incrementversion.py [options]\n\n\
Options:\n\
    -h, --help            show this help message and exit\n\
    --tfs-user=TFS_USER\n\
    --tfs-pass=TFS_PASS\n\
    --versionxmlfile=(path to version xml file)\n\
    --versioncsfile=(path to version cs file)\n\
    --projectroot=(path of project)\n")
        exit(1)

    def checkoutVersionFiles(self):
        ibslib.tfsGet(self.options.versionxmlfile, self.options.tfs_user, self.options.tfs_pass)
        ibslib.tfsGet(self.options.versioncsfile, self.options.tfs_user, self.options.tfs_pass)
        ibslib.tfsCheckout(self.options.versionxmlfile, self.options.tfs_user, self.options.tfs_pass)
        ibslib.tfsCheckout(self.options.versioncsfile, self.options.tfs_user, self.options.tfs_pass)
        
        # For Telenor builds only, check out config files
        if self.options.versionxmlfile.__contains__('Telenor'):
            ibslib.tfsGet(self.configfile, self.options.tfs_user, self.options.tfs_pass)
            ibslib.tfsCheckout(self.configfile, self.options.tfs_user, self.options.tfs_pass)

    def checkinVersionFiles(self):
        tfscheckincomment = "IncrementVersion script: Incremented " + self.projectname + " IC version to " + self.newversion
        ibslib.tfsCheckin(self.options.versionxmlfile, tfscheckincomment, self.options.tfs_user, self.options.tfs_pass)

        tfscheckincomment = "IncrementVersion script: Updated " + self.projectname + " IC version.cs to " + self.newversion
        ibslib.tfsCheckin(self.options.versioncsfile, tfscheckincomment, self.options.tfs_user, self.options.tfs_pass)

        # For Telenor builds only, check in config files
        if self.options.versionxmlfile.__contains__('Telenor'):
            tfscheckincomment = "IncrementVersion script: Updated " + self.projectname + " " + self.configfile.split("\\")[len(self.configfile.split("\\"))-1] + " to " + self.newversion
            ibslib.tfsCheckin(self.configfile, tfscheckincomment, self.options.tfs_user, self.options.tfs_pass)

    def incrementVersion(self):
        if os.path.exists(self.options.versionxmlfile):
            tree = ElementTree(file=self.options.versionxmlfile)
            IncrementNode = tree.find(".//Increment")
            if IncrementNode == None:
                  print("Increment element not found in version XML file!")
                  exit(1)
            incrementText = "".join(IncrementNode.itertext())
            LastVersionNode = tree.find(".//LastVersion")
            if LastVersionNode == None:
                  print("LastVersion element not found in version XML file!")
                  exit(1)
            lastVersionText = "".join(LastVersionNode.itertext())
            ProjectNameNode = tree.find(".//ProjectName")
            if ProjectNameNode == None:
                  print("ProjectName element not found in version XML file!")
                  exit(1)
            self.projectname = "".join(ProjectNameNode.itertext())

            newmajor = int(lastVersionText.split('.')[0])
            newminor = int(lastVersionText.split('.')[1])
            newrevision = int(lastVersionText.split('.')[2])
            newbuild = int(lastVersionText.split('.')[3])
            
            if incrementText.lower() == "major":
                newmajor = str(eval("newmajor+1"))
            elif incrementText.lower() == "minor":
                newminor = str(eval("newminor+1"))
            elif incrementText.lower() == "revision":
                newrevision = str(eval("newrevision+1"))
            elif incrementText.lower() == "build":
                newbuild = str(eval("newbuild+1"))
                
            newversion = str(newmajor) + "." + str(newminor) + "." + str(newrevision) + "." + str(newbuild)

            self.newversion = newversion
            LastVersionNode.text = newversion
            tree.write(self.options.versionxmlfile)

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
            if self.options.versionxmlfile.__contains__('Telenor'):
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
        parser.add_option("--versionxmlfile", action="store", type="string", dest="versionxmlfile")
        parser.add_option("--versioncsfile", action="store", type="string", dest="versioncsfile")
        parser.add_option("--tfs-user", action="store", type="string", dest="tfs_user")
        parser.add_option("--tfs-pass", action="store", type="string", dest="tfs_pass")
        parser.add_option("--projectroot", action="store", type="string", dest="projectroot")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            self.showUsage
        else:
            self.options = options
        if self.options.versionxmlfile.__contains__('Telenor'):
            self.configfile = self.options.projectroot + "\\Applications\\WindowsService\\App.config"
        self.checkoutVersionFiles()
        self.incrementVersion()
        self.checkinVersionFiles()

    def validateOptions(self, options):
        if options.versionxmlfile == None or \
            options.versioncsfile == None or \
            options.tfs_user == None or \
            options.tfs_pass == None:
            return False
        else:
            return True

IncrementVersion()
