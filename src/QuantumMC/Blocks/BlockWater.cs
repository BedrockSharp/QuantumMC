namespace QuantumMC.Blocks
{
    public class BlockWater : Block
    {
        public static int ID { get; internal set; }
        public override int RuntimeId => ID;

        public BlockWater() : base("minecraft:water") { }
    }
}
