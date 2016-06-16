import subprocess, os, sys
import glob, ibslib
import datetime
from optparse import OptionParser
from shutil import *
from xml.etree.ElementTree import Element, SubElement, ElementTree

class CreateBranch():
    def __init__(self):
        self.main()

    def showUsage(self):
        print("\
Usage: CreateBranch.py [options]\n\n\
Options:\n\
    -h, --help            show this help message and exit\n\
    --tfs-user=TFS_USER\n\
    --tfs-pass=TFS_PASS\n\
    --versionxmlfile=(path to version xml file)\n\
    --integrationroot={integration root directory)\n\
    --projectroot=(path of project to branch)\n")
        exit(1)

    def createBranch(self):
        if os.path.exists(self.options.versionxmlfile):
            tree = ElementTree(file=self.options.versionxmlfile)
            
            LastVersionNode = tree.find(".//LastVersion")
            if LastVersionNode == None:
                  print("LastVersion element not found in version XML file!")
                  exit(1)
            lastVersionText = "".join(LastVersionNode.itertext())
            
            LabelNode = tree.find(".//Label")
            if LabelNode == None:
                  print("Label element not found in version XML file!")
                  exit(1)
            labelText = "".join(LabelNode.itertext())
            labelText = labelText.replace("${LastVersion}", lastVersionText)
            
            TargetBranchNode = tree.find(".//TargetTFSBranchPath")
            if TargetBranchNode == None:
                  print("Target TFS branch path element not found in version XML file!")
                  exit(1)
            targetBranchText = "".join(TargetBranchNode.itertext())
            targetBranchText = targetBranchText.replace("${LastVersion}", lastVersionText)
            targetBranchText = targetBranchText.replace("${Year}", str(datetime.date.today().year))
            
            ServerBranchNode = tree.find(".//LocalBranchPath")
            if ServerBranchNode == None:
                  print("Local branch path element not found in version XML file!")
                  exit(1)
            serverBranchText = "".join(ServerBranchNode.itertext())
            serverBranchText = serverBranchText.replace("${LastVersion}", lastVersionText)
            serverBranchText = serverBranchText.replace("${Year}", str(datetime.date.today().year))
            serverBranchText = serverBranchText.replace("${Integration_Root}", self.options.integrationroot)

            IsICMainNode = tree.find(".//IsICMainBuild")
            if IsICMainNode == None:
                  print("IsICMainBuild element not found in version XML file!")
                  exit(1)
            isICMainBuild = bool(int("".join(IsICMainNode.itertext())))

            BuildsFolderNode = tree.find(".//BuildsFolder")
            if BuildsFolderNode == None:
                  print("BuildsFolder element not found in version XML file!")
                  exit(1)
            buildsFolderText = "".join(BuildsFolderNode.itertext())
            buildsFolderText = buildsFolderText.replace("${Integration_Root}", self.options.integrationroot)

            tfscheckincomment = "CreateBranch script: Creating branch from label '" + labelText + "'"
            
            # Decloak build folders
            for folder in ibslib.tfsDir(buildsFolderText, self.options.tfs_user, self.options.tfs_pass):
                ibslib.tfsDecloak(buildsFolderText + "\\" + folder.split("$")[1], self.options.tfs_user, self.options.tfs_pass)
            
            if isICMainBuild:
                # Generate full padded IC main version string
                icmainversion = lastVersionText.split(".")[0].rjust(2, "0") + "." + lastVersionText.split(".")[1].rjust(2, "0") + "." + lastVersionText.split(".")[2].rjust(2, "0") + "." + lastVersionText.split(".")[3].rjust(2, "0")
                
                # Actually create the branch                
                ibslib.tfsBranch(labelText, self.options.projectroot, targetBranchText, True, self.options.tfs_user, self.options.tfs_pass)

                # Create the IC Main DLL folders
                ibslib.makeDir(serverBranchText)
                ibslib.makeDir(serverBranchText + "\\IC Main DLL")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Schemas")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\es")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Oracle DLL")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Templates")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Transformers")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Common")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Nagra")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Nagra\\Aladin.1.4")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Nagra\\Common")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Nagra\\Merlin.2.1")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Control.6.14")
                ibslib.makeDir(serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Control.6.14\\Common")
                
                # Copy the IC Main DLLs over                
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\*.*", serverBranchText + "\\IC Main DLL\\")
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\Schemas\\*.*", serverBranchText + "\\IC Main DLL\\Schemas\\")
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\es\\*.*", serverBranchText + "\\IC Main DLL\\es\\")
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\Oracle DLL\\*.*", serverBranchText + "\\IC Main DLL\\Oracle DLL\\")
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\Templates\\*.*", serverBranchText + "\\IC Main DLL\\Templates\\")
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\Transformers\\*.*", serverBranchText + "\\IC Main DLL\\Transformers\\")              
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\Transformers\\ConditionalAccess\\*.*", serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\")
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Common\\*.*", serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Common\\")                
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Nagra\\Aladin.1.4\\*.*", serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Nagra\\Aladin.1.4\\")
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Nagra\\Common\\*.*", serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Nagra\\Common\\")
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Nagra\\Merlin.2.1\\*.*", serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Nagra\\Merlin.2.1\\")
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Control.6.14\\*.*", serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Control.6.14\\")
                ibslib.copyFiles(self.options.projectroot + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Control.6.14\\Common\\*.*", serverBranchText + "\\IC Main DLL\\Transformers\\ConditionalAccess\\Control.6.14\\Common\\")

                # Create the IC Main Scripts folders                
                ibslib.makeDir(serverBranchText + "\\IC Main Scripts")
                ibslib.makeDir(serverBranchText + "\\IC Main Scripts\\CI_CENTRAL")
                ibslib.makeDir(serverBranchText + "\\IC Main Scripts\\CI_CENTRAL\\" + icmainversion)
                ibslib.makeDir(serverBranchText + "\\IC Main Scripts\\CI_QUEUEDATA")
                ibslib.makeDir(serverBranchText + "\\IC Main Scripts\\CI_QUEUEDATA\\" + icmainversion)
                ibslib.makeDir(serverBranchText + "\\IC Main Scripts\\CI_BUSINESSDATA")
                ibslib.makeDir(serverBranchText + "\\IC Main Scripts\\CI_BUSINESSDATA\\" + icmainversion)

                # Move the DatabaseObjects folders
                ibslib.moveFiles(self.options.projectroot + "\\CreateCI_DB\\CI_CENTRAL\\UpdateScripts\\" + icmainversion + "\\DatabaseObjects", serverBranchText + "\\IC Main Scripts\\CI_CENTRAL\\" + icmainversion + "\\")
                ibslib.moveFiles(self.options.projectroot + "\\CreateCI_DB\\CI_QUEUEDATA\\UpdateScripts\\" + icmainversion + "\\DatabaseObjects", serverBranchText + "\\IC Main Scripts\\CI_QUEUEDATA\\" + icmainversion + "\\")
                ibslib.moveFiles(self.options.projectroot + "\\CreateCI_DB\\CI_BUSINESSDATA\\UpdateScripts\\" + icmainversion + "\\DatabaseObjects", serverBranchText + "\\IC Main Scripts\\CI_BUSINESSDATA\\" + icmainversion + "\\")

                # Copy ad-hoc scripts
                ibslib.copyFiles(self.options.projectroot + "\\CreateCI_DB\\CI_CENTRAL\\UpdateScripts\\*.*", serverBranchText + "\\IC Main Scripts\\CI_CENTRAL\\" + icmainversion)
                ibslib.copyFiles(self.options.projectroot + "\\CreateCI_DB\\CI_QUEUEDATA\\UpdateScripts\\*.*", serverBranchText + "\\IC Main Scripts\\CI_QUEUEDATA\\" + icmainversion)
                ibslib.copyFiles(self.options.projectroot + "\\CreateCI_DB\\CI_BUSINESSDATA\\UpdateScripts\\*.*", serverBranchText + "\\IC Main Scripts\\CI_BUSINESSDATA\\" + icmainversion)

                # Delete ad-hoc scripts from TFS                
                ibslib.tfsDelete(self.options.projectroot + "\\CreateCI_DB\\CI_CENTRAL\\UpdateScripts\\*.*", self.options.tfs_user, self.options.tfs_pass)
                ibslib.tfsDelete(self.options.projectroot + "\\CreateCI_DB\\CI_QUEUEDATA\\UpdateScripts\\*.*", self.options.tfs_user, self.options.tfs_pass)
                ibslib.tfsDelete(self.options.projectroot + "\\CreateCI_DB\\CI_BUSINESSDATA\\UpdateScripts\\*.*", self.options.tfs_user, self.options.tfs_pass)

                # Delete intermediary files                
                ibslib.delFiles(self.options.projectroot + "\\CreateCI_DB\\CI_CENTRAL\\UpdateScripts\\" + icmainversion)
                ibslib.delFiles(self.options.projectroot + "\\CreateCI_DB\\CI_QUEUEDATA\\UpdateScripts\\" + icmainversion)
                ibslib.delFiles(self.options.projectroot + "\\CreateCI_DB\\CI_BUSINESSDATA\\UpdateScripts\\" + icmainversion)

                # Add branch DLLs and scripts to TFS                
                ibslib.tfsAdd(serverBranchText + "\\IC Main DLL", self.options.tfs_user, self.options.tfs_pass)
                ibslib.tfsAdd(serverBranchText+ "\\IC Main Scripts", self.options.tfs_user, self.options.tfs_pass)
                
                # Check in files
                ibslib.tfsCheckin(self.options.projectroot + "\\CreateCI_DB", "CreateBranch script: Deleting IC Main update scripts", self.options.tfs_user, self.options.tfs_pass)
            else:
                ibslib.tfsBranch(labelText, self.options.projectroot, targetBranchText, False, self.options.tfs_user, self.options.tfs_pass)
                
            ibslib.tfsCheckin(serverBranchText, tfscheckincomment, self.options.tfs_user, self.options.tfs_pass)

            # Cloak builds folder
            for folder in ibslib.tfsDir(buildsFolderText, self.options.tfs_user, self.options.tfs_pass):
                ibslib.tfsCloak(buildsFolderText + "\\" + folder, self.options.tfs_user, self.options.tfs_pass)
        else:
            print("Version XML file not found!")
            exit(1)

    def main(self):
        parser = OptionParser()
        parser.add_option("--versionxmlfile", action="store", type="string", dest="versionxmlfile")
        parser.add_option("--integrationroot", action="store", type="string", dest="integrationroot")
        parser.add_option("--projectroot", action="store", type="string", dest="projectroot")
        parser.add_option("--tfs-user", action="store", type="string", dest="tfs_user")
        parser.add_option("--tfs-pass", action="store", type="string", dest="tfs_pass")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            self.showUsage()
            exit(1)
        else:
            self.options = options

        self.createBranch()

    def validateOptions(self, options):
        if options.versionxmlfile == None or \
            options.integrationroot == None or \
            options.projectroot == None or \
            options.tfs_user == None or \
            options.tfs_pass == None:
            return False
        else:
            return True

CreateBranch()
