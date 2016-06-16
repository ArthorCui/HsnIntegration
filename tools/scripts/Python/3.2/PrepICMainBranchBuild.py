import subprocess, os, sys
import glob, ibslib
import datetime, zipfile
from optparse import OptionParser
from shutil import *

class PrepICMainBranchBuild():
    def __init__(self):
        self.main()

    def showUsage(self):
        print("\
Usage: PrepICMainBranchBuild.py [options]\n\n\
Options:\n\
    -h, --help            show this help message and exit\n\
    --tfs-user=TFS_USER\n\
    --tfs-pass=TFS_PASS\n\
    --projectroot=(path of project to package)\n")
        exit(1)

    def prepICMainBranchBuild(self):
        ibslib.tfsCheckout(self.options.projectroot + "\\IC Main DLL", self.options.tfs_user, self.options.tfs_pass)
        for folder in glob.glob(self.options.projectroot + "\\IC Main Scripts\\CI_CENTRAL\\*"):
            ibslib.tfsDelete(folder, self.options.tfs_user, self.options.tfs_pass)
        for folder in glob.glob(self.options.projectroot + "\\IC Main Scripts\\CI_QUEUEDATA\\*"):
            ibslib.tfsDelete(folder, self.options.tfs_user, self.options.tfs_pass)
        
    def main(self):
        parser = OptionParser()
        parser.add_option("--tfs-user", action="store", type="string", dest="tfs_user")
        parser.add_option("--tfs-pass", action="store", type="string", dest="tfs_pass")
        parser.add_option("--projectroot", action="store", type="string", dest="projectroot")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            self.showUsage
        else:
            self.options = options

        self.prepICMainBranchBuild()

    def validateOptions(self, options):
        if options.projectroot == None or \
            options.tfs_user == None or \
            options.tfs_pass == None:
            return False
        else:
            return True

PrepICMainBranchBuild()
