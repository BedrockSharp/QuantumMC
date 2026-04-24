namespace QuantumMC.World
{
    public interface IWorldGenerator
    {
        /// <summary>
        /// Generates terrain data for the given chunk.
        /// The chunk's X/Z coordinates can be used for position-dependent generation.
        /// </summary>
        void Generate(Chunk chunk);
    }
}
