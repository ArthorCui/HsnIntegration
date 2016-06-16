import subprocess, os, sys
import glob, ibslib
import datetime, zipfile
from optparse import OptionParser
from shutil import *
from xml.etree.ElementTree import Element, SubElement, ElementTree

class PackageBuildICMain():
    def __init__(self):
        self.main()

    def showUsage(self):
        print("\
Usage: PackageBuildICMain.py [options]\n\n\
Options:\n\
    -h, --help            show this help message and exit\n\
    --tfs-user=TFS_USER\n\
    --tfs-pass=TFS_PASS\n\
    --projectroot=(path of project to package)\n\
    --integrationroot=(path of integration root)\n\
    --versionxmlfile=(path to version xml file)\n\
    --version=(version) note: will override versionxmlfile\n")
        exit(1)

    def packageBuildICMain(self):
        self.targetpath = self.options.projectroot + "\\Setup\\Deploy"
        
        # Branch build steps
        if self.branchPath == None:
            # For branch builds, the branch path must be the same as the target path
            self.branchPath = self.options.projectroot
                
            # Create the IC Main Scripts folders                
            ibslib.makeDir(self.branchPath + "\\IC Main Scripts")
            ibslib.makeDir(self.branchPath + "\\IC Main Scripts\\CI_CENTRAL")
            ibslib.makeDir(self.branchPath + "\\IC Main Scripts\\CI_CENTRAL\\" + self.newversion)
            ibslib.makeDir(self.branchPath + "\\IC Main Scripts\\CI_QUEUEDATA")
            ibslib.makeDir(self.branchPath + "\\IC Main Scripts\\CI_QUEUEDATA\\" + self.newversion)
            ibslib.makeDir(self.branchPath + "\\IC Main Scripts\\CI_BUSINESSDATA")
            ibslib.makeDir(self.branchPath + "\\IC Main Scripts\\CI_BUSINESSDATA\\" + self.newversion)
    
            # Move the DatabaseObjects folders
            ibslib.moveFiles(self.branchPath + "\\CreateCI_DB\\CI_CENTRAL\\UpdateScripts\\" + self.newversion + "\\DatabaseObjects", self.branchPath + "\\IC Main Scripts\\CI_CENTRAL\\" + self.newversion + "\\DatabaseObjects")
            ibslib.moveFiles(self.branchPath + "\\CreateCI_DB\\CI_QUEUEDATA\\UpdateScripts\\" + self.newversion + "\\DatabaseObjects", self.branchPath + "\\IC Main Scripts\\CI_QUEUEDATA\\" + self.newversion + "\\DatabaseObjects")
            ibslib.moveFiles(self.branchPath + "\\CreateCI_DB\\CI_BUSINESSDATA\\UpdateScripts\\" + self.newversion + "\\DatabaseObjects", self.branchPath + "\\IC Main Scripts\\CI_BUSINESSDATA\\" + self.newversion + "\\DatabaseObjects")
    
            # Copy ad-hoc scripts
            ibslib.copyFiles(self.branchPath + "\\CreateCI_DB\\CI_CENTRAL\\UpdateScripts\\*.*", self.branchPath + "\\IC Main Scripts\\CI_CENTRAL\\" + self.newversion)
            ibslib.copyFiles(self.branchPath + "\\CreateCI_DB\\CI_QUEUEDATA\\UpdateScripts\\*.*", self.branchPath + "\\IC Main Scripts\\CI_QUEUEDATA\\" + self.newversion)
            ibslib.copyFiles(self.branchPath + "\\CreateCI_DB\\CI_BUSINESSDATA\\UpdateScripts\\*.*", self.branchPath + "\\IC Main Scripts\\CI_BUSINESSDATA\\" + self.newversion)
    
            # Delete ad-hoc scripts from TFS                
            ibslib.tfsDelete(self.branchPath + "\\CreateCI_DB\\CI_CENTRAL\\UpdateScripts\\*.*", self.options.tfs_user, self.options.tfs_pass)
            ibslib.tfsDelete(self.branchPath + "\\CreateCI_DB\\CI_QUEUEDATA\\UpdateScripts\\*.*", self.options.tfs_user, self.options.tfs_pass)
            ibslib.tfsDelete(self.branchPath + "\\CreateCI_DB\\CI_BUSINESSDATA\\UpdateScripts\\*.*", self.options.tfs_user, self.options.tfs_pass)
    
            # Delete intermediary files                
            ibslib.delFiles(self.branchPath + "\\CreateCI_DB\\CI_CENTRAL\\UpdateScripts\\" + self.newversion)
            ibslib.delFiles(self.branchPath + "\\CreateCI_DB\\CI_QUEUEDATA\\UpdateScripts\\" + self.newversion)
            ibslib.delFiles(self.branchPath + "\\CreateCI_DB\\CI_BUSINESSDATA\\UpdateScripts\\" + self.newversion)
    
            # Add branch scripts to TFS                
            ibslib.tfsAdd(self.branchPath + "\\IC Main Scripts\\CI_CENTRAL\\" + self.newversion, self.options.tfs_user, self.options.tfs_pass)
            ibslib.tfsAdd(self.branchPath + "\\IC Main Scripts\\CI_QUEUEDATA\\" + self.newversion, self.options.tfs_user, self.options.tfs_pass)
            ibslib.tfsAdd(self.branchPath + "\\IC Main Scripts\\CI_BUSINESSDATA\\" + self.newversion, self.options.tfs_user, self.options.tfs_pass)
            
            # Check in files
            ibslib.tfsCheckin(self.branchPath + "\\CreateCI_DB", "PackageBuildICMain script: Deleting IC Main update scripts", self.options.tfs_user, self.options.tfs_pass)
            ibslib.tfsCheckin(self.branchPath + "\\IC Main DLL", "PackageBuildICMain script: Updating IC Main DLLs", self.options.tfs_user, self.options.tfs_pass)
            ibslib.tfsCheckin(self.branchPath + "\\IC Main Scripts", "PackageBuildICMain script: Adding new IC Main DB Scripts", self.options.tfs_user, self.options.tfs_pass)
            
        self.zipfilenameDLL = self.branchPath + "\\IC Main " + self.newversion + " DLL.zip"
        self.zipfilenameDBScripts = self.branchPath + "\\IC Main " + self.newversion + " DBScripts.zip"
        
        # Create destination folders
        ibslib.makeDir(self.options.projectroot + "\\Setup")
        ibslib.makeDir(self.options.projectroot + "\\Setup\\Deploy")
        
        # Delete any previous zip files if they exist
        ibslib.delFiles(self.targetpath + "\\*.zip")
        ibslib.delFiles(self.branchPath + "\\*.zip")

        # Create DLL zip file
        buildzipfile = zipfile.ZipFile(self.zipfilenameDLL, "w")
        for name in ibslib.getFilesRecursive(self.branchPath + "\\IC Main DLL", "*.*"):
            relativepath = name.split("IC Main DLL\\")[1]
            print("Compressing - %s" % relativepath)
            buildzipfile.write(name, relativepath, zipfile.ZIP_DEFLATED)
        buildzipfile.close()

        # Create DBScripts zip file
        buildzipfile = zipfile.ZipFile(self.zipfilenameDBScripts, "w")
        for name in ibslib.getFilesRecursive(self.branchPath + "\\IC Main Scripts", "*.*"):
            relativepath = name.split("IC Main Scripts\\")[1]
            print("Compressing - %s" % relativepath)
            buildzipfile.write(name, relativepath, zipfile.ZIP_DEFLATED)
        buildzipfile.close()
        
        # Copy artifacts for TeamCity
        ibslib.copyFiles(self.branchPath + "\\*.zip", self.targetpath)

    def getVersion(self):
        if os.path.exists(self.options.versionxmlfile):
            tree = ElementTree(file=self.options.versionxmlfile)
            LastVersionNode = tree.find(".//LastVersion")
            if LastVersionNode == None:
                  print("LastVersion element not found in version XML file!")
                  exit(1)
            lastVersionText = "".join(LastVersionNode.itertext())
            self.newversion = lastVersionText
            ServerBranchNode = tree.find(".//LocalBranchPath")
            if ServerBranchNode == None:
                  print("Local branch path element not found in version XML file!")
                  exit(1)
            serverBranchText = "".join(ServerBranchNode.itertext())
            serverBranchText = serverBranchText.replace("${LastVersion}", lastVersionText)
            serverBranchText = serverBranchText.replace("${Year}", str(datetime.date.today().year))
            serverBranchText = serverBranchText.replace("${Integration_Root}", self.options.integrationroot)
            self.branchPath = serverBranchText
        else:
            print("Version XML file not found!")
            exit(1)
            
        # If a version is passed in, it overrides the version in the version.xml file
        if not self.options.version == None:
            self.newversion = self.options.version
            self.branchPath = None
        
    def main(self):
        parser = OptionParser()
        parser.add_option("--tfs-user", action="store", type="string", dest="tfs_user")
        parser.add_option("--tfs-pass", action="store", type="string", dest="tfs_pass")
        parser.add_option("--projectroot", action="store", type="string", dest="projectroot")
        parser.add_option("--integrationroot", action="store", type="string", dest="integrationroot")
        parser.add_option("--versionxmlfile", action="store", type="string", dest="versionxmlfile")
        parser.add_option("--version", action="store", type="string", dest="version")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            self.showUsage()
        else:
            self.options = options

        self.getVersion()
        self.packageBuildICMain()

    def validateOptions(self, options):
        if options.versionxmlfile == None or \
            options.projectroot == None or \
            options.integrationroot == None or \
            options.tfs_user == None or \
            options.tfs_pass == None:
            return False
        else:
            return True

PackageBuildICMain()
