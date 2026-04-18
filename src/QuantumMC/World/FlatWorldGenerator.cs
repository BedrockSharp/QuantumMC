using BedrockProtocol.Types;

namespace QuantumMC.World
{
    public class FlatWorldGenerator : IWorldGenerator
    {
        public void Generate(Chunk chunk)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    chunk.SetBlock(x, -64, z, Blocks.BlockBedrock.ID);

                    for (int y = -63; y <= 62; y++)
                    {
                        chunk.SetBlock(x, y, z, Blocks.BlockStone.ID);
                    }

                    chunk.SetBlock(x, 63, z, Blocks.BlockDirt.ID);

                    chunk.SetBlock(x, 64, z, Blocks.BlockGrass.ID);
                }
            }
        }
    }
}
