import subprocess, os, sys
import glob, ibslib
from optparse import OptionParser
from shutil import *
from xml.etree.ElementTree import Element, SubElement, ElementTree

class CreateVersionedDailyDbScripts():
    def __init__(self):
        self.main()

    def showUsage(self):
        print("\
Usage: CreateVersionedDailyDbScripts.py [options]\n\n\
Options:\n\
  -h, --help            show this help message and exit\n")
        for schema in self.schemas:
            print("\
    --" + schema + "_SCHEMA=" + schema + "_SCHEMA")
        print("\
  -p BASE_PATH, --base_path=BASE_PATH\n\
  -u TFS_USER, --tfs-user=TFS_USER\n\
  -f TFS_PASS, --tfs-pass=TFS_PASS\n\
  -v, --verbose        \n\
  -d NEW_DB_VERSION, --new-db-version=NEW_DB_VERSION\n")
        exit()

    def createVersionedDbScripts(self):
        if self.options.verbose:
            print("\nrunning createVersionedDbScripts() function")
            print("tfs_user=", self.options.tfs_user)
            print("tfs_pass=", self.options.tfs_pass)
            print("base_path=", self.options.base_path)
            print("new_db_version=", self.options.new_db_version)
            
        ibslib.makeDir(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.options.new_db_version)
        for schema in self.schemas:
            targetschema = schema
            if schema == "QD":
                targetschema = "CI_QUEUEDATA"
                
            # Create versioned schema folders for today's build
            ibslib.makeDir(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.options.new_db_version + "\\" + targetschema)
            
            # Copy approved SQL scripts into today's build folders
            ibslib.copyFiles(self.options.base_path + "\\" + schema + "\\UpdateScripts\\Approved\\*.approved", self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.options.new_db_version + "\\" + targetschema + "\\")

            # Rename .Approved to .SQL
            ibslib.moveFiles(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.options.new_db_version + "\\" + targetschema + "\\*.approved", self.options.base_path + "\\..\Setup\\DbUpgrade\\DBScripts\\" + self.options.new_db_version + "\\" + targetschema + "\\*.SQL")

            # Make sure at least one .SQL file exists for each schema
            if len(glob.glob(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.options.new_db_version + "\\" + targetschema + "\\*.sql")) == 0:
                fi = open(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.options.new_db_version + "\\" + targetschema + "\\blank.sql", "w")
                fi.close()
        
        # TFS Add Files to installer area
        ibslib.tfsAdd(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.options.new_db_version + "\\*.sql", self.options.tfs_user, self.options.tfs_pass)
        ibslib.tfsCheckin(self.options.base_path + "\\..\Setup\\DbUpgrade\\DBScripts\\" + self.options.new_db_version + "\\*.sql", "CreateAll script: Checking in moved scripts", self.options.tfs_user, self.options.tfs_pass)

        for schema in self.schemas:
            # Remove old SQL files from TFS
            ibslib.tfsDelete(self.options.base_path + "\\" + schema + "\\UpdateScripts\\Approved\\*.approved", self.options.tfs_user, self.options.tfs_pass)
            ibslib.tfsCheckin(self.options.base_path + "\\" + schema + "\\UpdateScripts\\Approved", "CreateAll script: Deleting old approved scripts", self.options.tfs_user, self.options.tfs_pass)

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
        parser.add_option("-d", "--new-db-version", action="store", type="string", dest="new_db_version")
        parser.add_option("-v", "--verbose", action="store_true", dest="verbose")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            self.getAnswerFileSettings(options)
        else:
            self.options = options
            
        self.createVersionedDbScripts()

    def validateOptions(self, options):
        if options.base_path == None or \
            options.tfs_user == None or \
            options.tfs_pass == None or \
            options.new_db_version == None:
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
            tree = ET.parse("C:\AnswerFile\MACHINE_ANSWER_FILE.xml")
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

CreateVersionedDailyDbScripts()
