using BedrockProtocol.Packets;
using BedrockProtocol.Packets.Types;
using BedrockProtocol.Utils;
using QuantumMC.Utils;
using QuantumMC.Event.Impl;
using Serilog;

namespace QuantumMC.Network.Handler
{
    public class PlayHandler : PacketHandler
    {
        public override void Handle(PlayerSession session, uint packetId, byte[] payload)
        {
            switch ((PacketIds)packetId)
            {
                case PacketIds.RequestChunkRadius:
                    HandleRequestChunkRadius(session, payload);
                    break;

                case PacketIds.SetLocalPlayerAsInitialized:
                    HandleSetLocalPlayerAsInitialized(session, payload);
                    break;
            }
        }

        private void HandleRequestChunkRadius(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new RequestChunkRadiusPacket();
            packet.Decode(stream);

            Log.Debug("Player {Username} requested chunk radius: {Radius} (max: {MaxRadius})",
                session.Username, packet.Radius, packet.MaxRadius);

            if (session.Player.World == null)
            {
                Log.Error("World is null for player {Username}, cannot send chunks", session.Username);
                return;
            }

            int grantedRadius = Math.Min(packet.Radius, session.Player.World.MaxChunkRadius);
            session.Player.ChunkRadius = grantedRadius;

            var radiusResponse = new ChunkRadiusUpdatedPacket
            {
                Radius = grantedRadius
            };
            session.SendPacket(radiusResponse);

            session.Player.UpdateChunks();

            var spawnStatus = new PlayStatusPacket
            {
                Status = BedrockProtocol.Packets.Enums.PlayStatus.PlayerSpawn
            };
            session.SendPacket(spawnStatus);

            var playerJoinEvent = new PlayerJoinEvent(session.Player, "%multiplayer.player.joined");
            Server.Instance.PluginManager.EventManager.CallEventAsync(playerJoinEvent).GetAwaiter().GetResult();

            if (playerJoinEvent.JoinMessage != null)
            {
                Server.Instance.SendTranslation(TextFormat.Yellow + playerJoinEvent.JoinMessage, [session.Username]);
            }
        }

        private void HandleSetLocalPlayerAsInitialized(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new SetLocalPlayerAsInitializedPacket();
            packet.Decode(stream);

            session.State = SessionState.InGamePhase;
            session.Player.SendCommandData();
        }
    }
}
