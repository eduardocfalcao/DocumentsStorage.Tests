using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Elasticsearch.GridFS.Streams
{
    public class IncrementalMD5
    {
        private readonly IncrementalHash _incrementalHash;

        public IncrementalMD5()
        {
            _incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        }

        public void AppendData(byte[] data, int offset, int count)
        {
            _incrementalHash.AppendData(data, offset, count);
        }

        public void Dispose()
        {
            _incrementalHash.Dispose();
        }

        public byte[] GetHashAndReset()
        {
            return _incrementalHash.GetHashAndReset();
        }
    }
}
