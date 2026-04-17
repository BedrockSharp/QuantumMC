using BedrockProtocol;
using BedrockProtocol.Packets;
using BedrockProtocol.Packets.Enums;
using BedrockProtocol.Utils;
using BedrockProtocol.Packets.Types;
using BedrockProtocol.Types;
using Serilog;
using Nbt;
using System.Collections.Generic;
using System;

namespace QuantumMC.Network.Handler
{
    public class SessionStartPacketHandler : PacketHandler
    {
        public override void Handle(PlayerSession session, uint packetId, byte[] payload)
        {
            if ((PacketIds)packetId == PacketIds.RequestNetworkSettings)
            {
                HandleRequestNetworkSettings(session, payload);
            }
        }

        private void HandleRequestNetworkSettings(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new RequestNetworkSettingsPacket();
            packet.Decode(stream);

            int clientProtocol = System.Net.IPAddress.NetworkToHostOrder(packet.ProtocolVersion);

            Log.Information("Received RequestNetworkSettings from {EndPoint} (Protocol: {Protocol})", session.EndPoint, clientProtocol);

            if (clientProtocol != Protocol.CurrentProtocol)
            {
                Log.Warning("Protocol mismatch from {EndPoint}: client={ClientProtocol}, server={ServerProtocol}", session.EndPoint, clientProtocol, Protocol.CurrentProtocol);
                session.Disconnect();
                return;
            }

            var response = new NetworkSettingsPacket
            {
                CompressionThreshold = 256,
                CompressionAlgorithm = CompressionAlgorithm.Zlib,
                ClientThrottling = false,
                ThrottlingThreshold = 0,
                ThrottlingScalar = 0f
            };

            session.SendPacket(response);
            session.CompressionReady = true;
            session.State = SessionState.LoginPhase;
            Log.Information("Sent NetworkSettings to {EndPoint}", session.EndPoint);
        }
    }
}
