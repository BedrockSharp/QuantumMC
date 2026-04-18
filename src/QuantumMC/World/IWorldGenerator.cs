namespace QuantumMC.World
{
    /// <summary>
    /// Interface for world generators that populate chunk data.
    /// </summary>
    public interface IWorldGenerator
    {
        /// <summary>
        /// Generates terrain data for the given chunk.
        /// The chunk's X/Z coordinates can be used for position-dependent generation.
        /// </summary>
        void Generate(Chunk chunk);
    }
}
