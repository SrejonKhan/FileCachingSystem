using System.Collections;
using UnityEngine;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Security.Cryptography;
using System;

namespace FileCachingSystem
{
    public class FileCacher
    {
        public static CachedFileInfo Load(string actualPath, string fileName, string reference = "", string folderName = "")
        {
            string hash = FileCacherHelper.GenerateMD5Hash(File.ReadAllBytes(actualPath));

            if (FileCacherPersistence.TryGetFileInfo(hash, out var info))
            {
                return info;
            }

            string uid = FileCacherHelper.GenerateUniqueID();

            CachedFileInfo fileInfo = new CachedFileInfo();
            fileInfo.id = uid;
            fileInfo.hash = hash;
            fileInfo.path = Path.Combine(Application.persistentDataPath, "Cache", folderName, uid);
            fileInfo.reference = reference;

            FileCacherPersistence.SaveInfo(fileInfo);

            //Copy file to cache
            File.Copy(actualPath, fileInfo.path);

            return fileInfo;
        }

        public static CachedFileInfo Save(byte[] buffer, string reference, string extension = "", string folderName = "")
        {

            string hash = FileCacherHelper.GenerateMD5Hash(buffer);

            string uid = FileCacherHelper.GenerateUniqueID();

            if (FileCacherPersistence.TryGetFileInfo(hash, out var info))
            {
                if (File.Exists(info.path))
                {
                    // same file
                    if (hash == info.hash) return info;
                }
                else
                {
                    // local file missing, but same data
                    if (hash == info.hash)
                        uid = info.id;
                }
            }

            CachedFileInfo fileInfo = new CachedFileInfo();
            fileInfo.id = uid;
            fileInfo.hash = hash;
            fileInfo.path = Path.Combine(Application.persistentDataPath, "Cache", folderName, uid + extension);
            fileInfo.reference = reference;

            FileCacherPersistence.SaveInfo(fileInfo);
            FileCacherPersistence.SaveIntegrity(fileInfo);

            //save file
            File.WriteAllBytes(fileInfo.path, buffer);

            return fileInfo;
        }
    }
}
