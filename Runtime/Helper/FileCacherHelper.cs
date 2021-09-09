using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace FileCachingSystem
{
    public static partial class FileCacherHelper
    {
        public static string GenerateMD5Hash(byte[] buffer)
        {
            return Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(buffer));
        }
    }
}
