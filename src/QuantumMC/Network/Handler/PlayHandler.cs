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

                default:
                    Log.Debug("PlayHandler received unhandled packet 0x{PacketId:X2} from {Username}",
                        packetId, session.Username);
                    break;
            }
        }

        private void HandleRequestChunkRadius(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new RequestChunkRadiusPacket();
            packet.Decode(stream);

            Log.Information("Player {Username} requested chunk radius: {Radius} (max: {MaxRadius})",
                session.Username, packet.Radius, packet.MaxRadius);

            if (session.World == null)
            {
                Log.Error("World is null for player {Username}, cannot send chunks", session.Username);
                return;
            }

            int grantedRadius = Math.Min(packet.Radius, session.World.MaxChunkRadius);
            session.ChunkRadius = grantedRadius;

            var radiusResponse = new ChunkRadiusUpdatedPacket
            {
                Radius = grantedRadius
            };
            session.SendPacket(radiusResponse);
            Log.Information("Sent ChunkRadiusUpdated to {Username}: radius={Radius}",
                session.Username, grantedRadius);

            int spawnX = session.World.SpawnX;
            int spawnY = session.World.SpawnY;
            int spawnZ = session.World.SpawnZ;

            var publisherUpdate = new NetworkChunkPublisherUpdatePacket
            {
                Position = new BlockPosition(spawnX, spawnY, spawnZ),
                Radius = grantedRadius * 16 
            };
            session.SendPacket(publisherUpdate);
            Log.Information("Sent NetworkChunkPublisherUpdate to {Username}: pos=({X},{Y},{Z}), radius={Radius}",
                session.Username, spawnX, spawnY, spawnZ, grantedRadius * 16);

            int centerChunkX = spawnX >> 4;
            int centerChunkZ = spawnZ >> 4;

            var chunks = session.World.GetChunksInRadius(centerChunkX, centerChunkZ, grantedRadius);
            Log.Information("Sending {Count} chunks to {Username}...", chunks.Count, session.Username);

            foreach (var chunk in chunks)
            {
                var (chunkPayload, subChunkCount) = chunk.Serialize();

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

            Log.Information("Finished sending {Count} chunks to {Username}", chunks.Count, session.Username);

            var spawnStatus = new PlayStatusPacket
            {
                Status = BedrockProtocol.Packets.Enums.PlayStatus.PlayerSpawn
            };
            session.SendPacket(spawnStatus);
            Log.Information("Sent PlayStatus(PlayerSpawn) to {Username}", session.Username);
        }

        private void HandleSetLocalPlayerAsInitialized(PlayerSession session, byte[] payload)
        {
            var stream = new BinaryStream(payload);
            var packet = new SetLocalPlayerAsInitializedPacket();
            packet.Decode(stream);

            Log.Information("Player {Username} is fully initialized (runtimeEntityId={RuntimeEntityId})",
                session.Username, packet.RuntimeEntityId);

            session.State = SessionState.PlayPhase;
        }
    }
}
