using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;


namespace NES_Decom;

class ReadNESRom
{
    /// <summary>
    /// Reads the Provided NES ROM and Text file to be iterated through and sends translations to text file.
    /// </summary>
    /// <param name="fName"></param>
    /// <param name="outputName"></param>
    /// <param name="romName"></param>
    public unsafe static void NESRom(string input_file, string output_file)
    {
        var rom_name = Path.GetFileNameWithoutExtension(input_file);

        var byteArray = File.ReadAllBytes(input_file);

        var NESheader = 16; //size of the iNES Header. 16 bytes (0x10 OR 10h)
        var defaultPRG = 16384; //default size of the PRG ROM data, increases by a multiplication of ROM[4] (list how many PRG banks there are at this area of ROM)
        var defaultCHR = 8192; //default size of the CHR ROM data, increases by a multiplication of ROM[5] (list how many CHR banks there are at this area of CHR)

        var PRGLoc = byteArray[4]; //takes this number and multiplies it by defaultPRG to get the size of the Program Data.
        var CHRLoc = byteArray[5]; //takes the number stored at this index and multiplies it by the defaultCHR to get the Character Data.
        var PRGSize = defaultPRG * PRGLoc; //get the size in bytes of the PRG 
        var CHRSize = defaultCHR * CHRLoc; //get the size in bytes of the CHR


        var flag6 = byteArray[6];
        var flag6Convert = Convert.ToString(flag6, 2);
        flag6Convert = ("00000000" + flag6Convert)[(flag6Convert.Length)..];

        var flag6Char = flag6Convert.ToCharArray();

        using var writer = new StreamWriter(output_file, false);
        writer.AutoFlush = true;

        writer.WriteLine(
            flag6Char[4] == '0'
            ? $";{rom_name} uses Horizontal Mirroring"
            : $";{rom_name} uses Vertical Mirroring");
        writer.WriteLine(
            flag6Char[3] == '0'
            ? $";{rom_name} does not use SRAM"
            : $";{rom_name} uses SRAM");
        writer.WriteLine(
            flag6Char[2] == '0'
            ? $";{rom_name} does not use a trainer"
            : $";{rom_name} uses a trainer");

        writer.WriteLine(
            (flag6Char[1] == '0')
            ? $";{rom_name} does not use four-screen VRAM"
            : $";{rom_name} uses four-screen VRAM");

        //Trying a new way to test file checking logic... Kind of. Will mature over time. 
        //I might need to take Bikini Bottom and push it somewhere else so it's not just randomly placed within ROM flag scraping code. 
        var iNESFormat = false;
        if (Convert.ToChar(byteArray[0]) == 'N' 
            && Convert.ToChar(byteArray[1]) == 'E' 
            && Convert.ToChar(byteArray[2]) == 'S' 
            && byteArray[3] == 0x1A)
        {
            writer.WriteLine(";This uses the iNES 1.0 ROM header"); 
            iNESFormat = true;
        }
        if (iNESFormat && (byteArray[7] & 0x0c) == 0x08)
        {
            writer.WriteLine(";This uses the iNES 2.0 ROM header");
        }

        writer.WriteLine(";The full size of {0} is {1}KB", rom_name, (byteArray.Length >> 10));
        writer.WriteLine(";The size of the {0} PRG ROM is {1}KB", rom_name, (byteArray.Length - (CHRSize + NESheader)) >> 10); 
        writer.WriteLine(";The size of the {0} CHR ROM is {1}KB\n", rom_name, (byteArray.Length - (PRGSize + NESheader)) >> 10); 

        fixed (byte* ToArrayBytes = byteArray)
        {
            var nes = new NESDisassemble();

            int pc = NESheader; //we want to start the PC at where the the header ends. 

            while (pc < byteArray.Length - (CHRSize + pc))  //16 -> end of PRG ROM
            {
                pc += nes.Disassemble(writer, ToArrayBytes, pc);
            }
        }
    }
}
