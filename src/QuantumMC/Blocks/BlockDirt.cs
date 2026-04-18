namespace QuantumMC.Blocks
{
    public class BlockDirt : Block
    {
        public static int ID { get; internal set; }
        public override int RuntimeId => ID;

        public BlockDirt() : base("minecraft:dirt") { }
    }
}
