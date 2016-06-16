import sys, os, subprocess, glob, ibslib
from optparse import OptionParser

class CopyCoreDLLs():
    def __init__(self):
        self.main()

    def showUsage(self):
        print "\
Usage: CopyCoreDLLs.py [options]\n\n\
Options:\n\
  -h, --help            show this help message and exit\n\
  -c CORE_PATH, --core-path=CORE_PATH\n\
  -a ASM_VERSION, --asm-version=VERSION\n\
  -b BSM_VERSION, --bsm-version=VERSION\n\
  -i ISM_VERSION, --ism-version=VERSION\n\
  -p BASE_PATH, --base_path=BASE_PATH\n\
  -u TFS_USER, --tfs-user=TFS_USER\n\
  -f TFS_PASS, --tfs-pass=TFS_PASS\n\n\
  Notes:\n\n\
  This script will check out the DLLs in the BASE_PATH you specify, copy the Core DLLs over, and check in the DLLs.\n\n\
  Be sure you do a CD to somewhere in your TFS workspace before running.\n\n\
  Be sure to set the directory structure up beforehand like the following:\n\
  <root folder>\ASM\<asm_version>\WS\n\
  <root folder>\ISM\<ism_version>\WS\n\
  <root folder>\BSM\<bsm_version>\services\n\n\
  You must use short filenames (no spaces) when giving the TFS path.\n\n\
  Example:\n\
  C:\\Integration\\THOR\\Integration-62>CopyCoreDLLs.py -c 'C:\\62 Core Builds' -a 62.9.0.34 -b 62.9.0.32 -i 62.9.0.34 -p \
'C:\\integration\\THOR\\Integration-62\\All\\Soluti~1\\bin' -u 'Tfs.User' -f 'tfspass'"


    def copyCoreDLLs(self):
        dlls = ["ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\Entriq.*.dll',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\*ServiceContracts.dll',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.ApplicationServices.SharedContracts.DLL',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.ApplicationServices.ServiceMediator.DLL', 
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.ApplicationServices.ServiceLocator.DLL',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.ApplicationServices.Factory.Internal.DLL',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.ApplicationServices.Factory.dll',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.ApplicationServices.ErrorHandler.DLL',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.ApplicationServices.CustomFields.BizObj.dll',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.ApplicationServices.ConfigurationBase.DLL',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.ApplicationServices.CacheInterceptor.DLL',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.Security.Interfaces.DLL',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.Security.AuthenticationInterceptor.DLL',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.Security.Authentication.AuthenticationBase.DLL',
        "ASM\\" + self.options.asm_version + '\\WS\\ALL\\bin\\PayMedia.Logging.DLL',
        "ISM\\" + self.options.ism_version + '\\WS\\EventDistributorService\\bin\\PayMedia.Framework.Messaging.Contracts.dll',
        "ISM\\" + self.options.ism_version + '\\WS\\EventDistributorService\\bin\\PayMedia.Framework.Integration.Contracts.dll',
        "ISM\\" + self.options.ism_version + '\\WS\\EventDistributorService\\bin\\PayMedia.Configuration.dll',
        "BSM\\" + self.options.bsm_version + '\\services\\PayMedia.Background.Processor.DLL',
        "BSM\\" + self.options.bsm_version + '\\services\\PayMedia.Background.Interfaces.DLL',
        "BSM\\" + self.options.bsm_version + '\\services\\PayMedia.Background.CriticalAction.dll',
        "BSM\\" + self.options.bsm_version + '\\services\\Entriq.Framework.Configuration.DLL']
        print "Checking out Core DLLs..."
        self.checkoutDLLs()
        for dllmask in dlls:
            print "Copying %s" % self.options.core_path + "\\" + dllmask
            ibslib.copyFiles(self.options.core_path + "\\" + dllmask, self.options.base_path + "\\")
        self.checkinDLLs()

    def checkoutDLLs(self):
        print "Checking out DLLs..."
        ibslib.runCommand("tf.exe get \"" + self.options.base_path + "\\*.dll\" /login:" + self.options.tfs_user + "," + self.options.tfs_pass + " /noprompt /recursive")
        ibslib.runCommand("tf.exe checkout \"" + self.options.base_path + "\\*.dll\" /login:" + self.options.tfs_user + "," + self.options.tfs_pass + " /noprompt /recursive")
		
    def checkinDLLs(self):
        print "Checking in DLLs..."
        tfscheckincomment = "Updating Core service contracts to " + self.options.asm_version
        ibslib.runCommand("tf.exe checkin " + self.options.base_path + "\\*.dll /login:" + self.options.tfs_user + "," + self.options.tfs_pass + \
                " /noprompt /recursive /comment:\"" + tfscheckincomment + "\"")
	
    def main(self):
        parser = OptionParser()
        parser.add_option("-c", "--core_path", action="store", type="string", dest="core_path")
        parser.add_option("-a", "--asm-version", action="store", type="string", dest="asm_version")
        parser.add_option("-b", "--bsm-version", action="store", type="string", dest="bsm_version")
        parser.add_option("-i", "--ism-version", action="store", type="string", dest="ism_version")
        parser.add_option("-p", "--base_path", action="store", type="string", dest="base_path")
        parser.add_option("-u", "--tfs-user", action="store", type="string", dest="tfs_user")
        parser.add_option("-f", "--tfs-pass", action="store", type="string", dest="tfs_pass")
        (options, args) = parser.parse_args()
        if options.core_path == None or \
            options.asm_version == None or \
            options.bsm_version == None or \
            options.ism_version == None or \
            options.base_path == None or \
            options.tfs_user == None or \
            options.tfs_pass == None:
            self.showUsage()
            exit()

        self.options = options
        self.copyCoreDLLs()
		
CopyCoreDLLs()
