using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;

namespace Roslyn.Resolvers.Demo
{
    public class RemoteFileResolver : SourceReferenceResolver
    {
        private Dictionary<string, Stream> remoteFiles = new Dictionary<string, Stream>();
        private SourceFileResolver fileBasedResolver;

        public RemoteFileResolver(ImmutableArray<string> searchPaths, string baseDirectory)
        {
            fileBasedResolver = new SourceFileResolver(searchPaths, baseDirectory);
        }

        public override bool Equals(object other)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return -1;
        }

        public override string NormalizePath(string path, string baseFilePath)
        {
            var uri = GetUri(path);
            if (uri == null) return fileBasedResolver.NormalizePath(path, baseFilePath);

            return path;
        }

        public override Stream OpenRead(string resolvedPath)
        {
            var uri = GetUri(resolvedPath);
            if (uri == null) return fileBasedResolver.OpenRead(resolvedPath);

            return remoteFiles[resolvedPath];
        }

        public override string ResolveReference(string path, string baseFilePath)
        {
            var uri = GetUri(path);
            if (uri == null) return fileBasedResolver.ResolveReference(path, baseFilePath);

            var client = new HttpClient();
            var response = client.GetAsync(path).Result;

            if (response.IsSuccessStatusCode)
            {
                remoteFiles.Add(path, response.Content.ReadAsStreamAsync().Result);
            }
            return path;
        }

        private Uri GetUri(string input)
        {
            Uri uriResult;
            if (Uri.TryCreate(input, UriKind.Absolute, out uriResult)
                          && (uriResult.Scheme == "http"
                              || uriResult.Scheme == "https"))
            {
                return uriResult;
            }

            return null;
        }
    }
}
