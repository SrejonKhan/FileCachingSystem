using System;
using System.IO;
using System.Text;

namespace FileCachingSystem
{
    public class FileCacherHelper
    {
        public static byte[] ValidateFileHeader(FileStream fileStream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                if (fileStream != null)
                {
                    CacheFileHeader fileHeader = GetFileHeader(fileStream);

                    memoryStream.Write(fileHeader.buffer, 0, 8); // file header
                    memoryStream.Write(GetBuffer(fileHeader.totalCachedFile + 1), 0, 4); // cached files length

                    byte[] existingFileBuffer = new byte[fileStream.Length-16];
                    fileStream.Read(existingFileBuffer, 0, existingFileBuffer.Length);
                    memoryStream.Write(existingFileBuffer, 0, existingFileBuffer.Length);
                    fileStream.Close();
                    fileStream.Dispose();
                }
                else
                {
                    // new file should have file header
                    memoryStream.Write(GetBuffer("FSTR"), 0, 4);
                    memoryStream.Write(GetBuffer("FHDR"), 0, 4);
                    memoryStream.Write(GetBuffer(1), 0, 4); // cache file length
                }

                return memoryStream.ToArray();
            }
        }

        public static CacheFileHeader GetFileHeader(Stream stream)
        {
            CacheFileHeader fileHeader = new CacheFileHeader();

            byte[] buffer = new byte[12];
            stream.Read(buffer, 0, 12);

            if (Encoding.ASCII.GetString(buffer, 0, 4) != "FSTR")
            {
                throw new InvalidDataException("Corrupted File Encountered.");
            }
            fileHeader.hasStartEncountered = true;

            if (Encoding.ASCII.GetString(buffer, 4, 4) != "FHDR")
            {
                throw new InvalidDataException("File Header Chunk is missing.");
            }
            fileHeader.hasHeaderEncountered = true;

            fileHeader.totalCachedFile = BitConverter.ToUInt32(buffer, 8);
            fileHeader.buffer = buffer;

            return fileHeader;
        }

        public static uint GetUInt(MemoryStream stream, int offset = 0)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, offset, 4);
            return BitConverter.ToUInt32(buffer, 0);    
        }

        public static string GetString(MemoryStream stream, int offset, int count)
        {
            byte[] buffer = new byte[count];
            stream.Read(buffer, offset, count);
            return Encoding.ASCII.GetString(buffer);
        }

        public static byte[] GetBuffer(uint value)
        {
            return BitConverter.GetBytes(value);            
        }

        public static byte[] GetBuffer(string value)
        {
            return Encoding.ASCII.GetBytes(value);            
        }

        public static byte[] GetBuffer(string value, out int length)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(value);
            length = buffer.Length;
            return buffer;
        }

    }

}
