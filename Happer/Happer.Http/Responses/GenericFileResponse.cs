using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Happer.Http.Utilities;

namespace Happer.Http.Responses
{
    public class GenericFileResponse : Response
    {
        public static IList<string> SafePaths { get; private set; }

        public static int BufferSize = 4 * 1024 * 1024;

        static GenericFileResponse()
        {
            SafePaths = new List<string>();
        }

        public GenericFileResponse(string filePath)
            : this(filePath, MimeTypes.GetMimeType(filePath))
        {
        }

        public GenericFileResponse(string filePath, Context context)
            : this(filePath, MimeTypes.GetMimeType(filePath), context)
        {
        }

        public GenericFileResponse(string filePath, string contentType, Context context = null)
        {
            InitializeGenericFileResponse(filePath, contentType, context);
        }

        public string FileName { get; protected set; }

        private static Action<Stream> GetFileContent(string filePath, long length)
        {
            return stream =>
            {
                using (var file = File.OpenRead(filePath))
                {
                    file.CopyTo(stream, (int)(length < BufferSize ? length : BufferSize));
                }
            };
        }

        static bool IsSafeFilePath(string rootPath, string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var fullPath = Path.GetFullPath(filePath);

            return fullPath.StartsWith(Path.GetFullPath(rootPath), StringComparison.OrdinalIgnoreCase);
        }

        private void InitializeGenericFileResponse(string filePath, string contentType, Context context)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                StatusCode = HttpStatusCode.NotFound;
                return;
            }

            if (SafePaths == null || !SafePaths.Any())
            {
                throw new InvalidOperationException("No SafePaths defined.");
            }

            foreach (var rootPath in SafePaths)
            {
                string fullPath;
                if (Path.IsPathRooted(filePath))
                {
                    fullPath = filePath;
                }
                else
                {
                    fullPath = Path.Combine(rootPath, filePath);
                }

                if (IsSafeFilePath(rootPath, fullPath))
                {
                    this.FileName = Path.GetFileName(fullPath);

                    this.SetResponseValues(contentType, fullPath, context);

                    return;
                }
            }

            StatusCode = HttpStatusCode.NotFound;
        }

        private void SetResponseValues(string contentType, string fullPath, Context context)
        {
            var fi = new FileInfo(fullPath);

            var lastWriteTimeUtc = fi.LastWriteTimeUtc;
            var etag = string.Concat("\"", lastWriteTimeUtc.Ticks.ToString("x"), "\"");
            var lastModified = lastWriteTimeUtc.ToString("R");
            var length = fi.Length;

            if (ReturnNotModified(etag, lastWriteTimeUtc, context))
            {
                this.StatusCode = HttpStatusCode.NotModified;
                this.ContentType = null;
                this.Contents = NoBody;

                return;
            }

            this.Headers["ETag"] = etag;
            this.Headers["Last-Modified"] = lastModified;
            this.Headers["Content-Length"] = length.ToString();

            if (length > 0)
            {
                this.Contents = GetFileContent(fullPath, length);
            }

            this.ContentType = contentType;
            this.StatusCode = HttpStatusCode.OK;
        }

        private static bool ReturnNotModified(string etag, DateTime? lastModified, Context context)
        {
            if (context == null || context.Request == null)
            {
                return false;
            }

            var requestEtag = context.Request.Headers.IfNoneMatch.FirstOrDefault();

            if (requestEtag != null && !string.IsNullOrEmpty(etag))
            {
                return requestEtag.Equals(etag, StringComparison.Ordinal);
            }

            var requestDate = context.Request.Headers.IfModifiedSince;

            if (requestDate.HasValue && lastModified.HasValue && ((int)(lastModified.Value - requestDate.Value).TotalSeconds) <= 0)
            {
                return true;
            }

            return false;
        }
    }
}
