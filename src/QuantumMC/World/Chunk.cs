using BedrockProtocol.Types;

namespace QuantumMC.World
{
    /// <summary>
    /// Represents a full chunk column with 24 sub-chunks covering Y=-64 to Y=319 (overworld).
    /// Each sub-chunk is 16x16x16 blocks.
    /// </summary>
    public class Chunk
    {
        /// <summary>
        /// Number of sub-chunks in a chunk column (Bedrock overworld: 24).
        /// Covers Y range from MinY to MinY + (SubChunkCount * 16).
        /// </summary>
        public const int SubChunkCount = 24;

        /// <summary>
        /// Minimum Y coordinate for the overworld.
        /// </summary>
        public const int MinY = -64;

        /// <summary>
        /// Maximum Y coordinate (exclusive) for the overworld.
        /// </summary>
        public const int MaxY = MinY + (SubChunkCount * 16); // 320

        /// <summary>
        /// The sub-chunk index offset (MinY / 16).
        /// Sub-chunk at index 0 covers Y=-64 to Y=-49.
        /// </summary>
        public const int SubChunkIndexOffset = MinY / 16; // -4

        public int ChunkX { get; }
        public int ChunkZ { get; }

        private readonly SubChunk?[] _subChunks = new SubChunk?[SubChunkCount];

        public Chunk(int chunkX, int chunkZ)
        {
            ChunkX = chunkX;
            ChunkZ = chunkZ;
        }

        /// <summary>
        /// Converts an absolute Y coordinate to a sub-chunk array index.
        /// </summary>
        private static int GetSubChunkArrayIndex(int y)
        {
            return (y - MinY) >> 4; // (y - MinY) / 16
        }

        /// <summary>
        /// Gets or creates a sub-chunk at the given array index.
        /// </summary>
        private SubChunk GetOrCreateSubChunk(int arrayIndex)
        {
            _subChunks[arrayIndex] ??= new SubChunk(Blocks.BlockAir.ID);
            return _subChunks[arrayIndex]!;
        }

        /// <summary>
        /// Sets a block at absolute world coordinates within this chunk.
        /// X and Z should be 0-15 (local to chunk), Y is absolute (-64 to 319).
        /// </summary>
        public void SetBlock(int x, int y, int z, int runtimeId)
        {
            if (y < MinY || y >= MaxY) return;

            int arrayIndex = GetSubChunkArrayIndex(y);
            int localY = y & 0xF; // y % 16

            GetOrCreateSubChunk(arrayIndex).SetBlock(x, localY, z, runtimeId);
        }

        /// <summary>
        /// Gets the runtime ID of the block at absolute world coordinates.
        /// </summary>
        public int GetBlock(int x, int y, int z)
        {
            if (y < MinY || y >= MaxY) return Blocks.BlockAir.ID; // air

            int arrayIndex = GetSubChunkArrayIndex(y);
            if (_subChunks[arrayIndex] == null) return Blocks.BlockAir.ID; // air

            int localY = y & 0xF;
            return _subChunks[arrayIndex]!.GetBlock(x, localY, z);
        }

        /// <summary>
        /// Serializes this chunk column into a LevelChunkPacket payload.
        /// Returns (payload, subChunkCount).
        /// </summary>
        public (byte[] Payload, int SubChunkCount) Serialize()
        {
            var subChunkDataArray = new SubChunkData?[SubChunkCount];

            for (int i = 0; i < SubChunkCount; i++)
            {
                if (_subChunks[i] != null && !_subChunks[i]!.IsEmpty)
                {
                    sbyte subChunkYIndex = (sbyte)(i + SubChunkIndexOffset);
                    subChunkDataArray[i] = _subChunks[i]!.Serialize(subChunkYIndex);
                }
            }

            return ChunkSerializer.Serialize(subChunkDataArray, Blocks.BlockAir.ID);
        }
    }
}
