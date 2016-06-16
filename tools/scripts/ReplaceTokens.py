################################################################################
# File: ReplaceTokens.py
# Purpose: Perform token replacement in a template file
# History:
# 01/02/2008 TJohnson - Created.
################################################################################
import sys, os, subprocess
from optparse import OptionParser

class ReplaceTokens():
    def __init__(self):
        self.main()

    def showUsage(self):
        print "\
Usage: ReplaceTokens.py [options]\n\n\
Options:\n\
  -h, --help            show this help message and exit\n\
  -m MACHINE_ANSWER_FILE, --machine_answer_file=MACHINE_ANSWER_FILE.ini\n\
  -t TEMPLATE_FILE, --template-file=configfile.xml.template\n\
  -o OUTPUT_FILE, --output-file=configfile.xml\n"
  
    def performTokenReplacement(self, inifile, templatefile, outfile):
        # Check to make sure ini file exists
        try:
            fi = open(inifile, "r")
        except:
            print "Error: unable to open ini file!"
            return False
        
        # Check to make sure template file exists
        try:
            ft = open(templatefile, "r")
        except:
            print "Error: unable to open template file!"
            return False
        
        # Perform token replacement
        outfile_data = ""
        try:
            template_lines = ft.readlines()
            ini_lines = fi.readlines()
            for template_line in template_lines:
                template_line = template_line.replace("\n", "")
                for ini_line in ini_lines:
                    if not ini_line.startswith(";"):
                        if ini_line.startswith("["):
                            if ini_line.find("]") > -1:
                                section = ini_line.split("[")[1].split("]")[0]
                        else:
                            if len(ini_line.split("=")) > 1:
                                ini_line = ini_line.replace("\n", "")
                                template_line = template_line.replace("${" + section + "/" + ini_line.split("=")[0] + "}", ini_line.partition("=")[2])
                outfile_data += template_line + "\n"
            fi.close()
            ft.close()
        except:
            print "Error: unable to perform token replacement."
            return False
        
        # Ensure we have output data to write
        if len(outfile_data) == 0:
            print "Error: output file data is blank."
            return False
        
        try:
            # Write output data
            fo = open(outfile, "w")
            fo.write(outfile_data)
            fo.close()
        except (IOError, os.error), why:
            print "Error writing output data: %s" % str(why)
            
        return True

    def main(self):
        parser = OptionParser()
        parser.add_option("-m", "--machine_answer_file", action="store", type="string", dest="machine_answer_file")
        parser.add_option("-t", "--template_file", action="store", type="string", dest="template_file")
        parser.add_option("-o", "--output_file", action="store", type="string", dest="output_file")
        (options, args) = parser.parse_args()
        if options.machine_answer_file == None or \
            options.template_file == None or \
            options.output_file == None:
            self.showUsage()
            exit()

        self.options = options
        self.performTokenReplacement(options.machine_answer_file, options.template_file, options.output_file)

ReplaceTokens()
