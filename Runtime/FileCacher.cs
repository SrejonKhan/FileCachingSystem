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


                CacheFileHeader fileHeader = FileCacherHelper.GetFileHeader(fileStream);

                Debug.Log(fileHeader.totalCachedFile);

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
                            Texture2D tex = new Texture2D(1, 1);
                            tex.LoadRawTextureData(cacheChunk.GetData());
                            tex.Apply();
                            File.WriteAllBytes($"E:/output/{UnityEngine.Random.Range(0, 10000)}.png", tex.EncodeToPNG());
                        }
                        else
                        {
                            endOfFile = true;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Try Read Cached File's Single Chunk
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="chunkGroup"></param>
        /// <returns></returns>
        public static bool TryReadChunk(FileStream fileStream, out CacheChunkGroup chunkGroup)
        {
            chunkGroup = new CacheChunkGroup();

            // check if we met the end of fille
            if (fileStream.Position == fileStream.Length)
            {
                chunkGroup.encounteredEndOfFile = true;
                return true;
            }
            
            // chunk name
            byte[] chunkNameBuffer = new byte[4];

            fileStream.Read(chunkNameBuffer, 0, 4);
            string chunkName = Encoding.ASCII.GetString(chunkNameBuffer);
            chunkGroup.chunkName = chunkName;

            // check if it's start of a chunk
            if (chunkName != "CSTR")
            {
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

        public static void WriteCache(string path, string id, string reference, string hash, byte[] dataBuffer)
        {
            FileStream fileStream = null;
            bool freshCache = !File.Exists(path); // if new created cache

            /*_______________ File Header _____________*/
            byte[] fileHeaderBuffer = FileCacherHelper.ValidateFileHeader(path, freshCache);

            // Append to existing file
            if (!freshCache)
            {
                fileStream = new FileStream(path, FileMode.Append);  
            }
            // Create new file
            else
            {
                fileStream = File.Create(path);
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // writing Header to fresh file
                if (freshCache)
                    memoryStream.Write(fileHeaderBuffer, 0, fileHeaderBuffer.Length);

                /*_______________ Creating Buffers _____________*/
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
                // CSTR
                memoryStream.Write(startChunkBuffer, 0, startChunkBuffer.Length);
                // IDLT, IDST Chunks 
                memoryStream.Write(idLengthBuffer, 0, idLengthBuffer.Length);
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

                /*_______________ Persistency _____________*/
                // appending to old file
                fileStream.Write(memoryStream.ToArray(), 0, (int)memoryStream.Length);
                // close and dispose
                fileStream.Close();
                fileStream.Dispose();
            }
        }
    }
}
