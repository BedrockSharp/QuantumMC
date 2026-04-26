using System;
using System.Collections.Generic;
using QuantumMC.Network;
using QuantumMC.World;
using BedrockProtocol.Packets;
using BedrockProtocol.Packets.Types;
using BedrockProtocol.Packets.Enums;

namespace QuantumMC.Player
{
    public class Player
    {
        public string Username { get; set; } = string.Empty;
        public string Xuid { get; set; } = string.Empty;
        public string Uuid { get; set; } = string.Empty;
        public Network.LoginChainData? ChainData { get; set; }
        
        public long EntityUniqueId { get; set; }
        public ulong EntityRuntimeId { get; set; }
        
        public PlayerSession Session { get; }
        
        private World.World? _world;
        public World.World? World 
        { 
            get => _world;
            set
            {
                if (_world != value)
                {
                    SentChunks.Clear();
                    _lastChunkX = int.MaxValue;
                    _lastChunkZ = int.MaxValue;
                }
                _world = value;
            }
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public float HeadYaw { get; set; }
        public int Gamemode { get; set; } = 1;
        public int ChunkRadius { get; set; } = 4;
        
        public HashSet<(int X, int Z)> SentChunks { get; } = new();
        private int _lastChunkX = int.MaxValue;
        private int _lastChunkZ = int.MaxValue;

        public Player(PlayerSession session)
        {
            Session = session;
            Username = session.Username;
            World = Server.Instance.WorldManager.DefaultWorld;
            
            EntityUniqueId = Server.Instance.Network.SessionManager.GetNextEntityId();
            EntityRuntimeId = (ulong)EntityUniqueId;

            X = 0; Y = 100; Z = 0;
            if (World != null) {
                X = World.SpawnX;
                Y = World.SpawnY;
                Z = World.SpawnZ;
            }
        }

        public void UpdateChunks()
        {
            if (World == null) return;

            int currentChunkX = (int)Math.Floor(X) >> 4;
            int currentChunkZ = (int)Math.Floor(Z) >> 4;

            if (currentChunkX == _lastChunkX && currentChunkZ == _lastChunkZ) return;

            _lastChunkX = currentChunkX;
            _lastChunkZ = currentChunkZ;

            var publisherUpdate = new NetworkChunkPublisherUpdatePacket
            {
                Position = new BlockPosition((int)X, (int)Y, (int)Z),
                Radius = (ChunkRadius * 16)
            };
            Session.SendPacket(publisherUpdate);

            var chunks = World.GetChunksInRadius(currentChunkX, currentChunkZ, ChunkRadius);
            foreach (var chunk in chunks)
            {
                if (SentChunks.Contains((chunk.ChunkX, chunk.ChunkZ))) continue;

                var chunkData = chunk.Serialize();
                var levelChunkPacket = new LevelChunkPacket
                {
                    ChunkX = chunk.ChunkX,
                    ChunkZ = chunk.ChunkZ,
                    Dimension = 0,
                    SubChunkCount = chunkData.SubChunkCount,
                    CacheEnabled = false,
                    RequestSubChunks = false,
                    Payload = chunkData.Payload
                };

                Session.SendPacket(levelChunkPacket);
                SentChunks.Add((chunk.ChunkX, chunk.ChunkZ));
            }
        }

        public void SendMessage(string message)
        {
            var textPacket = new TextPacket
            {
                Message = message,
                Type = TextType.Raw
            };

            Session.SendPacket(textPacket);
        }
    }
}
