namespace FileCachingSystem
{
    public class CachedFileInfo
    {
        public string id; // for local identification and query
        public string path; // local file path
        public string hash; // file hash for integrity test
        public string reference; // user defined reference
    }
}
