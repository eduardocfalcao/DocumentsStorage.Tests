using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsStorate.Api
{
    public interface IStoreService
    {
        void StoreFile(FileMetadata file, byte[] fileContent);
    }

    public class StoreService : IStoreService
    {
        public void StoreFile(FileMetadata file, byte[] fileContent)
        {
            throw new NotImplementedException();
        }
    }
}
