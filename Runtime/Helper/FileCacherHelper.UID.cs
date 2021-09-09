using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace FileCachingSystem
{
    public static partial class FileCacherHelper
    {
        const string pushChars = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
        private static long lastPushTime = 0;
        private static char[] lastRandChars = new char[12];
        private static System.Random rng = new System.Random();

        public static string GenerateUniqueID()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var duplicateTime = (now == lastPushTime);
            lastPushTime = now;

            var timeStampChars = new char[8];
            for (var i = 7; i >= 0; i--)
            {
                timeStampChars[i] = pushChars[(int)(now % 64)];
                now = now >> 6;
            }
            if (now != 0) throw new Exception("We should have converted the entire timestamp.");

            var id = string.Join(string.Empty, timeStampChars);

            if (!duplicateTime)
            {
                for (var i = 0; i < 4; i++)
                {
                    lastRandChars[i] = (char)rng.Next(0, 63);
                }
            }
            else
            {
                // If the timestamp hasn't changed since last push, use the same random number, except incremented by 1.
                int i;
                for (i = 3; i >= 0 && lastRandChars[i] == 63; i--)
                {
                    lastRandChars[i] = (char)0;
                }
                lastRandChars[i]++;
            }
            for (var i = 0; i < 4; i++)
            {
                id += pushChars[lastRandChars[i]];
            }
            if (id.Length != 12) throw new Exception("Length should be 12.");

            return id;
        }
    }
}
