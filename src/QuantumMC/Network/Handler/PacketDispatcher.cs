using System.Collections.Generic;
using BedrockProtocol.Packets.Enums;
using BedrockProtocol.Packets;
using Serilog;

namespace QuantumMC.Network.Handler
{
    public static class PacketDispatcher
    {
        private static readonly Dictionary<uint, PacketHandler> _handlers = new();

        static PacketDispatcher()
        {
            var loginHandler = new LoginPacketHandler();
            var sessionHandler = new SessionStartPacketHandler();

            _handlers.Add((uint)PacketIds.Login, loginHandler);
            _handlers.Add((uint)PacketIds.RequestNetworkSettings, sessionHandler);
            _handlers.Add((uint)PacketIds.ResourcePackClientResponse, sessionHandler);
        }

        public static void Dispatch(PlayerSession session, uint packetId, byte[] payload)
        {
            if (_handlers.TryGetValue(packetId, out var handler))
            {
                handler.Handle(session, packetId, payload);
            }
            else
            {
                Log.Debug("Unhandled packet 0x{PacketId:X2} from {EndPoint}", packetId, session.EndPoint);
            }
        }
    }
}
