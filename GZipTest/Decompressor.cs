using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    class Decompressor
    {
        public static void Decompress(string inFile, string outFile)
        {
            DateTime start = DateTime.Now;
            Thread[] threadPool;
            int threadsNum = Environment.ProcessorCount;
            byte[][] dataArray = new byte[threadsNum][];
            byte[][] zDataArray = new byte[threadsNum][];
            int partSize;
            int zPartSize;
            int offsetMark = 4;
            int dataLength = offsetMark * 2;
            byte[] buffer = new byte[dataLength];
            try
            {
                using (FileStream inStream = new FileStream(inFile, FileMode.Open))
                {
                    using (FileStream outStream = File.Create(outFile))
                    {
                        Console.WriteLine("Decompressing... Please wait...");
                        while (inStream.Position < inStream.Length)
                        {
                            threadPool = new Thread[threadsNum];
                            for (int partCounter = 0; partCounter < threadsNum; partCounter++)
                            {
                                if (inStream.Position < inStream.Length)
                                {
                                    inStream.Read(buffer, 0, dataLength);         
                                    zPartSize = BitConverter.ToInt32(buffer, offsetMark); 
                                    zDataArray[partCounter] = new byte[zPartSize]; 
                                    buffer.CopyTo(zDataArray[partCounter], 0); 
                                    inStream.Read(zDataArray[partCounter], dataLength, zPartSize - dataLength);   
                                    partSize = BitConverter.ToInt32(zDataArray[partCounter], zPartSize - offsetMark);  
                                    dataArray[partCounter] = new byte[partSize];    
                                    threadPool[partCounter] = new Thread(DecompressMachine);
                                    threadPool[partCounter].Start(partCounter);
                                }
                            }
                            for (int partCounter = 0; partCounter < threadsNum; partCounter++)
                            {
                                if (threadPool[partCounter] != null)
                                {
                                    threadPool[partCounter].Join();
                                    outStream.Write(dataArray[partCounter], 0, dataArray[partCounter].Length);
                                }
                            }
                        }
                        void DecompressMachine(object obj)
                        {
                            int thread = (int)obj;
                            using (MemoryStream mStream = new MemoryStream(zDataArray[thread]))
                            {
                                using (GZipStream unzipStream = new GZipStream(mStream, CompressionMode.Decompress))
                                {
                                    unzipStream.Read(dataArray[thread], 0, dataArray[thread].Length);
                                }
                            }
                            Console.Write("|");
                        }
                        DateTime end = DateTime.Now;
                        Console.WriteLine("\nFile {0} is decompressed!", inFile);
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
