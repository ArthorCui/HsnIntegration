import subprocess, os, sys
import glob, ibslib
import datetime, zipfile
from optparse import OptionParser
from shutil import *
from xml.etree.ElementTree import Element, SubElement, ElementTree

class PackageBuild():
    def __init__(self):
        self.main()

    def showUsage(self):
        print("\
Usage: PackageBuild.py [options]\n\n\
Options:\n\
    -h, --help            show this help message and exit\n\
    --versionxmlfile=(path to version xml file)\n\
    --projectroot=(path of project to package)\n\
    --target-path=(path of build destination folder)\n\
    --version=(version) note: will override versionxmlfile\n\
    --zipfilename=(zip file name)")
        exit(1)

    def packageBuild(self):
        self.targetpath = self.options.target_path + "\\" + self.zipfilename.replace("${LastVersion}", self.newversion).split(".zip")[0]
        if os.path.exists(self.targetpath):
            print("Target directory already exists!")
            exit(1)
            
        # Create destination folder
        ibslib.makeDir(self.targetpath)

        # Copy files
        if len(glob.glob(self.options.projectroot + "\\Integration.MassImport\\*.exe")) > 0:
            ibslib.copyFiles(self.options.projectroot + "\\Integration.MassImport\\*.exe", self.targetpath + "\\")
        if len(glob.glob(self.options.projectroot + "\\Integration.UI\\*.exe")) > 0:
            ibslib.copyFiles(self.options.projectroot + "\\Integration.UI\\*.exe", self.targetpath + "\\")
        if len(glob.glob(self.options.projectroot + "\\Setup\\WindowsService\\Debug\\*.msi")) > 0:
            ibslib.copyFiles(self.options.projectroot + "\\Setup\\WindowsService\\Debug\\*.msi", self.targetpath + "\\")
        if len(glob.glob(self.options.projectroot + "\\Setup\\DBCreate\\*.exe")) > 0:
            ibslib.copyFiles(self.options.projectroot + "\\Setup\\DBCreate\\*.exe", self.targetpath + "\\")
        if len(glob.glob(self.options.projectroot + "\\Setup\\DBUpgrade\\*.exe")) > 0:
            ibslib.copyFiles(self.options.projectroot + "\\Setup\\DBUpgrade\\*.exe", self.targetpath + "\\")
        if len(glob.glob(self.options.projectroot + "\\GenericXMLInterfaceSetup\\Debug\\*.msi")) > 0:
            ibslib.copyFiles(self.options.projectroot + "\\GenericXMLInterfaceSetup\\Debug\\*.msi", self.targetpath + "\\")
            
        # Copy PDBs
        if len(glob.glob(self.options.projectroot + "\\Applications\\WindowsService\\bin\\Debug\\*.pdb")) > 0:
            ibslib.makeDir(self.targetpath + "\\PDBs")
            ibslib.copyFiles(self.options.projectroot + "\\Applications\\WindowsService\\bin\\Debug\\*.pdb", self.targetpath + "\\PDBs\\")

        # Create README.txt
        readmePath = "\\Setup\\WindowsService\\README.txt"
        if self.options.versionxmlfile.__contains__('ICFramework'):
            readmePath = "\\Integration.MassImport\\README.txt"
        if len(glob.glob(self.options.projectroot + readmePath)) > 0:
            currentdate = datetime.date.today().isoformat()
            fi = open(self.options.projectroot + readmePath, "r")
            fo = open(self.targetpath + "\\README.txt", "w")
            lines = fi.readlines()
            for line in lines:
                line = line.replace("${Version}", self.newversion)
                line = line.replace("${Date}", currentdate)
                fo.write(line)
            fo.close()
            fi.close()

        # Create README.doc
        readmePath = "\\Setup\\WindowsService\\readme.doc"
        if len(glob.glob(self.options.projectroot + readmePath)) > 0:
            currentdate = datetime.date.today().isoformat()
            os.system('wordreplace "' + self.options.projectroot + readmePath + '" "' + self.targetpath + '\\readme.doc" "${Version}|${Date}" "' + self.newversion + '|' + currentdate + '"')

        # Create config HTML documentation
        configXsltPath = "\\Setup\\config_stylesheet.xslt"
        if len(glob.glob(self.options.projectroot + configXsltPath)) > 0:
            os.system('xsltprocessor "' + self.options.projectroot + '\\Setup\\DM\\ICAnswerFile.xml" "' + self.targetpath + '\\ic_config.html" "' + self.options.projectroot + configXsltPath + '"')

        # Copy Telenor-62 specific files
        if self.options.versionxmlfile.__contains__('Telenor'):
            ibslib.makeDir(self.targetpath + "\\Java Files")
            ibslib.copyFiles(self.options.projectroot + "\\Common\\Crypto\\jhbci-provider.jar", self.targetpath + "\\Java Files\\")
            ibslib.copyFiles(self.options.projectroot + "\\Common\\Crypto\\local_policy.jar", self.targetpath + "\\Java Files\\")
            ibslib.copyFiles(self.options.projectroot + "\\Common\\Crypto\\US_export_policy.jar", self.targetpath + "\\Java Files\\")
            ibslib.copyFiles(self.options.projectroot + "\\..\\..\\MassImport\\MassImportSetup\\Debug\\MassImportSetup.msi", self.targetpath + "\\")
            ibslib.makeDir(self.targetpath + "\\ConfigurationDocuments")
            ibslib.copyFiles(self.options.projectroot + "\\Setup\\WindowsService\\ConfigurationDocuments\\*.*", self.targetpath + "\\ConfigurationDocuments\\")
            ibslib.copyFiles(self.options.projectroot + "\\Setup\\WindowsService\\SAMPLE_MACHINE_ANSWER_FILE.xml", self.targetpath + "\\")
            ibslib.copyFiles(self.options.projectroot + "\\..\\..\\Build\\AnswerFile\\EndUser\\MACHINE_ANSWER_FILE.xsd", self.targetpath + "\\")
            
        # Copy GET-specific files
        if self.options.versionxmlfile.__contains__('GET'):
            ibslib.copyFiles(self.options.projectroot + "\\Configuration Scripts\\IC\\SAMPLE_MACHINE_ANSWER_FILE.xml", self.targetpath + "\\")
            ibslib.copyFiles(self.options.projectroot + "\\..\\..\\Build\\AnswerFile\\EndUser\\MACHINE_ANSWER_FILE.xsd", self.targetpath + "\\")

        # Copy BBCL-specific files
        if self.options.versionxmlfile.__contains__('VideoCon'):
            ibslib.copyFiles(self.options.projectroot + "\\Setup\\WindowsService\\SAMPLE_MACHINE_ANSWER_FILE.xml", self.targetpath + "\\")
            ibslib.copyFiles(self.options.projectroot + "\\..\\..\\Build\\AnswerFile\\EndUser\\MACHINE_ANSWER_FILE.xsd", self.targetpath + "\\")
        
        # Copy mass import-specific files
        if self.options.versionxmlfile.__contains__('ICFramework'):
            ibslib.copyFiles(self.options.projectroot + "\\Integration.MassImport\\Customer_Mass_Import_Integration_Framework_Guide.doc", self.targetpath + "\\")
            ibslib.copyFiles(self.options.projectroot + "\\MACHINE_ANSWER_FILE.xml", self.targetpath + "\\")
            ibslib.copyFiles(self.options.projectroot + "\\..\\..\\..\\Build\\AnswerFile\\EndUser\\MACHINE_ANSWER_FILE.xsd", self.targetpath + "\\")

        # Copy CAMC-specific files
        if self.options.versionxmlfile.__contains__('CAMC'):
            ibslib.copyFiles(self.options.projectroot + "\\Setup\\WindowsService\\SAMPLE_MACHINE_ANSWER_FILE.xml", self.targetpath + "\\")
            ibslib.copyFiles(self.options.projectroot + "\\Setup\\WindowsService\\MACHINE_ANSWER_FILE.xsd", self.targetpath + "\\")
            ibslib.makeDir(self.targetpath + "\\DbUpgradeConversionScripts")
            ibslib.copyFiles(self.options.projectroot + "\\Setup\\WindowsService\\DbUpgradeConversionScripts\\*.sql", self.targetpath + "\\DbUpgradeConversionScripts\\")
            ibslib.makeDir(self.targetpath + "\\CoreFiles")
            ibslib.copyFiles(self.options.projectroot + "\\Setup\WindowsService\\CoreFiles\\*.*", self.targetpath + "\\CoreFiles\\")

        # Create zip file
        buildzipfile = zipfile.ZipFile(self.options.target_path + "\\" + self.zipfilename, "w")
        for name in glob.glob(self.targetpath + "\\*.*"):
            buildzipfile.write(name, self.zipfilename.split(".zip")[0] + "\\" + os.path.basename(name), zipfile.ZIP_DEFLATED)
        for name in glob.glob(self.targetpath + "\\PDBs\\*.*"):
            buildzipfile.write(name, self.zipfilename.split(".zip")[0] + "\\PDBs\\" + os.path.basename(name), zipfile.ZIP_DEFLATED)
        for name in glob.glob(self.targetpath + "\\Java Files\\*.*"):
            buildzipfile.write(name, self.zipfilename.split(".zip")[0] + "\\Java Files\\" + os.path.basename(name), zipfile.ZIP_DEFLATED)
        for name in glob.glob(self.targetpath + "\\ConfigurationDocuments\\*.*"):
            buildzipfile.write(name, self.zipfilename.split(".zip")[0] + "\\ConfigurationDocuments\\" + os.path.basename(name), zipfile.ZIP_DEFLATED)
        for name in glob.glob(self.targetpath + "\\DbUpgradeConversionScripts\\*.*"):
            buildzipfile.write(name, self.zipfilename.split(".zip")[0] + "\\DbUpgradeConversionScripts\\" + os.path.basename(name), zipfile.ZIP_DEFLATED)
        for name in glob.glob(self.targetpath + "\\CoreFiles\\*.*"):
            buildzipfile.write(name, self.zipfilename.split(".zip")[0] + "\\CoreFiles\\" + os.path.basename(name), zipfile.ZIP_DEFLATED)
        buildzipfile.close()

        # Copy artifacts for TeamCity
        if self.options.versionxmlfile.__contains__('CAMC') or self.options.versionxmlfile.__contains__('GET') or self.options.versionxmlfile.__contains__('Telenor'):
            ibslib.delFiles(self.options.projectroot + "\\Setup\\Deploy\\*.zip")
            ibslib.copyFiles(self.options.target_path + "\\" + self.zipfilename, self.options.projectroot + "\\Setup\\Deploy\\")
        
    def getVersion(self):
        if (not self.options.version == None and not self.options.zipfilename == None):
            self.newversion = self.options.version
            self.zipfilename = self.options.zipfilename.replace("${LastVersion}", self.newversion)
        else:
            if os.path.exists(self.options.versionxmlfile):
                tree = ElementTree(file=self.options.versionxmlfile)
                LastVersionNode = tree.find(".//LastVersion")
                if LastVersionNode == None:
                      print("LastVersion element not found in version XML file!")
                      exit(1)
                lastVersionText = "".join(LastVersionNode.itertext())
                self.newversion = lastVersionText
                ZipFileNameNode = tree.find(".//ZipFileName")
                if ZipFileNameNode == None:
                      print("ZipFileName element not found in version XML file!")
                      exit(1)
                zipFileNameText = "".join(ZipFileNameNode.itertext())
                self.zipfilename = zipFileNameText.replace("${LastVersion}", self.newversion)
            else:
                print("Version XML file not found!")
                exit(1)

    def main(self):
        parser = OptionParser()
        parser.add_option("--versionxmlfile", action="store", type="string", dest="versionxmlfile")
        parser.add_option("--projectroot", action="store", type="string", dest="projectroot")
        parser.add_option("--target-path", action="store", type="string", dest="target_path")
        parser.add_option("--version", action="store", type="string", dest="version")
        parser.add_option("--zipfilename", action="store", type="string", dest="zipfilename")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            self.showUsage
        else:
            self.options = options

        self.getVersion()
        self.packageBuild()

    def validateOptions(self, options):
        if (options.versionxmlfile == None and (options.version == None or options.zipfilename == None)) or \
            options.projectroot == None or \
            options.target_path == None:
            return False
        else:
            return True

PackageBuild()
