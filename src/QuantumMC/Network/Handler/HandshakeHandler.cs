using BedrockProtocol.Packets;
using BedrockProtocol.Packets.Enums;
using BedrockProtocol.Utils;
using Serilog;

namespace QuantumMC.Network.Handler
{
    public class HandshakeHandler : PacketHandler
    {
        public override void Handle(PlayerSession session, uint packetId, byte[] payload)
        {
            Log.Information("Received ClientToServerHandshake from {Username}", session.Username);

            var playStatus = new PlayStatusPacket
            {
                Status = PlayStatus.LoginSuccess
            };
            session.SendPacket(playStatus);

            var resourcePacksInfo = new ResourcePacksInfoPacket
            {
                MustAccept = false,
                HasAddons = false,
                HasScripts = false,
                ForceDisableVibrantVisuals = false,
                WorldTemplateId = Guid.Empty,
                WorldTemplateVersion = string.Empty
            };
            session.SendPacket(resourcePacksInfo);

            session.State = SessionState.ResourcePackPhase;
            Log.Information("Sent PlayStatus(LoginSuccess) + ResourcePacksInfo to {Username}", session.Username);
        }
    }
}
