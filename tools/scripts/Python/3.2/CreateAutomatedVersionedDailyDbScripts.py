import subprocess, os, sys
import glob, ibslib, time
from optparse import OptionParser
from shutil import *
from xml.etree.ElementTree import Element, SubElement, ElementTree
from functools import cmp_to_key

class CreateAutomatedVersionedDailyDbScripts():
    def __init__(self):
        self.main()

    def showUsage(self):
        print("\
Usage: CreateAutomatedVersionedDailyDbScripts.py [options]\n\n\
Options:\n\
  -h, --help            show this help message and exit\n\
  -p BASE_PATH, --base_path=BASE_PATH\n\
  -u TFS_USER, --tfs-user=TFS_USER\n\
  -f TFS_PASS, --tfs-pass=TFS_PASS\n\
  --versionxmlfile=(path to version xml file)\n\
  --schema-list=comma-delimited list of schemas to include\n\
  -v, --verbose\n\
  --is-icmain-client=true/false\n\
  --is-icmain=true/false\n\
  --version=(version) note: will override versionxmlfile")
        exit()

    def createVersionedDbScripts(self):
        if self.options.verbose:
            print("\nrunning createVersionedDbScripts() function")
            print("tfs_user=", self.options.tfs_user)
            print("tfs_pass=", self.options.tfs_pass)
            print("base_path=", self.options.base_path)

        self.schemas = self.options.schemalist.split(',')
        specialschemas = ["_PreProcess", "SMP", "zzPostProcess"]
        databaseobjectfolders = ["ForeignKeys", "Functions", "Packages", "Procedures", "Triggers", "Views", "Tables", "Sequences", "Types"]
        self.header = "/*-------------------------------------------------------------------------\n\
Version:            ${Version}\n\
--------------------------------------------------------------------------*/"
        self.icmain_header = "/*-------------------------------------------------------------------------\n\
IC Main Version:    ${Version}\n\
--------------------------------------------------------------------------*/"
        self.icmain_client_header = "/*-------------------------------------------------------------------------\n\
Client Version:     ${Version}\n\
IC Main Version:    ${ClientICMainVersion}\n\
--------------------------------------------------------------------------*/"
        
        self.viewcommentsql = "\n\nCOMMENT ON TABLE ${View} IS '" + self.header + "';\n/\n"
        self.tablecommentsql = "\n\nCOMMENT ON TABLE ${Table} IS '" + self.header + "${TableComment}';\n/\n"
        self.icmain_viewcommentsql = "\n\nCOMMENT ON TABLE ${View} IS '" + self.icmain_header + "';\n/\n"
        self.icmain_client_viewcommentsql = "\n\nCOMMENT ON TABLE ${View} IS '" + self.icmain_client_header + "';\n/\n"
        self.icmain_tablecommentsql = "\n\nCOMMENT ON TABLE ${Table} IS '" + self.icmain_header + "${TableComment}';\n/\n"
        self.icmain_client_tablecommentsql = "\n\nCOMMENT ON TABLE ${Table} IS '" + self.icmain_client_header + "${TableComment}';\n/\n"
        self.new_icmain_version = ""
        self.client_icmain_version = ""

        if self.options.is_icmain:
            self.new_icmain_version = self.new_db_version.split(".")[0].rjust(2, "0") + "." + self.new_db_version.split(".")[1].rjust(2, "0") + "." + self.new_db_version.split(".")[2].rjust(2, "0") + "." + self.new_db_version.split(".")[3].rjust(2, "0")
        else:
            ibslib.makeDir(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.new_db_version)
            for specialschema in specialschemas:
                ibslib.makeDir(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + specialschema)
            
        for schema in self.schemas:
            if self.options.is_icmain:
                # Create versioned schema folders for today's build
                if len(glob.glob(self.options.base_path + "\\" + schema + "\\UpdateScripts\\" + self.new_icmain_version)) == 0:
                    ibslib.makeDir(self.options.base_path + "\\" + schema + "\\UpdateScripts\\" + self.new_icmain_version)
                if len(glob.glob(self.options.base_path + "\\" + schema + "\\UpdateScripts\\" + self.new_icmain_version + "\\DatabaseObjects")) == 0:
                    ibslib.makeDir(self.options.base_path + "\\" + schema + "\\UpdateScripts\\" + self.new_icmain_version + "\\DatabaseObjects")
                for databaseobjectfolder in databaseobjectfolders:
                    if len(glob.glob(self.options.base_path + "\\" + schema + "\\UpdateScripts\\" + self.new_icmain_version + "\\DatabaseObjects\\" + databaseobjectfolder)) == 0:
                        ibslib.makeDir(self.options.base_path + "\\" + schema + "\\UpdateScripts\\" + self.new_icmain_version + "\\DatabaseObjects\\" + databaseobjectfolder)
            else:
                # Create versioned schema folders for today's build
                ibslib.makeDir(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.new_db_version + "\\" + schema)
                for specialschema in specialschemas:
                    ibslib.makeDir(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + specialschema + "\\" + schema)
                
                # Copy approved SQL scripts into today's build folders
                ibslib.copyFiles(self.options.base_path + "\\" + schema + "\\UpdateScripts\\Approved\\*.approved", self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.new_db_version + "\\" + schema + "\\")
                for specialschema in specialschemas:
                    ibslib.copyFiles(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\Approved\\*.approved", self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + specialschema + "\\" + schema + "\\")

                # Rename .Approved to .SQL
                ibslib.moveFiles(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.new_db_version + "\\" + schema + "\\*.approved", self.options.base_path + "\\..\Setup\\DbUpgrade\\DBScripts\\" + self.new_db_version + "\\" + schema + "\\*.SQL")
                for specialschema in specialschemas:
                    ibslib.moveFiles(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + specialschema + "\\" + schema + "\\*.approved", self.options.base_path + "\\..\Setup\\DbUpgrade\\DBScripts\\" + specialschema + "\\" + schema + "\\*.SQL")

                # Make sure at least one .SQL file exists for each schema
                if len(glob.glob(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.new_db_version + "\\" + schema + "\\*.sql")) == 0:
                    fi = open(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.new_db_version + "\\" + schema + "\\blank.sql", "w")
                    fi.close()
                for specialschema in specialschemas:
                    if len(glob.glob(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + specialschema + "\\" + schema + "\\*.sql")) == 0:
                        fi = open(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + specialschema + "\\" + schema + "\\blank.sql", "w")
                        fi.close()
                    
            # Copy database objects
            for databaseobjectfolder in databaseobjectfolders:
                # determine the dbscript folder
                dbscriptpath = self.options.base_path + "\\" + schema + "\\DatabaseObjects\\" + databaseobjectfolder 

                # check out everything in the dbscript folder                
                ibslib.tfsCheckout( dbscriptpath, self.options.tfs_user, self.options.tfs_pass)
                
                # Determine output folder
                if self.options.is_icmain:
                    destscriptfolder = self.options.base_path + "\\" + schema + "\\UpdateScripts\\" + self.new_icmain_version + "\\DatabaseObjects\\" + databaseobjectfolder
                else:
                    destscriptfolder = self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\_PreProcess\\" + schema
            
                # check out everything in the destination folder.
                ibslib.tfsCheckout( destscriptfolder, self.options.tfs_user, self.options.tfs_pass)
                
                for dbscript in glob.glob(dbscriptpath + "\\*.sql"):
                    # Add comments to script
                    tokens,values = self.addComments(dbscript, databaseobjectfolder)

                    destscriptpath = destscriptfolder + "\\" + dbscript.rpartition("\\")[2]
                    
                    # Perform token replacement on script and copy to destination folder
                    replaceresultsuccess = ibslib.replacetokens(tokens, values, dbscript, destscriptpath)
                    
                    if not replaceresultsuccess:
                        print("Error running ReplaceTokens for database object '%s'." % dbscript)
                        exit(1)

                # Undo original script (template)
                ibslib.tfsUndo(dbscriptpath, self.options.tfs_user, self.options.tfs_pass)

            if self.options.is_icmain_client:
                # Copy IC Main scripts to setup folder
                icmain_folders = glob.glob(self.options.base_path + "\\..\\IC Main Scripts\\" + schema + "\\*")
                if len(icmain_folders) > 0:
                    for icmain_folder in icmain_folders:
                        # Check out destination scripts if they exist
                        tempDestPath = self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.new_db_version + "\\" + schema 
                        ibslib.tfsCheckout( tempDestPath, self.options.tfs_user, self.options.tfs_pass)
                        ibslib.copyFiles(icmain_folder + "\\*.*", tempDestPath + "\\")
                        
                    # We need to get DatabaseObjects from most recent IC Main folder,
                    # so sort the list to determine the most recent version.
                    icmain_folder_versions = []
                    for icmain_folder in icmain_folders:
                        icmain_folder_versions.append(icmain_folder.rpartition("\\")[2])
                    icmain_folder_versions = sorted(icmain_folder_versions, key=cmp_to_key(ibslib.compareVersions))
                    icmain_folder_versions.reverse()
                    latest_icmain_folder = self.options.base_path + "\\..\\IC Main Scripts\\" + schema + "\\" + icmain_folder_versions[0]
                    
                    # Copy IC Main DB objects to setup folder
                    for databaseobjectfolder in databaseobjectfolders:
                        temp_latest_icmain_database_object_folder = latest_icmain_folder + "\\DatabaseObjects\\" + databaseobjectfolder
                        # Check out script
                        ibslib.tfsCheckout( temp_latest_icmain_database_object_folder, self.options.tfs_user, self.options.tfs_pass )

                        # Determine output path
                        destscriptfolder = self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\_PreProcess\\" + schema 
                        
                        # Check out destination scripts if it exists
                        ibslib.tfsCheckout(destscriptfolder, self.options.tfs_user, self.options.tfs_pass)

                        for dbscript in glob.glob(temp_latest_icmain_database_object_folder + "\\*.sql"):
                            # Add comments to script
                            tokens,values = self.addComments(dbscript, databaseobjectfolder)
                            
                            # Determine output path
                            destscriptpath = destscriptfolder + "\\" + dbscript.rpartition("\\")[2]
                            
                            # Perform token replacement on script and copy to destination folder
                            replaceresultsuccess = ibslib.replacetokens(tokens, values, dbscript, destscriptpath)
                            
                        # Undo original script (template)
                        ibslib.tfsUndo(temp_latest_icmain_database_object_folder, self.options.tfs_user, self.options.tfs_pass)
                    
        if not self.options.is_icmain:
            # TFS Add Files to installer area
            ibslib.tfsAdd(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + self.new_db_version + "\\*.*", self.options.tfs_user, self.options.tfs_pass)
            for specialschema in specialschemas:
                for schema in self.schemas:
                    ibslib.tfsAdd(self.options.base_path + "\\..\\Setup\\DbUpgrade\\DBScripts\\" + specialschema + "\\" + schema + "\\*.sql", self.options.tfs_user, self.options.tfs_pass)
            
            ibslib.tfsCheckin(self.options.base_path + "\\..\Setup\\DbUpgrade\\DBScripts\\*.*", "CreateAll script: Checking in moved scripts", self.options.tfs_user, self.options.tfs_pass)

            for schema in self.schemas:
                # Remove old SQL files from TFS
                ibslib.tfsDelete(self.options.base_path + "\\" + schema + "\\UpdateScripts\\Approved\\*.approved", self.options.tfs_user, self.options.tfs_pass)
                for specialschema in specialschemas:
                    ibslib.tfsDelete(self.options.base_path + "\\" + schema + "\\Data\\" + specialschema + "\\Approved\\*.approved", self.options.tfs_user, self.options.tfs_pass)
                
            ibslib.tfsCheckin(self.options.base_path, "CreateAll script: Deleting old approved scripts", self.options.tfs_user, self.options.tfs_pass)

    def addComments(self, dbscript, databaseobjectfolder):

        if dbscript.rpartition("\\")[2].count( "." ) == 3:
            # Some of our db files now store the file with funny names now so that the filenames will sort in the order they need to execute.
            # The object name is still in the filename, but we have to pick it out differently than before.
            # example: D:\tfs\Integration\Main 6.2\Integration\CreateCI_DB\CI_CENTRAL\DatabaseObjects\Tables\000.50.APPLICATION_SETTINGS.sql
            # The desired object is simply APPLICATION_SETTINGS.
            objectname = dbscript.rpartition("\\")[2].partition(".")[2].partition(".")[2].rpartition(".")[0].upper()
        else:
            # if the filename does NOT have three "." then assume it's in the file name minus the path and extension.
            # example:  d:\folder1\folder2\MyTable.sql = object name MyTable.
            objectname = dbscript.rpartition("\\")[2].rpartition(".")[0].upper()

        # Get properties of TFS item
#        properties = ibslib.tfsProperties(dbscript, self.options.tfs_user, self.options.tfs_pass).split('\r\n')

        changeset = ""
        lastmodifieddate = ""
        lastmodifiedby = ""
        checkincomment = ""

#        for property in properties:
#            if property.split(':')[0].__contains__('Changeset'):
#                changeset = property.partition(':')[2].lstrip(' ').rstrip('\r\n')
#            elif property.split(':')[0].__contains__('Last modified'):
#                lastmodifieddate = property.partition(':')[2].lstrip(' ').rstrip('\r\n')
        
        # Get properties of latest TFS changeset item is associated with
##        properties = ibslib.tfsChangeset(changeset, self.options.tfs_user, self.options.tfs_pass).split('\r\n')
##        for i in range(len(properties)):
##            property = properties[i]
##            if property.split(':')[0].startswith('User'):
##                lastmodifiedby = property.split(':')[1].lstrip(' ').rstrip('\r\n')
##            elif property.split(':')[0].startswith('Comment'):
##                checkincomment = properties[i + 1].lstrip(' ').rstrip('\r\n')
##                break
            
        tokens = ["${Version}", "${ClientICMainVersion}", "${Changeset}", "${LastModifiedDate}"]
        values = ["", "", "", ""]
        values[0] = self.new_db_version
        if self.options.is_icmain_client:
            if self.client_icmain_version == "":
                self.client_icmain_version = ibslib.getVersionNumber(self.options.base_path + "\\..\\IC Main DLL\\Integration.Main.Common.dll")
            values[1] = self.client_icmain_version
        else:
            values[1] = ""
        values[2] = changeset
        values[3] = lastmodifieddate
        
        # Add comment to database object
        #ibslib.tfsCheckout(dbscript, self.options.tfs_user, self.options.tfs_pass)
        fi = open(dbscript, "r")
        lines = fi.readlines()
        fi.close()
        fi = open(dbscript, "w")
        
        # Determine type of object and add comment accordingly
        # this is now set above -- objectname = dbscript.rpartition("\\")[2].rpartition(".")[0].upper()
        tablecomment = ""
        tabledrop = False
        for i in range(len(lines)):
            line = lines[i]
            
            # If the table script is a drop script, no need to add comment
            if line.upper().__contains__('DROP TABLE'):
                tabledrop = True
            
            # Write table script content
            if databaseobjectfolder == "Tables":
                fi.write(line)
                
            if databaseobjectfolder == "Packages" and (line.upper().rstrip('\r\n').endswith("AS") or line.upper().rstrip('\r\n').endswith("IS")):
                fi.write(line)
                if self.options.is_icmain:
                    fi.write(self.icmain_header + "\n")
                elif self.options.is_icmain_client:
                    fi.write(self.icmain_client_header + "\n")
                else:
                    fi.write(self.header + "\n")
            elif (databaseobjectfolder == "Triggers" or databaseobjectfolder == "Functions" or databaseobjectfolder == "Procedures") and line.upper().lstrip(" ").startswith("BEGIN"):
                fi.write(line)
                if self.options.is_icmain:
                    fi.write(self.icmain_header + "\n")
                elif self.options.is_icmain_client:
                    fi.write(self.icmain_client_header + "\n")
                else:
                    fi.write(self.header + "\n")
            elif databaseobjectfolder == "Tables" and line.upper().lstrip(" ").startswith("COMMENT ON TABLE") and not self.options.is_icmain_client:
                tablecomment = line.split("'")[1]
            elif databaseobjectfolder != "Tables":
                # Write all other database objects (sequences, types, etc.)
                fi.write(line)
                
        if databaseobjectfolder == "Views":
            if self.options.is_icmain:
                fi.write(self.icmain_viewcommentsql.replace("${View}", objectname))
            elif self.options.is_icmain_client:
                fi.write(self.icmain_client_viewcommentsql.replace("${View}", objectname))
            else:
                fi.write(self.viewcommentsql.replace("${View}", objectname))
        elif databaseobjectfolder == "Tables":
            if tablecomment != "":
                tablecomment = "\n" + tablecomment
            if self.options.is_icmain:
                fi.write(self.icmain_tablecommentsql.replace("${Table}", objectname).replace("${TableComment}", tablecomment))
            elif self.options.is_icmain_client:
                fi.write(self.icmain_client_tablecommentsql.replace("${Table}", objectname).replace("${TableComment}", tablecomment))
            elif not tabledrop:
                fi.write(self.tablecommentsql.replace("${Table}", objectname).replace("${TableComment}", tablecomment))
        fi.close()
        
        return (tokens,values)
        
    def getDataFromVersionFile(self):
        if not self.options.version == None:
            self.new_db_version = self.options.version
        else:
            if os.path.exists(self.options.versionxmlfile):
                tree = ElementTree(file=self.options.versionxmlfile)
                LastVersionNode = tree.find(".//LastVersion")
                if LastVersionNode == None:
                      print("LastVersion element not found in version XML file!")
                      exit(1)
                lastVersionText = "".join(LastVersionNode.itertext())

                newmajor = lastVersionText.split('.')[0]
                newminor = lastVersionText.split('.')[1]
                newrevision = lastVersionText.split('.')[2]
                newbuild = lastVersionText.split('.')[3]
                newversion = newmajor + "." + newminor + "." + newrevision + "." + newbuild

                self.new_db_version = newversion
            else:
                print("Version XML file not found!")
                exit(1)

    def main(self):
        parser = OptionParser()
        parser.add_option("-p", "--base_path", action="store", type="string", dest="base_path")
        parser.add_option("-u", "--tfs-user", action="store", type="string", dest="tfs_user")
        parser.add_option("-f", "--tfs-pass", action="store", type="string", dest="tfs_pass")
        parser.add_option("--versionxmlfile", action="store", type="string", dest="versionxmlfile")
        parser.add_option("--schema-list", action="store", type="string", dest="schemalist")
        parser.add_option("-v", "--verbose", action="store_true", dest="verbose")
        parser.add_option("--is-icmain-client", action="store_true", dest="is_icmain_client")
        parser.add_option("--is-icmain", action="store_true", dest="is_icmain")
        parser.add_option("--version", action="store", type="string", dest="version")
        (options, args) = parser.parse_args()
        if (self.validateOptions(options) == False):
            print("Invalid options passed!")
            exit(1)
        else:
            self.options = options

        self.getDataFromVersionFile()

        startTime = time.localtime()
        self.createVersionedDbScripts()
        completed = time.localtime()

        print("\nStarted: " + time.asctime( startTime ))
        print("\nCompleted: " + time.asctime( completed ))

    def validateOptions(self, options):
        if options.base_path == None or \
            options.tfs_user == None or \
            options.tfs_pass == None or \
            (options.versionxmlfile == None and options.version == None) or \
            options.schemalist == None:
            return False
        else:
            return True

CreateAutomatedVersionedDailyDbScripts()
