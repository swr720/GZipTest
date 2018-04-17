using System;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string inFile;
            string outFile;
            try
            {
                if (args[0].ToLower() == "compress")
                {
                    inFile = args[1];
                    outFile = args[2];
                    Compressor.Compress(inFile, outFile);
                }
                else if (args[0].ToLower() == "decompress")
                {
                    inFile = args[1];
                    outFile = args[2];
                    Decompressor.Decompress(inFile, outFile);
                }
                else
                {
                    Console.WriteLine("Incorrect input! Command example: \nGZipTest.exe [compress/decompress] [path to source file] [path to result file]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }
    }
}