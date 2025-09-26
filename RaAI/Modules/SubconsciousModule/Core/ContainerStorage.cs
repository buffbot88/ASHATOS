using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RaAI.Modules;
using RaAI.Handlers;

namespace RaAI.Modules.SubconsciousModule.Core
{
    // Append-only bin container, manifest snapshotting, compaction (pack).
    public class BinContainer(string containerPath, string manifestPath, byte[] magic) : IContainerStorage
    {
        private readonly string _containerPath = containerPath;
        private readonly string _manifestPath = manifestPath;
        private readonly byte[] _magic = magic;
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<Guid, EntryInfo> _index = new ConcurrentDictionary<Guid, EntryInfo>();
        private bool _disposed;

        public async Task EnsureCreatedAsync(CancellationToken ct = default)
        {
            if (!File.Exists(_containerPath))
            {
                using var fs = new FileStream(_containerPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await fs.WriteAsync(_magic, 0, _magic.Length, ct).ConfigureAwait(false);
            }
            if (!File.Exists(_manifestPath))
            {
                await File.WriteAllTextAsync(_manifestPath, "[]", ct).ConfigureAwait(false);
            }
        }

        public async Task<Guid> AppendEntryAsync(byte[] entryBytes, DateTime createdAt, byte entryType, CancellationToken ct = default)
        {
            await _writeLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var id = Guid.NewGuid();
                using var fs = new FileStream(_containerPath, FileMode.Append, FileAccess.Write, FileShare.None);
                long offset = fs.Position;
                int totalLength = entryBytes.Length;
                await fs.WriteAsync(entryBytes, 0, entryBytes.Length, ct).ConfigureAwait(false);

                var info = new EntryInfo
                {
                    Id = id,
                    Offset = offset,
                    TotalLength = totalLength,
                    CreatedAt = createdAt,
                    EntryType = entryType
                };
                _index[id] = info;
                return id;
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task<byte[]?> ReadEntryRawAsync(Guid id, CancellationToken ct = default)
        {
            if (!_index.TryGetValue(id, out var info))
                return null;

            using var fs = new FileStream(_containerPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fs.Seek(info.Offset, SeekOrigin.Begin);
            var buffer = new byte[info.TotalLength];
            int read = await fs.ReadAsync(buffer, 0, info.TotalLength, ct).ConfigureAwait(false);
            if (read != info.TotalLength)
                return null;
            return buffer;
        }

        public Task<IReadOnlyCollection<EntryInfo>> ListIndexedEntriesAsync()
        {
            return Task.FromResult((IReadOnlyCollection<EntryInfo>)_index.Values.ToList());
        }

        public async Task SnapshotManifestAsync(CancellationToken ct = default)
        {
            var manifest = _index.Values.ToList();
            var json = JsonSerializer.Serialize(manifest);
            await File.WriteAllTextAsync(_manifestPath, json, ct).ConfigureAwait(false);
        }

        public async Task CompactAsync(CancellationToken ct = default)
        {
            await _writeLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var tempPath = _containerPath + ".tmp";
                using var tempFs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await tempFs.WriteAsync(_magic, 0, _magic.Length, ct).ConfigureAwait(false);

                var newIndex = new ConcurrentDictionary<Guid, EntryInfo>();
                long offset = _magic.Length;
                foreach (var entry in _index.Values.OrderBy(e => e.CreatedAt))
                {
                    var data = await ReadEntryRawAsync(entry.Id, ct).ConfigureAwait(false);
                    if (data == null) continue;
                    await tempFs.WriteAsync(data, 0, data.Length, ct).ConfigureAwait(false);

                    newIndex[entry.Id] = new EntryInfo
                    {
                        Id = entry.Id,
                        Offset = offset,
                        TotalLength = data.Length,
                        CreatedAt = entry.CreatedAt,
                        EntryType = entry.EntryType
                    };
                    offset += data.Length;
                }
                tempFs.Flush();
                File.Replace(tempPath, _containerPath, null);
                _index.Clear();
                foreach (var kv in newIndex)
                    _index[kv.Key] = kv.Value;
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task ExportAsync(string destinationPath, CancellationToken ct = default)
        {
            await _writeLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                File.Copy(_containerPath, destinationPath, true);
                var manifestDest = destinationPath + ".manifest.json";
                File.Copy(_manifestPath, manifestDest, true);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task ImportAsync(string sourcePath, CancellationToken ct = default)
        {
            await _writeLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                File.Copy(sourcePath, _containerPath, true);
                var manifestSrc = sourcePath + ".manifest.json";
                File.Copy(manifestSrc, _manifestPath, true);

                var manifestJson = await File.ReadAllTextAsync(_manifestPath, ct).ConfigureAwait(false);
                var manifest = JsonSerializer.Deserialize<List<EntryInfo>>(manifestJson);
                _index.Clear();
                if (manifest != null)
                {
                    foreach (var entry in manifest)
                        _index[entry.Id] = entry;
                }
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _writeLock.Dispose();
            _disposed = true;
        }
    }
    public interface IContainerStorage : IDisposable
    {
        Task EnsureCreatedAsync(CancellationToken ct = default);
        Task<Guid> AppendEntryAsync(byte[] entryBytes, DateTime createdAt, byte entryType, CancellationToken ct = default);
        Task<byte[]?> ReadEntryRawAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyCollection<EntryInfo>> ListIndexedEntriesAsync();
        Task SnapshotManifestAsync(CancellationToken ct = default);
        Task CompactAsync(CancellationToken ct = default);
        Task ExportAsync(string destinationPath, CancellationToken ct = default);
        Task ImportAsync(string sourcePath, CancellationToken ct = default);
    }
}