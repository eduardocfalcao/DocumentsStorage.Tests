using System;
using System.Collections.Generic;
using System.Text;

namespace Elasticsearch.GridFS
{
    public static class HashUtils
    {
        public static string ToHexString(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            var length = bytes.Length;
            var c = new char[length * 2];

            for (int i = 0, j = 0; i < length; i++)
            {
                var b = bytes[i];
                c[j++] = ToHexChar(b >> 4);
                c[j++] = ToHexChar(b & 0x0f);
            }

            return new string(c);
        }

        public static char ToHexChar(int value)
        {
            return (char)(value + (value < 10 ? '0' : 'a' - 10));
        }
    }
}
