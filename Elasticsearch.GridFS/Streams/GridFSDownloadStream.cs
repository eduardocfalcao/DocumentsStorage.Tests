using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Elasticsearch.GridFS.Streams
{
    public abstract class GridFSDownloadStream : Stream
    {
        // constructors
        internal GridFSDownloadStream()
        {
        }

        // public properties
        /// <summary>
        /// Gets the files collection document.
        /// </summary>
        /// <value>
        /// The files collection document.
        /// </value>
        public abstract FileDocument FileInfo { get; }

        // public methods
#if NETSTANDARD1_5 || NETSTANDARD1_6
        /// <summary>
        /// Closes the GridFS stream.
        /// </summary>
        public virtual void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#endif

        /// <summary>
        /// Closes the GridFS stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public abstract void Close(CancellationToken cancellationToken);

        /// <summary>
        /// Closes the GridFS stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public abstract Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
