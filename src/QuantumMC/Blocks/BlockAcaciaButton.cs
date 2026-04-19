namespace QuantumMC.Blocks
{
    public class BlockAcaciaButton : Block
    {
        public static int ID { get; internal set; }
        public override int RuntimeId => ID;

        public BlockAir() : base("minecraft:acacia_button") { }
    }
}
