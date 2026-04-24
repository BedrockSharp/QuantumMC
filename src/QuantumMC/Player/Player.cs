using System;
using QuantumMC.Network;

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
        public World.World? World { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public int Gamemode { get; set; } = 1;
        public int ChunkRadius { get; set; } = 4;
        
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
    }
}
