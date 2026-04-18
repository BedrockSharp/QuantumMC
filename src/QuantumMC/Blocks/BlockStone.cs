namespace QuantumMC.Blocks
{
    public class BlockStone : Block
    {
        public static int ID { get; internal set; }
        public override int RuntimeId => ID;

        public BlockStone() : base("minecraft:stone") { }
    }
}
