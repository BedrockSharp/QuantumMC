using BedrockProtocol.Packets;
using BedrockProtocol.Packets.Types;
using BedrockProtocol.Utils;
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

            int spawnX = session.Player.World.SpawnX;
            int spawnY = session.Player.World.SpawnY;
            int spawnZ = session.Player.World.SpawnZ;

            var publisherUpdate = new NetworkChunkPublisherUpdatePacket
            {
                Position = new BlockPosition(spawnX, spawnY, spawnZ),
                Radius = grantedRadius * 16 
            };
            session.SendPacket(publisherUpdate);

            int centerChunkX = spawnX >> 4;
            int centerChunkZ = spawnZ >> 4;

            var chunks = session.Player.World.GetChunksInRadius(centerChunkX, centerChunkZ, grantedRadius);

            foreach (var chunk in chunks)
            {
                var chunkData = chunk.Serialize();
                byte[] chunkPayload = chunkData.Payload;
                int subChunkCount = chunkData.SubChunkCount;

                var levelChunkPacket = new LevelChunkPacket
                {
                    ChunkX = chunk.ChunkX,
                    ChunkZ = chunk.ChunkZ,
                    Dimension = 0,
                    SubChunkCount = subChunkCount,
                    CacheEnabled = false,
                    RequestSubChunks = false,
                    Payload = chunkPayload
                };

                session.SendPacket(levelChunkPacket);
            }

            var spawnStatus = new PlayStatusPacket
            {
                Status = BedrockProtocol.Packets.Enums.PlayStatus.PlayerSpawn
            };
            session.SendPacket(spawnStatus);
        }

        private void HandleSetLocalPlayerAsInitialized(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new SetLocalPlayerAsInitializedPacket();
            packet.Decode(stream);

            session.State = SessionState.PlayPhase;
        }
    }
}
