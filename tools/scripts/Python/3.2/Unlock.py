import subprocess, os, sys
import ibslib
from optparse import OptionParser
from shutil import *

class Unlock():
    def __init__(self):
        self.main()

    def showUsage(self):
        print("\
Usage: unlock.py [options]\n\n\
Options:\n\
    -h, --help            show this help message and exit\n\
    --tfs-user=TFS_USER\n\
    --tfs-pass=TFS_PASS\n\
    --tfs-path=(tfs path to unlock)\n")
        exit(1)

    def unlock(self):
        ibslib.tfsUnlock(self.options.tfspath, self.options.tfs_user, self.options.tfs_pass)

    def main(self):
        parser = OptionParser()
        parser.add_option("--tfs-path", action="store", type="string", dest="tfspath")
        parser.add_option("--tfs-user", action="store", type="string", dest="tfs_user")
        parser.add_option("--tfs-pass", action="store", type="string", dest="tfs_pass")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            self.showUsage
        else:
            self.options = options

        self.unlock()

    def validateOptions(self, options):
        if options.tfspath == None or \
            options.tfs_user == None or \
            options.tfs_pass == None:
            return False
        else:
            return True

Unlock()
