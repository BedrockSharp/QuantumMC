namespace QuantumMC.World
{
    public interface IWorldProvider
    {
        string LevelName { get; }
        int SpawnX { get; }
        int SpawnY { get; }
        int SpawnZ { get; }

        /// <summary>
        /// Loads a chunk from storage if it exists. Returns null if not found.
        /// </summary>
        Chunk? LoadChunk(int x, int z);

        /// <summary>
        /// Saves a chunk to storage.
        /// </summary>
        void SaveChunk(Chunk chunk);
    }
}
