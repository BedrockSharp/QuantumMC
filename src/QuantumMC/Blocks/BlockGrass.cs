namespace QuantumMC.Blocks
{
    public class BlockGrass : Block
    {
        public static int ID { get; internal set; }
        public override int RuntimeId => ID;

        public BlockGrass() : base("minecraft:grass_block") { }
    }
}
