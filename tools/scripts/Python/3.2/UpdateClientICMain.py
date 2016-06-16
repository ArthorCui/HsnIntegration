import subprocess, os, sys
import glob, ibslib, datetime
from optparse import OptionParser
from shutil import *

class UpdateClientICMain():
    def __init__(self):
        self.main()

    def showUsage(self):
        print("\
Usage: UpdateClientICMain.py [options]\n\n\
Options:\n\
    -h, --help            show this help message and exit\n\
    --tfs-user=TFS_USER\n\
    --tfs-pass=TFS_PASS\n\
    --icmainroot=(root directory of ic main)\n\
    --icmainversion=(version of ic main)\n\
    --clientroot=(path of project)\n")
        exit(1)

    def updateICMain(self):
        schemas = ["CI_CENTRAL", "CI_QUEUEDATA"]
        
        # 1. tfs get latest of client\IC Main Scripts
        ibslib.tfsGet(self.options.clientroot + "\\IC Main Scripts", self.options.tfs_user, self.options.tfs_pass)
        
        # 2. tfs get latest of main\builds\yyyy\<version>\IC Main DLL
        ibslib.tfsGet(self.options.icmainroot + "\\builds\\" + str(datetime.date.today().year) + "\\" + self.options.icmainversion + "\\IC Main DLL", self.options.tfs_user, self.options.tfs_pass)
        
        # 3. tfs get latest of main\integration\CreateCI_DB
        ibslib.tfsGet(self.options.icmainroot + "\\integration\\CreateCI_DB", self.options.tfs_user, self.options.tfs_pass)

        # 4. tfs delete each folder in client\IC Main Scripts\<SCHEMA>
        for schema in schemas:
            # perform tfs undo of any pending checkins
            ibslib.tfsUndo(self.options.clientroot + "\\IC Main Scripts\\" + schema, self.options.tfs_user, self.options.tfs_pass)
            
            # perform tfs delete of any subfolders remaining
            for folder in glob.glob(self.options.clientroot + "\\IC Main Scripts\\" + schema + "\\*"):
                ibslib.tfsDelete(folder, self.options.tfs_user, self.options.tfs_pass)
                
                # delete all left over files, if any exist
                if os.path.exists(folder):
                    ibslib.unsetReadOnly(folder)
                    rmtree(folder)
        
        # 5. tfs recursive checkout client\IC Main DLL\*
        ibslib.tfsCheckout(self.options.clientroot + "\\IC Main DLL\\*", self.options.tfs_user, self.options.tfs_pass)
        
        # ensure remaining files are writable
        ibslib.unsetReadOnly(self.options.clientroot + "\\IC Main DLL")
        
        # 6. recursive copy main\builds\yyyy\<version>\IC Main DLL\* to client\IC Main DLL
        ibslib.copyFiles(self.options.icmainroot + "\\builds\\" + str(datetime.date.today().year) + "\\" + self.options.icmainversion + "\\IC Main DLL\\*", self.options.clientroot + "\\IC Main DLL\\")
        
        # 7. recursive copy main\integration\CreateCI_DB\<SCHEMA>\UpdateScripts\<version> to client\IC Main Scripts\<SCHEMA>
        paddedversion = self.options.icmainversion.split(".")[0].rjust(2, "0") + "." + self.options.icmainversion.split(".")[1].rjust(2, "0") + "." + self.options.icmainversion.split(".")[2].rjust(2, "0") + "." + self.options.icmainversion.split(".")[3].rjust(2, "0")
        for schema in schemas:
            copytree(self.options.icmainroot + "\\integration\\CreateCI_DB\\" + schema + "\\UpdateScripts\\" + paddedversion, self.options.clientroot + "\\IC Main Scripts\\" + schema + "\\" + paddedversion)
            
            # 8. tfs add client\IC Main Scripts\<SCHEMA>\<version>
            ibslib.tfsAdd(self.options.clientroot + "\\IC Main Scripts\\" + schema + "\\" + paddedversion, self.options.tfs_user, self.options.tfs_pass)
        
        # 9. tfs checkin client
        projectname = self.options.clientroot.split("\\")[self.options.clientroot.split("\\").__len__() - 2]
        tfscheckincomment = "UpdateClientICMain script: Incremented " + projectname + " IC Main version to " + self.options.icmainversion
        ibslib.tfsCheckin(self.options.clientroot, tfscheckincomment, self.options.tfs_user, self.options.tfs_pass)

    def main(self):
        parser = OptionParser()
        parser.add_option("--tfs-user", action="store", type="string", dest="tfs_user")
        parser.add_option("--tfs-pass", action="store", type="string", dest="tfs_pass")
        parser.add_option("--icmainroot", action="store", type="string", dest="icmainroot")
        parser.add_option("--icmainversion", action="store", type="string", dest="icmainversion")
        parser.add_option("--clientroot", action="store", type="string", dest="clientroot")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            self.showUsage()
        else:
            self.options = options
            self.updateICMain()

    def validateOptions(self, options):
        if options.icmainroot == None or \
            options.icmainversion == None or \
            options.clientroot == None or \
            options.tfs_user == None or \
            options.tfs_pass == None:
            return False
        else:
            return True

UpdateClientICMain()
