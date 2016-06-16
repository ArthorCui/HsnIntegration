import subprocess, os, sys
import glob, ibslib, fileinput
from optparse import OptionParser
from shutil import *
from xml.etree.ElementTree import Element, SubElement, ElementTree

# This script applies the daily upgrade scripts to the master database, generates new create all scripts, moves the upgrade scripts to the
# approved folder and then checks in the new create all scripts to tfs.

# ****Prerequisites******
# tfs command line utility 'tf.exe' must be in PATH variable this tool is usually installed under C:\Program Files\Microsoft Visual Studio 8\Common7\IDE
# Download  pscp.exe and copy it to a path that is in the environment path (like system32) Use the following URl: http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html (This tool allows you to execute commands on a unix server)
# Sqlplus must be installed.
#

class CreateAllCheckIn():
    def __init__(self):
        self.main()

    def showUsage(self):
            print("\
Usage: createallcheckin.py [options]\n\n\
Options:\n\
    -h, --help            show this help message and exit\n")
            for schema in self.schemas:
                print("\
    --" + schema + "_SCHEMA=" + schema + "_SCHEMA")
            print("\
    -p BASE_PATH, --base_path=BASE_PATH\n\
    -u TFS_USER, --tfs-user=TFS_USER\n\
    -f TFS_PASS, --tfs-pass=TFS_PASS\n\
    -t TNS_NAME, --tns-name=TNS_NAME\n\
    -a, --approved        \n\
    -v, --verbose        \n\
    -s SYSTEM_USER, --system-user=SYSTEM_USER\n\
    -y SYSTEM_PASS, --system-pass=SYSTEM_PASS\n\
    -d DBSERVER_NAME, --dbserver-name=DBSERVER_NAME\n\
    -e DBSERVER_USER, --dbserver-user=DBSERVER_USER\n\
    -g DBSERVER_PASS, --dbserver-pass=DBSERVER_PASS\n")
            exit()

    def checkoutScripts(self):
        if self.options.verbose:
            print("\nrunning checkoutScripts() function")
            print("base_path=", self.options.base_path)
            print("tfs_user=", self.options.tfs_user)
            print("tfs_pass=", self.options.tfs_pass)
        ibslib.tfsGet(self.options.base_path + "\\*.sql", self.options.tfs_user, self.options.tfs_pass)
        ibslib.tfsCheckout(self.options.base_path + "\\*.sql", self.options.tfs_user, self.options.tfs_pass)

    def generateUpdateScripts(self):
        if self.options.verbose:
            print("\nrunning generateUpdateScripts() function")
            for schema in self.schemas:
                print(schema + "_SCHEMA=", eval("self.options." + schema + "_SCHEMA"))
    
        for schema in self.schemas:
            f = open(self.options.base_path + "\\" + schema + "_Upgrades.sql", "w")
            f.write("--start script\n")
            for script in glob.glob(self.options.base_path + "\\" + schema + "\\UpdateScripts\\*.sql"):
                f.write("@" + script + "\n")
            for script in glob.glob(self.options.base_path + "\\" + schema + "\\Data\\_PreProcss\\*.sql"):
                f.write("@" + script + "\n")
            for script in glob.glob(self.options.base_path + "\\" + schema + "\\Data\\SMP\\*.sql"):
                f.write("@" + script + "\n")
            for script in glob.glob(self.options.base_path + "\\" + schema + "\\Data\\zzPostProcess\\*.sql"):
                f.write("@" + script + "\n")
            for script in glob.glob(self.options.base_path + "\\" + schema + "\\DatabaseObjects\\Functions\\*.sql"):
                f.write("@" + script + "\n")
            for script in glob.glob(self.options.base_path + "\\" + schema + "\\DatabaseObjects\\Packages\\*.sql"):
                f.write("@" + script + "\n")
            for script in glob.glob(self.options.base_path + "\\" + schema + "\\DatabaseObjects\\Procedures\\*.sql"):
                f.write("@" + script + "\n")
            for script in glob.glob(self.options.base_path + "\\" + schema + "\\DatabaseObjects\\Triggers\\*.sql"):
                f.write("@" + script + "\n")
            for script in glob.glob(self.options.base_path + "\\" + schema + "\\DatabaseObjects\\Views\\*.sql"):
                f.write("@" + script + "\n")
            f.write("quit\n")
            f.close()
        
        try:
            input("\
**********************************************************************************\n\
NOW VERIFY THE UPGRADE SCRIPTS\n\
but do NOT run them against the master DB.\n\
Press Ctrl + C if scripts are not good!!!!\n\
Press ENTER if the scripts are correct.\n\
**********************************************************************************\n\
")
        except:
            quit()

    def runUpdateScripts(self):
            if self.options.verbose:
                print("\nrunning runUpdateScripts() function")
                for schema in self.schemas:
                    print(schema + "_SCHEMA=", eval("self.options." + schema + "_SCHEMA"))
                print("tns_name=", self.options.tns_name)
            for schema in self.schemas:
                output = ibslib.runCommand("sqlplus " + eval("self.options." + schema + "_SCHEMA") + "/" + eval("self.options." + schema + "_SCHEMA") + "@" + self.options.tns_name + \
                " @" + self.options.base_path + "\\" + schema + "_Upgrades")
                print(output)

    def generateCreateAllScripts(self):
        if self.options.verbose:
            print("\nrunning generateCreateAllScripts() function")
            print("base_path=", self.options.base_path)
            for schema in self.schemas:
                print(schema + "_SCHEMA=", eval("self.options." + schema + "_SCHEMA"))
            print("system_user=", self.options.system_user)
            print("system_pass=", self.options.system_pass)
            print("tns_name=", self.options.tns_name)
            for schema in self.schemas:
                f = open(self.options.base_path + "\\gen.sql", "w")
                f.write("begin\n\
                ibs_admin.Gen_script('" + eval("self.options." + schema + "_SCHEMA") + "', '" + schema + "');\n\
                END;\n\
                /\n\
                EXIT;")
                f.close()
                output = ibslib.runCommand("sqlplus " + self.options.system_user + "/" + self.options.system_pass + "@" + self.options.tns_name + " @" + self.options.base_path + "\\gen")
                print(output)
                output = ibslib.runCommand("pscp  -pw " + self.options.dbserver_pass + " " + self.options.dbserver_user + "@" + self.options.dbserver_name + ":/tmp/logfiles/*" + schema + "_*.sql " + self.options.base_path + "\\" + schema)
                print(output)
                copyfile(self.options.base_path + "\\" + schema + "\\ins_" + schema + "_objects.sql", self.options.base_path + "\\ins_" + schema + "_objects.sql")

                # Table CreateAll script output needs a bit of tweaking
                for line in fileinput.FileInput(self.options.base_path + "\\" + schema + "\\" + schema + "_tables.sql", inplace=1):
                    print(line.replace("(,0)", ""), end=' ')
        
    def approveScripts(self):
        if self.options.verbose:
            print("\nrunning approveScripts() function")
            print("base_path=", self.options.base_path)
            for schema in self.schemas:
                print(schema + "_SCHEMA=", eval("self.options." + schema + "_SCHEMA"))
            print("tfs_user=", self.options.tfs_user)
            print("tfs_pass=", self.options.tfs_pass)
            specialschemas = ["_PreProcess", "SMP", "zzPostProcess"]

            for schema in self.schemas:
                # *********** Move upgrade script   ******************
                ibslib.copyFiles(self.options.base_path + "\\" + schema + "\\UpdateScripts\\*.sql", self.options.base_path + "\\" + schema + "\\UpdateScripts\\Approved\\")
                for specialschema in specialschemas:
                    if os.path.exists(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema):
                        ibslib.copyFiles(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\*.sql", self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\Approved\\")

                # Rename approved scripts and check them in
                ibslib.moveFiles(self.options.base_path + "\\" + schema + "\\UpdateScripts\\Approved\\*.sql", self.options.base_path + "\\" + schema + "\\UpdateScripts\\Approved\\*.approved")
                for specialschema in specialschemas:
                    if os.path.exists(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\Approved"):
                        ibslib.moveFiles(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\Approved\\*.sql", self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\Approved\\*.approved")

                ibslib.tfsAdd(self.options.base_path + "\\" + schema + "\\UpdateScripts\\Approved\\*.approved", self.options.tfs_user, self.options.tfs_pass)
                for specialschema in specialschemas:
                    if os.path.exists(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\Approved"):
                        ibslib.tfsAdd(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\Approved\\*.approved", self.options.tfs_user, self.options.tfs_pass)

                # *******We need to undo the checkout of the upgrade scripts in order to delete them ***
                ibslib.tfsUndo(self.options.base_path + "\\" + schema + "\\UpdateScripts\\*.sql", self.options.tfs_user, self.options.tfs_pass)
                for specialschema in specialschemas:
                    if os.path.exists(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\Approved"):
                        ibslib.tfsUndo(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\*.sql", self.options.tfs_user, self.options.tfs_pass)

                ibslib.tfsDelete(self.options.base_path + "\\" + schema + "\\UpdateScripts\\*.sql", self.options.tfs_user, self.options.tfs_pass)
                for specialschema in specialschemas:
                    if os.path.exists(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\Approved"):
                        ibslib.tfsDelete(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\*.sql", self.options.tfs_user, self.options.tfs_pass)

    def checkInScripts(self):
        if self.options.verbose:
            print("\nrunning checkInScripts() function")
            print("tfs_user=", self.options.tfs_user)
            print("tfs_pass=", self.options.tfs_pass)
        tfscheckincomment = "CreateAll script: Upgrade scripts approved and new create all scripts generated"

        for schema in self.schemas:
            # Add new scripts if needed to TFS
            ibslib.tfsAdd(self.options.base_path + "\\" + schema + "\\*.sql", self.options.tfs_user, self.options.tfs_pass)

        # Check in scripts
        ibslib.tfsCheckin(self.options.base_path + "\\*.sql", tfscheckincomment, self.options.tfs_user, self.options.tfs_pass)
        ibslib.tfsCheckin(self.options.base_path + "\\*.approved", tfscheckincomment, self.options.tfs_user, self.options.tfs_pass)
        
    def main(self):
        self.getSchemaList()
        if len(self.schemas) == 0:
            print("No schemas were found in the answer file.")
            exit()
        parser = OptionParser()
        for schema in self.schemas:
            parser.add_option("--" + schema + "_SCHEMA", action="store", type="string", dest=schema + "_SCHEMA")
        parser.add_option("-p", "--base_path", action="store", type="string", dest="base_path")
        parser.add_option("-u", "--tfs-user", action="store", type="string", dest="tfs_user")
        parser.add_option("-f", "--tfs-pass", action="store", type="string", dest="tfs_pass")
        parser.add_option("-t", "--tns-name", action="store", type="string", dest="tns_name")
        parser.add_option("-a", "--approved", action="store_true", dest="approved")
        parser.add_option("-v", "--verbose", action="store_true", dest="verbose")
        parser.add_option("-s", "--system-user", action="store", type="string", dest="system_user")
        parser.add_option("-y", "--system-pass", action="store", type="string", dest="system_pass")
        parser.add_option("-d", "--dbserver-name", action="store", type="string", dest="dbserver_name")
        parser.add_option("-e", "--dbserver-user", action="store", type="string", dest="dbserver_user")
        parser.add_option("-g", "--dbserver-pass", action="store", type="string", dest="dbserver_pass")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            self.getAnswerFileSettings(options)
        else:
            self.options = options
            
        self.checkoutScripts()
        #self.generateUpdateScripts()
        #self.runUpdateScripts()
        #self.generateCreateAllScripts()
        if options.approved:
            self.approveScripts()
        self.checkInScripts()
    
    def validateOptions(self, options):
        if options.base_path == None or \
            options.tfs_user == None or \
            options.tfs_pass == None or \
            options.tns_name == None or \
            options.system_user == None or \
            options.system_pass == None or \
            options.dbserver_name == None or \
            options.dbserver_user == None or \
            options.dbserver_pass == None:
            return False
        else:
            for schema in self.schemas:
                if eval("options." + schema + "_SCHEMA") == None:
                    return False
            return True

    def getSchemaList(self):
        # If answer file exists, see if it contains values we can use.
        # Check for XML answer file first
        self.optioncmds = []
        self.schemas = []
        if os.path.exists("C:\AnswerFile\MACHINE_ANSWER_FILE.xml"):
            tree = ElementTree(file="C:\AnswerFile\MACHINE_ANSWER_FILE.xml")
            for keynode in tree.findall(".//Group[@Name='DBScripts']/Key"):
                if keynode.attrib.get("Name").endswith("_SCHEMA"):
                    self.schemas.append(keynode.attrib.get("Name").replace("_SCHEMA", ""))
                self.optioncmds.append("options." + keynode.attrib.get("Name") + "=\"" + "".join(keynode.itertext()) + "\"")
        elif os.path.exists("C:\AnswerFile\MACHINE_ANSWER_FILE.ini"):
            fi = open("C:\AnswerFile\MACHINE_ANSWER_FILE.ini", "r")
            ini_lines = fi.readlines()
            for ini_line in ini_lines:
                if not ini_line.startswith(";"):
                    if ini_line.startswith("["):
                        if ini_line.find("]") > -1:
                            section = ini_line.split("[")[1].split("]")[0]
                    else:
                        if len(ini_line.split("=")) > 1 and section == "DBScripts":
                            ini_line = ini_line.replace("\n", "")
                            keyname = ini_line.split("=")[0]
                            keyvalue = ini_line.partition("=")[2]
                            if keyname.endswith("_SCHEMA"):
                                self.schemas.append(keyname.replace("_SCHEMA", ""))
                            self.optioncmds.append("options." + keyname + "= \"" + keyvalue + "\"")
            fi.close()

    def getAnswerFileSettings(self, options):
        if len(self.optioncmds) > 0:
            for optioncmd in self.optioncmds:
                exec(optioncmd)
                                                   
        if self.validateOptions(options):
            self.options = options
        else:
            self.showUsage()

CreateAllCheckIn()
