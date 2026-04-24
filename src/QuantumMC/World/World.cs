using System.Collections.Concurrent;
using Serilog;

namespace QuantumMC.World
{
    public class World : IDisposable
    {
        private readonly ConcurrentDictionary<(int X, int Z), Chunk> _chunks = new();
        private readonly IWorldGenerator _generator;
        private readonly IWorldProvider? _provider;

        /// <summary>
        /// The maximum chunk radius the server will allow.
        /// </summary>
        public int MaxChunkRadius { get; set; } = 8;
        
        public string Name { get; set; } = "world";

        /// <summary>
        /// The world spawn position.
        /// </summary>
        public int SpawnX { get; set; } = 0;
        public int SpawnY { get; set; } = 65;
        public int SpawnZ { get; set; } = 0;

        public World(IWorldGenerator generator, IWorldProvider? provider = null)
        {
            _generator = generator;
            _provider = provider;

            if (_provider != null)
            {
                Name = _provider.LevelName;
                SpawnX = _provider.SpawnX;
                SpawnY = _provider.SpawnY;
                SpawnZ = _provider.SpawnZ;
            }
        }

        /// <summary>
        /// Gets an existing chunk or generates a new one at the given coordinates.
        /// Thread-safe via ConcurrentDictionary.
        /// </summary>
        public Chunk GetOrGenerateChunk(int chunkX, int chunkZ)
        {
            return _chunks.GetOrAdd((chunkX, chunkZ), key =>
            {
                var chunk = _provider?.LoadChunk(key.X, key.Z);
                if (chunk != null)
                {
                    return chunk;
                }

                chunk = new Chunk(key.X, key.Z);
                _generator.Generate(chunk);

                _provider?.SaveChunk(chunk);
                return chunk;
            });
        }

        /// <summary>
        /// Returns all chunks within the given radius around a center chunk position.
        /// </summary>
        public List<Chunk> GetChunksInRadius(int centerChunkX, int centerChunkZ, int radius)
        {
            var chunks = new List<Chunk>();

            for (int x = centerChunkX - radius; x <= centerChunkX + radius; x++)
            {
                for (int z = centerChunkZ - radius; z <= centerChunkZ + radius; z++)
                {
                    chunks.Add(GetOrGenerateChunk(x, z));
                }
            }

            Log.Debug("Generated/loaded {Count} chunks around ({CenterX}, {CenterZ}) with radius {Radius}",
                chunks.Count, centerChunkX, centerChunkZ, radius);

            return chunks;
        }

        /// <summary>
        /// Gets the number of currently loaded chunks.
        /// </summary>
        public int LoadedChunkCount => _chunks.Count;

        /// <summary>
        /// Disposes the world and its provider.
        /// </summary>
        public void Dispose()
        {
            if (_provider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }
        }
    }
}
