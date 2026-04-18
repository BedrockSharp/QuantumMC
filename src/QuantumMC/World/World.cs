using System.Collections.Concurrent;
using Serilog;

namespace QuantumMC.World
{
    /// <summary>
    /// Manages the world state, including chunk generation and caching.
    /// </summary>
    public class World
    {
        private readonly ConcurrentDictionary<(int X, int Z), Chunk> _chunks = new();
        private readonly IWorldGenerator _generator;

        /// <summary>
        /// The maximum chunk radius the server will allow.
        /// </summary>
        public int MaxChunkRadius { get; set; } = 8;

        /// <summary>
        /// The world spawn position.
        /// </summary>
        public int SpawnX { get; set; } = 0;
        public int SpawnY { get; set; } = 65;
        public int SpawnZ { get; set; } = 0;

        public World(IWorldGenerator generator)
        {
            _generator = generator;
            Log.Information("World initialized with generator: {Generator}", generator.GetType().Name);
        }

        /// <summary>
        /// Gets an existing chunk or generates a new one at the given coordinates.
        /// Thread-safe via ConcurrentDictionary.
        /// </summary>
        public Chunk GetOrGenerateChunk(int chunkX, int chunkZ)
        {
            return _chunks.GetOrAdd((chunkX, chunkZ), key =>
            {
                var chunk = new Chunk(key.X, key.Z);
                _generator.Generate(chunk);
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
    }
}
