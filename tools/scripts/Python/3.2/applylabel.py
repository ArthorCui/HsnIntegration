import subprocess, os, sys
import glob, ibslib
from optparse import OptionParser
from shutil import *
from xml.etree.ElementTree import Element, SubElement, ElementTree

class ApplyLabel():
    def __init__(self):
        self.main()

    def showUsage(self):
        print("\
Usage: applylabel.py [options]\n\n\
Options:\n\
    -h, --help            show this help message and exit\n\
    --tfs-user=TFS_USER\n\
    --tfs-pass=TFS_PASS\n\
    --versionxmlfile=(path to version xml file)\n\
    --projectroot=(path of project to apply label to)\n\
    --version=(version) note: will override versionxmlfile\n\
    --label=(label) note: will override versionxmlfile")
        exit(1)

    def applyLabel(self):
        if not self.options.version == None and not self.options.label == None:
            self.new_db_version = self.options.version
            labelText = self.options.label.replace("${LastVersion}", self.new_db_version)
            self.newlabel = labelText
        else:
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
                self.newlabel = labelText
            else:
                print("Version XML file not found!")
                exit(1)
        ibslib.tfsLabel(self.newlabel, self.options.projectroot, self.options.tfs_user, self.options.tfs_pass)

    def main(self):
        parser = OptionParser()
        parser.add_option("--versionxmlfile", action="store", type="string", dest="versionxmlfile")
        parser.add_option("--projectroot", action="store", type="string", dest="projectroot")
        parser.add_option("--tfs-user", action="store", type="string", dest="tfs_user")
        parser.add_option("--tfs-pass", action="store", type="string", dest="tfs_pass")
        parser.add_option("--version", action="store", type="string", dest="version")
        parser.add_option("--label", action="store", type="string", dest="label")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            self.showUsage
        else:
            self.options = options

        self.applyLabel()

    def validateOptions(self, options):
        if  (options.versionxmlfile == None and (options.version == None or options.label == None)) or \
            options.projectroot == None or \
            options.tfs_user == None or \
            options.tfs_pass == None:
            return False
        else:
            return True

ApplyLabel()
