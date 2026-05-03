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
            var loginHandler = new LoginHandler();
            var handshakeHandler = new HandshakeHandler();
            var resourcePackHandler = new ResourcePackHandler();
            var sessionHandler = new SessionStartPacketHandler();
            var playHandler = new PlayHandler();
            var inGameHandler = new InGameHandler();

            _handlers.Add((uint)PacketIds.Login, loginHandler);
            _handlers.Add((uint)PacketIds.ClientToServerHandshake, handshakeHandler);
            _handlers.Add((uint)PacketIds.ResourcePackClientResponse, resourcePackHandler);
            _handlers.Add((uint)PacketIds.RequestNetworkSettings, sessionHandler);
            _handlers.Add((uint)PacketIds.RequestChunkRadius, playHandler);
            _handlers.Add((uint)PacketIds.SetLocalPlayerAsInitialized, playHandler);
            _handlers.Add((uint)PacketIds.Text, inGameHandler);
            _handlers.Add((uint)PacketIds.MovePlayer, inGameHandler);
            _handlers.Add((uint)PacketIds.PlayerAuthInput, inGameHandler);
            _handlers.Add((uint)PacketIds.Animate, inGameHandler);
            _handlers.Add((uint)PacketIds.CommandRequest, inGameHandler);
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
