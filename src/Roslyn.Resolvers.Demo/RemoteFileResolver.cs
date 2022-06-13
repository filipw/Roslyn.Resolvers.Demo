﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Roslyn.Resolvers.Demo
{
    public class RemoteFileResolver : SourceReferenceResolver
    {
        private readonly Dictionary<string, string> _remoteFiles = new();
        private readonly SourceFileResolver _fileBasedResolver;

        public RemoteFileResolver() : this(ImmutableArray.Create(Array.Empty<string>()),
                AppContext.BaseDirectory)
        {
        }

        public RemoteFileResolver(ImmutableArray<string> searchPaths, string baseDirectory)
        {
            _fileBasedResolver = new SourceFileResolver(searchPaths, baseDirectory);
        }

        public override string NormalizePath(string path, string baseFilePath)
        {
            var uri = GetUri(path);
            if (uri == null) return _fileBasedResolver.NormalizePath(path, baseFilePath);

            return path;
        }

        public override Stream OpenRead(string resolvedPath)
        {
            var uri = GetUri(resolvedPath);
            if (uri == null) return _fileBasedResolver.OpenRead(resolvedPath);

            if (_remoteFiles.ContainsKey(resolvedPath))
            {
                var storedFile = _remoteFiles[resolvedPath];
                return new MemoryStream(Encoding.UTF8.GetBytes(storedFile));
            }

            return Stream.Null;
        }

        public override string ResolveReference(string path, string baseFilePath)
        {
            var uri = GetUri(path);
            if (uri == null) return _fileBasedResolver.ResolveReference(path, baseFilePath);

            var client = new HttpClient();
            var response = client.GetAsync(path).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseFile = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrWhiteSpace(responseFile))
                {
                    _remoteFiles.Add(path, responseFile);
                }
            }
            return path;
        }

        private static Uri GetUri(string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult)
                          && (uriResult.Scheme == "http"
                              || uriResult.Scheme == "https"))
            {
                return uriResult;
            }

            return null;
        }

        protected bool Equals(RemoteFileResolver other)
        {
            return Equals(_remoteFiles, other._remoteFiles) && Equals(_fileBasedResolver, other._fileBasedResolver);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RemoteFileResolver)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 37;
                hashCode = (hashCode * 397) ^ (_remoteFiles?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_fileBasedResolver?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
