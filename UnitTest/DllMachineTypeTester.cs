using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace UnitTest
{
    [TestFixture]
    public class DllMachineTypeTester
    {
        /// <summary>
        /// Returns true if the dll is 64-bit, false if 32-bit, and null if unknown
        /// </summary>
        /// <param name="dllPath"></param>
        /// <returns></returns>
        public static bool? UnmanagedDllIs64Bit(string dllPath)
        {
            switch (GetDllMachineType(dllPath))
            {
                case MachineType.IMAGE_FILE_MACHINE_AMD64:
                case MachineType.IMAGE_FILE_MACHINE_IA64:
                    return true;
                case MachineType.IMAGE_FILE_MACHINE_I386:
                    return false;
                default:
                    return null;
            }
        }

        public static MachineType GetDllMachineType(string dllPath)
        {
            // See http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
            // Offset to PE header is always at 0x3C.
            // The PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00,
            // followed by a 2-byte machine type field (see the document above for the enum).
            //
            FileStream fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            fs.Seek(0x3c, SeekOrigin.Begin);
            Int32 peOffset = br.ReadInt32();
            fs.Seek(peOffset, SeekOrigin.Begin);
            UInt32 peHead = br.ReadUInt32();

            if (peHead != 0x00004550) // "PE\0\0", little-endian
                throw new Exception("Can't find PE header");

            MachineType machineType = (MachineType)br.ReadUInt16();
            br.Close();
            fs.Close();
            return machineType;
        }

        public enum MachineType : ushort
        {
            IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
            IMAGE_FILE_MACHINE_AM33 = 0x1d3,
            IMAGE_FILE_MACHINE_AMD64 = 0x8664,
            IMAGE_FILE_MACHINE_ARM = 0x1c0,
            IMAGE_FILE_MACHINE_EBC = 0xebc,
            IMAGE_FILE_MACHINE_I386 = 0x14c,
            IMAGE_FILE_MACHINE_IA64 = 0x200,
            IMAGE_FILE_MACHINE_M32R = 0x9041,
            IMAGE_FILE_MACHINE_MIPS16 = 0x266,
            IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
            IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
            IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
            IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
            IMAGE_FILE_MACHINE_R4000 = 0x166,
            IMAGE_FILE_MACHINE_SH3 = 0x1a2,
            IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
            IMAGE_FILE_MACHINE_SH4 = 0x1a6,
            IMAGE_FILE_MACHINE_SH5 = 0x1a8,
            IMAGE_FILE_MACHINE_THUMB = 0x1c2,
            IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169,
        }

        [Test]
        public void dllTypeTest()
        {
            var filePath_11g = @"D:\tfs-w2k08\Integration\Main 6.2\Integration-MR22\Framework\Oracle DLL\Oracle.DataAccess.dll";
            var filePath_12c = @"D:\tfs-w2k08\Integration\Main 6.2\Integration\Framework\Oracle DLL\Oracle.DataAccess.dll";
            var filePath_ic27 = @"\\ic27\C$\Program Files (x86)\IBS Interprit\DbUpgradeUtility\DbUpgrade.exe";
            var filepath_ic08 = @"\\ic08\C$\Program Files (x86)\IBS Interprit\DbUpgradeUtility\Oracle.DataAccess.dll";
            var filepath_ic08_12c = @"\\ic08\C$\Program Files (x86)\IBS Interprit\DbUpgradeUtility\lib\oracle12c\Oracle.DataAccess.dll";
            var filepath_ic08_entriq = @"\\ic08\C$\Program Files (x86)\IBS Interprit\DbUpgradeUtility\Entriq.DataAccess.Oracle.DLL";
            var filepath_ic08_service = @"\\ic08\C$\Program Files (x86)\IBS Interprit\Integration\WindowsService\Entriq.DataAccess.dll";
            var filepath_icmain = @"D:\tfs-w2k08\Integration\Main 6.2\Integration\Core DLL\Entriq.DataAccess.Oracle.dll";

            var file_msvcrt = @"D:\app\cuir\product\11.2.0\client_1\jdk\jre\bin\msvcrt.dll";
            //Console.WriteLine(UnmanagedDllIs64Bit(filePath_11g));
            //Console.WriteLine(UnmanagedDllIs64Bit(filePath_12c));
            //Console.WriteLine(UnmanagedDllIs64Bit(filePath_ic27));
            //Console.WriteLine(UnmanagedDllIs64Bit(filepath_ic08));
            //Console.WriteLine(UnmanagedDllIs64Bit(filepath_ic08_12c));
            //Console.WriteLine(UnmanagedDllIs64Bit(filepath_ic08_entriq));
            //Console.WriteLine(UnmanagedDllIs64Bit(filepath_ic08_service));
            Console.WriteLine(UnmanagedDllIs64Bit(file_msvcrt));
        }

        [Test]
        public void DirectoryDllTest()
        {
            var folderPath = @"D:\Temp\12c_2_0_oracle\12c_x86_xcopy_dll";
            var folderPath_TNOR_29 = @"\\ic25\C$\Users\entriqeng\Desktop\temp_TNOR_29\DbUpgradeUtility";

            var s = @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\Remote Debugger\x86";
            DirectoryInfo folder = new DirectoryInfo(s);
            foreach (FileInfo file in folder.GetFiles("*.dll"))
            {
                Console.WriteLine(file.Name + ": " + UnmanagedDllIs64Bit(file.FullName));
            }
        }

    }
}
