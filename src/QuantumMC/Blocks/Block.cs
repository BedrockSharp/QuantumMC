namespace QuantumMC.Blocks
{
    public abstract class Block
    {
        public string Identifier { get; }
        public abstract int RuntimeId { get; }

        protected Block(string identifier)
        {
            Identifier = identifier;
        }

    }
}
