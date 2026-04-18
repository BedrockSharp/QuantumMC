namespace QuantumMC.Blocks
{
    public class BlockAir : Block
    {
        public static int ID { get; internal set; }
        public override int RuntimeId => ID;

        public BlockAir() : base("minecraft:air") { }
    }
}
