using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsStorate.Api.Actors
{
    public class FileStoreMessage
    {
        public FileMetadata FileMetadata { get; set; }

        public byte FileContent { get; set; }
    }
}
