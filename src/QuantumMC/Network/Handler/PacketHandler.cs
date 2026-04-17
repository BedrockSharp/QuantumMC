namespace QuantumMC.Network.Handler
{
    public abstract class PacketHandler
    {
        public abstract void Handle(PlayerSession session, uint packetId, byte[] payload);
    }
}
