using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Elasticsearch.GridFS
{
    public abstract class GridFSUploadStream : Stream
    {
        // constructors
        internal GridFSUploadStream()
        {
        }

        // public properties
        /// <summary>
        /// Gets the id of the file being added to GridFS.
        /// </summary>
        /// <value>
        /// The id of the file being added to GridFS.
        /// </value>
        public abstract Guid Id { get; }

        // public methods
        /// <summary>
        /// Aborts an upload operation.
        /// </summary>
        /// <remarks>
        /// Any partial results already written to the server are deleted when Abort is called.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        public abstract void Abort(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Aborts an upload operation.
        /// </summary>
        /// <remarks>
        /// Any partial results already written to the server are deleted when AbortAsync is called.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public abstract Task AbortAsync(CancellationToken cancellationToken = default(CancellationToken));

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
        /// Closes the Stream and completes the upload operation.
        /// </summary>
        /// <remarks>
        /// Any data remaining in the Stream is flushed to the server and the GridFS files collection document is written.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        public abstract void Close(CancellationToken cancellationToken);

        /// <summary>
        /// Closes the Stream and completes the upload operation.
        /// </summary>
        /// <remarks>
        /// Any data remaining in the Stream is flushed to the server and the GridFS files collection document is written.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task.</returns>
        public abstract Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
