using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace FileCachingSystem
{
    public class FileCacher
    {
        public static void ReadCacheFile(string path)
        {
            using (FileStream fileStream = File.OpenRead(path))
            {
                // FSTR (File Start) 
                // FEND (File End)


                byte[] fileStartChunkBuffer = new byte[4];
                fileStream.Read(fileStartChunkBuffer, 0, 4);

                string fileStartChunk = Encoding.ASCII.GetString(fileStartChunkBuffer);

                if (fileStartChunk != "FSTR")
                {
                    throw new InvalidDataException("File doesn't contain Start Chunk, which mean it's a corrupted cache file.");
                }

                bool endOfFile = false;

                while (!endOfFile)
                {
                    if (TryReadChunk(fileStream, out var cacheChunk))
                    {
                        if (!cacheChunk.encounteredEndOfFile)
                        {
                            Debug.Log(cacheChunk.GetID());
                            Debug.Log(cacheChunk.GetRef());
                            Debug.Log(cacheChunk.GetHash());
                        }
                        else
                        {
                            endOfFile = true;
                        }
                    }
                }
            }

        }

        public static bool TryReadChunk(FileStream fileStream, out CacheChunkGroup chunkGroup)
        {
            chunkGroup = new CacheChunkGroup();

            // Try Read Cached File's Chunk
            // Cached File Chunks -

            byte[] startChunkBuffer = new byte[4];
            fileStream.Read(startChunkBuffer, 0, 4);
            string startChunk = Encoding.ASCII.GetString(startChunkBuffer);
            if (startChunk != "CSTR")
            {
                if (startChunk == "FEND")
                {
                    chunkGroup.encounteredEndOfFile = true;
                    return true;
                }
                return false;
            }

            // IDLT (ID Length) - 4 Bytes 
            byte[] idBuffer = new byte[4];
            fileStream.Read(idBuffer, 0, 4);
            uint idLength = BitConverter.ToUInt32(idBuffer, 0);

            // IDST (ID String) - IDLT Bytes
            byte[] idStrBuffer = new byte[idLength];
            fileStream.Read(idStrBuffer, 0, (int)idLength);
            string id = Encoding.ASCII.GetString(idStrBuffer);
            chunkGroup.id = id;

            // RFLT (Ref Length) - 4 Bytes
            byte[] refLengthBuffer = new byte[4];
            fileStream.Read(refLengthBuffer, 0, 4);
            uint refLength = BitConverter.ToUInt32(refLengthBuffer, 0);
            chunkGroup.refLength = refLength;

            // HSLT (Hash Length) - 4 Bytes
            byte[] hashLengthBuffer = new byte[4];
            fileStream.Read(hashLengthBuffer, 0, 4);
            uint hashLength = BitConverter.ToUInt32(hashLengthBuffer, 0);
            chunkGroup.hashLength = hashLength;

            // DTLT (Data Length) - 4 Bytes
            byte[] dataLengthBuffer = new byte[4];
            fileStream.Read(dataLengthBuffer, 0, 4);
            uint dataLength = BitConverter.ToUInt32(dataLengthBuffer, 0);
            chunkGroup.dataLength = dataLength;

            int totalTrioChunksLength = (int)(refLength + hashLength + dataLength);

            byte[] trioChunksBuffer = new byte[totalTrioChunksLength];
            fileStream.Read(trioChunksBuffer, 0, totalTrioChunksLength);
            chunkGroup.trioChunksBuffer = trioChunksBuffer;

            byte[] endChunkBuffer = new byte[4];
            fileStream.Read(endChunkBuffer, 0, 4);
            string endChunk = Encoding.ASCII.GetString(endChunkBuffer);

            if (endChunk != "CEND")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void WriteCache()
        {
            FileStream fileStream = null;
            if (File.Exists("E://cache.fcs"))
                fileStream = File.OpenRead("E://cache.fcs"); ;
            
            /*______________THIS IS FOR TEST ONLY_____________*/
            string id = "-MswQsa2hOp1dsa2";
            string reference = "https://example.com/httmp.jpeg";
            string hash = "hsWWQagof21asW31gas-";
            byte[] dataBuffer = new byte[]
            {
            0x30, 0x32, 0x32, 0x32, 0xe7, 0x30, 0xaa, 0x7f, 0x32, 0x32, 0x32, 0x32, 0xf9, 0x40, 0xbc, 0x7f,
            0x03, 0x03, 0x03, 0x03, 0xf6, 0x30, 0x02, 0x05, 0x03, 0x03, 0x03, 0x03, 0xf4, 0x30, 0x03, 0x06,
            0x32, 0x32, 0x32, 0x32, 0xf7, 0x40, 0xaa, 0x7f, 0x32, 0xf2, 0x02, 0xa8, 0xe7, 0x30, 0xff, 0xff,
            0x03, 0x03, 0x03, 0xff, 0xe6, 0x40, 0x00, 0x0f, 0x00, 0xff, 0x00, 0xaa, 0xe9, 0x40, 0x9f, 0xff,
            0x5b, 0x03, 0x03, 0x03, 0xca, 0x6a, 0x0f, 0x30, 0x03, 0x03, 0x03, 0xff, 0xca, 0x68, 0x0f, 0x30,
            0xaa, 0x94, 0x90, 0x40, 0xba, 0x5b, 0xaf, 0x68, 0x40, 0x00, 0x00, 0xff, 0xca, 0x58, 0x0f, 0x20,
            0x00, 0x00, 0x00, 0xff, 0xe6, 0x40, 0x01, 0x2c, 0x00, 0xff, 0x00, 0xaa, 0xdb, 0x41, 0xff, 0xff,
            0x00, 0x00, 0x00, 0xff, 0xe8, 0x40, 0x01, 0x1c, 0x00, 0xff, 0x00, 0xaa, 0xbb, 0x40, 0xff, 0xff,
            };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                if (fileStream != null)
                {
                    byte[] existingFileBuffer = new byte[fileStream.Length - 4];
                    fileStream.Read(existingFileBuffer, 0, existingFileBuffer.Length);
                    memoryStream.Write(existingFileBuffer, 0, existingFileBuffer.Length);
                    fileStream.Close();
                    fileStream.Dispose();
                }
                else
                {
                    // new file should have file start chunk
                    byte[] fileStartChunkBuffer = Encoding.ASCII.GetBytes("FSTR");
                    memoryStream.Write(fileStartChunkBuffer, 0, fileStartChunkBuffer.Length);
                }

                
                byte[] startChunkBuffer = Encoding.ASCII.GetBytes("CSTR");

                byte[] idBuffer = Encoding.ASCII.GetBytes(id);
                byte[] idLengthBuffer = BitConverter.GetBytes((uint)idBuffer.Length);

                byte[] refBuffer = Encoding.ASCII.GetBytes(reference);
                byte[] refLengthBuffer = BitConverter.GetBytes((uint)refBuffer.Length);

                byte[] hashBuffer = Encoding.ASCII.GetBytes(hash);
                byte[] hashLengthBuffer = BitConverter.GetBytes((uint)hashBuffer.Length);

                byte[] dataLengthBuffer = BitConverter.GetBytes((uint)dataBuffer.Length);

                byte[] endChunkBuffer = Encoding.ASCII.GetBytes("CEND");
                byte[] fileEndChunkBuffer = Encoding.ASCII.GetBytes("FEND");

                /*_______________ Wrting to Memory Steaming_____________*/
                // STRT
                memoryStream.Write(startChunkBuffer, 0, startChunkBuffer.Length);
                // IDLT, IDST Chunks
                memoryStream.Write(idLengthBuffer, 0, idLengthBuffer. Length);
                memoryStream.Write(idBuffer, 0, idBuffer.Length);
                // RFLT
                memoryStream.Write(refLengthBuffer, 0, refLengthBuffer.Length);
                // HSLT
                memoryStream.Write(hashLengthBuffer, 0, hashLengthBuffer.Length);
                // DTLT
                memoryStream.Write(dataLengthBuffer, 0, dataLengthBuffer.Length);

                // RFST
                memoryStream.Write(refBuffer, 0, refBuffer.Length);
                // HSST 
                memoryStream.Write(hashBuffer, 0, hashBuffer.Length);
                // DTBT
                memoryStream.Write(dataBuffer, 0, dataBuffer.Length);

                // CEND 
                memoryStream.Write(endChunkBuffer, 0, endChunkBuffer.Length);

                // FEND
                memoryStream.Write(fileEndChunkBuffer, 0, fileEndChunkBuffer.Length);

                // Hard Saving for Test only
                File.WriteAllBytes("E://cache.fcs", memoryStream.ToArray());
            }
        }
    }

    public class CacheChunkGroup
    {
        public string id;
        public uint refLength;
        public uint hashLength;
        public uint dataLength;
        public byte[] trioChunksBuffer;

        public bool encounteredEndOfFile;
        
            
        public string GetID()
        {
            return id;
        }
        public string GetRef()
        {
            byte[] refBuffer = new byte[refLength];
            Buffer.BlockCopy(trioChunksBuffer, 0, refBuffer, 0, (int)refLength);
            return Encoding.ASCII.GetString(refBuffer);
        }

        public string GetHash()
        {
            byte[] hashBuffer = new byte[hashLength];
            int offset = (int)refLength;
            Buffer.BlockCopy(trioChunksBuffer, offset, hashBuffer, 0, (int)hashLength);
            return Encoding.ASCII.GetString(hashBuffer);
        }

        public byte[] GetData()
        {
            byte[] dataBuffer = new byte[dataLength];
            int offset = (int)refLength;
            Buffer.BlockCopy(trioChunksBuffer, offset, dataBuffer, 0, (int)dataLength);
            return dataBuffer;
        }
    }

}
