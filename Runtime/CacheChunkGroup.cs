using System;
using System.Text;

namespace FileCachingSystem
{
    public class CacheChunkGroup
    {
        public string chunkName; 
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
