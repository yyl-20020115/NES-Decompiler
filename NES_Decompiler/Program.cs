using System;
using System.IO;

namespace NES_Decompiler;

class Program
{
    static void Main(string[] args)
    {
        string input = args[0];
        string output = Path.ChangeExtension(input, ".asm");

        string ext = Path.GetExtension(input);

        if (ext.Equals(".NES", StringComparison.InvariantCultureIgnoreCase))
        {
            //string fileName, string romName, string ext, string outputName
            NESRomProcessor.Process(input, output);

        }

        if (ext == ".GB")
        {
            //GBRom();


        }
    }
}
