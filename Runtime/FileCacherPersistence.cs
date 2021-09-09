using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

namespace FileCachingSystem
{
    internal class FileCacherPersistence
    {
        private static Dictionary<string, CachedFileInfo> infoList = new Dictionary<string, CachedFileInfo>(); // id - info
        private static Dictionary<string, string> refList = new Dictionary<string, string>(); // ref - id
        private static Dictionary<string, string> integrityList = new Dictionary<string, string>(); // hash - id

        private static string infosJsonPath;


        public static bool TryGetFileInfo(string hash, out CachedFileInfo info)
        {
            LoadAllInfo();

            if (integrityList.ContainsKey(hash))
            {
                string id = integrityList[hash];
                LoadAllInfo();
                var result = infoList[id];
                info = result;
                return true;
            }
            info = null;
            return false;
        }

        public static bool TryGetFileInfoFromId(string id, out CachedFileInfo info)
        {
            LoadAllInfo();

            if (refList.ContainsKey(id))
            {
                LoadAllInfo();
                var result = infoList[id];
                info = result;
                return true;
            }
            info = null;
            return false;
        }

        /// <summary>
        /// Save Cached File Info to FileInfos.json
        /// </summary>
        /// <param name="fileInfo">File Info to save</param>
        public static void SaveInfo(CachedFileInfo fileInfo)
        {
            LoadAllInfo();

            if (infoList.ContainsKey(fileInfo.id))
                infoList[fileInfo.id] = fileInfo;
            else
                infoList.Add(fileInfo.id, fileInfo);

            if (!refList.ContainsKey(fileInfo.id))
                refList.Add(fileInfo.reference , fileInfo.id);

            SaveAllInfo();
        }

        /// <summary>
        /// Load all Cached File Info from FileInfos.json
        /// </summary>
        public static void LoadAllInfo()
        {
            VerifyInfosPath();

            if (!File.Exists(infosJsonPath)) return;

            infoList.Clear();

            string json = File.ReadAllText(infosJsonPath);

            if (!string.IsNullOrEmpty(json)) return;

            var result = JsonConvert.DeserializeObject<InfoJsonObject>(json);
            infoList = result.Info;
            refList = result.Reference;
        }

        /// <summary>
        /// Save all Cached File Info to FileInfos.json
        /// </summary>
        public static void SaveAllInfo()
        {
            VerifyInfosPath();
            var jsonObject = new InfoJsonObject(infoList, refList);
            string json = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            File.WriteAllText(infosJsonPath, json);
        }

        /// <summary>
        /// Chech if CachedFile.json path is defined or not. 
        /// Define if it's null
        /// </summary>
        private static void VerifyInfosPath()
        {
            VerifyCacheFolder();
            if (string.IsNullOrEmpty(infosJsonPath))
                infosJsonPath = Path.Combine(Application.persistentDataPath, "Cache", "FileInfos.json");
        }

        private static void VerifyCacheFolder()
        {
            string folderPath = Path.Combine(Application.persistentDataPath, "Cache");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }
    }

    internal class InfoJsonObject
    {
        public Dictionary<string, CachedFileInfo> Info;
        public List<string> Reference;

        public InfoJsonObject(Dictionary<string, CachedFileInfo> infoList, List<string> refList)
        {
            Info = infoList;
            Reference = refList;
        }
    }
}
