using BedrockProtocol.Types;

namespace QuantumMC.World
{
    /// <summary>
    /// Server-side representation of a 16x16x16 sub-chunk.
    /// Wraps PalettedBlockStorage for convenient block manipulation.
    /// </summary>
    public class SubChunk
    {
        private readonly PalettedBlockStorage _storage;

        public SubChunk(int defaultBlockId)
        {
            _storage = new PalettedBlockStorage(defaultBlockId);
        }

        /// <summary>
        /// Whether this sub-chunk has any non-air blocks.
        /// </summary>
        public bool IsEmpty => _storage.IsEmpty;

        /// <summary>
        /// Sets a block at local coordinates (0-15 each).
        /// </summary>
        public void SetBlock(int x, int y, int z, int runtimeId)
        {
            _storage.Set(x, y, z, runtimeId);
        }

        /// <summary>
        /// Gets the runtime ID of the block at local coordinates (0-15 each).
        /// </summary>
        public int GetBlock(int x, int y, int z)
        {
            return _storage.Get(x, y, z);
        }

        /// <summary>
        /// Creates a SubChunkData for network serialization.
        /// </summary>
        public SubChunkData Serialize(sbyte subChunkIndex)
        {
            var data = new SubChunkData
            {
                SubChunkIndex = subChunkIndex
            };
            data.Layers.Add(_storage);
            return data;
        }
    }
}
