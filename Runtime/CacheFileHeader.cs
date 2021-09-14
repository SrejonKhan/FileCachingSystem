namespace FileCachingSystem
{
    public class CacheFileHeader
    {
        public bool hasStartEncountered;
        public bool hasHeaderEncountered; 
        public uint totalCachedFile;
        public byte[] buffer;
    }

}
