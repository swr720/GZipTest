using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    class Compressor
    {
        public static void Compress(string inFile, string outFile)
        {
            DateTime start = DateTime.Now;
            Thread[] threadPool;
            int threadsNum = Environment.ProcessorCount;
            byte[][] dataArray = new byte[threadsNum][];
            byte[][] zDataArray = new byte[threadsNum][];
            int partSize = 1000000;
            int offsetMark = 4;
            try
            {
                using (FileStream inStream = new FileStream(inFile, FileMode.Open))
                {
                    using (FileStream outStream = File.Create(outFile))
                    {
                        Console.WriteLine("Compressing... Please wait...");
                        while (inStream.Position < inStream.Length)
                        {
                            threadPool = new Thread[threadsNum];
                            for (int partCounter = 0; partCounter < threadsNum; partCounter++)
                            {
                                if (inStream.Position < inStream.Length)
                                {
                                    if (partSize > inStream.Length - inStream.Position)
                                    {
                                        partSize = (int)(inStream.Length - inStream.Position);
                                    }
                                    dataArray[partCounter] = new byte[partSize];     
                                    inStream.Read(dataArray[partCounter], 0, partSize);   
                                    threadPool[partCounter] = new Thread(CompressMachine);
                                    threadPool[partCounter].Start(partCounter);
                                }
                            }
                            for (int partCounter = 0; partCounter < threadsNum; partCounter++)
                            {
                                if (threadPool[partCounter] != null)
                                {
                                    threadPool[partCounter].Join();
                                    outStream.Write(zDataArray[partCounter], 0, zDataArray[partCounter].Length);
                                }
                            }
                        }
                        void CompressMachine(object obj)
                        {
                            int thread = (int)obj;
                            using (MemoryStream mStream = new MemoryStream(dataArray[thread].Length))
                            {
                                using (GZipStream zipStream = new GZipStream(mStream, CompressionMode.Compress))
                                {
                                    zipStream.Write(dataArray[thread], 0, dataArray[thread].Length);
                                }
                                zDataArray[thread] = mStream.ToArray();
                                BitConverter.GetBytes(zDataArray[thread].Length).CopyTo(zDataArray[thread], offsetMark);
                            }
                            Console.Write("|");
                        }
                        DateTime end = DateTime.Now;
                        Console.WriteLine("\nFile {0} is compressed! \nOriginal size: {1} bytes \nCompressed size: {2} bytes",
                            inFile, inStream.Length.ToString(), outStream.Length.ToString());
                        Console.WriteLine("Passed time: {0}", end - start);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
