namespace QuantumMC.Blocks
{
    public class BlockBedrock : Block
    {
        public static int ID { get; internal set; }
        public override int RuntimeId => ID;

        public BlockBedrock() : base("minecraft:bedrock") { }
    }
}
