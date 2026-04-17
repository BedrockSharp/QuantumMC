using System;
using BedrockProtocol.Packets;
using BedrockProtocol.Packets.Enums;
using BedrockProtocol.Utils;
using Serilog;

namespace QuantumMC.Network.Handler
{
    public class LoginPacketHandler : PacketHandler
    {
        public override void Handle(PlayerSession session, uint packetId, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new LoginPacket();
            packet.Decode(stream);

            Log.Information("Received Login from {EndPoint} (Protocol: {Protocol})", session.EndPoint, packet.ProtocolVersion);

            session.Username = ExtractUsernameFromChain(packet.ChainDataJwt);
            Log.Information("Player {Username} is logging in from {EndPoint}", session.Username, session.EndPoint);

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

        private string ExtractUsernameFromChain(string chainDataJwt)
        {
            try
            {
                string[] chains = chainDataJwt.Split('.');
                if (chains.Length < 2)
                    return "Unknown";

                string payloadBase64 = chains[1];
                int padding = 4 - (payloadBase64.Length % 4);
                if (padding < 4)
                    payloadBase64 += new string('=', padding);

                byte[] payloadBytes = Convert.FromBase64String(payloadBase64);
                string payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);

                int nameIndex = payloadJson.IndexOf("\"displayName\"", StringComparison.Ordinal);
                if (nameIndex == -1)
                    return "Unknown";

                int colonIndex = payloadJson.IndexOf(':', nameIndex);
                int firstQuote = payloadJson.IndexOf('"', colonIndex + 1);
                int secondQuote = payloadJson.IndexOf('"', firstQuote + 1);

                if (firstQuote == -1 || secondQuote == -1)
                    return "Unknown";

                return payloadJson.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
