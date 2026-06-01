using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kartverket.Metadatakatalog.Service
{
    public class PopularMetadataResolver
    {
        private const string FileName = "external_popularMetadata.txt";
        private readonly Dictionary<string, int> _scores;

        public PopularMetadataResolver(IHostEnvironment hostEnvironment, ILogger<PopularMetadataResolver> logger)
        {
            _scores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var path = Path.Combine(hostEnvironment.ContentRootPath, "App_Data", FileName);
            if (!File.Exists(path))
            {
                logger.LogWarning("Popular metadata list not found at {Path}; popularMetadata field will not be set.", path);
                return;
            }

            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                var separator = line.IndexOf('=');
                if (separator <= 0 || separator == line.Length - 1)
                    continue;

                var uuid = line.Substring(0, separator).Trim();
                var valuePart = line.Substring(separator + 1).Trim();

                if (int.TryParse(valuePart, out var score))
                    _scores[uuid] = score;
            }

            logger.LogInformation("Loaded {Count} entries from popular metadata list at {Path}.", _scores.Count, path);
        }

        public int? GetScore(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return null;

            return _scores.TryGetValue(uuid, out var score) ? score : (int?)null;
        }
    }
}
